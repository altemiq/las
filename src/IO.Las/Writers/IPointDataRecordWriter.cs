// -----------------------------------------------------------------------
// <copyright file="IPointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers;

/// <summary>
/// The <see cref="IBasePointDataRecord"/> writer.
/// </summary>
public interface IPointDataRecordWriter
{
    /// <summary>
    /// Writes the point data record.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> destination, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes);

    /// <summary>
    /// Writes the point data record asynchronously.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of bytes written.</returns>
    ValueTask<int> WriteAsync(Memory<byte> destination, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default);
}