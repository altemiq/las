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
    int IPointDataRecordWriter.Write(Span<byte> destination, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes) => record is T t ? this.Write(destination, t, extraBytes) : throw new InvalidOperationException();

    /// <inheritdoc/>
    ValueTask<int> IPointDataRecordWriter.WriteAsync(Memory<byte> destination, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken) => record is T t ? this.WriteAsync(destination, t, extraBytes, cancellationToken) : throw new InvalidOperationException();

    /// <inheritdoc cref="IPointDataRecordWriter.Write"/>
    public abstract int Write(Span<byte> destination, T record, ReadOnlySpan<byte> extraBytes);

    /// <inheritdoc cref="IPointDataRecordWriter.WriteAsync"/>
    public abstract ValueTask<int> WriteAsync(Memory<byte> destination, T record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default);
}