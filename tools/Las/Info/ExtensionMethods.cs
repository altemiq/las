// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1200
global using Statistics = (
    Altemiq.IO.Las.Info.IMinMax<int> X,
    Altemiq.IO.Las.Info.IMinMax<int> Y,
    Altemiq.IO.Las.Info.IMinMax<int> Z,
    Altemiq.IO.Las.Info.IMinMax<int> Intensity,
    Altemiq.IO.Las.Info.IMinMax<int> ReturnNumber,
    Altemiq.IO.Las.Info.IMinMax<int> NumberOfReturns,
    bool EdgeOfFlightLine,
    bool ScanDirectionFlag,
    Altemiq.IO.Las.Info.IMinMax<byte> Classification,
    Altemiq.IO.Las.Info.IMinMax<sbyte>? ScanAngleRank,
    Altemiq.IO.Las.Info.IMinMax<byte> UserData,
    Altemiq.IO.Las.Info.IMinMax<ushort> PointSourceId,
#if LAS1_4_OR_GREATER
    Altemiq.IO.Las.Info.IMinMax<short>? ScanAngle,
#endif
    Altemiq.IO.Las.Info.IMinMax<double>? Gps,
#if LAS1_3_OR_GREATER
    Altemiq.IO.Las.Info.IMinMax<byte>? WavePacketDescriptorIndex,
    Altemiq.IO.Las.Info.IMinMax<ulong>? ByteOffsetToWaveformData,
    Altemiq.IO.Las.Info.IMinMax<uint>? WaveformPacketSizeInBytes,
    Altemiq.IO.Las.Info.IMinMax<float>? ReturnPointWaveformLocation,
    Altemiq.IO.Las.Info.IMinMax<float>? ParametricDx,
    Altemiq.IO.Las.Info.IMinMax<float>? ParametricDy,
    Altemiq.IO.Las.Info.IMinMax<float>? ParametricDz,
#endif
#if LAS1_4_OR_GREATER
    System.Collections.Generic.IEnumerable<Altemiq.IO.Las.Info.IMinMax> ExtraBytes,
#endif
    int FirstReturns,
    int IntermediateReturns,
    int LastReturns,
    int SingleReturns,
    long[] OverviewReturnNumber,
    long[] OverviewNumberOfReturns,
    int[] Histogram);
#pragma warning restore SA1200

namespace Altemiq.IO.Las.Info;

/// <summary>
/// Extension methods.
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Returns a string that represents the <see cref="LasReader"/> by using the formatting conventions of a specified culture.
    /// </summary>
    /// <param name="lasReader">The LAS reader.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <returns>A result string formatted by using the conventions of <paramref name="formatProvider"/>.</returns>
    public static string ToString(this LasReader lasReader, IFormatProvider? formatProvider)
    {
        var formatter = new StringBuilderLasReaderFormatter(formatProvider);
        formatter.Format(lasReader, noMinMax: true, noReturns: true);
        return formatter.ToString();
    }

    /// <summary>
    /// Formats the <see cref="LasReader"/> by using the formatting conventions of a specified formatter.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="lasReader">The LAS reader.</param>
    /// <param name="noMinMax">Set to <see langword="true"/> to not report the min/max statistics.</param>
    /// <param name="noReturns">Set to <see langword="true"/> to not report the return statistics.</param>
    /// <param name="boundingBox">The optional bounding box.</param>
    public static void Format(this ILasReaderFormatter builder, LasReader lasReader, bool noMinMax, bool noReturns, BoundingBox? boundingBox = default)
    {
        _ = builder
            .AppendHeader(lasReader)
            .AppendVariableLengthRecords(lasReader)
            .AppendExtendedVariableLengthRecords(lasReader);

        Statistics? statistics = default;
        if (!noMinMax)
        {
            _ = builder.AppendStatistics(lasReader, GetStatisticsValue);
        }

        if (!noReturns)
        {
            _ = builder.AppendReturns(lasReader, GetStatisticsValue);
        }

        if (!noMinMax)
        {
            builder.AppendHistograms(lasReader, GetStatisticsValue);
        }

        Statistics GetStatisticsValue(LasReader reader)
        {
            return statistics ??= GetStatistics(reader, boundingBox);
        }
    }

    private static Statistics GetStatistics(LasReader reader, BoundingBox? box = default)
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

#if LAS1_4_OR_GREATER
        while (reader.ReadPointDataRecord() is { PointDataRecord: { } record, ExtraBytes: var data })
#else
        while (reader.ReadPointDataRecord() is { PointDataRecord: { } record })
#endif
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
            switch (record)
            {
#if LAS1_4_OR_GREATER
                case IExtendedPointDataRecord extendedPointDataRecord:
                {
                    scanAngle ??= MinMax.Create<short>();
                    var classificationValue = (byte)extendedPointDataRecord.Classification;
                    classification.Update(classificationValue);
                    scanAngle.Update(extendedPointDataRecord.ScanAngle);

                    histogram[classificationValue]++;
                    break;
                }
#endif
                case IPointDataRecord pointDataRecord:
                {
                    scanAngleRank ??= MinMax.Create<sbyte>();
                    var classificationValue = (byte)pointDataRecord.Classification;
                    classification.Update(classificationValue);
                    scanAngleRank.Update(pointDataRecord.ScanAngleRank);

                    histogram[classificationValue]++;
                    break;
                }
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

#if LAS1_4_OR_GREATER
    private readonly struct NullExtraBytes : IExtraBytes
    {
        int IReadOnlyCollection<ExtraBytesItem>.Count => 0;

        ExtraBytesItem IReadOnlyList<ExtraBytesItem>.this[int index] => throw new NotSupportedException();

        IEnumerator<ExtraBytesItem> IEnumerable<ExtraBytesItem>.GetEnumerator() => throw new NotSupportedException();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotSupportedException();

        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is NullExtraBytes;

        public override int GetHashCode() => default;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "False positive")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "Checked")]
        public override string ToString() => nameof(NullExtraBytes);

        public object? GetValue(int index, ReadOnlySpan<byte> source) => default;

        public IReadOnlyList<object?> GetValues(ReadOnlySpan<byte> source) => [];

        public ValueTask<object?> GetValueAsync(int index, ReadOnlyMemory<byte> source) => new(default(object));

        public ValueTask<IReadOnlyList<object?>> GetValuesAsync(ReadOnlyMemory<byte> source) => new([]);
    }
#endif
}