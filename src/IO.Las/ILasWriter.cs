// -----------------------------------------------------------------------
// <copyright file="ILasWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a LAS file writer.
/// </summary>
public interface ILasWriter
{
    /// <summary>
    /// Writes the header.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="records">The records.</param>
    void Write(in HeaderBlock header, params IEnumerable<VariableLengthRecord> records);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(PointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(GpsPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

#if LAS1_2_OR_GREATER
    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(ColorPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(GpsColorPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);
#endif

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(GpsWaveformPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(GpsColorWaveformPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(ExtendedGpsPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(ExtendedGpsColorPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(ExtendedGpsColorNearInfraredPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(ExtendedGpsWaveformPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(ExtendedGpsColorNearInfraredWaveformPointDataRecord record, ReadOnlySpan<byte> extraBytes = default);
#endif

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [System.Runtime.CompilerServices.OverloadResolutionPriority(-100)]
    void Write(IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Writes the extended variable length record.
    /// </summary>
    /// <param name="record">The extended variable length record value.</param>
    void Write(ExtendedVariableLengthRecord record);
#endif

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(PointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(GpsPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

#if LAS1_2_OR_GREATER
    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(ColorPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(GpsColorPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);
#endif

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(GpsWaveformPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(GpsColorWaveformPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(ExtendedGpsPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(ExtendedGpsColorPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(ExtendedGpsColorNearInfraredPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(ExtendedGpsWaveformPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(ExtendedGpsColorNearInfraredWaveformPointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);
#endif

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    [System.Runtime.CompilerServices.OverloadResolutionPriority(-100)]
    ValueTask WriteAsync(IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);
}