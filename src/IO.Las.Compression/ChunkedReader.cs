// -----------------------------------------------------------------------
// <copyright file="ChunkedReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The chunked <see cref="IPointReader"/>.
/// </summary>
internal abstract class ChunkedReader : IPointReader
{
    private const uint DefaultChunkCount = 256;

    private uint chunkSize;

    private uint chunkCount;

    private uint currentChunk;

    private uint numberChunksValue = uint.MaxValue;

    private uint tabledChunksValue;

    private long[] chunkStartValues = [];

    private long pointStart;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkedReader"/> class.
    /// </summary>
    /// <param name="reader">The chunk reader.</param>
    /// <param name="chunkSize">The chunk size.</param>
    protected ChunkedReader(ChunkReader reader, uint chunkSize)
    {
        this.Reader = reader;

        this.chunkSize = chunkSize is default(uint) ? uint.MaxValue : chunkSize;
        this.chunkCount = this.chunkSize;
    }

    /// <summary>
    /// Gets the point reader.
    /// </summary>
    protected ChunkReader Reader { get; }

    /// <inheritdoc/>
    public virtual void Close(Stream stream)
    {
    }

    /// <inheritdoc/>
    public virtual void Initialize(Stream stream)
    {
    }

    /// <inheritdoc/>
    public LasPointSpan Read(Stream stream)
    {
        if (this.chunkCount == this.chunkSize)
        {
            if (this.pointStart is not 0)
            {
                this.Reader.Decoder.Done();
                this.currentChunk++;

                // check integrity
                if (this.currentChunk < this.tabledChunksValue)
                {
                    var here = stream.Position;
                    if (this.chunkStartValues[this.currentChunk] != here)
                    {
                        // previous chunk was corrupt
                        this.currentChunk--;
                        throw new InvalidOperationException(Compression.Properties.Resources.PreviousChunkWasCorrupt);
                    }
                }
            }

            _ = stream.SwitchStreamIfMultiple(LazStreams.FormatChunk((int)this.currentChunk));
            _ = this.InitializeDecoder(stream);

            if (this.currentChunk == this.tabledChunksValue)
            {
                // no, or incomplete chunk table
                if (this.currentChunk >= this.numberChunksValue)
                {
                    this.numberChunksValue += DefaultChunkCount;
                    Array.Resize(ref this.chunkStartValues, (int)(this.numberChunksValue + 1));
                }

                this.chunkStartValues[this.tabledChunksValue] = this.pointStart; // needs fixing
                this.tabledChunksValue++;
            }

            // read the chunk here
            if (stream is ICacheStream cacheStream)
            {
                var chunkStart = this.chunkStartValues[this.currentChunk];
                var chunkLength = this.chunkStartValues[this.currentChunk + 1] - chunkStart;
                cacheStream.Cache(chunkStart, (int)chunkLength);
            }

            this.chunkCount = default;
        }

        this.chunkCount++;

        return this.Reader.Read(stream);
    }

    /// <inheritdoc/>
    public async ValueTask<LasPointMemory> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (this.chunkCount == this.chunkSize)
        {
            if (this.pointStart is not 0)
            {
                this.Reader.Decoder.Done();
                this.currentChunk++;

                // check integrity
                if (this.currentChunk < this.tabledChunksValue)
                {
                    var here = stream.Position;
                    if (this.chunkStartValues[this.currentChunk] != here)
                    {
                        // previous chunk was corrupt
                        this.currentChunk--;
                        throw new InvalidOperationException(Compression.Properties.Resources.PreviousChunkWasCorrupt);
                    }
                }
            }

            _ = stream.SwitchStreamIfMultiple(LazStreams.FormatChunk((int)this.currentChunk));
            _ = this.InitializeDecoder(stream);

            if (this.currentChunk == this.tabledChunksValue)
            {
                // no, or incomplete chunk table
                if (this.currentChunk >= this.numberChunksValue)
                {
                    this.numberChunksValue += DefaultChunkCount;
                    Array.Resize(ref this.chunkStartValues, (int)(this.numberChunksValue + 1));
                }

                this.chunkStartValues[this.tabledChunksValue] = this.pointStart; // needs fixing
                this.tabledChunksValue++;
            }

            switch (stream)
            {
                // read the chunk here
                case IAsyncCacheStream asyncCacheStream:
                    var asyncChunkStart = this.chunkStartValues[this.currentChunk];
                    var asyncChunkLength = this.chunkStartValues[this.currentChunk + 1] - asyncChunkStart;
                    await asyncCacheStream.CacheAsync(asyncChunkStart, (int)asyncChunkLength, cancellationToken).ConfigureAwait(false);
                    break;

                case ICacheStream cacheStream:
                    var chunkStart = this.chunkStartValues[this.currentChunk];
                    var chunkLength = this.chunkStartValues[this.currentChunk + 1] - chunkStart;
                    cacheStream.Cache(chunkStart, (int)chunkLength);
                    break;
            }

            this.chunkCount = default;
        }

        this.chunkCount++;

        return await this.Reader.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Moves to the specified chunk.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="current">The current point index.</param>
    /// <param name="index">The chunk index.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    public bool MoveToChunk(Stream stream, ulong current, int index)
    {
        if (index < 0)
        {
            return false;
        }

        if (this.pointStart is 0)
        {
            _ = this.InitializeDecoder(stream);
            this.chunkCount = default;
        }

        if (index > this.numberChunksValue)
        {
            return false;
        }

        _ = this.MoveToPoint(stream, current, this.GetFirstPoint(index));
        return true;
    }

    /// <summary>
    /// Moves to the specified chunk.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="current">The current point index.</param>
    /// <param name="index">The chunk index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    public async ValueTask<bool> MoveToChunkAsync(Stream stream, ulong current, int index, CancellationToken cancellationToken = default)
    {
        if (index < 0)
        {
            return false;
        }

        if (this.pointStart is 0)
        {
            _ = this.InitializeDecoder(stream);
            this.chunkCount = default;
        }

        if (index > this.numberChunksValue)
        {
            return false;
        }

        _ = await this.MoveToPointAsync(stream, current, this.GetFirstPoint(index), cancellationToken).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets the chunk index.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="chunkStart">The start position of the chunk.</param>
    /// <returns>The chunk index.</returns>
    public int GetChunkIndex(Stream stream, long chunkStart)
    {
        if (this.pointStart is 0)
        {
            _ = this.InitializeDecoder(stream);
            this.chunkCount = default;
        }

        // find the chunk in the chunk tables
        return SearchChunkTable(chunkStart, 0, (int)this.numberChunksValue);

        int SearchChunkTable(long index, int lower, int upper)
        {
            while (true)
            {
                if (lower + 1 == upper)
                {
                    return lower;
                }

                var mid = (lower + upper) / 2;
                if (index >= this.chunkStartValues[mid])
                {
                    lower = mid;
                    continue;
                }

                upper = mid;
            }
        }
    }

    /// <inheritdoc/>
    public bool MoveToPoint(Stream stream, ulong current, ulong target)
    {
        if (this.pointStart is 0)
        {
            _ = this.InitializeDecoder(stream);
            this.chunkCount = default;
        }

        var (targetChunk, delta) = this.GetDelta(target);
        this.chunkSize = this.GetChunkSize(targetChunk);

        if (targetChunk >= this.tabledChunksValue)
        {
            if (this.currentChunk < (this.tabledChunksValue - 1))
            {
                this.Reader.Decoder.Done();
                this.currentChunk = this.tabledChunksValue - 1;
                Prepare();
                stream.MoveToPositionAbsolute(this.chunkStartValues[this.currentChunk]);
                _ = this.InitializeDecoder(stream);
                this.chunkCount = default;
            }

            delta += (this.chunkSize * (targetChunk - this.currentChunk)) - this.chunkCount;
        }
        else if (this.currentChunk != targetChunk || current > target)
        {
            this.Reader.Decoder.Done();
            this.currentChunk = targetChunk;
            Prepare();
            stream.MoveToPositionAbsolute(this.chunkStartValues[this.currentChunk]);
            _ = this.InitializeDecoder(stream);
            this.chunkCount = default;
        }
        else
        {
            delta = (uint)(target - current);
        }

        while (delta > 0)
        {
            _ = this.Read(stream);
            delta--;
        }

        return true;

        void Prepare()
        {
            // read the chunk here
            if (stream is not ICacheStream cacheStream)
            {
                return;
            }

            var chunkStart = this.chunkStartValues[this.currentChunk];
            var chunkLength = this.chunkStartValues[this.currentChunk + 1] - chunkStart;
            cacheStream.Cache(chunkStart, (int)chunkLength);
        }
    }

    /// <inheritdoc/>
    public async ValueTask<bool> MoveToPointAsync(Stream stream, ulong current, ulong target, CancellationToken cancellationToken = default)
    {
        if (this.pointStart is 0)
        {
            _ = this.InitializeDecoder(stream);
            this.chunkCount = default;
        }

        var (targetChunk, delta) = this.GetDelta(target);
        this.chunkSize = this.GetChunkSize(targetChunk);

        if (targetChunk >= this.tabledChunksValue)
        {
            if (this.currentChunk < (this.tabledChunksValue - 1))
            {
                this.Reader.Decoder.Done();
                this.currentChunk = this.tabledChunksValue - 1;
                await PrepareAsync().ConfigureAwait(false);
                stream.MoveToPositionAbsolute(this.chunkStartValues[this.currentChunk]);
                _ = this.InitializeDecoder(stream);
                this.chunkCount = default;
            }

            delta += (this.chunkSize * (targetChunk - this.currentChunk)) - this.chunkCount;
        }
        else if (this.currentChunk != targetChunk || current > target)
        {
            this.Reader.Decoder.Done();
            this.currentChunk = targetChunk;
            await PrepareAsync().ConfigureAwait(false);
            stream.MoveToPositionAbsolute(this.chunkStartValues[this.currentChunk]);
            _ = this.InitializeDecoder(stream);
            this.chunkCount = default;
        }
        else
        {
            delta = (uint)(target - current);
        }

        while (delta > 0)
        {
            _ = await this.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
            delta--;
        }

        return true;

        async ValueTask PrepareAsync()
        {
            switch (stream)
            {
                // read the chunk here
                case IAsyncCacheStream asyncCacheStream:
                    var asyncChunkStart = this.chunkStartValues[this.currentChunk];
                    var asyncChunkLength = this.chunkStartValues[this.currentChunk + 1] - asyncChunkStart;
                    await asyncCacheStream.CacheAsync(asyncChunkStart, (int)asyncChunkLength, cancellationToken).ConfigureAwait(false);
                    break;

                case ICacheStream cacheStream:
                    var chunkStart = this.chunkStartValues[this.currentChunk];
                    var chunkLength = this.chunkStartValues[this.currentChunk + 1] - chunkStart;
                    cacheStream.Cache(chunkStart, (int)chunkLength);
                    break;
            }
        }
    }

    /// <summary>
    /// Reads the chunk table.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="chunksStart">The chunk start.</param>
    /// <param name="readChunkTotals">Set to <see langword="true"/> to read the chunk totals.</param>
    /// <param name="decoder">The decoder.</param>
    /// <returns>The chunk table.</returns>
    internal static (long[] ChunkStarts, uint NumberChunks, uint TabledChunks, uint[]? ChunkTotals) ReadChunkTable(Stream stream, long chunksStart, bool readChunkTotals, IEntropyDecoder decoder)
    {
        long[]? chunkStarts = default;
        uint numberChunks = default;
        uint tabledChunks = default;

        ReadChunkTable(stream, chunksStart, readChunkTotals, decoder, ref chunkStarts, ref numberChunks, ref tabledChunks, out var chunkTotals);

        return (chunkStarts, numberChunks, tabledChunks, chunkTotals);
    }

    /// <summary>
    /// Reads the chunk table.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="chunksStart">The chunks start.</param>
    /// <param name="decoder">The decoder.</param>
    /// <param name="chunkStarts">The chunk starts.</param>
    /// <param name="numberChunks">The number of chunks.</param>
    /// <param name="tabledChunks">The number of tabled chunks.</param>
    /// <exception cref="InvalidOperationException">Version is invalid.</exception>
    protected static void ReadChunkTable(
        Stream stream,
        long chunksStart,
        IEntropyDecoder decoder,
        [System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(stream))]
        ref long[]? chunkStarts,
        ref uint numberChunks,
        ref uint tabledChunks) => ReadChunkTable(stream, chunksStart, readChunkTotals: false, decoder, ref chunkStarts, ref numberChunks, ref tabledChunks, out _);

    /// <summary>
    /// Reads the chunk table with chunk totals.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="chunksStart">The chunks start.</param>
    /// <param name="decoder">The decoder.</param>
    /// <param name="chunkStarts">The chunk starts.</param>
    /// <param name="numberChunks">The number of chunks.</param>
    /// <param name="tabledChunks">The number of tabled chunks.</param>
    /// <param name="chunkTotals">The chunk totals.</param>
    /// <exception cref="InvalidOperationException">Version is invalid.</exception>
    protected static void ReadChunkTable(
        Stream stream,
        long chunksStart,
        IEntropyDecoder decoder,
        [System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(stream))]
        ref long[]? chunkStarts,
        ref uint numberChunks,
        ref uint tabledChunks,
        [System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(stream))]
        out uint[]? chunkTotals) =>
        ReadChunkTable(
            stream,
            chunksStart,
            readChunkTotals: true,
            decoder,
            ref chunkStarts,
            ref numberChunks,
            ref tabledChunks,
            out chunkTotals);

    /// <summary>
    /// Gets the chunk size for the specified chunk.
    /// </summary>
    /// <param name="chunk">The chunk to get the size for.</param>
    /// <returns>The chunk size.</returns>
    protected virtual uint GetChunkSize(uint chunk) => this.chunkSize;

    /// <summary>
    /// Initializes the decoder.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <returns><see langword="true"/> if the decoder was successfully initialized; otherwise <see langword="false"/>.</returns>
    protected bool InitializeDecoder(Stream stream)
    {
        if (this.chunkStartValues is { Length: 0 })
        {
            if (!ReadChunkTableCore())
            {
                return false;
            }

            this.currentChunk = default;
        }

        this.chunkSize = this.GetChunkSize(this.currentChunk);

        this.pointStart = stream.Position;

        return this.Reader.InitializeDecoder();

        bool ReadChunkTableCore()
        {
            // read the 8 bytes that store the location of the chunk table
            _ = stream.SwitchStreamIfMultiple(LazStreams.ChunkTablePosition);
            var chunkTableStartPosition = stream.ReadInt64LittleEndian();

            // this is where the chunks start
            var chunksStart = stream.Position;

            // was compressor interrupted before getting a chance to write the chunk table?
            if ((chunkTableStartPosition + 8) == chunksStart)
            {
                // no choice but to fail if adaptive chunking was used
                if (this.chunkSize is uint.MaxValue)
                {
                    return false;
                }

                // otherwise we build the chunk table as we read the file
                this.numberChunksValue = DefaultChunkCount;
                this.chunkStartValues = new long[this.numberChunksValue + 1];

                this.chunkStartValues[0] = chunksStart;
                this.tabledChunksValue = 1;

                return true;
            }

            // get the chunk table stream
            _ = stream.SwitchStreamIfMultiple(LazStreams.ChunkTable);

            // maybe the stream is not seekable
            if (!stream.CanSeek)
            {
                // no choice but to fail if adaptive chunking was used
                if (this.chunkSize is uint.MaxValue)
                {
                    return false;
                }

                // then we cannot seek to the chunk table but won't need it anyway
                this.numberChunksValue = default;
                this.tabledChunksValue = default;
                return true;
            }

            if (chunkTableStartPosition is -1)
            {
                // the compressor was writing to a non-seekable stream and wrote the chunk table start at the end
                if (stream.Seek(8, SeekOrigin.End) is 0)
                {
                    return false;
                }

                try
                {
                    chunkTableStartPosition = stream.ReadInt64LittleEndian();
                }
                catch (IOException)
                {
                    return false;
                }
            }

            // read the chunk table
            try
            {
                // fail if we did not manage to seek there
                if (stream.Seek(chunkTableStartPosition, SeekOrigin.Begin) != chunkTableStartPosition)
                {
                    throw new InvalidOperationException(Compression.Properties.Resources.FailedToSeek);
                }

                if (stream is ICacheStream cacheStream)
                {
                    // prepare the chunk table
                    cacheStream.Cache(chunkTableStartPosition, 2048);
                }

                this.ReadChunkTable(
                    stream,
                    chunksStart,
                    ref this.chunkStartValues,
                    ref this.numberChunksValue,
                    ref this.tabledChunksValue);
            }
            catch
            {
                // no choice but to fail if adaptive chunking was used
                if (this.chunkSize is uint.MaxValue)
                {
                    return false;
                }

                // did we not even read the number of chunks
                if (this.tabledChunksValue is default(uint))
                {
                    // then compressor was interrupted before getting a chance to write the chunk table
                    this.numberChunksValue = DefaultChunkCount;
                    this.chunkStartValues = new long[this.numberChunksValue + 1];
                    this.chunkStartValues[0] = chunksStart;
                    this.tabledChunksValue = 1;
                }
                else
                {
                    // otherwise fix as many additional chunkStarts as possible
                    for (var i = 1; i < this.tabledChunksValue; i++)
                    {
                        this.chunkStartValues[i] += this.chunkStartValues[i - 1];
                    }
                }
            }

            return stream.Seek(chunksStart, SeekOrigin.Begin) is not 0;
        }
    }

    /// <summary>
    /// Reads the chunk table.
    /// </summary>
    /// <param name="stream">The input stream.</param>
    /// <param name="chunksStart">The chunks start.</param>
    /// <param name="chunkStarts">The chunk starts.</param>
    /// <param name="numberChunks">The number of chunks.</param>
    /// <param name="tabledChunks">The number of tabled chunks.</param>
    protected virtual void ReadChunkTable(
        Stream stream,
        long chunksStart,
        ref long[] chunkStarts,
        ref uint numberChunks,
        ref uint tabledChunks) => ReadChunkTable(stream, chunksStart, this.Reader.Decoder, ref this.chunkStartValues, ref this.numberChunksValue, ref this.tabledChunksValue);

    /// <summary>
    /// Gets the target chunk, and position within the chunk for the specified point.
    /// </summary>
    /// <param name="target">The target point.</param>
    /// <returns>The target chunk and position within the chunk of <paramref name="target"/>.</returns>
    protected virtual (uint Chunk, uint Delta) GetDelta(ulong target) => ((uint)(target / this.chunkSize), (uint)(target % this.chunkSize));

    /// <summary>
    /// Gets the first point for the specified chunk index.
    /// </summary>
    /// <param name="index">The chunk index.</param>
    /// <returns>The index of the first point of <paramref name="index" />.</returns>
    protected virtual ulong GetFirstPoint(int index) => (ulong)index * this.chunkSize;

    private static void ReadChunkTable(
        Stream stream,
        long chunksStart,
        [System.Diagnostics.CodeAnalysis.DoesNotReturnIf(true)]
        bool readChunkTotals, // hack for forcing chunkTotals to not be null when `true`
        IEntropyDecoder decoder,
        [System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(stream))]
        ref long[]? chunkStarts,
        ref uint numberChunks,
        ref uint tabledChunks,
        out uint[]? chunkTotals)
    {
        chunkTotals = default;

        // fail if the version is wrong
        if (stream.ReadUInt32LittleEndian() is not 0)
        {
            throw new System.Diagnostics.UnreachableException(Compression.Properties.Resources.IncorrectVersion);
        }

        numberChunks = stream.ReadUInt32LittleEndian();
        if (readChunkTotals)
        {
            chunkTotals = new uint[numberChunks + 1];
        }

        chunkStarts = new long[numberChunks + 1];
        chunkStarts[0] = chunksStart;
        tabledChunks = 1U;
        if (numberChunks <= 0)
        {
            return;
        }

#pragma warning disable S1121
        _ = decoder.Initialize(stream);
        var decompressor = new IntegerDecompressor(decoder, 32, 2);
        decompressor.Initialize();
        for (var i = 1; i <= numberChunks; i++)
        {
            _ = chunkTotals?[i] = (uint)decompressor.Decompress(i > 1 ? (int)chunkTotals[i - 1] : 0);
            chunkStarts[i] = decompressor.Decompress(i > 1 ? (int)chunkStarts[i - 1] : 0, 1U);
            tabledChunks++;
        }

        decoder.Done();
        for (var i = 1; i <= numberChunks; i++)
        {
            _ = chunkTotals?[i] += chunkTotals[i - 1];
            chunkStarts[i] += chunkStarts[i - 1];
            if (chunkStarts[i] <= chunkStarts[i - 1])
            {
                throw new InvalidOperationException(Compression.Properties.Resources.ChunkStartsNotInOrder);
            }
        }
#pragma warning restore S1121
    }
}