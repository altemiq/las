// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The append format delegate.
/// </summary>
/// <param name="style">The style.</param>
/// <param name="formatProvider">The format provider.</param>
/// <param name="format">The format.</param>
/// <param name="args">The arguments.</param>
public delegate void AppendFormat(Style? style, IFormatProvider formatProvider, string format, params object?[] args);

/// <summary>
/// The append delegate.
/// </summary>
/// <param name="value">The text to append.</param>
/// <param name="style">The style.</param>
public delegate void Append(string value, Style? style = default);

/// <summary>
/// The append line delegate.
/// </summary>
/// <param name="value">The value.</param>
/// <param name="style">The style.</param>
public delegate void AppendLine(string? value = default, Style? style = default);

/// <summary>
/// Extension methods.
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Gets the LAS statistics.
    /// </summary>
    /// <param name="reader">The LAS reader.</param>
    /// <param name="box">The bounding box.</param>
    /// <returns>The LAS statistics.</returns>
    public static (
        IMinMax<int> X,
        IMinMax<int> Y,
        IMinMax<int> Z,
        IMinMax<int> Intensity,
        IMinMax<int> ReturnNumber,
        IMinMax<int> NumberOfReturns,
        bool EdgeOfFlightLine,
        bool ScanDirectionFlag,
        IMinMax<byte> Classification,
        IMinMax<sbyte>? ScanAngleRank,
        IMinMax<byte> UserData,
        IMinMax<ushort> PointSourceId,
#if LAS1_4_OR_GREATER
        IMinMax<short>? ScanAngle,
#endif
        IMinMax<double>? Gps,
#if LAS1_3_OR_GREATER
        IMinMax<byte>? WavePacketDescriptorIndex,
        IMinMax<ulong>? ByteOffsetToWaveformData,
        IMinMax<uint>? WaveformPacketSizeInBytes,
        IMinMax<float>? ReturnPointWaveformLocation,
        IMinMax<float>? ParametricDx,
        IMinMax<float>? ParametricDy,
        IMinMax<float>? ParametricDz,
#endif
#if LAS1_4_OR_GREATER
        IEnumerable<IMinMax> ExtraBytes,
#endif
        int FirstReturns,
        int IntermediateReturns,
        int LastReturns,
        int SingleReturns,
        long[] OverviewReturnNumber,
        long[] OverviewNumberOfReturns,
        int[] Histogram) GetStatistics(this ILasReader reader, BoundingBox? box)
    {
        var x = MinMax.Create<int>();
        var y = MinMax.Create<int>();
        var z = MinMax.Create<int>();
        var intensity = MinMax.Create<int>();
        var returnNumber = MinMax.Create<int>();
        var numberOfReturns = MinMax.Create<int>();
        var edgeOfFlightLine = false;
        var scanDirectionFlag = false;
        var classification = MinMax.Create<byte>();
        IMinMax<sbyte>? scanAngleRank = default;
        var userData = MinMax.Create<byte>();
        var pointSourceId = MinMax.Create<ushort>();
#if LAS1_4_OR_GREATER
        IMinMax<short>? scanAngle = default;
#endif
        IMinMax<double>? gps = default;
#if LAS1_3_OR_GREATER
        IMinMax<byte>? wavePacketDescriptorIndex = default;
        IMinMax<ulong>? byteOffsetToWaveformData = default;
        IMinMax<uint>? waveformPacketSizeInBytes = default;
        IMinMax<float>? returnPointWaveformLocation = default;
        IMinMax<float>? parametricDx = default;
        IMinMax<float>? parametricDy = default;
        IMinMax<float>? parametricDz = default;
#endif

        var firstReturns = default(int);
        var intermediateReturns = default(int);
        var lastReturns = default(int);
        var singleReturns = default(int);

#if LAS1_4_OR_GREATER
        var extraBytesRecord = reader.VariableLengthRecords
            .OfType<IExtraBytes>()
            .FirstOrDefault() ?? default(NullExtraBytes);
        var extraBytes = extraBytesRecord.Count is 0 ? [] : extraBytesRecord.Select(MinMax.Create).ToArray();
#endif

        var histogram = new int[256];
        var numberOfPointsByReturn = new long[16];
        var numberOfReturnsArray = new long[16];

        Func<IBasePointDataRecord, bool> filter = _ => true;
        if (box.HasValue)
        {
            var quantizer = new PointDataRecordQuantizer(reader.Header);
            var boundingBox = box.Value;
            filter = point =>
            {
                var (px, py, pz) = quantizer.Get(point);
                return boundingBox.Contains(px, py, pz);
            };
        }

        while (reader.ReadPointDataRecord() is { PointDataRecord: { } record, ExtraBytes: var data })
        {
            if (!filter(record))
            {
                continue;
            }

            x.Update(record.X);
            y.Update(record.Y);
            z.Update(record.Z);
            intensity.Update(record.Intensity);
            var returnNumberValue = record.ReturnNumber;
            returnNumber.Update(returnNumberValue);
            numberOfPointsByReturn[returnNumberValue]++;
            var numberOfReturnsValue = record.NumberOfReturns;
            numberOfReturns.Update(numberOfReturnsValue);
            numberOfReturnsArray[numberOfReturnsValue]++;
            if (returnNumberValue.IsFirst())
            {
                firstReturns++;
            }

            if (returnNumberValue.IsIntermediate(numberOfReturnsValue))
            {
                intermediateReturns++;
            }

            if (returnNumberValue.IsLast(numberOfReturnsValue))
            {
                lastReturns++;
            }

            if (numberOfReturnsValue.IsSingle())
            {
                singleReturns++;
            }

            edgeOfFlightLine |= record.EdgeOfFlightLine;
            scanDirectionFlag |= record.ScanDirectionFlag;
#if LAS1_4_OR_GREATER
            if (record is IExtendedPointDataRecord extendedPointDataRecord)
            {
                scanAngle ??= MinMax.Create<short>();
                var classificationValue = (byte)extendedPointDataRecord.Classification;
                classification.Update(classificationValue);
                scanAngle.Update(extendedPointDataRecord.ScanAngle);

                histogram[classificationValue]++;
            }
#endif

            if (record is IPointDataRecord pointDataRecord)
            {
                scanAngleRank ??= MinMax.Create<sbyte>();
                var classificationValue = (byte)pointDataRecord.Classification;
                classification.Update(classificationValue);
                scanAngleRank.Update(pointDataRecord.ScanAngleRank);

                histogram[classificationValue]++;
            }

            userData.Update(record.UserData);
            pointSourceId.Update(record.PointSourceId);

            if (record is IGpsPointDataRecord gpsPointDataRecord)
            {
                gps ??= MinMax.Create<double>();
                gps.Update(gpsPointDataRecord.GpsTime);
            }

#if LAS1_3_OR_GREATER
            if (record is IWaveformPointDataRecord waveformPointDataRecord)
            {
                wavePacketDescriptorIndex ??= MinMax.Create<byte>();
                wavePacketDescriptorIndex.Update(waveformPointDataRecord.WavePacketDescriptorIndex);
                byteOffsetToWaveformData ??= MinMax.Create<ulong>();
                byteOffsetToWaveformData.Update(waveformPointDataRecord.ByteOffsetToWaveformData);
                waveformPacketSizeInBytes ??= MinMax.Create<uint>();
                waveformPacketSizeInBytes.Update(waveformPointDataRecord.WaveformPacketSizeInBytes);
                returnPointWaveformLocation ??= MinMax.Create<float>();
                returnPointWaveformLocation.Update(waveformPointDataRecord.ReturnPointWaveformLocation);
                parametricDx ??= MinMax.Create<float>();
                parametricDx.Update(waveformPointDataRecord.ParametricDx);
                parametricDy ??= MinMax.Create<float>();
                parametricDy.Update(waveformPointDataRecord.ParametricDy);
                parametricDz ??= MinMax.Create<float>();
                parametricDz.Update(waveformPointDataRecord.ParametricDz);
            }
#endif

#if LAS1_4_OR_GREATER
            for (var i = 0; i < extraBytesRecord.Count; i++)
            {
                if (extraBytesRecord.GetValue(i, data) is { } value)
                {
                    extraBytes[i].Update(value);
                }
            }
#endif
        }

        return (
            x,
            y,
            z,
            intensity,
            returnNumber,
            numberOfReturns,
            edgeOfFlightLine,
            scanDirectionFlag,
            classification,
            scanAngleRank,
            userData,
            pointSourceId,
#if LAS1_4_OR_GREATER
            scanAngle,
#endif
            gps,
#if LAS1_3_OR_GREATER
            wavePacketDescriptorIndex,
            byteOffsetToWaveformData,
            waveformPacketSizeInBytes,
            returnPointWaveformLocation,
            parametricDx,
            parametricDy,
            parametricDz,
#endif
#if LAS1_4_OR_GREATER
            extraBytes,
#endif
            firstReturns,
            intermediateReturns,
            lastReturns,
            singleReturns,
            numberOfPointsByReturn,
            numberOfReturnsArray,
            histogram);
    }

    /// <summary>
    /// Returns a string that represents the <see cref="LasReader"/> by using the formatting conventions of a specified culture.
    /// </summary>
    /// <param name="lasReader">The LAS reader.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <returns>A result string formatted by using the conventions of <paramref name="formatProvider"/>.</returns>
    public static string ToString(this LasReader lasReader, IFormatProvider? formatProvider)
    {
        var stringBuilder = new System.Text.StringBuilder();
        Format(
            lasReader,
            formatProvider,
            (_, fp, format, args) => stringBuilder.AppendFormat(fp, format, args),
            (header, _) => stringBuilder.Append(header),
            (value, _) => stringBuilder.AppendLine(value));
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Formats the <see cref="LasReader"/> by using the formatting conventions of a specified culture.
    /// </summary>
    /// <param name="lasReader">The LAS reader.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <param name="format">The function to format the data.</param>
    /// <param name="append">The function to append the data.</param>
    /// <param name="newLine">The function to add a new line to the data.</param>
    public static void Format(
        this LasReader lasReader,
        IFormatProvider? formatProvider,
        AppendFormat format,
        Append append,
        AppendLine newLine)
    {
        formatProvider = new LasFormatProvider(formatProvider, lasReader);
        newLine("reporting all LAS header entries:", AnsiConsoleStyles.MajorHeader);
        foreach (var (header, value) in Information.GetInformation(lasReader))
        {
            if (header is null)
            {
                // just data
                format(default, formatProvider, "    {0}", value);
            }
            else if (value is string stringValue)
            {
                append($"{header,-27}", AnsiConsoleStyles.Header);
                format(default, formatProvider, " '{0}'", stringValue);
            }
            else
            {
                append($"{header,-27}", AnsiConsoleStyles.Header);
                format(default, formatProvider, " {0}", value);
            }

            newLine();
        }

        for (var i = 0; i < lasReader.VariableLengthRecords.Count; i++)
        {
            format(AnsiConsoleStyles.MajorHeader, formatProvider, "variable length header record {0} of {1}:", i + 1, lasReader.VariableLengthRecords.Count);
            newLine();
            foreach (var (header, value) in Information.GetInformation(lasReader.Header, lasReader.VariableLengthRecords[i]))
            {
                var actualValue = value switch
                {
                    GeoKeyEntry keyEntry when lasReader.TryGetAsciiValue(keyEntry, out var asciiValue) => asciiValue,
                    GeoKeyEntry keyEntry when lasReader.TryGetDoubleValue(keyEntry, out var doubleValue) => doubleValue,
                    _ => value,
                };

                if (header is null)
                {
                    // just data
                    format(
                        default,
                        formatProvider,
                        "    {0}",
                        actualValue);
                }
                else if (header is string { Length: 0 })
                {
                    // just information
                    format(
                        default,
                        formatProvider,
                        "  {0}",
                        actualValue);
                }
                else if (actualValue is string stringValue)
                {
                    append($"  {header,-20}", AnsiConsoleStyles.Header);
                    format(
                        default,
                        formatProvider,
                        " '{0}'",
                        stringValue);
                }
                else
                {
                    append($"  {header,-20}", AnsiConsoleStyles.Header);
                    format(
                        default,
                        formatProvider,
                        " {0}",
                        actualValue);
                }

                newLine();
            }
        }

#if LAS1_4_OR_GREATER
        for (var i = 0; i < lasReader.ExtendedVariableLengthRecords.Count; i++)
        {
            format(AnsiConsoleStyles.MajorHeader, formatProvider, "extended variable length header record {0} of {1}:", i + 1, lasReader.ExtendedVariableLengthRecords.Count);
            newLine();
            foreach (var (header, value) in Information.GetInformation(lasReader.ExtendedVariableLengthRecords[i]))
            {
                if (header is null)
                {
                    // just data
                    format(
                        default,
                        formatProvider,
                        "    {0}",
                        value);
                }
                else if (value is string stringValue)
                {
                    append($"  {header,-20}");
                    format(
                        default,
                        formatProvider,
                        " '{0}'",
                        stringValue);
                }
                else
                {
                    append($"  {header,-20}");
                    format(
                        default,
                        formatProvider,
                        " {0}",
                        value);
                }

                newLine();
            }
        }
#endif
    }

#if LAS1_4_OR_GREATER
    private readonly struct NullExtraBytes : IExtraBytes
    {
        int IReadOnlyCollection<ExtraBytesItem>.Count => 0;

        ExtraBytesItem IReadOnlyList<ExtraBytesItem>.this[int index] => throw new NotSupportedException();

        IEnumerator<ExtraBytesItem> IEnumerable<ExtraBytesItem>.GetEnumerator() => throw new NotSupportedException();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotSupportedException();

        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is NullExtraBytes;

        public override int GetHashCode() => default;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "Checked")]
        public override string ToString() => nameof(NullExtraBytes);

        public object? GetValue(int index, ReadOnlySpan<byte> source) => default;

        public IReadOnlyList<object?> GetValues(ReadOnlySpan<byte> source) => [];

        public ValueTask<object?> GetValueAsync(int index, ReadOnlyMemory<byte> source) => new(default(object));

        public ValueTask<IReadOnlyList<object?>> GetValuesAsync(ReadOnlyMemory<byte> source) => new([]);
    }
#endif
}