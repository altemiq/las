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
    /// <returns>The number of bytes written.</returns>
    public abstract int Write(Span<byte> destination, IBasePointDataRecord record);

    /// <summary>
    /// Writes the point data record asynchronously.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="record">The record.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of bytes written.</returns>
    public abstract ValueTask<int> WriteAsync(Memory<byte> destination, IBasePointDataRecord record, CancellationToken cancellationToken = default);
}