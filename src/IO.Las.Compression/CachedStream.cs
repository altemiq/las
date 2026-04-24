// -----------------------------------------------------------------------
// <copyright file="CachedStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// A cached <see cref="Stream"/>.
/// </summary>
/// <param name="baseStream">The base stream.</param>
/// <param name="bufferSize">The buffer size.</param>
public sealed class CachedStream(Stream baseStream, int bufferSize = ushort.MaxValue + 1) : Stream, ICacheStream, IAsyncCacheStream
{
    private byte[] internalBuffer = new byte[bufferSize];

    private long bufferStart;

    private int bufferPosition;

    private int bufferLength;

    /// <inheritdoc />
    public override bool CanRead => baseStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => baseStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => baseStream.CanWrite;

    /// <inheritdoc />
    public override long Length => baseStream.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => this.bufferStart + this.bufferPosition;
        set => this.Seek(value, SeekOrigin.Begin);
    }

    /// <summary>
    /// Tries to read the span of the required length.
    /// </summary>
    /// <param name="length">The required length.</param>
    /// <param name="output">The output span.</param>
    /// <returns><see langword="true"/> if the span was read correctly; otherwise <see langword="false"/>.</returns>
    /// <remarks>When the return if <see langword="true"/> the stream has been moved forward by <paramref name="length"/>.</remarks>
    public bool TryGetSpan(int length, out ReadOnlySpan<byte> output)
    {
        if (length <= this.bufferLength - this.bufferPosition)
        {
            output = this.internalBuffer.AsSpan(this.bufferPosition, length);
            this.bufferPosition += length;
            return true;
        }

        output = default;
        return false;
    }

    /// <inheritdoc/>
    public override void Flush() => baseStream.Flush();

    /// <inheritdoc/>
    public override int ReadByte()
    {
        if (this.bufferPosition != this.bufferLength)
        {
            return this.internalBuffer[this.bufferPosition++];
        }

        this.Cache(baseStream.Position);
        if (this.bufferLength == 0)
        {
            return -1;
        }

        return this.internalBuffer[this.bufferPosition++];
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var totalRead = 0;
        while (totalRead < count)
        {
            if (this.bufferPosition == this.bufferLength)
            {
                this.Cache(baseStream.Position);
                if (this.bufferLength == 0)
                {
                    break;
                }
            }

            var length = Math.Min(this.bufferLength - this.bufferPosition, count - totalRead);

            this.internalBuffer.AsSpan(this.bufferPosition, length).CopyTo(buffer.AsSpan(offset + totalRead));
            this.bufferPosition += length;
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
            if (this.bufferPosition == this.bufferLength)
            {
                this.Cache(baseStream.Position);
                if (this.bufferLength == 0)
                {
                    break;
                }
            }

            var length = Math.Min(this.bufferLength - this.bufferPosition, buffer.Length - totalRead);

            this.internalBuffer.AsSpan(this.bufferPosition, length).CopyTo(buffer[totalRead..]);
            this.bufferPosition += length;
            totalRead += length;
        }

        return totalRead;
    }
#endif

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => await this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var totalRead = 0;
        while (totalRead < buffer.Length)
        {
            if (this.bufferPosition == this.bufferLength)
            {
                await this.CacheAsync(baseStream.Position, cancellationToken).ConfigureAwait(false);
                if (this.bufferLength == 0)
                {
                    break;
                }
            }

            var length = Math.Min(this.bufferLength - this.bufferPosition, buffer.Length - totalRead);

            this.internalBuffer.AsSpan(this.bufferPosition, length).CopyTo(buffer.Span[totalRead..]);
            this.bufferPosition += length;
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
            SeekOrigin.Current => this.bufferStart + this.bufferPosition + offset,
            SeekOrigin.End => this.Length - offset,
            _ => throw new System.Diagnostics.UnreachableException(),
        };

        // if this is within the current buffer, then just adjust
        if (position >= this.bufferStart && position <= this.bufferStart + this.bufferLength)
        {
            this.bufferPosition = (int)(position - this.bufferStart);
            return this.bufferStart + this.bufferPosition;
        }

        // set the base position and reset the cache
        this.bufferPosition = 0;
        this.bufferLength = 0;
        return this.bufferStart = baseStream.Seek(position, SeekOrigin.Begin);
    }

    /// <inheritdoc/>
    public override void SetLength(long value) => baseStream.SetLength(value);

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => baseStream.Write(buffer, offset, count);

    /// <inheritdoc/>
    public void Cache(long start) => this.Cache(start, this.internalBuffer.Length);

    /// <inheritdoc/>
    public void Cache(long start, int length)
    {
        // check to see if the current buffer covers this
        if (this.Position > start && this.Position < start + length)
        {
            return;
        }

        baseStream.Seek(start, SeekOrigin.Begin);
        this.bufferStart = baseStream.Position;
        this.bufferPosition = 0;
        this.Resize(length);
        this.bufferLength = baseStream.Read(this.internalBuffer, 0, length);
    }

    /// <inheritdoc/>
    public ValueTask CacheAsync(long start, CancellationToken cancellationToken = default) => this.CacheAsync(start, this.internalBuffer.Length, cancellationToken);

    /// <inheritdoc/>
    public async ValueTask CacheAsync(long start, int length, CancellationToken cancellationToken = default)
    {
        baseStream.Seek(start, SeekOrigin.Begin);
        this.bufferStart = baseStream.Position;
        this.bufferPosition = 0;
        this.Resize(length);
        this.bufferLength = await baseStream
            .ReadAsync(this.internalBuffer.AsMemory(0, length), cancellationToken)
            .ConfigureAwait(false);
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    /// <inheritdoc />
    public async override ValueTask DisposeAsync()
    {
        await baseStream.DisposeAsync().ConfigureAwait(false);
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
        baseStream.Dispose();
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
            if (n is 0)
            {
                // By definition, next power of 2 for 0 is 1
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
            n++;
            return n;
        }
    }
}