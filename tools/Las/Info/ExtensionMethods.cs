// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

using System.Runtime.Intrinsics;

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
            .AppendVariableLengthRecords(lasReader);

#if LAS1_4_OR_GREATER
        _ = builder.AppendExtendedVariableLengthRecords(lasReader);
#endif

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
        var values = MinMax.Create<Vector256<int>>();
        var edgeOfFlightLine = false;
        var scanDirectionFlag = false;
        var classification = MinMax.Create<byte>();
        IMinMax<sbyte>? scanAngleRank = default;
        var userData = MinMax.Create<byte>();
        var pointSourceId = MinMax.Create<ushort>();
#if LAS1_4_OR_GREATER
        IMinMax<short>? scanAngle = default;
        IMinMax<byte>? scannerChannel = default;
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
            filter = point => boundingBox.Contains(quantizer.Get(point));
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

            var returnNumberValue = record.ReturnNumber;
            numberOfPointsByReturn[returnNumberValue]++;
            var numberOfReturnsValue = record.NumberOfReturns;
            numberOfReturnsArray[numberOfReturnsValue]++;
            values.Update(Vector256.Create(record.X, record.Y, record.Z, record.Intensity, returnNumberValue, numberOfReturnsValue, default, default));
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

                        scannerChannel ??= MinMax.Create<byte>();
                        scanAngle.Update(extendedPointDataRecord.ScannerChannel);

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

        return new(
            values,
            edgeOfFlightLine,
            scanDirectionFlag,
            classification,
            scanAngleRank,
            userData,
            pointSourceId,
#if LAS1_4_OR_GREATER
            scanAngle,
            scannerChannel,
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
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)]
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

        public ExtraBytesValue GetValue(int index, ReadOnlySpan<byte> source) => default;

        public IReadOnlyList<ExtraBytesValue> GetValues(ReadOnlySpan<byte> source) => [];

        public ValueTask<ExtraBytesValue> GetValueAsync(int index, ReadOnlyMemory<byte> source) => default;

        public ValueTask<IReadOnlyList<ExtraBytesValue>> GetValuesAsync(ReadOnlyMemory<byte> source) => new([]);
    }
#endif
}