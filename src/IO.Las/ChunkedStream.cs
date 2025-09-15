// -----------------------------------------------------------------------
// <copyright file="ChunkedStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// A chunked <see cref="Las"/> <see cref="Stream"/>.
/// </summary>
/// <param name="length">The length of the resource.</param>
public abstract class ChunkedStream(long length) : Stream, ICacheStream, IAsyncCacheStream
{
    private Stream? stream;

    private long streamStart;

    private long streamLength = -1;

    /// <inheritdoc/>
    public sealed override bool CanRead => this.stream?.CanRead ?? true;

    /// <inheritdoc/>
    public sealed override bool CanSeek => true;

    /// <inheritdoc/>
    public sealed override bool CanWrite => this.stream?.CanWrite ?? false;

    /// <inheritdoc/>
    public sealed override long Length { get; } = length;

    /// <inheritdoc/>
    public sealed override long Position
    {
        get => this.streamStart + (this.stream?.Position ?? 0);
        set => this.Seek(value, SeekOrigin.Begin);
    }

    /// <inheritdoc/>
    public sealed override void Flush() => this.stream?.Flush();

    /// <inheritdoc/>
    public sealed override int Read(byte[] buffer, int offset, int count)
    {
        if (this.stream is null && !this.CacheCore(this.streamStart, count))
        {
            return 0;
        }

        // bytes left
        var bytesLeft = (int)(this.streamLength - this.stream.Position);

        if (count <= bytesLeft)
        {
            return this.stream.Read(buffer, offset, count);
        }

        // copy in what we have
        var bytesRead = this.stream.Read(buffer, offset, bytesLeft);
        count -= bytesRead;
        offset += bytesRead;

        // get the rest of what we require
        return this.CacheCore(this.streamStart + this.streamLength, count)
            ? bytesRead + this.stream.Read(buffer, offset, count)
            : bytesRead;
    }

    /// <inheritdoc/>
    public sealed override long Seek(long offset, SeekOrigin origin)
    {
        if (this.stream is not null)
        {
            // see if this is in the current range
            if (origin is SeekOrigin.Begin)
            {
                if (offset >= this.streamStart && offset <= (this.streamStart + this.streamLength))
                {
                    // set the position within the current stream
                    var streamPosition = this.stream.Seek(offset - this.streamStart, SeekOrigin.Begin);
                    return this.streamStart + streamPosition;
                }

                this.DisposeStream();
                this.streamStart = offset;
                this.streamLength = -1;
                return this.streamStart;
            }

            if (origin is SeekOrigin.Current)
            {
                if (this.stream.Position + offset >= 0 && this.stream.Position + offset <= this.streamLength)
                {
                    // set the position within the current stream
                    var streamPosition = this.stream.Seek(offset - this.streamStart, SeekOrigin.Current);
                    return this.streamStart + streamPosition;
                }

                this.DisposeStream();
                this.streamStart += offset;
                this.streamLength = -1;
                return this.streamStart;
            }
        }

        return -1;
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public sealed override void SetLength(long value) => throw new NotSupportedException(string.Format(Properties.Resources.Culture, Properties.Resources.CannotSetOn, "length", "chunked stream"));

    /// <inheritdoc/>
    public sealed override void Write(byte[] buffer, int offset, int count) => this.stream?.Write(buffer, offset, count);

    /// <inheritdoc/>
    void ICacheStream.Cache(long start) => this.CacheCore(start, (int)(this.Length - start));

    /// <inheritdoc/>
    void ICacheStream.Cache(long start, int length) => this.CacheCore(start, length);

    /// <inheritdoc/>
    async ValueTask IAsyncCacheStream.CacheAsync(long start, CancellationToken cancellationToken) => await this.CacheAsyncCore(start, (int)(this.Length - start), cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    async ValueTask IAsyncCacheStream.CacheAsync(long start, int length, CancellationToken cancellationToken) => await this.CacheAsyncCore(start, length, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    protected sealed override void Dispose(bool disposing)
    {
        this.DisposeStream();
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="length">The number of bytes to prepare.</param>
    /// <returns>The stream.</returns>
    protected abstract Stream? GetStream(long start, int length);

    /// <summary>
    /// Gets the stream asynchronously.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="length">The number of bytes to prepare.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stream.</returns>
    protected abstract ValueTask<Stream?> GetStreamAsync(long start, int length, CancellationToken cancellationToken = default);

    private void DisposeStream()
    {
        if (this.stream is { } disposable)
        {
            disposable.Dispose();
        }

        this.stream = default;
    }

#if NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    private async ValueTask DisposeStreamAsync()
    {
        if (this.stream is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER
        else if (this.stream is { } disposable)
        {
            disposable.Dispose();
        }
#endif

        this.stream = default;
    }
#endif

    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(stream))]
    private bool CacheCore(long start, int length)
    {
        if (this.stream is not null && this.streamStart <= start && (this.streamStart + this.streamLength) >= (start + length))
        {
            // the current stream covers the required range
            return true;
        }

        this.DisposeStream();

        if ((this.stream = this.GetStream(start, length)) is not null)
        {
            this.streamStart = start;
            this.streamLength = this.stream.Length;
            return true;
        }

        this.streamStart = this.streamLength = -1;
        return false;
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(stream))]
    private async ValueTask<bool> CacheAsyncCore(long start, int length, CancellationToken cancellationToken = default)
    {
        if (this.stream is not null && this.streamStart <= start && (this.streamStart + this.streamLength) >= (start + length))
        {
            // the current stream covers the required range
            return true;
        }

#if NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await this.DisposeStreamAsync().ConfigureAwait(false);
#else
        this.DisposeStream();
#endif

        if ((this.stream = await this.GetStreamAsync(start, length, cancellationToken).ConfigureAwait(false)) is not null)
        {
            this.streamStart = start;
            this.streamLength = this.stream.Length;
            return true;
        }

        this.streamStart = this.streamLength = -1;
        return false;
    }
}