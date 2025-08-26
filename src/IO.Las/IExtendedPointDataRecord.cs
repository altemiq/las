// -----------------------------------------------------------------------
// <copyright file="IExtendedPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// An extended <see cref="IBasePointDataRecord"/>.
/// </summary>
public interface IExtendedPointDataRecord : IGpsPointDataRecord
{
    /// <summary>
    /// Gets a value indicating whether this point is within the overlap region of two or more swath or takes.
    /// </summary>
    /// <remarks>If set, this point is within the overlap region of two or more swaths or takes.
    /// Setting this bit is not mandatory(unless, of course, it is mandated by a particular delivery specification) but allows Classification of overlap points to be preserved.</remarks>
    bool Overlap { get; init; }

    /// <summary>
    /// Gets the scanner channel.
    /// </summary>
    /// <remarks>Scanner Channel is used to indicate the channel (scanner head) of a multichannel system. Channel 0 is used for single scanner systems. Up to four channels are supported(0 - 3).</remarks>
    byte ScannerChannel { get; init; }

    /// <summary>
    /// Gets the extended classification.
    /// </summary>
    /// <value>The standard ASPRS classification as defined in <see cref="ExtendedClassification"/>.</value>
    ExtendedClassification Classification { get; init; }

    /// <summary>
    /// Gets the scan angle.
    /// </summary>
    /// <remarks>The Scan Angle is a signed short that represents the rotational position of the emitted laser pulse with respect to the vertical dimension of the coordinate system of the data.
    /// Down in the data coordinate system is the 0.0 position. Each increment represents 0.006 degrees.
    /// Counter-clockwise rotation, as viewed from the rear of the sensor, facing in the along-track (positive trajectory) direction, is positive.
    /// The maximum value in the positive sense is 30,000 (180 degrees which is up in the coordinate system of the data).
    /// The maximum value in the negative direction is -30,000 which is also directly up. </remarks>
    short ScanAngle { get; init; }
}