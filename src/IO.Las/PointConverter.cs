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
    /// Converts the <see cref="IPointDataRecord"/> to its equivalent <see cref="IExtendedPointDataRecord"/>.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The extended point.</returns>
    public static IExtendedPointDataRecord ToExtended(IPointDataRecord point) =>
        point switch
        {
            PointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsPointDataRecord(),
            GpsPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsPointDataRecord(),
            GpsColorPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsColorPointDataRecord(),
            GpsWaveformPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsWaveformPointDataRecord(),
            GpsColorWaveformPointDataRecord pointDataRecord => pointDataRecord.ToExtendedGpsColorNearInfraredWaveformPointDataRecord(),
            _ => throw new InvalidCastException(),
        };

    private static short ScanAngleRankToScanAngle(sbyte scanAngleRank) => (short)Math.Round(scanAngleRank * 30000F / 180F, 0, MidpointRounding.AwayFromZero);

    private static sbyte ScanAngleToScanAngleRank(short scanAngle) => (sbyte)Math.Round(scanAngle * 180F / 30000F, 0, MidpointRounding.AwayFromZero);
#endif
}