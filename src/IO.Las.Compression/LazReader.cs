// -----------------------------------------------------------------------
// <copyright file="LazReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a reader for LAZ files.
/// </summary>
public sealed partial class LazReader : LasReader, ILazReader
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

    private LazReader(Stream input, bool leaveOpen, HeaderBlockReader headerReader, in HeaderBlock header)
        : base(input, leaveOpen, headerReader, header) => this.pointReader = this.CreatePointReader();

    /// <inheritdoc/>
    public bool IsCompressed => this.pointReader is not Las.RawReader;

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

        var point = this.pointReader.Read(this.BaseStream, this.PointDataLength);
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

        var point = await this.pointReader.ReadAsync(this.BaseStream, this.PointDataLength, cancellationToken).ConfigureAwait(false);
        this.IncrementPointIndex();
        return point;
    }

    /// <summary>
    /// Moves to the specified chunk.
    /// </summary>
    /// <param name="index">The chunk index.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal bool MoveToChunk(int index) => index >= 0 && this.pointReader is ChunkedReader chunkedReader && chunkedReader.MoveToChunk(this.BaseStream, this.PointDataLength, this.GetCurrentIndex(), index);

    /// <summary>
    /// Moves to the specified chunk start.
    /// </summary>
    /// <param name="chunkStart">The chunk start.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal bool MoveToChunk(long chunkStart) => this.MoveToChunk(this.GetChunkIndex(chunkStart));

    /// <summary>
    /// Moves to the specified chunk.
    /// </summary>
    /// <param name="index">The chunk index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal async ValueTask<bool> MoveToChunkAsync(int index, CancellationToken cancellationToken = default) => index >= 0 && this.pointReader is ChunkedReader chunkedReader && await chunkedReader.MoveToChunkAsync(this.BaseStream, this.PointDataLength, this.GetCurrentIndex(), index, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Moves to the specified chunk start.
    /// </summary>
    /// <param name="chunkStart">The chunk start.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal ValueTask<bool> MoveToChunkAsync(long chunkStart, CancellationToken cancellationToken = default) => this.MoveToChunkAsync(this.GetChunkIndex(chunkStart), cancellationToken);

    /// <summary>
    /// Gets the chunk index.
    /// </summary>
    /// <param name="chunkStart">The start position of the chunk.</param>
    /// <returns>The chunk index.</returns>
    internal int GetChunkIndex(long chunkStart) => this.pointReader is ChunkedReader chunkedReader ? chunkedReader.GetChunkIndex(this.BaseStream, chunkStart) : -1;

    /// <inheritdoc/>
    protected override bool MoveToPoint(ulong current, ulong target)
        => this.pointReader.MoveToPoint(this.BaseStream, this.PointDataLength, current, target);

    private IPointReader CreatePointReader()
    {
        return Initialize(this.Header, this.BaseStream.Position, this.VariableLengthRecords, this.RawReader);

        static IPointReader Initialize(
            in HeaderBlock header,
            long pointStart,
            IReadOnlyList<VariableLengthRecord> variableLengthRecords,
            Readers.IPointDataRecordReader reader)
        {
            LasZip? zip = default;
            for (var i = 0; i < variableLengthRecords.Count; i++)
            {
                var variableLengthRecord = variableLengthRecords[i];
                if (variableLengthRecord is CompressedTag compressedTag)
                {
                    zip = LasZip.From(compressedTag);
                }
                else if (variableLengthRecord is UnknownVariableLengthRecord { Header.RecordId: CompressedTag.TagRecordId } record)
                {
                    compressedTag = new(record.Header, record.Data);

                    // update the variable length records
                    if (variableLengthRecords is IList<VariableLengthRecord> records)
                    {
                        records[i] = compressedTag;
                    }

                    zip = LasZip.From(compressedTag);
                }
            }

            return zip switch
            {
                null or { Items: null } or { Compressor: Compressor.None } => new RawReader(reader, pointStart),
                { Compressor: Compressor.PointWise } => new PointWiseReader(reader, header, zip, pointStart),
                { Compressor: Compressor.PointWiseChunked } => new PointWiseChunkedReader(reader, header, zip),
#if LAS1_4_OR_GREATER
                { Compressor: Compressor.LayeredChunked, ChunkSize: LasZip.VariableChunkSize } => new VariableLayeredChunkedReader(reader, header, zip),
                { Compressor: Compressor.LayeredChunked } => new FixedLayeredChunkedReader(reader, header, zip),
#endif
                _ => throw new InvalidOperationException(),
            };
        }
    }
}