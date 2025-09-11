// -----------------------------------------------------------------------
// <copyright file="IBasePointDataRecord{TSelf}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The common interface for point data records.
/// </summary>
/// <typeparam name="TSelf">The type of point.</typeparam>
public interface IBasePointDataRecord<out TSelf> : IBasePointDataRecord
    where TSelf : IBasePointDataRecord<TSelf>
{
    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(PointDataRecord pointDataRecord);

    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(GpsPointDataRecord pointDataRecord);

#if LAS1_2_OR_GREATER
    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(ColorPointDataRecord pointDataRecord);

    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="GpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(GpsColorPointDataRecord pointDataRecord);
#endif

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(GpsWaveformPointDataRecord pointDataRecord);

    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="GpsColorWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(GpsColorWaveformPointDataRecord pointDataRecord);
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="ExtendedGpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(ExtendedGpsPointDataRecord pointDataRecord);

    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="ExtendedGpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(ExtendedGpsColorPointDataRecord pointDataRecord);

    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord);

    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="ExtendedGpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(ExtendedGpsWaveformPointDataRecord pointDataRecord);

    /// <summary>
    /// Creates a <typeparamref name="TSelf"/> from a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf FromPointDataRecord(ExtendedGpsColorNearInfraredWaveformPointDataRecord pointDataRecord);
#endif

    /// <summary>
    /// Creates a new instance of <typeparamref name="TSelf"/> from the data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The instance of <typeparamref name="TSelf"/>.</returns>
    static abstract TSelf Create(ReadOnlySpan<byte> data);

    /// <summary>
    /// Clones this instance to a new instance.
    /// </summary>
    /// <returns>The new instance.</returns>
    new TSelf Clone();
}