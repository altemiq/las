// -----------------------------------------------------------------------
// <copyright file="Statistics.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The statistics.
/// </summary>
internal sealed record Statistics(
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
    IMinMax<byte>? ScannerChannel,
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
    int[] Histogram);