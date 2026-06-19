// -----------------------------------------------------------------------
// <copyright file="CachedStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// A cached <see cref="Stream"/>.
/// </summary>
public sealed class CachedStream : Stream, ICacheStream, IAsyncCacheStream
{
    private readonly Stream baseStream;

    private readonly bool readable;

    private byte[] internalBuffer;

    private long bufferStart;

    private int bufferOffset;

    private int bufferIndex;

    private int bufferLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachedStream"/> class.
    /// </summary>
    /// <param name="baseStream">The base stream.</param>
    /// <param name="bufferSize">The buffer size.</param>
    public CachedStream(Stream baseStream, int bufferSize = ushort.MaxValue + 1)
    {
        this.baseStream = baseStream;
        if (baseStream is MemoryStream memoryStream
            && memoryStream.TryGetBuffer(out var buffer)
            && buffer.Array is { } byteArray)
        {
            this.internalBuffer = byteArray;
            this.bufferStart = -buffer.Offset;
            this.bufferOffset = buffer.Offset;
            this.bufferLength = byteArray.Length;
            this.bufferIndex = (int)memoryStream.Position;
            this.readable = false;
            return;
        }

        this.internalBuffer = new byte[bufferSize];
        this.readable = true;
    }

    /// <inheritdoc />
    public override bool CanRead => this.baseStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => this.baseStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => this.baseStream.CanWrite;

    /// <inheritdoc />
    public override long Length => this.baseStream.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => this.bufferStart + this.BufferPosition;
        set => this.Seek(value, SeekOrigin.Begin);
    }

    private int BufferPosition => this.bufferOffset + this.bufferIndex;

    /// <summary>
    /// Tries to read the span of the required length.
    /// </summary>
    /// <param name="length">The required length.</param>
    /// <param name="output">The output span.</param>
    /// <returns><see langword="true"/> if the span was read correctly; otherwise <see langword="false"/>.</returns>
    /// <remarks>When the return if <see langword="true"/> the stream has been moved forward by <paramref name="length"/>.</remarks>
    public bool TryGetSpan(int length, out ReadOnlySpan<byte> output)
    {
        if (length <= this.bufferLength - this.bufferIndex)
        {
            output = this.internalBuffer.AsSpan(this.BufferPosition, length);
            this.bufferIndex += length;
            return true;
        }

        output = default;
        return false;
    }

    /// <inheritdoc/>
    public override void Flush() => this.baseStream.Flush();

    /// <inheritdoc/>
    public override int ReadByte()
    {
        if (this.bufferIndex != this.bufferLength)
        {
            return this.internalBuffer[this.bufferOffset + this.bufferIndex++];
        }

        this.Cache(this.baseStream.Position);
        return this.bufferLength is not 0
            ? this.internalBuffer[this.bufferOffset + this.bufferIndex++]
            : -1;
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var totalRead = 0;
        while (totalRead < count)
        {
            if (this.bufferIndex == this.bufferLength)
            {
                this.Cache(this.baseStream.Position);
                if (this.bufferLength is 0)
                {
                    break;
                }
            }

            var length = Math.Min(this.bufferLength - this.bufferIndex, count - totalRead);
            this.internalBuffer.AsSpan(this.BufferPosition, length).CopyTo(buffer.AsSpan(offset + totalRead));
            this.bufferIndex += length;
            totalRead += length;
        }

        return totalRead;
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            if (this.bufferIndex == this.bufferLength)
            {
                this.Cache(this.baseStream.Position);
                if (this.bufferLength is 0)
                {
                    break;
                }
            }

            var length = Math.Min(this.bufferLength - this.bufferIndex, buffer.Length - totalRead);
            this.internalBuffer.AsSpan(this.BufferPosition, length).CopyTo(buffer[totalRead..]);
            this.bufferIndex += length;
            totalRead += length;
        }

        return totalRead;
    }
#endif

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var totalRead = 0;
        while (totalRead < count)
        {
            if (this.bufferIndex == this.bufferLength)
            {
                await this.CacheAsync(this.baseStream.Position, cancellationToken).ConfigureAwait(false);
                if (this.bufferLength is 0)
                {
                    break;
                }
            }

            var length = Math.Min(this.bufferLength - this.bufferIndex, buffer.Length - totalRead);
            this.internalBuffer.AsSpan(this.BufferPosition, length).CopyTo(buffer.AsSpan(offset + totalRead));
            this.bufferIndex += length;
            totalRead += length;
        }

        return totalRead;
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            if (this.bufferIndex == this.bufferLength)
            {
                await this.CacheAsync(this.baseStream.Position, cancellationToken).ConfigureAwait(false);
                if (this.bufferLength is 0)
                {
                    break;
                }
            }

            var length = Math.Min(this.bufferLength - this.bufferIndex, buffer.Length - totalRead);
            this.internalBuffer.AsSpan(this.BufferPosition, length).CopyTo(buffer.Span[totalRead..]);
            this.bufferIndex += length;
            totalRead += length;
        }

        return totalRead;
    }
#endif

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        // adjust the offset to be a position
        var position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => this.Position + offset,
            SeekOrigin.End => this.Length - offset,
            _ => throw new System.Diagnostics.UnreachableException(),
        };

        // if this is within the current buffer, then just adjust
        if (position >= this.bufferStart + this.bufferOffset && position <= this.bufferStart + this.bufferOffset + this.bufferLength)
        {
            this.bufferIndex = (int)(position - this.bufferStart - this.bufferOffset);
            return this.Position;
        }

        // set the base position and reset the cache
        this.bufferIndex = 0;
        this.bufferLength = 0;
        this.bufferOffset = 0;
        return this.bufferStart = this.baseStream.Seek(position, SeekOrigin.Begin);
    }

    /// <inheritdoc/>
    public override void SetLength(long value) => this.baseStream.SetLength(value);

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => this.baseStream.Write(buffer, offset, count);

    /// <inheritdoc cref="ICacheStream.Cache(long)"/>
    public void Cache(long start) => this.Cache(start, this.internalBuffer.Length);

    /// <inheritdoc cref="ICacheStream.Cache(long,int)"/>
    public void Cache(long start, int length)
    {
        // check to see we should read or if the current buffer covers this
        if (!this.readable || (this.Position >= start && this.bufferStart + this.bufferOffset + this.bufferLength >= start + length))
        {
            return;
        }

        this.baseStream.Seek(start, SeekOrigin.Begin);
        this.bufferStart = this.baseStream.Position;
        this.bufferOffset = 0;
        this.bufferIndex = 0;
        this.Resize(length);
        this.bufferLength = this.baseStream.Read(this.internalBuffer, this.bufferOffset, length);
    }

    /// <inheritdoc/>
    public ValueTask CacheAsync(long start, CancellationToken cancellationToken = default) => this.CacheAsync(start, this.internalBuffer.Length, cancellationToken);

    /// <inheritdoc/>
    public async ValueTask CacheAsync(long start, int length, CancellationToken cancellationToken = default)
    {
        // check to see we should read or if the current buffer covers this
        if (!this.readable || (this.Position >= start && this.bufferStart + this.bufferOffset + this.bufferLength >= start + length))
        {
            return;
        }

        this.baseStream.Seek(start, SeekOrigin.Begin);
        this.bufferStart = this.baseStream.Position;
        this.bufferOffset = 0;
        this.bufferIndex = 0;
        this.Resize(length);
        this.bufferLength = await this.baseStream
            .ReadAsync(this.internalBuffer.AsMemory(this.bufferOffset, length), cancellationToken)
            .ConfigureAwait(false);
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    /// <inheritdoc />
    public async override ValueTask DisposeAsync()
    {
        await this.baseStream.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
#endif

    /// <summary>
    /// Creates a cached stream if required.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns><paramref name="input"/> if it implements <see cref="ICacheStream"/>; otherwise a new instance of <see cref="CachedStream"/> that wraps <paramref name="input"/>.</returns>
    internal static Stream Create(Stream input) => input is ICacheStream ? input : new CachedStream(input);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        this.baseStream.Dispose();
        base.Dispose(disposing);
    }

    private void Resize(int minimumSize)
    {
        if (this.internalBuffer.Length >= minimumSize)
        {
            return;
        }

        Array.Resize(ref this.internalBuffer, GetNextPower(minimumSize));

        static int GetNextPower(int n)
        {
            if (n <= 1)
            {
                return 1;
            }

            // If already a power of 2, return itself
            if ((n & (n - 1)) is 0)
            {
                return n;
            }

            // Bit manipulation to round up to next power of 2
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }
    }
}