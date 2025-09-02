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
    void Write(IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes = default);

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Writes the extended variable length record.
    /// </summary>
    /// <param name="record">The extended variable length record value.</param>
    void Write(ExtendedVariableLengthRecord record);
#endif

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default);
}