// -----------------------------------------------------------------------
// <copyright file="LazReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a reader for LAZ files.
/// </summary>
public sealed class LazReader : LasReader, ILazReader
{
    private readonly IPointReader pointReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazReader"/> class based on the specified stream, and optionally leaves the stream open.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LazReader"/> object is disposed; otherwise <see langword="false"/>.</param>
    public LazReader(Stream input, bool leaveOpen = false)
        : base(input, leaveOpen) => this.pointReader = this.CreatePointReader();

    /// <summary>
    /// Initializes a new instance of the <see cref="LazReader"/> class based on the specified stream, and optionally leaves the stream open.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="fileSignature">The file signature.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LazReader"/> object is disposed; otherwise <see langword="false"/>.</param>
    public LazReader(Stream input, string fileSignature, bool leaveOpen = false)
        : base(input, fileSignature, leaveOpen) => this.pointReader = this.CreatePointReader();

    /// <summary>
    /// Initializes a new instance of the <see cref="LazReader"/> class based on the specified path.
    /// </summary>
    /// <param name="path">The file to be opened for reading.</param>
    public LazReader(string path)
        : this(CreateStream(path))
    {
    }

    private LazReader(Stream input, bool leaveOpen, HeaderBlockReader headerReader, in HeaderBlock header)
        : base(input, leaveOpen, headerReader, header) => this.pointReader = this.CreatePointReader();

    /// <inheritdoc/>
    public bool IsCompressed => this.pointReader is not Las.RawReader;

    /// <inheritdoc/>
    bool ILazReader.IsChunked => this.pointReader is ChunkedReader;

    /// <summary>
    /// Creates either a <see cref="LazReader"/> or <see cref="LasReader"/> depending on whether the header specifies compression.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The <see cref="LazReader"/> or <see cref="LasReader"/> depending on whether the header specifies compression.</returns>
    public static LasReader Create(string path) => Create(File.OpenRead(path));

    /// <summary>
    /// Creates either a <see cref="LazReader"/> or <see cref="LasReader"/> depending on whether the header specifies compression.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LasReader"/> object is disposed; otherwise <see langword="false"/>.</param>
    /// <returns>The <see cref="LazReader"/> or <see cref="LasReader"/> depending on whether the header specifies compression.</returns>
    public static LasReader Create(Stream input, bool leaveOpen = false)
    {
        // register compression here.
        _ = VariableLengthRecordProcessor.Instance.TryRegisterCompression();

        var headerReader = new HeaderBlockReader(input);
        var header = headerReader.GetHeaderBlock();
        return header.IsCompressed()
            ? new LazReader(input, leaveOpen, headerReader, header)
            : new LasReader(input, leaveOpen, headerReader, header);
    }

    /// <inheritdoc/>
    public override LasPointSpan ReadPointDataRecord()
    {
        if (!this.CheckPointIndex())
        {
            return default;
        }

        var point = this.pointReader.Read(this.BaseStream);
        this.IncrementPointIndex();
        return point;
    }

    /// <inheritdoc />
    public override async ValueTask<LasPointMemory> ReadPointDataRecordAsync(CancellationToken cancellationToken = default)
    {
        if (!this.CheckPointIndex())
        {
            return default;
        }

        var point = await this.pointReader.ReadAsync(this.BaseStream, cancellationToken).ConfigureAwait(false);
        this.IncrementPointIndex();
        return point;
    }

    /// <inheritdoc/>
    ChunkedReader.ChunkedLasPointSpanEnumerable ILazReader.ReadChunk() =>
        this.pointReader is ChunkedReader chunkedReader
            ? chunkedReader.ReadChunk(this)
            : default;

    /// <inheritdoc/>
    ChunkedReader.ChunkedLasPointSpanEnumerable ILazReader.ReadChunk(int chunk) =>
        this.pointReader is ChunkedReader chunkedReader && this.MoveToChunkImpl(chunkedReader, chunk)
            ? chunkedReader.ReadChunk(this)
            : default;

    /// <inheritdoc/>
    ChunkedReader.ChunkedLasPointMemoryEnumerable ILazReader.ReadChunkAsync() =>
        this.pointReader is ChunkedReader chunkedReader
            ? chunkedReader.ReadChunkAsync(this)
            : default;

    /// <inheritdoc/>
    ChunkedReader.ChunkedLasPointMemoryEnumerable ILazReader.ReadChunkAsync(int chunk) =>
        this.pointReader is ChunkedReader chunkedReader && this.MoveToChunkImpl(chunkedReader, chunk)
            ? chunkedReader.ReadChunkAsync(this)
            : default;

    /// <inheritdoc/>
    bool ILazReader.MoveToChunk(int index) => this.MoveToChunkImpl(index);

    /// <inheritdoc/>
    bool ILazReader.MoveToChunk(long chunkStart) => this.MoveToChunkImpl(this.GetChunkIndex(chunkStart));

    /// <inheritdoc/>
    ValueTask<bool> ILazReader.MoveToChunkAsync(int index, CancellationToken cancellationToken) => this.MoveToChunkImplAsync(index, cancellationToken);

    /// <inheritdoc/>
    ValueTask<bool> ILazReader.MoveToChunkAsync(long chunkStart, CancellationToken cancellationToken) => this.MoveToChunkImplAsync(this.GetChunkIndex(chunkStart), cancellationToken);

    /// <summary>
    /// Gets the chunk index.
    /// </summary>
    /// <param name="chunkStart">The start position of the chunk.</param>
    /// <returns>The chunk index.</returns>
    internal int GetChunkIndex(long chunkStart) => this.pointReader is ChunkedReader chunkedReader ? chunkedReader.GetChunkIndex(this.BaseStream, chunkStart) : -1;

    /// <inheritdoc/>
    protected override bool MoveToPoint(ulong current, ulong target)
        => this.pointReader.MoveToPoint(this.BaseStream, current, target);

    /// <inheritdoc/>
    protected override ValueTask<bool> MoveToPointAsync(ulong current, ulong target, CancellationToken cancellationToken = default)
        => this.pointReader.MoveToPointAsync(this.BaseStream, current, target, cancellationToken);

    private static Stream CreateStream(string path) => path switch
    {
        not null when File.Exists(path) => File.OpenRead(path),
        not null when Directory.Exists(path) => LazMultipleFileStream.OpenRead(path),
        _ => throw new NotSupportedException(),
    };

    private bool MoveToChunkImpl(int index) => this.pointReader is ChunkedReader chunkedReader && this.MoveToChunkImpl(chunkedReader, index);

    private bool MoveToChunkImpl(ChunkedReader chunkedReader, int index) => index >= 0 && chunkedReader.MoveToChunk(this.BaseStream, this.GetCurrentIndex(), index);

    private async ValueTask<bool> MoveToChunkImplAsync(int index, CancellationToken cancellationToken) => this.pointReader is ChunkedReader chunkedReader && await this.MoveToChunkImplAsync(chunkedReader, index, cancellationToken).ConfigureAwait(false);

    private async ValueTask<bool> MoveToChunkImplAsync(ChunkedReader chunkedReader, int index, CancellationToken cancellationToken) => index >= 0 && await chunkedReader.MoveToChunkAsync(this.BaseStream, this.GetCurrentIndex(), index, cancellationToken).ConfigureAwait(false);

    private IPointReader CreatePointReader()
    {
        return GetLasZip(this.VariableLengthRecords) switch
        {
            null or { Items: null } or { Compressor: Compressor.None } => new RawReader(this.RawReader, this.PointDataLength, this.BaseStream.Position),
            { Compressor: Compressor.PointWise } zip => new PointWiseReader(this.RawReader, this.Header, zip, this.PointDataLength, this.BaseStream.Position),
            { Compressor: Compressor.PointWiseChunked } zip => new PointWiseChunkedReader(this.RawReader, this.Header, zip, this.PointDataLength),
#if LAS1_4_OR_GREATER
            { Compressor: Compressor.LayeredChunked, ChunkSize: LasZip.VariableChunkSize } zip => new VariableLayeredChunkedReader(this.RawReader, this.Header, zip, this.PointDataLength),
            { Compressor: Compressor.LayeredChunked } zip => new FixedLayeredChunkedReader(this.RawReader, this.Header, zip, this.PointDataLength),
#endif
            _ => throw new System.Diagnostics.UnreachableException(),
        };

        static LasZip? GetLasZip(IReadOnlyList<VariableLengthRecord> variableLengthRecords)
        {
            for (var i = 0; i < variableLengthRecords.Count; i++)
            {
                var variableLengthRecord = variableLengthRecords[i];
                if (variableLengthRecord is CompressedTag compressedTag)
                {
                    return LasZip.From(compressedTag);
                }

                if (variableLengthRecord is not UnknownVariableLengthRecord { Header.RecordId: CompressedTag.TagRecordId } record)
                {
                    // this is not a compressed TAG
                    continue;
                }

                // this was not explicitly a compressed TAG, so let's create one.
                compressedTag = new(record.Header, record.Data);

                // update the variable length records
                if (variableLengthRecords is IList<VariableLengthRecord> records)
                {
                    records[i] = compressedTag;
                }

                return LasZip.From(compressedTag);
            }

            return default;
        }
    }
}