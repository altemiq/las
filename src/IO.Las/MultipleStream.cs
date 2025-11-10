// -----------------------------------------------------------------------
// <copyright file="MultipleStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// A stream that is backed by multiple <see cref="Stream"/> instances.
/// </summary>
/// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> to use to back this instance.</param>
public abstract class MultipleStream(IDictionary<string, Stream> dictionary) : Stream
{
    private string currentName = string.Empty;

    private Stream currentStream = Null;

    private long currentOffset;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleStream"/> class.
    /// </summary>
    protected MultipleStream()
        : this(default(StringComparer))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleStream"/> class.
    /// </summary>
    /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or <see langword="null"/> to use <see cref="StringComparer.Ordinal"/>.</param>
    protected MultipleStream(IEqualityComparer<string>? comparer)
        : this(new Dictionary<string, Stream>(comparer ?? StringComparer.Ordinal))
    {
    }

    /// <inheritdoc/>
    public override bool CanRead => this.currentStream.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => this.currentStream.CanSeek;

    /// <inheritdoc/>
    public override bool CanTimeout => this.currentStream.CanTimeout;

    /// <inheritdoc/>
    public override bool CanWrite => this.currentStream.CanWrite;

    /// <inheritdoc/>
    public override long Length => dictionary.Sum(static kvp => kvp.Value.Length);

    /// <inheritdoc/>
    public override int ReadTimeout { get => this.currentStream.ReadTimeout; set => this.currentStream.ReadTimeout = value; }

    /// <inheritdoc/>
    public override int WriteTimeout { get => this.currentStream.WriteTimeout; set => this.currentStream.WriteTimeout = value; }

    /// <inheritdoc/>
    public override long Position
    {
        get => this.currentOffset + this.currentStream.Position;
        set => this.SetPosition(value);
    }

#if NETSTANDARD2_0_OR_GREATER || NET20_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    /// <inheritdoc/>
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => this.currentStream.BeginRead(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public override int EndRead(IAsyncResult asyncResult) => this.currentStream.EndRead(asyncResult);

    /// <inheritdoc/>
    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => this.currentStream.BeginWrite(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public override void EndWrite(IAsyncResult asyncResult) => this.currentStream.EndWrite(asyncResult);

    /// <inheritdoc/>
    public override void Close()
    {
        foreach (var stream in dictionary.Values)
        {
            stream.Flush();
            stream.Close();
        }

        this.currentStream = Null;
        base.Close();
    }
#endif

    /// <inheritdoc/>
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        // copy from the current stream to the end.
        using var enumerator = dictionary.GetEnumerator();
        var streamOffset = 0L;
        while (enumerator.MoveNext() && enumerator.Current.Value != this.currentStream)
        {
            streamOffset += enumerator.Current.Value.Length;
        }

        do
        {
            var kvp = enumerator.Current;
            this.SetCurrent(kvp.Key, kvp.Value, streamOffset);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
            // ignore the buffer size here, and use the best size for each stream
            await this.currentStream.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
#else
            await GetTask(this.currentStream, destination, cancellationToken).ConfigureAwait(false);
#endif
            streamOffset += kvp.Value.Length;
        }
        while (enumerator.MoveNext());

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP
        static Task GetTask(Stream source, Stream destination, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
#if NETSTANDARD1_3_OR_GREATER || NET46_OR_GREATER
                return Task.FromCanceled(cancellationToken);
#else
                return Task.Run(static () => { }, cancellationToken);
#endif
            }

            return source.CopyToAsync(destination);
        }
#endif
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        if (dictionary.Values is not IEnumerable<Stream> enumerable)
        {
            return;
        }

        // flush all the buffers for this
        foreach (var stream in enumerable)
        {
            stream.Flush();
        }
    }

    /// <inheritdoc/>
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        // flush all the buffers for this
        if (dictionary.Values is IEnumerable<Stream> enumerable)
        {
            foreach (var stream in enumerable)
            {
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var totalDataRead = 0;
        var dataToRead = count;

        while (true)
        {
            // see how much is left
            var dataLeft = this.currentStream.Length - this.currentStream.Position;

            if (dataLeft > 0)
            {
                var dataRead = this.currentStream.Read(buffer, offset + totalDataRead, (int)Math.Min(dataLeft, dataToRead));
                dataToRead -= dataRead;
                totalDataRead += dataRead;

                if (dataToRead <= 0)
                {
                    return totalDataRead;
                }
            }

            if (!this.TryMoveToStartOfNext())
            {
                // we've got to the end
                return totalDataRead;
            }
        }
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        var totalDataRead = 0;
        var dataToRead = buffer.Length;

        while (true)
        {
            // see how much is left
            var dataLeft = this.currentStream.Length - this.currentStream.Position;

            if (dataLeft > 0)
            {
                var dataRead = this.currentStream.Read(buffer);
                dataToRead -= dataRead;
                totalDataRead += dataRead;

                if (dataToRead <= 0)
                {
                    return totalDataRead;
                }

                buffer = buffer[dataRead..];
            }

            if (!this.TryMoveToStartOfNext())
            {
                // we've got to the end
                return totalDataRead;
            }
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var totalDataRead = 0;
        var dataToRead = buffer.Length;

        while (true)
        {
            // see how much is left
            var dataLeft = this.currentStream.Length - this.currentStream.Position;

            if (dataLeft > 0)
            {
                var dataRead = await this.currentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                dataToRead -= dataRead;
                totalDataRead += dataRead;

                if (dataToRead <= 0)
                {
                    return totalDataRead;
                }

                buffer = buffer[dataRead..];
            }

            if (!this.TryMoveToStartOfNext())
            {
                // we've got to the end
                return totalDataRead;
            }
        }
    }
#endif

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var memory = new Memory<byte>(buffer, offset, count);
        return this.ReadAsync(memory, cancellationToken).AsTask();
    }
#else
    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var totalDataRead = 0;
        var dataToRead = count;

        while (true)
        {
            // see how much is left
            var dataLeft = this.currentStream.Length - this.currentStream.Position;

            if (dataLeft > 0)
            {
                var dataRead = await this.currentStream.ReadAsync(buffer, offset + totalDataRead, (int)Math.Min(dataLeft, dataToRead), cancellationToken).ConfigureAwait(false);

                dataToRead -= dataRead;
                totalDataRead += dataRead;

                if (dataToRead <= 0)
                {
                    return totalDataRead;
                }
            }

            if (!this.TryMoveToStartOfNext())
            {
                // we've got to the end
                return totalDataRead;
            }
        }
    }
#endif

    /// <inheritdoc/>
    public override int ReadByte() => this.currentStream.ReadByte();

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        var position = origin switch
        {
            SeekOrigin.Current => this.Position + offset,
            SeekOrigin.End => this.Length - offset,
            _ => offset,
        };

        this.SetPosition(position);
        return this.Position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value) => this.currentStream.SetLength(value);

    /// <inheritdoc/>
    public override string? ToString() => this.currentStream.ToString();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => this.currentStream.Write(buffer, offset, count);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer) => this.currentStream.Write(buffer);

    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => this.currentStream.WriteAsync(buffer, cancellationToken);
#endif

    /// <inheritdoc/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => this.currentStream.WriteAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc/>
    public override void WriteByte(byte value) => this.currentStream.WriteByte(value);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    /// <inheritdoc/>
    public override void CopyTo(Stream destination, int bufferSize)
    {
        using var enumerator = dictionary.GetEnumerator();
        var streamOffset = 0L;
        while (enumerator.MoveNext() && enumerator.Current.Value != this.currentStream)
        {
            streamOffset += enumerator.Current.Value.Length;
        }

        do
        {
            var kvp = enumerator.Current;

            // set the position back to zero to copy it all, if it is not the start stream
            if (kvp.Value != this.currentStream)
            {
                kvp.Value.Position = 0;
            }

            // set this as the current stream
            this.SetCurrent(kvp.Key, kvp.Value, streamOffset + kvp.Value.Position);

            // ignore the buffer size here, and use the best size for each stream
            this.currentStream.CopyTo(destination);

            // update the offset to the start of the next stream
            streamOffset += kvp.Value.Length;
        }
        while (enumerator.MoveNext());
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        foreach (var stream in dictionary.Values.OfType<IAsyncDisposable>())
        {
            await stream.DisposeAsync().ConfigureAwait(false);
        }

        await base.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
#endif

    /// <summary>
    /// Tries to add the specified name to this instance using the function to create the stream.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="func">The function to create the stream.</param>
    /// <returns><see langword="true" /> when the <paramref name="name"/> and result of <paramref name="func"/> are successfully added to this instance; <see langword="false" /> when this instance dictionary contains the specified <paramref name="name"/>, in which case nothing gets added.</returns>
    public bool TryAdd(string name, Func<Stream> func)
    {
        if (dictionary.ContainsKey(name))
        {
            return false;
        }

        dictionary.Add(name, func());
        return true;
    }

    /// <summary>
    /// Add the specified name and stream to this instance.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="stream">The stream.</param>
    public void Add(string name, Stream stream) => dictionary.Add(name, stream);

    /// <summary>
    /// Switches the current stream to using the specified name.
    /// </summary>
    /// <param name="name">The name of the stream.</param>
    /// <returns><see langword="true" /> when the current stream was successfully switched to the stream specified in <paramref name="name"/>; otherwise <see langword="false" />.</returns>
    public bool SwitchTo(string name)
    {
        if (string.Equals(this.currentName, name, StringComparison.Ordinal))
        {
            return false;
        }

        if (name.Length is 0)
        {
            this.SetCurrent(string.Empty, Null, 0);
            return true;
        }

        if (!dictionary.TryGetValue(name, out var stream))
        {
            stream = this.CreateStream(name);
            this.Add(name, stream);
        }

        if (this.currentStream == stream)
        {
            return false;
        }

        this.SetCurrent(name, stream, dictionary
            .TakeWhile(kvp => kvp.Value != stream)
            .Sum(kvp => kvp.Value.Length));

        return true;
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public void Reset()
    {
        foreach (var kvp in dictionary)
        {
            kvp.Value.Position = 0;
        }

        var first = dictionary.First();
        this.SetCurrent(first.Key, first.Value, 0);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var stream in dictionary.Values.OfType<IDisposable>())
            {
                stream.Dispose();
            }

            dictionary.Clear();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Creates a stream using the name.
    /// </summary>
    /// <param name="name">The name to use to create the stream.</param>
    /// <returns>The created stream.</returns>
    protected abstract Stream CreateStream(string name);

    private void SetCurrent(string name, Stream stream, long offset)
    {
        this.currentStream.Flush();
        this.currentName = name;
        this.currentStream = stream;
        this.currentOffset = offset;
    }

    private void SetPosition(long position)
    {
        var currentPosition = position - this.currentOffset;
        if (currentPosition >= 0 && currentPosition < this.currentStream.Length)
        {
            this.currentStream.Position = currentPosition;
            return;
        }

        var streamOffset = 0L;
        foreach (var kvp in dictionary)
        {
            currentPosition = position - streamOffset;
            var length = kvp.Value.Length;
            if (currentPosition < length)
            {
                this.SetCurrent(kvp.Key, kvp.Value, streamOffset);
                kvp.Value.Position = currentPosition;
                return;
            }

            streamOffset += length;
        }
    }

    private bool TryMoveToStartOfNext()
    {
        // get the current index
        var streamOffset = 0L;
        using var enumerator = dictionary.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var kvp = enumerator.Current;
            var length = kvp.Value.Length;
            streamOffset += length;

            if (enumerator.Current.Value != this.currentStream)
            {
                continue;
            }

            if (!enumerator.MoveNext())
            {
                return false;
            }

            enumerator.Current.Value.Position = 0;
            this.SetCurrent(enumerator.Current.Key, enumerator.Current.Value, streamOffset);
            return true;
        }

        return false;
    }
}