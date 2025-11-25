// -----------------------------------------------------------------------
// <copyright file="RawWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The uncompressed point writer.
/// </summary>
/// <param name="writer">The point writer.</param>
/// <param name="pointDataLength">The point data length.</param>
internal class RawWriter(Writers.IPointDataRecordWriter writer, int pointDataLength) : IPointWriter
{
    private readonly byte[] buffer = new byte[pointDataLength];

    /// <inheritdoc/>
    public virtual void Write(Stream stream, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes) => this.Write(stream, writer, record, extraBytes);

    /// <inheritdoc/>
    public virtual ValueTask WriteAsync(Stream stream, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default) => this.WriteAsync(stream, writer, record, extraBytes, cancellationToken);

    /// <inheritdoc/>
    public virtual void Initialize(Stream stream) => stream.SwitchStreamIfMultiple(LasStreams.PointData);

    /// <inheritdoc/>
    public virtual void Close(Stream stream)
    {
    }

    /// <summary>
    /// Writes the point to the stream using the writer.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="pointWriter">The point writer.</param>
    /// <param name="record">The record to write.</param>
    /// <param name="extraBytes">The extra bytes to write.</param>
    protected void Write(Stream stream, Writers.IPointDataRecordWriter pointWriter, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes)
    {
        var bytesWritten = pointWriter.Write(this.buffer, record, extraBytes);
        stream.Write(this.buffer, 0, bytesWritten);
    }

    /// <summary>
    /// Writes the point to the stream using the writer asynchronously.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="pointWriter">The point writer.</param>
    /// <param name="record">The record to write.</param>
    /// <param name="extraBytes">The extra bytes to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    protected async ValueTask WriteAsync(Stream stream, Writers.IPointDataRecordWriter pointWriter, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default)
    {
        var bytesWritten = pointWriter.Write(this.buffer, record, extraBytes.Span);
        await stream.WriteAsync(this.buffer.AsMemory(0, bytesWritten), cancellationToken).ConfigureAwait(false);
    }
}