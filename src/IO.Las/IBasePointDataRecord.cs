// -----------------------------------------------------------------------
// <copyright file="IBasePointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The common interface for point data records.
/// </summary>
public interface IBasePointDataRecord
{
#if NET7_0_OR_GREATER
    /// <summary>
    /// Gets the size of the point data record.
    /// </summary>
    static virtual ushort Size => default;

    /// <summary>
    /// Gets the ID of the point data record.
    /// </summary>
    static virtual byte Id => default;
#endif

    /// <summary>
    /// Gets the X-coordinate.
    /// </summary>
    int X { get; init; }

    /// <summary>
    /// Gets the Y-coordinate.
    /// </summary>
    int Y { get; init; }

    /// <summary>
    /// Gets the Z-coordinate.
    /// </summary>
    int Z { get; init; }

    /// <summary>
    /// Gets the intensity.
    /// </summary>
    ushort Intensity { get; init; }

    /// <summary>
    /// Gets the pulse return number for a given output pulse.
    /// </summary>
    /// <remarks>A given output laser pulse can have many returns, and they must be marked in sequence of return. The first return will have a Return Number of one, the second a Return Number of two, and so on up to five returns.</remarks>
    byte ReturnNumber { get; init; }

    /// <summary>
    /// Gets the number of returns for a given pulse.
    /// </summary>
    /// <remarks>A laser data point may be return two (<see cref="ReturnNumber"/>) with a total number of five returns.</remarks>
    byte NumberOfReturns { get; init; }

    /// <summary>
    /// Gets a value indicating whether the scan mirror travel direction is positive or negative at the time of the output pulse.
    /// </summary>
    /// <value><see langword="true"/> if this is a positive scan direction; otherwise, <see langword="false"/>.</value>
    bool ScanDirectionFlag { get; init; }

    /// <summary>
    /// Gets a value indicating whether this is at the edge of a flight line.
    /// </summary>
    /// <value><see langword="true"/> if the point is at the end of a scan. It is the last point on a given scan line before it changes direction; otherwise, <see langword="false"/>.</value>
    bool EdgeOfFlightLine { get; init; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="PointDataRecord"/> is synthetic.
    /// </summary>
    /// <value><see langword="true"/> if this point was created by a technique other than LIDAR collection such as digitized from a photogrammetric stereo model; otherwise, <see langword="false"/>.</value>
    bool Synthetic { get; init; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="PointDataRecord"/> is a key-point.
    /// </summary>
    /// <value><see langword="true"/> if this point is considered to be a model key-point and thus generally should not be withheld in a thinning algorithm; otherwise, <see langword="false"/>.</value>
    bool KeyPoint { get; init; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="PointDataRecord"/> is withheld.
    /// </summary>
    /// <value><see langword="true"/> if this point should not be included in processing (synonymous with Deleted); otherwise, <see langword="false"/>.</value>
    bool Withheld { get; init; }

    /// <summary>
    /// Gets the user data.
    /// </summary>
    byte UserData { get; init; }

    /// <summary>
    /// Gets the point source id.
    /// </summary>
    ushort PointSourceId { get; init; }

    /// <summary>
    /// Clones this instance to a new instance.
    /// </summary>
    /// <returns>The new instance.</returns>
    IBasePointDataRecord Clone();

    /// <summary>
    /// Copies the contents of this instance into a destination <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    /// <returns>The number of bytes written.</returns>
    int CopyTo(Span<byte> destination);

    /// <summary>
    /// Converts this instance to a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    PointDataRecord ToPointDataRecord();

    /// <summary>
    /// Converts this instance to a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    GpsPointDataRecord ToGpsPointDataRecord();

#if LAS1_2_OR_GREATER
    /// <summary>
    /// Converts this instance to a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    ColorPointDataRecord ToColorPointDataRecord();

    /// <summary>
    /// Converts this instance to a <see cref="GpsColorPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
    GpsColorPointDataRecord ToGpsColorPointDataRecord();
#endif

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Converts this instance to a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    GpsWaveformPointDataRecord ToGpsWaveformPointDataRecord();

    /// <summary>
    /// Converts this instance to a <see cref="GpsColorWaveformPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="GpsColorWaveformPointDataRecord"/>.</returns>
    GpsColorWaveformPointDataRecord ToGpsColorWaveformPointDataRecord();
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Converts this instance to a <see cref="ExtendedGpsPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="ExtendedGpsPointDataRecord"/>.</returns>
    ExtendedGpsPointDataRecord ToExtendedGpsPointDataRecord();

    /// <summary>
    /// Converts this instance to a <see cref="ExtendedGpsColorPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="ExtendedGpsColorPointDataRecord"/>.</returns>
    ExtendedGpsColorPointDataRecord ToExtendedGpsColorPointDataRecord();

    /// <summary>
    /// Converts this instance to a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    ExtendedGpsColorNearInfraredPointDataRecord ToExtendedGpsColorNearInfraredPointDataRecord();

    /// <summary>
    /// Converts this instance to a <see cref="ExtendedGpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="ExtendedGpsWaveformPointDataRecord"/>.</returns>
    ExtendedGpsWaveformPointDataRecord ToExtendedGpsWaveformPointDataRecord();

    /// <summary>
    /// Converts this instance to a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.
    /// </summary>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.</returns>
    ExtendedGpsColorNearInfraredWaveformPointDataRecord ToExtendedGpsColorNearInfraredWaveformPointDataRecord();
#endif
}