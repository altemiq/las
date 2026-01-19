// -----------------------------------------------------------------------
// <copyright file="PointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Raw;

/// <summary>
/// The <see cref="IPointDataRecordWriter"/> for <see cref="IBasePointDataRecord"/> instances.
/// </summary>
internal sealed class PointDataRecordWriter : IPointDataRecordWriter
{
    /// <inheritdoc />
    public int Write(Span<byte> destination, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes)
    {
        var bytesWritten = record.CopyTo(destination);
        extraBytes.CopyTo(destination[bytesWritten..]);
        return bytesWritten + extraBytes.Length;
    }

    /// <inheritdoc />
    public ValueTask<int> WriteAsync(Memory<byte> destination, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytesWritten = record.CopyTo(destination.Span);
        extraBytes.CopyTo(destination[bytesWritten..]);
        return new(bytesWritten + extraBytes.Length);
    }
}