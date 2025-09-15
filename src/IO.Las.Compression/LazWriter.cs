// -----------------------------------------------------------------------
// <copyright file="LazWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a LAZ file writer.
/// </summary>
public sealed class LazWriter : LasWriter
{
#if LAS1_4_OR_GREATER
    private readonly List<EvlrRecord> extendedVariableLengthRecords = [];
#endif

    private IPointWriter? pointWriter;

    private bool closedWriting;

#if LAS1_4_OR_GREATER
    private long offsetToLasZipVlr = -1;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="LazWriter"/> class based on the specified stream, and optionally leaves the stream open.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LazWriter"/> object is disposed; otherwise <see langword="false"/>.</param>
    public LazWriter(Stream stream, bool leaveOpen = false)
        : base(stream, leaveOpen)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LazWriter"/> class based on the reader, and optionally leaves the stream open.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="leaveOpen"><see langword="true"/> to leave the stream open after the <see cref="LazWriter"/> object is disposed; otherwise <see langword="false"/>.</param>
    public LazWriter(LasReader reader, bool leaveOpen = false)
#if LAS1_4_OR_GREATER
        : base(
            reader.BaseStream,
            reader.Header.Version >= EvlrVersion ? reader.ExtendedVariableLengthRecords.Count : -1,
            leaveOpen)
#else
        : base(
            reader.BaseStream,
            leaveOpen)
#endif
    {
        _ = reader.MoveToVariableLengthRecords();

#if LAS1_4_OR_GREATER
        // get the compressed tag location
        for (var i = 0; i < reader.VariableLengthRecords.Count; i++)
        {
            var position = reader.BaseStream.Position;
            if (HeaderBlockReader.GetVariableLengthRecord(reader.BaseStream) is not CompressedTag)
            {
                continue;
            }

            this.offsetToLasZipVlr = position;
            break;
        }

        (_, this.pointWriter) = GetExtraByteCountAndPointWriter(reader.Header, this.RawWriter, [.. reader.VariableLengthRecords]);
#else
        this.pointWriter = GetPointWriter(reader.Header, this.RawWriter, [.. reader.VariableLengthRecords]);
#endif
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LazWriter"/> class based on the specified path.
    /// </summary>
    /// <param name="path">The file to be opened for writing.</param>
    public LazWriter(string path)
        : base(CreateStream(path))
    {
    }

    /// <inheritdoc/>
    public override void Write(in HeaderBlock header, params IEnumerable<VariableLengthRecord> records)
    {
        if (this.pointWriter is not null)
        {
            throw new InvalidOperationException("Cannot write header while writing is not complete");
        }

        var recordsList = records is IList<VariableLengthRecord> { IsReadOnly: false } r ? r : [.. records];
#if LAS1_4_OR_GREATER
        (var extraByteCount, this.pointWriter) = GetExtraByteCountAndPointWriter(header, this.RawWriter, recordsList);
#else
        this.pointWriter = GetPointWriter(header, this.RawWriter, recordsList);
#endif

        // offset to point data
        var recordSize = recordsList.Aggregate(0u, static (current, record) => current + record.Size());

#if LAS1_4_OR_GREATER
        this.WriteHeader(header, (uint)recordsList.Count, recordSize, extraByteCount);
#else
        this.WriteHeader(header, (uint)recordsList.Count, recordSize);
#endif

        _ = this.BaseStream.SwitchStreamIfMultiple(LasStreams.VariableLengthRecord);
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(recordsList.Max(static x => x.Size()));
        foreach (var record in recordsList)
        {
#if LAS1_4_OR_GREATER
            if (record is CompressedTag)
            {
                // this is the location of the Compressed tag.
                this.offsetToLasZipVlr = this.BaseStream.Position;
            }
#endif

            var bytesWritten = record.CopyTo(buffer);

            this.BaseStream.Write(buffer, 0, bytesWritten);
        }

        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);

        if (!this.closedWriting)
        {
            this.pointWriter.Initialize(this.BaseStream);
        }
    }

    /// <inheritdoc/>
    public override void Write(IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(record);
#else
        ThrowIfNull(record);
#endif
        this.pointWriter!.Write(this.BaseStream, record, extraBytes);
    }

    /// <summary>
    /// Writes the point to the specified chunk.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="chunkKey">The chunk key to write to.</param>
    public void Write(IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes, int chunkKey)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(record);
#else
        ThrowIfNull(record);
#endif

        if (this.pointWriter is ChunkedWriter chunkedWriter && this.BaseStream.CanSwitchStream())
        {
            chunkedWriter.Write(this.BaseStream, record, extraBytes, chunkKey);
        }
        else
        {
            this.pointWriter!.Write(this.BaseStream, record, extraBytes);
        }
    }

    /// <summary>
    /// Writes the points as a single chunk if possible.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="count">The number of points.</param>
    /// <exception cref="InvalidOperationException">The writer is already writing a chunk.</exception>
    /// <exception cref="ArgumentException"><paramref name="points"/> contains less than <paramref name="count"/> points.</exception>
    public void Write(IEnumerable<LasPointMemory> points, int count)
    {
#if LAS1_4_OR_GREATER
        if (this.pointWriter is VariableLayeredChunkedWriter chunked)
        {
            chunked.Write(this.BaseStream, points, count);
        }
        else
        {
            foreach (var point in points.Take(count))
            {
                this.Write(
                    point.PointDataRecord ?? throw new InvalidOperationException(),
                    point.ExtraBytes.Span);
            }
        }
#else
        foreach (var point in points.Take(count))
        {
            this.Write(
                point.PointDataRecord ?? throw new InvalidOperationException(),
                point.ExtraBytes.Span);
        }
#endif
    }

    /// <summary>
    /// Writes the points as a single chunk if possible.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <exception cref="InvalidOperationException">The writer is already writing a chunk.</exception>
    public void Write(IEnumerable<LasPointMemory> points)
    {
#if LAS1_4_OR_GREATER
        if (this.pointWriter is VariableLayeredChunkedWriter chunked)
        {
            chunked.Write(this.BaseStream, points);
        }
        else
        {
            foreach (var point in points)
            {
                this.Write(
                    point.PointDataRecord ?? throw new InvalidOperationException(),
                    point.ExtraBytes.Span);
            }
        }
#else
        foreach (var point in points)
        {
            this.Write(
                point.PointDataRecord ?? throw new InvalidOperationException(),
                point.ExtraBytes.Span);
        }
#endif
    }

    /// <summary>
    /// Writes the points as a single chunk.
    /// </summary>
    /// <param name="points">The points.</param>
    public void Write(ICollection<LasPointMemory> points) => this.Write(points, points.Count);

#if LAS1_4_OR_GREATER
    /// <inheritdoc/>
    /// <remarks>This holds the record until closing the reader.</remarks>
    public override void Write(ExtendedVariableLengthRecord record) => this.Write(record, special: false);

    /// <summary>
    /// Writes the extended variable length record.
    /// </summary>
    /// <param name="record">The extended variable length record value.</param>
    /// <param name="callback">The call back with the actual write position of the EVLR.</param>
    /// <remarks>This holds the record until closing the reader.</remarks>
    public void Write(ExtendedVariableLengthRecord record, Action<long> callback) => this.Write(record, special: false, callback);

    /// <summary>
    /// Writes the extended variable length record, either to standard or special location.
    /// </summary>
    /// <param name="record">The extended variable length record value.</param>
    /// <param name="special">Set to <see langword="true"/> to write as a special EVLR; otherwise <see langword="false"/>.</param>
    /// <remarks>This holds the record until closing the reader.</remarks>
    public void Write(ExtendedVariableLengthRecord record, bool special) => this.extendedVariableLengthRecords.Add(new(record, special, default));

    /// <summary>
    /// Writes the extended variable length record, either to standard or special location.
    /// </summary>
    /// <param name="record">The extended variable length record value.</param>
    /// <param name="special">Set to <see langword="true"/> to write as a special EVLR; otherwise <see langword="false"/>.</param>
    /// <param name="callback">The call back with the actual write position of the EVLR.</param>
    /// <remarks>This holds the record until closing the reader.</remarks>
    public void Write(ExtendedVariableLengthRecord record, bool special, Action<long> callback) => this.extendedVariableLengthRecords.Add(new(record, special, callback));
#endif

    /// <inheritdoc/>
    public override ValueTask WriteAsync(IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes = default, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(record);
#else
        ThrowIfNull(record);
#endif
        return this.pointWriter!.WriteAsync(this.BaseStream, record, extraBytes, cancellationToken);
    }

    /// <summary>
    /// Writes the point to the specified chunk asynchronously.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="chunkKey">The chunk to write to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    public ValueTask WriteAsync(IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, int chunkKey, CancellationToken cancellationToken = default)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(record);
#else
        ThrowIfNull(record);
#endif
        return this.pointWriter is ChunkedWriter chunkedWriter && this.BaseStream.CanSwitchStream()
            ? chunkedWriter.WriteAsync(this.BaseStream, record, extraBytes, chunkKey, cancellationToken)
            : this.pointWriter!.WriteAsync(this.BaseStream, record, extraBytes, cancellationToken);
    }

    /// <summary>
    /// Writes the points as a single chunk if possible.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="count">The number of points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">The writer is already writing a chunk.</exception>
    /// <exception cref="ArgumentException"><paramref name="points"/> contains less than <paramref name="count"/> points.</exception>
    public async ValueTask WriteAsync(IEnumerable<LasPointMemory> points, int count, CancellationToken cancellationToken = default)
    {
#if LAS1_4_OR_GREATER
        if (this.pointWriter is VariableLayeredChunkedWriter chunked)
        {
            await chunked.WriteAsync(this.BaseStream, points, count, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            foreach (var point in points.Take(count))
            {
                await this.WriteAsync(
                    point.PointDataRecord ?? throw new InvalidOperationException(),
                    point.ExtraBytes,
                    cancellationToken).ConfigureAwait(false);
            }
        }
#else
        foreach (var point in points.Take(count))
        {
            await this.WriteAsync(
                point.PointDataRecord ?? throw new InvalidOperationException(),
                point.ExtraBytes,
                cancellationToken).ConfigureAwait(false);
        }
#endif
    }

    /// <summary>
    /// Writes the points as a single chunk if possible.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">The writer is already writing a chunk.</exception>
    public async ValueTask WriteAsync(IEnumerable<LasPointMemory> points, CancellationToken cancellationToken = default)
    {
#if LAS1_4_OR_GREATER
        if (this.pointWriter is VariableLayeredChunkedWriter chunked)
        {
            await chunked.WriteAsync(this.BaseStream, points, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            foreach (var point in points)
            {
                await this.WriteAsync(
                    point.PointDataRecord ?? throw new InvalidOperationException(),
                    point.ExtraBytes,
                    cancellationToken).ConfigureAwait(false);
            }
        }
#else
        foreach (var point in points)
        {
            await this.WriteAsync(
                point.PointDataRecord ?? throw new InvalidOperationException(),
                point.ExtraBytes,
                cancellationToken).ConfigureAwait(false);
        }
#endif
    }

#if NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    /// <summary>
    /// Writes the points as a single chunk if possible.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="count">The number of points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">The writer is already writing a chunk.</exception>
    /// <exception cref="ArgumentException"><paramref name="points"/> contains less than <paramref name="count"/> points.</exception>
    public async ValueTask WriteAsync(IAsyncEnumerable<LasPointMemory> points, int count, CancellationToken cancellationToken = default)
    {
#if LAS1_4_OR_GREATER
        if (this.pointWriter is VariableLayeredChunkedWriter chunked)
        {
            await chunked.WriteAsync(this.BaseStream, points, count, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var current = default(int);
            await foreach (var point in points.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                await this.WriteAsync(
                    point.PointDataRecord ?? throw new InvalidOperationException(),
                    point.ExtraBytes,
                    cancellationToken).ConfigureAwait(false);

                current++;
                if (current == count)
                {
                    break;
                }
            }
        }
#else
        var current = default(int);
        await foreach (var point in points.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            await this.WriteAsync(
                point.PointDataRecord ?? throw new InvalidOperationException(),
                point.ExtraBytes,
                cancellationToken).ConfigureAwait(false);

            current++;
            if (current == count)
            {
                break;
            }
        }
#endif
    }
#endif

    /// <summary>
    /// Writes the points as a single chunk asynchronously.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    public ValueTask WriteAsync(ICollection<LasPointMemory> points, CancellationToken cancellationToken = default) => this.WriteAsync(points, points.Count, cancellationToken);

#if NETSTANDARD2_0_OR_GREATER || NETFRAMEWORK || NET
    /// <inheritdoc />
    public override void Close()
    {
        this.CloseWriting();
        base.Close();
    }
#endif

    /// <inheritdoc />
    public override void Flush()
    {
        this.CloseWriting();
        base.Flush();
    }

    /// <summary>
    /// Gets the chunk counts.
    /// </summary>
    /// <returns>The chunk counts.</returns>
    public IEnumerable<uint> GetChunkCounts() => this.pointWriter is ChunkedWriter chunkedWriter ? chunkedWriter.GetChunkTotals() : [];

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.CloseWriting();
        }

        base.Dispose(disposing);
    }

    private static Stream CreateStream(string path) => path switch
    {
        not null when Directory.Exists(path) => LazMultipleFileStream.OpenWrite(path),
        not null => File.Open(path, FileMode.Create),
        _ => throw new NotSupportedException(),
    };

#if !NET6_0_OR_GREATER
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNull(IBasePointDataRecord record)
    {
        if (record is null)
        {
            throw new ArgumentNullException(nameof(record));
        }
    }
#endif

#if LAS1_4_OR_GREATER
    private static (ushort ExtraBytesCount, IPointWriter PointWriter) GetExtraByteCountAndPointWriter(
        in HeaderBlock header,
        Writers.IPointDataRecordWriter rawWriter,
        ICollection<VariableLengthRecord> records)
    {
        // check for extra bytes to increase the point record size by
        var extraByteCount = records.OfType<ExtraBytes>().FirstOrDefault() switch
        {
            { } extraBytes => extraBytes.GetByteCount(),
            _ => default,
        };

        LasZip zip;
        if (GetCompressedTagAndRemoveDuplicates(records) is { } compressedTag)
        {
            // use the current compressed tag as the ZIP
            zip = LasZip.From(compressedTag);
            extraByteCount = zip.Items.GetExtraBytesCount();
        }
        else
        {
            zip = new(header.PointDataFormatId, extraByteCount, Compressor.LayeredChunked, LasZip.GetValidVersion(header.PointDataFormatId));
            records.Add(new CompressedTag(zip));
        }

        if (header.PointDataFormatId >= ExtendedGpsPointDataRecord.Id && zip.Compressor is not Compressor.LayeredChunked)
        {
            throw new InvalidOperationException(string.Format(Compression.Properties.Resources.Culture, Compression.Properties.v1_4.Resources.RequireNativeExtension, header.PointDataFormatId));
        }

        var pointDataRecordLength = header.GetPointDataRecordLength() + extraByteCount;

        return (extraByteCount, zip switch
        {
            { Compressor: Compressor.None } => new RawWriter(rawWriter, pointDataRecordLength),
            { Compressor: Compressor.PointWise } => new PointWiseWriter(rawWriter, pointDataRecordLength, header.PointDataFormatId, zip),
            { Compressor: Compressor.PointWiseChunked } => new PointWiseChunkedWriter(rawWriter, pointDataRecordLength, header.PointDataFormatId, zip),
            { Compressor: Compressor.LayeredChunked, ChunkSize: LasZip.VariableChunkSize } => new VariableLayeredChunkedWriter(rawWriter, pointDataRecordLength, header.PointDataFormatId, zip),
            { Compressor: Compressor.LayeredChunked } => new FixedLayeredChunkedWriter(rawWriter, pointDataRecordLength, header.PointDataFormatId, zip),
            _ => throw new InvalidOperationException(),
        });

        static CompressedTag? GetCompressedTagAndRemoveDuplicates(ICollection<VariableLengthRecord> records)
        {
            var compressedTags = records.OfType<CompressedTag>().ToList();
            if (compressedTags is { Count: 0 })
            {
                return default;
            }

            var compressedTag = compressedTags[0];

            if (compressedTags is { Count: 1 })
            {
                return compressedTag;
            }

            // remove all but the first VLR
            foreach (var record in compressedTags.Skip(1))
            {
                _ = records.Remove(record);
            }

            return compressedTag;
        }
    }
#else
    private static IPointWriter GetPointWriter(
        in HeaderBlock header,
        Writers.IPointDataRecordWriter rawWriter,
        ICollection<VariableLengthRecord> records)
    {
        LasZip zip;
        if (GetCompressedTagAndRemoveDuplicates(records) is { } compressedTag)
        {
            // use the current compressed tag as the ZIP
            zip = LasZip.From(compressedTag);
        }
        else
        {
            zip = new(header.PointDataFormatId, Compressor.PointWiseChunked, LasZip.GetValidVersion(header.PointDataFormatId));
            records.Add(new CompressedTag(zip));
        }

        var pointDataRecordLength = header.GetPointDataRecordLength();

        return zip switch
        {
            { Compressor: Compressor.None } => new RawWriter(rawWriter, pointDataRecordLength),
            { Compressor: Compressor.PointWise } => new PointWiseWriter(rawWriter, pointDataRecordLength, header.PointDataFormatId, zip),
            { Compressor: Compressor.PointWiseChunked } => new PointWiseChunkedWriter(rawWriter, pointDataRecordLength, header.PointDataFormatId, zip),
            _ => throw new InvalidOperationException(),
        };

        static CompressedTag? GetCompressedTagAndRemoveDuplicates(ICollection<VariableLengthRecord> records)
        {
            var compressedTags = records.OfType<CompressedTag>().ToList();
            if (compressedTags is { Count: 0 })
            {
                return default;
            }

            var compressedTag = compressedTags[0];

            if (compressedTags is { Count: 1 })
            {
                return compressedTag;
            }

            // remove all but the first VLR
            foreach (var record in compressedTags.Skip(1))
            {
                _ = records.Remove(record);
            }

            return compressedTag;
        }
    }
#endif

    private void CloseWriting()
    {
        if (this.closedWriting)
        {
            return;
        }

        this.pointWriter?.Close(this.BaseStream);
        this.pointWriter = null;

#if LAS1_4_OR_GREATER
        // write out the extended variable length records, with the special ones after the standard ones.
        foreach (var (record, special, callback) in this.extendedVariableLengthRecords
            .OrderBy(static kvp => kvp.Special))
        {
            WriteImpl(record, special, callback);
        }
#endif

        this.closedWriting = true;

#if LAS1_4_OR_GREATER
        void WriteImpl(ExtendedVariableLengthRecord record, bool special, Action<long>? callback)
        {
            if (!special)
            {
                callback?.Invoke(this.WriteExtendedVariableLengthRecord(record));
                return;
            }

            // write this as a special EVLR in the compressed tag, if we can
            if (!this.BaseStream.CanSeek || this.offsetToLasZipVlr < 0)
            {
                return;
            }

            _ = this.BaseStream.SwitchStreamIfMultiple(LasStreams.VariableLengthRecord);
            _ = this.BaseStream.Seek(this.offsetToLasZipVlr, SeekOrigin.Begin);
            var compressedTag = HeaderBlockReader.GetVariableLengthRecord(this.BaseStream) as CompressedTag
                                ?? throw new InvalidOperationException(Compression.Properties.Resources.FailedToReadCompressedTag);

            var numberOfSpecialEvlrs = compressedTag.NumOfSpecialEvlrs;
            var offsetToSpecialEvlrs = compressedTag.OffsetToSpecialEvlrs;

            if (numberOfSpecialEvlrs is -1 && offsetToSpecialEvlrs is -1)
            {
                numberOfSpecialEvlrs = 1;
                offsetToSpecialEvlrs = this.BaseStream.Length;
            }

            _ = this.BaseStream.SwitchStreamIfMultiple(LazStreams.SpecialExtendedVariableLengthRecord);
            _ = this.BaseStream.Seek(offsetToSpecialEvlrs, SeekOrigin.Begin);

            // write the evlr
            callback?.Invoke(this.BaseStream.Position);

            var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent((int)record.Size());
            var bytesWritten = record.CopyTo(bytes);
            this.BaseStream.Write(bytes, 0, bytesWritten);

            var position = this.BaseStream.Position;

            if (numberOfSpecialEvlrs is -1)
            {
                return;
            }

            _ = this.BaseStream.SwitchStreamIfMultiple(LasStreams.VariableLengthRecord);
            _ = this.BaseStream.Seek(this.offsetToLasZipVlr + 54 + 16, SeekOrigin.Begin);
            this.BaseStream.WriteInt64LittleEndian(numberOfSpecialEvlrs);
            this.BaseStream.WriteInt64LittleEndian(offsetToSpecialEvlrs);
            _ = this.BaseStream.Seek(position, SeekOrigin.Begin);
        }
#endif
    }

#if LAS1_4_OR_GREATER
    private readonly record struct EvlrRecord(ExtendedVariableLengthRecord Record, bool Special, Action<long>? Callback);
#endif
}