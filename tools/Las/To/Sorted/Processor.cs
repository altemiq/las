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

            while (reader.ReadPointDataRecord() is { PointDataRecord: { } point } record)
            {
                var x = quantizer.GetX(point.X);
                var y = quantizer.GetY(point.Y);
                var cellIndex = GetCellIndex(cells, x, y);
                writer.Write(PointConverter.ToExtended(point), record.ExtraBytes, cellIndex);
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

        static int GetCellIndex(IReadOnlyList<Indexing.LasIndexCell> cells, double x, double y)
        {
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i].Contains(x, y))
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

            values.Add(new CompressedTag(header, extraBytesCount, Compressor.LayeredChunked) { ChunkSize = -1, Options = default, NumOfSpecialEvlrs = -1, });

            return values;
        }

        static ICollection<(ExtendedVariableLengthRecord Record, bool Special)> GetExtendedVariableLengthRecords(IReadOnlyList<ExtendedVariableLengthRecord> records)
        {
            return [.. records.Where(static value => !value.IsForCompression() && !value.IsForCloudOptimization()).Select(r => (r, false))];
        }
    }
}