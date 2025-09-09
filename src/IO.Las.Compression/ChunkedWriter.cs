// -----------------------------------------------------------------------
// <copyright file="ChunkedWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The abstract chunked point writer.
/// </summary>
/// <param name="rawWriter">The raw writer.</param>
/// <param name="pointDataLength">The point data length.</param>
/// <param name="pointDataFormatId">The point data format ID.</param>
/// <param name="zip">The zip information.</param>
/// <param name="size">The chunk size information.</param>
internal abstract class ChunkedWriter(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip, uint size) : IPointWriter
{
    /// <summary>
    /// The default chunk key.
    /// </summary>
    protected const int DefaultChunkKey = -1;

    private const int InvalidChunkTableStartPosition = -1;

    private readonly ArithmeticEncoder encoder = zip.Coder switch
    {
        Coder.Arithmetic => new(),
        _ => throw new NotSupportedException(),
    };

    private readonly List<uint> chunkBytes = [];

    private readonly SortedDictionary<int, ChunkWriterWrapper> writers = [];

    private long chunkTableStartPosition;

    private ChunkWriterWrapper? currentChunkWriter;

    private int finalizedChunks;

    /// <summary>
    /// Gets the total chunks.
    /// </summary>
    protected int TotalChunks => this.finalizedChunks + this.writers.Count;

    /// <inheritdoc/>
    public virtual void Initialize(Stream stream)
    {
        this.chunkTableStartPosition = stream.CanSeek
            ? stream.Position
            : InvalidChunkTableStartPosition;
        stream.WriteInt64LittleEndian(this.chunkTableStartPosition);
    }

    /// <inheritdoc/>
    public virtual void Write(Stream stream, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes) => this.Write(stream, record, extraBytes, DefaultChunkKey);

    /// <summary>
    /// Writes the point to the specified chunk.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="record">The point.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="chunkKey">The chunk key to writer to.</param>
    public virtual void Write(Stream stream, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes, int chunkKey)
    {
        var chunkWriter = this.GetWriter(chunkKey, stream);
        if (chunkWriter.Count == size)
        {
            if (chunkKey is not DefaultChunkKey)
            {
                // we are trying to close a chunk that we've been explicitly asked to write to.
                throw new InvalidOperationException();
            }

            this.FinalizeChunk(stream, chunkWriter);
            chunkWriter = this.GetWriter(this.chunkBytes.Count, stream);
        }

        chunkWriter.Write(stream, record, extraBytes);

        if (chunkWriter.Count == size)
        {
            this.FinalizeChunk(stream, chunkWriter);
        }
    }

    /// <inheritdoc/>
    public virtual ValueTask WriteAsync(Stream stream, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default) => this.WriteAsync(stream, record, extraBytes, DefaultChunkKey, cancellationToken);

    /// <summary>
    /// Writes the point to the specified chunk asynchronously.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="record">The point.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="chunkKey">The chunk key to writer to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    public virtual async ValueTask WriteAsync(Stream stream, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, int chunkKey, CancellationToken cancellationToken = default)
    {
        var chunkWriter = this.GetWriter(chunkKey, stream);
        if (chunkWriter.Count == size)
        {
            if (chunkKey is not DefaultChunkKey)
            {
                // we are trying to close a chunk that we've explicitly tried to write to.
                throw new InvalidOperationException();
            }

            this.FinalizeChunk(stream, chunkWriter);

            // start the next chunk
            chunkWriter = this.GetWriter(this.chunkBytes.Count, stream);
        }

        await chunkWriter.WriteAsync(stream, record, extraBytes, cancellationToken).ConfigureAwait(false);

        if (chunkWriter.Count == size)
        {
            this.FinalizeChunk(stream, chunkWriter);
        }
    }

    /// <inheritdoc/>
    public virtual void Close(Stream stream)
    {
        // ensure that all writers are closed
        while (this.writers.Count > 0)
        {
            this.FinalizeChunk(stream, this.writers.Values.First());
        }

        _ = WriteChunkTable();

        bool WriteChunkTable()
        {
            var position = stream.Position;
            if (this.chunkTableStartPosition is not InvalidChunkTableStartPosition)
            {
                // stream is seekable
                if (stream.Seek(this.chunkTableStartPosition, SeekOrigin.Begin) is 0)
                {
                    return false;
                }

                stream.WriteInt64LittleEndian(position);

                if (stream.Seek(position, SeekOrigin.Begin) is 0)
                {
                    return false;
                }
            }

            const uint ChunkTableVersion = default;
            stream.WriteUInt32LittleEndian(ChunkTableVersion);
            stream.WriteUInt32LittleEndian((uint)this.chunkBytes.Count); // number of chunks

            _ = this.encoder.Initialize(stream);
            var compressor = new IntegerCompressor(this.encoder, 32, 2);
            compressor.Initialize();
            for (var i = 0; i < this.chunkBytes.Count; i++)
            {
                this.CompressValue(compressor, i);
            }

            this.encoder.Done();

            if (this.chunkTableStartPosition is InvalidChunkTableStartPosition)
            {
                // stream is not-seekable
                stream.WriteInt64LittleEndian(position);
            }

            return true;
        }
    }

    /// <summary>
    /// Gets the chunk totals.
    /// </summary>
    /// <returns>The chunk totals.</returns>
    public abstract IEnumerable<uint> GetChunkTotals();

    /// <summary>
    /// Finalizes the chunk.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="chunkWriter">The chunk to finalize.</param>
    protected void FinalizeChunk(Stream stream, ChunkWriterWrapper chunkWriter)
    {
        chunkWriter.Finalize(stream);
        this.AddChunkToTableIfCompleted(stream, chunkWriter);
    }

    /// <summary>
    /// Adds the chunk to the table.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="chunkWriter">The chunk writer wrapper.</param>
    protected virtual void AddChunkToTableIfCompleted(Stream stream, ChunkWriterWrapper? chunkWriter) => this.AddChunkToTableIfCompleted(stream, chunkWriter, static _ => { });

    /// <summary>
    /// Adds the chunk to the table.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <param name="chunkWriter">The chunk writer wrapper.</param>
    /// <param name="removed">Action taken when the wrapper is removed.</param>
    protected void AddChunkToTableIfCompleted(Stream stream, ChunkWriterWrapper? chunkWriter, Action<ChunkWriterWrapper> removed)
    {
        if (chunkWriter is null || chunkWriter.Start == chunkWriter.End)
        {
            // this is null or not complete
            return;
        }

        if (this.writers.Keys.First() != chunkWriter.Key)
        {
            // we can't remove this yet, as we can only update ones that are at the top.
            return;
        }

        var start = chunkWriter.Start;
        var end = chunkWriter.End;

        this.chunkBytes.Add((uint)(end - start));

        // remove this chunk writer from the list, and update all the other values
        _ = this.writers.Remove(chunkWriter.Key);
        this.finalizedChunks++;
        removed(chunkWriter);

        // offset all the other writers
        var values = this.writers.Values;
        foreach (var value in values)
        {
            value.SetOffset(chunkWriter.End);
        }

        // try the first one of the rest
        while (values.FirstOrDefault() is { } wrapper && wrapper.Count == size)
        {
            wrapper.Finalize(stream);

            this.AddChunkToTableIfCompleted(stream, wrapper, removed);
        }
    }

    /// <summary>
    /// Compresses the value at the specified index.
    /// </summary>
    /// <param name="integerCompressor">The integer compressor.</param>
    /// <param name="index">The index.</param>
    protected virtual void CompressValue(IntegerCompressor integerCompressor, int index) =>
        integerCompressor.Compress(index is 0 ? default : (int)this.chunkBytes[index - 1], this.chunkBytes[index], 1);

    /// <summary>
    /// Creates a chunk writer.
    /// </summary>
    /// <param name="rawWriter">The raw writer.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <param name="pointDataFormatId">The point data format ID.</param>
    /// <param name="zip">The zip information.</param>
    /// <returns>The chunk writer.</returns>
    protected virtual ChunkWriter Create(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip) => new(rawWriter, pointDataLength, pointDataFormatId, zip);

    /// <summary>
    /// Gets the writer for the specified chunk key.
    /// </summary>
    /// <param name="chunkKey">The chunk key.</param>
    /// <param name="stream">The stream.</param>
    /// <returns>The chunk writer wrapper.</returns>
    protected ChunkWriterWrapper GetWriter(int chunkKey, Stream stream)
    {
        if (chunkKey is DefaultChunkKey)
        {
            // Get the last chunk
            chunkKey = this.chunkBytes.Count;
        }

        // we're switching writer
        if (this.currentChunkWriter?.Key != chunkKey
            && !this.writers.TryGetValue(chunkKey, out this.currentChunkWriter))
        {
            this.currentChunkWriter = new(this.Create(rawWriter, pointDataLength, pointDataFormatId, zip), chunkKey);
            this.currentChunkWriter.Initialize(stream);
            this.writers.Add(chunkKey, this.currentChunkWriter);
        }

        return this.currentChunkWriter;
    }

    /// <summary>
    /// Gets the chunk writers.
    /// </summary>
    /// <returns>The chunk writers.</returns>
    protected IEnumerable<ChunkWriter> GetWriters()
    {
        foreach (var writer in this.writers)
        {
            yield return writer.Value.Writer;
        }
    }

    /// <summary>
    /// The chunk writer wrapper.
    /// </summary>
    /// <param name="writer">The chunk writer.</param>
    /// <param name="chunkKey">The chunk key.</param>
    protected sealed class ChunkWriterWrapper(ChunkWriter writer, int chunkKey)
    {
        /// <summary>
        /// Gets the start.
        /// </summary>
        public long Start { get; private set; }

        /// <summary>
        /// Gets the end.
        /// </summary>
        public long End { get; private set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public int Key => chunkKey;

        /// <summary>
        /// Gets the writer.
        /// </summary>
        public ChunkWriter Writer { get; } = writer;

        /// <inheritdoc cref="ChunkWriter.Count"/>
        public uint Count => this.Writer.Count;

        /// <inheritdoc cref="ChunkWriter.Initialize(Stream)"/>
        public void Initialize(Stream stream)
        {
            this.Writer.Initialize(stream);
            this.Start = this.End = stream.Position;
        }

        /// <summary>
        /// Finalizes the writer.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void Finalize(Stream stream)
        {
            this.Writer.Close(stream);
            this.End = stream.Position;
        }

        /// <summary>
        /// Offsets the start/end values by the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public void SetOffset(long offset) => this.Start = this.End = offset;

        /// <inheritdoc cref="ChunkWriter.Write"/>
        public void Write(Stream stream, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes) => this.Writer.Write(stream, record, extraBytes);

        /// <inheritdoc cref="ChunkWriter.WriteAsync"/>
        public ValueTask WriteAsync(Stream stream, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken) => this.Writer.WriteAsync(stream, record, extraBytes, cancellationToken);
    }
}