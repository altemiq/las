// -----------------------------------------------------------------------
// <copyright file="LasRecover.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#:package Altemiq.CommandLine
#:project ../src/IO.Las.Compression/IO.Las.Compression.csproj

using System.CommandLine;
using Altemiq.IO.Las;

VariableLengthRecordProcessor.Instance.RegisterCompression();

var inputOption = new Option<Uri[]>("-i") { CustomParser = System.CommandLine.Parsing.UriParser.ParseAll };
var outputOption = new Option<FileInfo>("-o") { Description = "use [n] as output file" };
var outputLazOption = new Option<bool>("-olaz") { Description = "output as LAZ (compressed LAS)" };
var outputSuffixOption = new Option<string>("-odix") { Description = "set output file name suffix to [n]" };
var gpsTimeStartOption = new Option<double>("-gps_time_start", "-gpstime_start") { DefaultValueFactory = _ => 100000000 };
var gpsTimeIncrementOption = new Option<double>("-gps_time_increment", "-gpstime_increment") { DefaultValueFactory = _ => 0.000001 };

var root = new RootCommand
{
    inputOption,
    outputOption,
    outputLazOption,
    outputSuffixOption,
    gpsTimeStartOption,
    gpsTimeIncrementOption,
};

root.SetAction(parseResult =>
{
    var gpsTime = parseResult.GetValue(gpsTimeStartOption);
    var gpsTimeIncrement = parseResult.GetValue(gpsTimeIncrementOption);
    var laz = parseResult.GetValue(outputLazOption);
    var input = parseResult.GetRequiredValue(inputOption);
    var output = parseResult.GetValue(outputOption);
    if (output is null)
    {
        var outputPath = input[0].LocalPath;
        if (laz)
        {
            outputPath = Path.ChangeExtension(outputPath, ".laz");
        }

        if (parseResult.GetValue(outputSuffixOption) is { } outputSuffix)
        {
            outputPath = Path.Combine(
                Path.GetDirectoryName(outputPath) ?? string.Empty,
                string.Concat(
                    Path.GetFileNameWithoutExtension(outputPath),
                    outputSuffix,
                    Path.GetExtension(outputPath)));
        }

        output = new(outputPath);
    }

    using var reader = LazReader.Create(File.OpenRead(input[0].LocalPath));
    var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().First();
    var sensorXIndex = IndexOrThrow(extraBytes, "sensor x coord");
    var sensorYIndex = IndexOrThrow(extraBytes, "sensor y coord");
    var sensorZIndex = IndexOrThrow(extraBytes, "sensor z coord");

    if (output.Exists)
    {
        using FileStream temp = output.OpenWrite();
        temp.SetLength(0);
    }

    var outputStream = output.OpenWrite();
    using var writer = laz
        ? new LazWriter(outputStream)
        : new LasWriter(outputStream);

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    var headerBuilder = new HeaderBlockBuilder(reader.Header);
    Clear(headerBuilder.LegacyNumberOfPointsByReturn);
    Clear(headerBuilder.NumberOfPointsByReturn);

    writer.Write(headerBuilder.HeaderBlock, reader.VariableLengthRecords);

    var count = 0U;
    var sensorX = double.MinValue;
    var sensorY = double.MinValue;
    var sensorZ = double.MinValue;
    IList<(IBasePointDataRecord, ReadOnlyMemory<byte>)> points = [];

    while (reader.ReadPointDataRecord() is { PointDataRecord: { } point, ExtraBytes: var data })
    {
        if (!sensorX.Equals(GetValue(data, extraBytes, sensorXIndex)) ||
            !sensorY.Equals(GetValue(data, extraBytes, sensorYIndex)) ||
            !sensorZ.Equals(GetValue(data, extraBytes, sensorZIndex)))
        {
            if (points.Count > 0)
            {
                for (var i = 0; i < points.Count; i++)
                {
                    WritePoint(writer, points[i], headerBuilder, i, points.Count, gpsTime);
                }

                gpsTime += gpsTimeIncrement;
            }

            points.Clear();

            sensorX = GetValue(data, extraBytes, sensorXIndex);
            sensorY = GetValue(data, extraBytes, sensorYIndex);
            sensorZ = GetValue(data, extraBytes, sensorZIndex);
        }

        var temp = new byte[data.Length];
        data.CopyTo(temp);
        points.Add((point.Clone(), temp));
        count++;

        static double GetValue(ReadOnlySpan<byte> data, IExtraBytes extraBytes, int index)
        {
            return extraBytes.GetValue(index, data) switch
            {
                double doubleValue => doubleValue,
                IConvertible convertable => convertable.ToDouble(default),
                _ => default,
            };
        }
    }

    // write last set of points
    for (var i = 0; i < points.Count; i++)
    {
        WritePoint(writer, points[i], headerBuilder, i, points.Count, gpsTime);
    }

    writer.Flush();

    foreach (var extendedVariableLengthRecord in reader.ExtendedVariableLengthRecords)
    {
        writer.Write(extendedVariableLengthRecord);
    }

    outputStream.Position = 0;

    writer.Write(headerBuilder.HeaderBlock, reader.VariableLengthRecords);

    parseResult.InvocationConfiguration.Output.WriteLine("processed {0} points in {1} sec", count, stopwatch.Elapsed.TotalSeconds);

    static void WritePoint(LasWriter writer, (IBasePointDataRecord, ReadOnlyMemory<byte>) record, HeaderBlockBuilder headerBuilder, int returnIndex, int totalReturns, double gpsTime)
    {
        IBasePointDataRecord toWrite = record.Item1 switch
        {
            PointDataRecord r => UpdatePointDataRecord(r),
            GpsPointDataRecord r => UpdateGpsPointDataRecord(r),
            ColorPointDataRecord r => UpdateColorPointDataRecord(r),
            GpsColorPointDataRecord r => UpdateGpsColorPointDataRecord(r),
            GpsWaveformPointDataRecord r => UpdateGpsWaveformPointDataRecord(r),
            GpsColorWaveformPointDataRecord r => UpdateGpsColorWaveformPointDataRecord(r),
            ExtendedGpsPointDataRecord r => UpdateExtendedGpsPointDataRecord(r),
            ExtendedGpsColorPointDataRecord r => UpdateExtendedGpsColorPointDataRecord(r),
            ExtendedGpsColorNearInfraredPointDataRecord r => UpdateExtendedGpsColorNearInfraredPointDataRecord(r),
            ExtendedGpsWaveformPointDataRecord r => UpdateExtendedGpsWaveformPointDataRecord(r),
            ExtendedGpsColorNearInfraredWaveformPointDataRecord r => UpdateExtendedGpsColorNearInfraredWaveformPointDataRecord(r),
            var r => r,
        };

        writer.Write(toWrite, record.Item2.Span);

        IBasePointDataRecord UpdatePointDataRecord(PointDataRecord record)
        {
            if (returnIndex < 5)
            {
                headerBuilder.LegacyNumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns };
        }

        IBasePointDataRecord UpdateGpsPointDataRecord(GpsPointDataRecord record)
        {
            if (returnIndex < 5)
            {
                headerBuilder.LegacyNumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateColorPointDataRecord(ColorPointDataRecord record)
        {
            if (returnIndex < 5)
            {
                headerBuilder.LegacyNumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns };
        }

        IBasePointDataRecord UpdateGpsColorPointDataRecord(GpsColorPointDataRecord record)
        {
            if (returnIndex < 5)
            {
                headerBuilder.LegacyNumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateGpsWaveformPointDataRecord(GpsWaveformPointDataRecord record)
        {
            if (returnIndex < 5)
            {
                headerBuilder.LegacyNumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateGpsColorWaveformPointDataRecord(GpsColorWaveformPointDataRecord record)
        {
            if (returnIndex < 5)
            {
                headerBuilder.LegacyNumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateExtendedGpsPointDataRecord(ExtendedGpsPointDataRecord record)
        {
            if (returnIndex < 16)
            {
                headerBuilder.NumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateExtendedGpsColorPointDataRecord(ExtendedGpsColorPointDataRecord record)
        {
            if (returnIndex < 16)
            {
                headerBuilder.NumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateExtendedGpsColorNearInfraredPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord record)
        {
            if (returnIndex < 16)
            {
                headerBuilder.NumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateExtendedGpsWaveformPointDataRecord(ExtendedGpsWaveformPointDataRecord record)
        {
            if (returnIndex < 16)
            {
                headerBuilder.NumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }

        IBasePointDataRecord UpdateExtendedGpsColorNearInfraredWaveformPointDataRecord(ExtendedGpsColorNearInfraredWaveformPointDataRecord record)
        {
            if (returnIndex < 16)
            {
                headerBuilder.NumberOfPointsByReturn[returnIndex]++;
            }

            return record with { ReturnNumber = (byte)(returnIndex + 1), NumberOfReturns = (byte)totalReturns, GpsTime = gpsTime };
        }
    }

    static int IndexOrThrow(ExtraBytes extraBytes, string name)
    {
        for (var i = 0; i < extraBytes.Count; i++)
        {
            if (string.Equals(extraBytes[i].Name, name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        throw new ArgumentException(string.Format(System.Globalization.CultureInfo.CurrentCulture, "could not find additional attribute '{0}'", name), nameof(name));
    }

    static void Clear<T>(IList<T> values)
        where T : struct
    {
        for (var i = 0; i < values.Count; i++)
        {
            values[i] = default;
        }
    }
});

await root.Parse(args).InvokeAsync().ConfigureAwait(false);