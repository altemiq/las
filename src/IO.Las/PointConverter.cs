// -----------------------------------------------------------------------
// <copyright file="PointConverter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Conversion methods.
/// </summary>
internal static partial class PointConverter
{
#if LAS1_4_OR_GREATER
    /// <summary>
    /// Converts the <see cref="IBasePointDataRecord"/> to its equivalent <see cref="IExtendedPointDataRecord"/>.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The extended point.</returns>
    /// <remarks>If <paramref name="point"/> is a <see cref="IExtendedPointDataRecord"/> then it is returned.</remarks>
    public static IExtendedPointDataRecord ToExtended(IBasePointDataRecord point) =>
        point switch
        {
            PointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsPointDataRecord(),
            GpsPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsPointDataRecord(),
            ColorPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsColorPointDataRecord(),
            GpsColorPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsColorPointDataRecord(),
            GpsWaveformPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsWaveformPointDataRecord(),
            GpsColorWaveformPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsColorNearInfraredWaveformPointDataRecord(),
            IExtendedPointDataRecord pointDataRecord => pointDataRecord,
            _ => throw new InvalidCastException(),
        };

    /// <summary>
    /// Converts the <see cref="IBasePointDataRecord"/> to its equivalent <see cref="IPointDataRecord"/>.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The converted point.</returns>
    /// <remarks>If <paramref name="point"/> is a <see cref="IPointDataRecord"/> then it is returned.</remarks>
    public static IPointDataRecord ToSimple(IBasePointDataRecord point) =>
        point switch
        {
            ExtendedGpsPointDataRecord pointDataRecord => pointDataRecord.ToGpsPointDataRecord(),
            ExtendedGpsColorPointDataRecord pointDataRecord => pointDataRecord.ToGpsColorPointDataRecord(),
            ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord => pointDataRecord.ToGpsColorPointDataRecord(),
            ExtendedGpsWaveformPointDataRecord pointDataRecord => pointDataRecord.ToGpsWaveformPointDataRecord(),
            ExtendedGpsColorNearInfraredWaveformPointDataRecord pointDataRecord => pointDataRecord.ToGpsColorWaveformPointDataRecord(),
            IPointDataRecord pointDataRecord => pointDataRecord,
            _ => throw new InvalidCastException(),
        };

    private static short ScanAngleRankToScanAngle(sbyte scanAngleRank) => (short)Math.Round(scanAngleRank * 30000F / 180F, 0, MidpointRounding.AwayFromZero);

    private static sbyte ScanAngleToScanAngleRank(short scanAngle) => (sbyte)Math.Round(scanAngle * 180F / 30000F, 0, MidpointRounding.AwayFromZero);
#endif
}