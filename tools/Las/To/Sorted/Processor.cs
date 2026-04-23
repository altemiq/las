// -----------------------------------------------------------------------
// <copyright file="Processor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.To.Sorted;

/// <summary>
/// The sorted LAS processor.
/// </summary>
internal static class Processor
{
    /// <summary>
    /// Processes the input to output.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="console">The console.</param>
    /// <param name="inputs">The input URIs.</param>
    /// <param name="output">The output file.</param>
    /// <param name="embedIndex">Set to <see langword="true"/> to embed the index.</param>
    public static void Process(IServiceProvider? serviceProvider, TextWriter console, Uri[] inputs, FileInfo? output, bool embedIndex)
    {
        var completeWritingTo = System.Text.CompositeFormat.Parse(Tool.Properties.Resources.CompletedWritingTo);

        foreach (var file in inputs)
        {
            var laxUri = Path.ChangeExtension(file, ".lax");
            var index = Path.Exists(laxUri, serviceProvider)
                ? CreateFromLax(laxUri, serviceProvider)
                : CreateFromReader(file, serviceProvider);

            using var reader = LazReader.Create(File.OpenRead(file, serviceProvider), leaveOpen: false);

            SortByIndex(File.OpenRead(file, serviceProvider), index, ref output, embedIndex);

            if (output is not null)
            {
                console.WriteLine(string.Format(Tool.Properties.Resources.Culture, completeWritingTo, output.FullName));
            }

            static Indexing.LasIndex CreateFromReader(Uri file, IServiceProvider? serviceProvider)
            {
                using var reader = LazReader.Create(File.OpenRead(file, serviceProvider), leaveOpen: false);
                return Index.IndexingExtensions.ReadOrCreateIndex(reader);
            }

            static Indexing.LasIndex CreateFromLax(Uri file, IServiceProvider? serviceProvider)
            {
                using var stream = File.OpenRead(file, serviceProvider);
                return Indexing.LasIndex.ReadFrom(stream);
            }
        }
    }

    /// <summary>
    /// Processes the input stream.
    /// </summary>
    /// <param name="input">The input stream.</param>
    /// <param name="index">The LAS index.</param>
    /// <param name="output">The output file.</param>
    /// <param name="embedIndex">Set to <see langword="true"/> to embed the index.</param>
    /// <exception cref="ArgumentOutOfRangeException">The point data format cannot be extended.</exception>
    public static void SortByIndex(Stream input, Indexing.LasIndex index, ref FileInfo? output, bool embedIndex)
    {
        var cells = index.AsReadOnly();

        // Extract cell boundaries for vectorized lookup
        var cellCount = cells.Count;
        var minX = new float[cellCount];
        var minY = new float[cellCount];
        var maxX = new float[cellCount];
        var maxY = new float[cellCount];
        for (var i = 0; i < cellCount; i++)
        {
            minX[i] = cells[i].Minimum.X;
            minY[i] = cells[i].Minimum.Y;
            maxX[i] = cells[i].Maximum.X;
            maxY[i] = cells[i].Maximum.Y;
        }

        using var reader = LazReader.Create(input, leaveOpen: false);

        var systemIdentifier = string.IsNullOrEmpty(reader.Header.SystemIdentifier)
            ? ToolConstants.SystemIdentifier
            : reader.Header.SystemIdentifier + " \"" + ToolConstants.SystemIdentifier + "\"";

        // set to version 1.4 and point 6+ to get layered chunks
        var builder = new HeaderBlockBuilder(reader.Header)
        {
            Version = new(1, 4),
            GlobalEncoding = GlobalEncoding.StandardGpsTime | GlobalEncoding.Wkt,
            PointDataFormatId = GetExtendedPointDataFormatId(reader.Header.PointDataFormatId),
            GeneratingSoftware = ToolConstants.GeneratingSoftware,
            SystemIdentifier = systemIdentifier,
        };

        builder.SetCompressed();
        var header = builder.HeaderBlock;

        var variableLengthRecords = GetVariableLengthRecords(header, reader.VariableLengthRecords);
        var extendedVariableLengthRecords = GetExtendedVariableLengthRecords(reader.ExtendedVariableLengthRecords);
        Indexing.LasIndex? newIndex;

        var stream = CreateStream();
        using (var writer = new LazWriter(stream, leaveOpen: true))
        {
            writer.Write(header, variableLengthRecords);

            // read via cells
            var quantizer = new PointDataRecordQuantizer(reader.Header);

            const int BatchSize = 1024;
            int pointLength = reader.PointDataLength;
            byte[] buffer = new byte[BatchSize * pointLength];
            var inputX = new int[BatchSize];
            var inputY = new int[BatchSize];
            var results = new System.Numerics.Vector2[BatchSize];
            var points = new IBasePointDataRecord[BatchSize];
            byte[]?[] extraBytes = new byte[BatchSize][];

            var count = 0;
            while (true)
            {
                int pointsRead = reader.ReadPointDataRecordData(buffer);
                if (pointsRead == 0)
                {
                    break;
                }

                for (var i = 0; i < pointsRead; i++)
                {
                    var pointSpan = reader.Read(buffer.AsSpan(i * pointLength, pointLength));
                    if (pointSpan.PointDataRecord is not { } point)
                    {
                        break;
                    }

                    inputX[count] = point.X;
                    inputY[count] = point.Y;
                    points[count] = point;

                    extraBytes[count] ??= new byte[pointSpan.ExtraBytes.Length];
                    pointSpan.ExtraBytes.CopyTo(extraBytes[count]);

                    count++;

                    if (count is not BatchSize)
                    {
                        continue;
                    }

                    quantizer.Quantize(inputX, inputY, results);
                    for (var j = 0; j < BatchSize; j++)
                    {
                        writer.Write(PointConverter.ToExtended(points[j]), extraBytes[j], GetCellIndex(results[j], minX, minY, maxX, maxY));
                    }

                    count = 0;
                }
            }

            if (count > 0)
            {
                quantizer.Quantize(inputX.AsSpan(0, count), inputY.AsSpan(0, count), results);
                for (var i = 0; i < count; i++)
                {
                    writer.Write(PointConverter.ToExtended(points[i]), extraBytes[i], GetCellIndex(results[i], minX, minY, maxX, maxY));
                }
            }

            var chunkCounts = writer.GetChunkCounts().ToArray();

            stream.Flush();

            // set up the new index
            newIndex = index.CloneEmpty();
            var currentChunkStart = 0U;
            foreach (var (chunkCount, cellIndex) in chunkCounts.Zip(cells, (chunkCount, cell) => (chunkCount, newIndex.IndexOf(cell))))
            {
                var chunkEnd = currentChunkStart + chunkCount;
                _ = newIndex.Add(cellIndex, currentChunkStart, chunkEnd - 1);
                currentChunkStart = chunkEnd;
            }

            // embed the index
            if (embedIndex)
            {
                extendedVariableLengthRecords.Add((new Indexing.LaxTag(newIndex), true));
                newIndex = default;
            }

            foreach (var (extendedVariableLengthRecord, special) in extendedVariableLengthRecords)
            {
                writer.Write(extendedVariableLengthRecord, special);
            }
        }

        // flush everything to the stream
        stream.Flush();

        stream.Reset();

        output ??= new(Path.ChangeExtension(Path.GetRandomFileName(), ".laz"));

        using (var fileStream = output.OpenWrite())
        {
            stream.CopyTo(fileStream);
        }

        // write out the new index
        if (newIndex is not null)
        {
            using var indexStream = File.OpenWrite(Path.ChangeExtension(output.FullName, ".lax"));
            newIndex.WriteTo(indexStream);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Meziantou", "MA0002:IEqualityComparer<string> or IComparer<string> is missing", Justification = "This is inaccessible.")]
        static MultipleStream CreateStream()
        {
            return new LazMultipleMemoryStream();
        }

        static int GetCellIndex(System.Numerics.Vector2 value, float[] minX, float[] minY, float[] maxX, float[] maxY)
        {
            var vectorX = new System.Numerics.Vector<float>(value.X);
            var vectorY = new System.Numerics.Vector<float>(value.Y);

            var vectorSize = System.Numerics.Vector<float>.Count;
            for (var i = 0; i <= minX.Length - vectorSize; i += vectorSize)
            {
                var vectorMinX = new System.Numerics.Vector<float>(minX, i);
                var vectorMinY = new System.Numerics.Vector<float>(minY, i);
                var vectorMaxX = new System.Numerics.Vector<float>(maxX, i);
                var vectorMaxY = new System.Numerics.Vector<float>(maxY, i);

                var mask = System.Numerics.Vector.GreaterThanOrEqual(vectorX, vectorMinX)
                           & System.Numerics.Vector.GreaterThanOrEqual(vectorY, vectorMinY)
                           & System.Numerics.Vector.LessThan(vectorX, vectorMaxX)
                           & System.Numerics.Vector.LessThan(vectorY, vectorMaxY);

                if (mask.Equals(System.Numerics.Vector<int>.Zero))
                {
                    continue;
                }

                // find the first set bit
                for (var j = 0; j < vectorSize; j++)
                {
                    if (mask[j] is not 0)
                    {
                        return i + j;
                    }
                }
            }

            // remainder
            var x = value.X;
            var y = value.Y;
            for (var i = (minX.Length / vectorSize) * vectorSize; i < minX.Length; i++)
            {
                if (x >= minX[i] && x < maxX[i] && y >= minY[i] && y < maxY[i])
                {
                    return i;
                }
            }

            return -1;
        }

        static byte GetExtendedPointDataFormatId(byte pointDataFormatId)
        {
            return pointDataFormatId switch
            {
                PointDataRecord.Id or GpsPointDataRecord.Id => ExtendedGpsPointDataRecord.Id,
                ColorPointDataRecord.Id or GpsColorPointDataRecord.Id => ExtendedGpsColorPointDataRecord.Id,
                GpsWaveformPointDataRecord.Id => ExtendedGpsWaveformPointDataRecord.Id,
                GpsColorWaveformPointDataRecord.Id => ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id,
                ExtendedGpsPointDataRecord.Id => ExtendedGpsPointDataRecord.Id,
                ExtendedGpsColorPointDataRecord.Id => ExtendedGpsColorPointDataRecord.Id,
                ExtendedGpsColorNearInfraredPointDataRecord.Id => ExtendedGpsColorNearInfraredPointDataRecord.Id,
                ExtendedGpsWaveformPointDataRecord.Id => ExtendedGpsWaveformPointDataRecord.Id,
                ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id => ExtendedGpsColorNearInfraredWaveformPointDataRecord.Id,
                _ => throw new ArgumentOutOfRangeException(nameof(pointDataFormatId), pointDataFormatId, default),
            };
        }

        static ICollection<VariableLengthRecord> GetVariableLengthRecords(in HeaderBlock header, IReadOnlyList<VariableLengthRecord> records)
        {
            var values = records.Select(static x => x).ToList();
            var geoTiffTags = new List<VariableLengthRecord>();
            var hasWkt = false;

            var extraBytesCount = default(ushort);
            for (var i = values.Count - 1; i >= 0; i--)
            {
                var value = values[i];
                if (value is ExtraBytes extraBytes)
                {
                    extraBytesCount = extraBytes.GetByteCount();
                }

                // remove unwanted tags
                if (value.IsForCompression() || value.IsForCloudOptimization())
                {
                    _ = values.Remove(value);
                    continue;
                }

                if (value.IsGeoTiff())
                {
                    geoTiffTags.Add(value);
                    _ = values.Remove(value);
                    continue;
                }

                hasWkt |= value.IsWkt();
            }

            // check to see if we have GeoTIFF tags, but no WKT
            if (geoTiffTags.Count is not 0 && !hasWkt)
            {
                // convert the GeoTIFF flags to WKT
                values.AddRange(Geodesy.GeodesyExtensions.ToWkt(geoTiffTags));
            }

            values.Add(new CompressedTag(header, extraBytesCount, Compressor.LayeredChunked) { ChunkSize = CompressedTag.VariableChunkSize, NumOfSpecialEvlrs = -1, });

            return values;
        }

        static ICollection<(ExtendedVariableLengthRecord Record, bool Special)> GetExtendedVariableLengthRecords(IReadOnlyList<ExtendedVariableLengthRecord> records)
        {
            return [.. records.Where(static value => !value.IsForCompression() && !value.IsForCloudOptimization()).Select(r => (r, false))];
        }
    }
}