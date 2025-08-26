// -----------------------------------------------------------------------
// <copyright file="IPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The point data record.
/// </summary>
public interface IPointDataRecord : IBasePointDataRecord
{
    /// <summary>
    /// Gets the classification.
    /// </summary>
    /// <value>The standard ASPRS classification as defined in <see cref="Classification"/>.</value>
    Classification Classification { get; init; }

    /// <summary>
    /// Gets the scan angle rank.
    /// </summary>
    /// <remarks>The Scan Angle Rank is a signed one-byte number with a valid range from -90 to +90.
    /// The Scan Angle Rank is the angle (rounded to the nearest integer in the absolute value sense) at which the laser point was output from the laser system including the roll of the aircraft.
    /// The scan angle is within 1 degree of accuracy from +90 to –90 degrees.
    /// The scan angle is an angle based on 0 degrees being nadir, and –90 degrees to the left side of the aircraft in the direction of flight.</remarks>
    sbyte ScanAngleRank { get; init; }
}