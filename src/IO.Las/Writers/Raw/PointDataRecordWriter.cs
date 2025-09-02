// -----------------------------------------------------------------------
// <copyright file="PointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Raw;

/// <summary>
/// The <see cref="PointDataRecordWriter{T}"/> for <see cref="IBasePointDataRecord"/> instances.
/// </summary>
internal sealed class PointDataRecordWriter : IPointDataRecordWriter
{
    /// <inheritdoc />
    public int Write(Span<byte> destination, IBasePointDataRecord record) => record.Write(destination);

    /// <inheritdoc />
    public ValueTask<int> WriteAsync(Memory<byte> destination, IBasePointDataRecord record, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(record.Write(destination.Span));
    }
}