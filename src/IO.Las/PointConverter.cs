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
    private static short ScanAngleRankToScanAngle(sbyte scanAngleRank) => (short)Math.Round(scanAngleRank * 30000F / 180F, 0, MidpointRounding.AwayFromZero);

    private static sbyte ScanAngleToScanAngleRank(short scanAngle) => (sbyte)Math.Round(scanAngle * 180F / 30000F, 0, MidpointRounding.AwayFromZero);
}