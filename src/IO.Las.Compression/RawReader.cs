// -----------------------------------------------------------------------
// <copyright file="RawReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The uncompressed point reader.
/// </summary>
/// <param name="reader">The point reader.</param>
/// <param name="pointStart">The point start.</param>
internal class RawReader(Readers.IPointDataRecordReader reader, long pointStart) : IPointReader
{
    private byte[] buffer = [];

    /// <summary>
    /// Gets the point start.
    /// </summary>
    protected long PointStart { get; } = pointStart;

    /// <inheritdoc/>
    public virtual LasPointSpan Read(Stream stream, int pointDataLength)
    {
        if (this.buffer.Length < pointDataLength)
        {
            this.buffer = new byte[pointDataLength];
        }

        var bytesRead = stream.Read(this.buffer, 0, pointDataLength);

        return reader.Read(this.buffer.AsSpan(0, bytesRead));
    }

    /// <inheritdoc/>
    public async virtual ValueTask<LasPointMemory> ReadAsync(Stream stream, int pointDataLength, CancellationToken cancellationToken = default)
    {
        if (this.buffer.Length < pointDataLength)
        {
            this.buffer = new byte[pointDataLength];
        }

        var bytesRead = await stream.ReadAsync(this.buffer, 0, pointDataLength, cancellationToken).ConfigureAwait(false);

        return await reader.ReadAsync(this.buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual void Initialize(Stream stream)
    {
    }

    /// <inheritdoc/>
    public virtual void Close(Stream stream)
    {
    }

    /// <inheritdoc/>
    public virtual bool MoveToPoint(Stream stream, int pointDataLength, ulong current, ulong target)
    {
        var delta = target - current;
        if (current > target)
        {
            // move to the point start
            stream.MoveToPositionAbsolute(this.PointStart);
            delta = target;
        }

        while (delta > 0)
        {
            _ = this.Read(stream, pointDataLength);
            delta--;
        }

        return true;
    }

    /// <inheritdoc/>
    public virtual async ValueTask<bool> MoveToPointAsync(Stream stream, int pointDataLength, ulong current, ulong target, CancellationToken cancellationToken = default)
    {
        var delta = target - current;
        if (current > target)
        {
            // move to the point start
            stream.MoveToPositionAbsolute(this.PointStart);
            delta = target;
        }

        while (delta > 0)
        {
            _ = await this.ReadAsync(stream, pointDataLength, cancellationToken).ConfigureAwait(false);
            delta--;
        }

        return true;
    }
}