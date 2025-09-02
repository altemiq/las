// -----------------------------------------------------------------------
// <copyright file="PointDataRecordWriter{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers;

/// <summary>
/// The <see cref="IPointDataRecordWriter"/> for <typeparamref name="T"/> instances.
/// </summary>
/// <typeparam name="T">The typeof of <see cref="IBasePointDataRecord"/>.</typeparam>
internal abstract class PointDataRecordWriter<T> : IPointDataRecordWriter
    where T : IBasePointDataRecord
{
    /// <inheritdoc/>
    int IPointDataRecordWriter.Write(Span<byte> destination, IBasePointDataRecord record) => record is T t ? this.Write(destination, t) : throw new InvalidOperationException();

    /// <inheritdoc/>
    ValueTask<int> IPointDataRecordWriter.WriteAsync(Memory<byte> destination, IBasePointDataRecord record, CancellationToken cancellationToken) => record is T t ? this.WriteAsync(destination, t, cancellationToken) : throw new InvalidOperationException();

    /// <summary>
    /// Writes the point data record.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="record">The record.</param>
    /// <returns>The number of bytes written.</returns>
    public abstract int Write(Span<byte> destination, T record);

    /// <summary>
    /// Writes the point data record asynchronously.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="record">The record.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of bytes written.</returns>
    public abstract ValueTask<int> WriteAsync(Memory<byte> destination, T record, CancellationToken cancellationToken = default);
}