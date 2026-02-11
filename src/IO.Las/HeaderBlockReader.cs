// -----------------------------------------------------------------------
// <copyright file="HeaderBlockReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="HeaderBlock"/> reader.
/// </summary>
/// <param name="stream">The input stream.</param>
public class HeaderBlockReader(Stream stream)
{
    private ushort headerSize;

#if LAS1_3_OR_GREATER
    private ulong startOfWaveformData;
#endif

#if LAS1_4_OR_GREATER
    private ulong startOfFirstExtendedVariableLengthRecord;
#endif

    /// <summary>
    /// Gets the offset to point data.
    /// </summary>
    public uint OffsetToPointData { get; private set; }

    /// <summary>
    /// Gets the length of the point data.
    /// </summary>
    /// <value>The length of the point data.</value>
    public ushort PointDataLength { get; private set; }

    /// <summary>
    /// Gets the variable length record count.
    /// </summary>
    public uint VariableLengthRecordCount { get; private set; }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the extended variable length record count.
    /// </summary>
    public uint ExtendedVariableLengthRecordCount { get; private set; }
#endif

    /// <summary>
    /// Gets the file signature.
    /// </summary>
    /// <returns>The file signature.</returns>
    public string GetFileSignature()
    {
        _ = stream.SwitchStreamIfMultiple(LasStreams.Header);

        // do a cache on the stream
        if (stream is ICacheStream cacheStream)
        {
            cacheStream.Cache(0, 2024);
        }

        // move this to the start if we can
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
        else if (stream is { Position: not 0 })
        {
            // throw an error, as we need to read this from the start of the stream
        }

        // read this
        Span<byte> buffer = stackalloc byte[4];
        _ = stream.Read(buffer);
        return System.Text.Encoding.ASCII.GetString(buffer);
    }

    /// <summary>
    /// Gets the file signature asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The file signature.</returns>
    public async Task<string> GetFileSignatureAsync(CancellationToken cancellationToken = default)
    {
        _ = stream.SwitchStreamIfMultiple(LasStreams.Header);

        switch (stream)
        {
            case IAsyncCacheStream asyncCacheStream:
                await asyncCacheStream.CacheAsync(0, 2024, cancellationToken).ConfigureAwait(false);
                break;
            case ICacheStream cacheStream:
                cacheStream.Cache(0, 2024);
                break;
        }

        // move this to the start if we can
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
        else if (stream is { Position: not 0 })
        {
            // throw an error, as we need to read this from the start of the stream
        }

        // read this
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(4);
        _ = await stream.ReadAsync(buffer.AsMemory(0, 4), cancellationToken).ConfigureAwait(false);
        var stringValue = System.Text.Encoding.ASCII.GetString(buffer.AsSpan(0, 4));
        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        return stringValue;
    }

    /// <summary>
    /// Gets the header block.
    /// </summary>
    /// <returns>The <see cref="HeaderBlock"/>.</returns>
    public HeaderBlock GetHeaderBlock() => this.GetHeaderBlock(this.GetFileSignature());

    /// <summary>
    /// Gets the header block.
    /// </summary>
    /// <param name="fileSignature">The file signature.</param>
    /// <returns>The <see cref="HeaderBlock"/>.</returns>
    public HeaderBlock GetHeaderBlock(string fileSignature) => this.GetHeaderBlockImpl(fileSignature);

    /// <summary>
    /// Gets the header block.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The <see cref="HeaderBlock"/>.</returns>
    public async Task<HeaderBlock> GetHeaderBlockAsync(CancellationToken cancellationToken = default) => await this.GetHeaderBlockImplAsync(await this.GetFileSignatureAsync(cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the header block.
    /// </summary>
    /// <param name="fileSignature">The file signature.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The <see cref="HeaderBlock"/>.</returns>
    public Task<HeaderBlock> GetHeaderBlockAsync(string fileSignature, CancellationToken cancellationToken = default) => this.GetHeaderBlockImplAsync(fileSignature, cancellationToken);

    /// <summary>
    /// Moves to the variable length records.
    /// </summary>
    /// <returns><see langword="true"/> if the current position is at the start of the variable length records.</returns>
    public bool MoveToVariableLengthRecords()
    {
        this.ThrowIfNotInitialized();

        _ = stream.SwitchStreamIfMultiple(LasStreams.VariableLengthRecord);

        if (stream is ICacheStream cacheStream)
        {
            cacheStream.Cache(this.headerSize, (int)(this.OffsetToPointData - this.headerSize));
        }

        stream.MoveToPositionForwardsOnly(this.headerSize);

        return stream.Position == this.headerSize;
    }

    /// <summary>
    /// Moves to the variable length records.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns><see langword="true"/> if the current position is at the start of the variable length records.</returns>
    public async Task<bool> MoveToVariableLengthRecordsAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfNotInitialized();

        _ = stream.SwitchStreamIfMultiple(LasStreams.VariableLengthRecord);

        switch (stream)
        {
            case IAsyncCacheStream asyncCacheStream:
                await asyncCacheStream.CacheAsync(this.headerSize, (int)(this.OffsetToPointData - this.headerSize), cancellationToken).ConfigureAwait(false);
                break;
            case ICacheStream cacheStream:
                cacheStream.Cache(this.headerSize, (int)(this.OffsetToPointData - this.headerSize));
                break;
        }

        await stream.MoveToPositionForwardsOnlyAsync(this.headerSize, cancellationToken).ConfigureAwait(false);

        return stream.Position == this.headerSize;
    }

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Moves to the extended variable length records.
    /// </summary>
    /// <param name="position">The start of the first extended variable length record.</param>
    /// <returns><see langword="true"/> if the current position is at the start of the extended variable length records.</returns>
    public bool MoveToExtendedVariableLengthRecords(long position)
    {
        this.ThrowIfNotInitialized();
        if (position < this.OffsetToPointData)
        {
            return false;
        }

        _ = stream.SwitchStreamIfMultiple(LasStreams.ExtendedVariableLengthRecord);

        if (stream is ICacheStream cacheStream)
        {
            cacheStream.Cache(position);
        }

        stream.MoveToPositionForwardsOnly(position);

        return stream.Position == position;
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Moves to the extended variable length records.
    /// </summary>
    /// <returns><see langword="true"/> if the current position is at the start of the extended variable length records.</returns>
    public bool MoveToExtendedVariableLengthRecords() => this.MoveToExtendedVariableLengthRecords((long)this.startOfFirstExtendedVariableLengthRecord);
#endif

    /// <summary>
    /// Moves to the extended variable length records asynchronously.
    /// </summary>
    /// <param name="position">The start of the first extended variable length record.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns><see langword="true"/> if the current position is at the start of the extended variable length records.</returns>
    public async Task<bool> MoveToExtendedVariableLengthRecordsAsync(long position, CancellationToken cancellationToken = default)
    {
        this.ThrowIfNotInitialized();
        if (position < this.OffsetToPointData)
        {
            return false;
        }

        _ = stream.SwitchStreamIfMultiple(LasStreams.ExtendedVariableLengthRecord);

        switch (stream)
        {
            case IAsyncCacheStream asyncCacheStream:
                await asyncCacheStream.CacheAsync(position, cancellationToken).ConfigureAwait(false);
                break;
            case ICacheStream cacheStream:
                cacheStream.Cache(position);
                break;
        }

        await stream.MoveToPositionForwardsOnlyAsync(position, cancellationToken).ConfigureAwait(false);

        return stream.Position == position;
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Moves to the extended variable length records asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns><see langword="true"/> if the current position is at the start of the extended variable length records.</returns>
    public Task<bool> MoveToExtendedVariableLengthRecordsAsync(CancellationToken cancellationToken = default) => this.MoveToExtendedVariableLengthRecordsAsync((long)this.startOfFirstExtendedVariableLengthRecord, cancellationToken);
#endif
#endif

    /// <summary>
    /// Gets the variable length record.
    /// </summary>
    /// <returns>The next <see cref="VariableLengthRecord"/>.</returns>
    public VariableLengthRecord GetVariableLengthRecord()
    {
        if (this.IsInVariableLengthRecords() || this.MoveToVariableLengthRecords())
        {
            return GetVariableLengthRecord(stream);
        }

        throw new InvalidDataException("Failed to move to variable length records");
    }

    /// <summary>
    /// Gets the variable length record asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The next <see cref="VariableLengthRecord"/>.</returns>
    public async Task<VariableLengthRecord> GetVariableLengthRecordAsync(CancellationToken cancellationToken = default)
    {
        if (this.IsInVariableLengthRecords() || await this.MoveToVariableLengthRecordsAsync(cancellationToken).ConfigureAwait(false))
        {
            return await GetVariableLengthRecordAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidDataException("Failed to move to variable length records");
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    public ExtendedVariableLengthRecord GetExtendedVariableLengthRecord(IEnumerable<VariableLengthRecord> records) => this.GetExtendedVariableLengthRecord(records, (long)this.startOfFirstExtendedVariableLengthRecord);

    /// <summary>
    /// Gets the extended variable length record asynchronously.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    public Task<ExtendedVariableLengthRecord> GetExtendedVariableLengthRecordAsync(IEnumerable<VariableLengthRecord> records, CancellationToken cancellationToken = default) =>
        this.GetExtendedVariableLengthRecordAsync(records, (long)this.startOfFirstExtendedVariableLengthRecord, cancellationToken);
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the end position of the point data records.
    /// </summary>
    /// <returns>The end position of the point data records.</returns>
#pragma warning disable IDE0055
    public long GetEndOfPointDataRecords() => (this.startOfWaveformData, this.startOfFirstExtendedVariableLengthRecord) switch
    {
        (> 0, > 0) => (long)Math.Min(this.startOfWaveformData, this.startOfFirstExtendedVariableLengthRecord),
        (> 0, _) => (long)this.startOfWaveformData,
        (_, > 0) => (long)this.startOfFirstExtendedVariableLengthRecord,
        _ => stream.Length,
    };
#pragma warning restore IDE0055
#elif LAS1_3_OR_GREATER
    /// <summary>
    /// Gets the end position of the point data records.
    /// </summary>
    /// <returns>The end position of the point data records.</returns>
    public long GetEndOfPointDataRecords() => this.startOfWaveformData switch
    {
        > 0 => (long)this.startOfWaveformData,
        _ => stream.Length,
    };
#else
    /// <summary>
    /// Gets the end position of the point data records.
    /// </summary>
    /// <returns>The end position of the point data records.</returns>
#pragma warning disable IDE0055
    public long GetEndOfPointDataRecords() => stream.Length;
#endif

    /// <summary>
    /// Gets the variable length record.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The next <see cref="VariableLengthRecord"/>.</returns>
    internal static VariableLengthRecord GetVariableLengthRecord(Stream stream)
    {
        Span<byte> byteArray = stackalloc byte[54];
        _ = stream.Read(byteArray);

        var header = VariableLengthRecordHeader.Create(byteArray);
        if (header.RecordLengthAfterHeader is not 0 and var recordLengthAfterHeader)
        {
            byteArray = stackalloc byte[recordLengthAfterHeader];
            _ = stream.Read(byteArray);
        }

        return VariableLengthRecordProcessor.Instance.Process(header, byteArray);
    }

    /// <summary>
    /// Gets the variable length record asynchronously.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The next <see cref="VariableLengthRecord"/>.</returns>
    internal static async Task<VariableLengthRecord> GetVariableLengthRecordAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(54);
        _ = await stream.ReadAsync(byteArray.AsMemory(0, 54), cancellationToken).ConfigureAwait(false);

        var header = VariableLengthRecordHeader.Create(byteArray);

        var recordLengthAfterHeader = header.RecordLengthAfterHeader;
        if (recordLengthAfterHeader is not 0)
        {
            if (recordLengthAfterHeader > byteArray.Length)
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
                byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(recordLengthAfterHeader);
            }

            _ = await stream.ReadAsync(byteArray.AsMemory(0, recordLengthAfterHeader), cancellationToken).ConfigureAwait(false);
        }

        var vlr = VariableLengthRecordProcessor.Instance.Process(header, byteArray.AsSpan(0, recordLengthAfterHeader));

        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

        return vlr;
    }

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="records">The variable length records.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    internal static ExtendedVariableLengthRecord GetExtendedVariableLengthRecord(Stream stream, IEnumerable<VariableLengthRecord> records)
    {
        var position = stream.Position;

        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(60);
        _ = stream.Read(byteArray.AsSpan(0, 60));
        var header = ExtendedVariableLengthRecordHeader.Create(byteArray);

        var recordLengthAfterHeader = (int)header.RecordLengthAfterHeader;
        if (recordLengthAfterHeader is not 0)
        {
            if (recordLengthAfterHeader > byteArray.Length)
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
                byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(recordLengthAfterHeader);
            }

            _ = stream.Read(byteArray.AsSpan(0, recordLengthAfterHeader));
        }

        var vlr = VariableLengthRecordProcessor.Instance.Process(header, records, position, byteArray.AsSpan(0, recordLengthAfterHeader));

        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

        return vlr;
    }

    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="records">The variable length records.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    internal static async Task<ExtendedVariableLengthRecord> GetExtendedVariableLengthRecordAsync(Stream stream, IEnumerable<VariableLengthRecord> records, CancellationToken cancellationToken = default)
    {
        var position = stream.Position;

        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(60);
        _ = await stream.ReadAsync(byteArray.AsMemory(0, 60), cancellationToken).ConfigureAwait(false);
        var header = ExtendedVariableLengthRecordHeader.Create(byteArray);

        var recordLengthAfterHeader = (int)header.RecordLengthAfterHeader;
        if (recordLengthAfterHeader is not 0)
        {
            if (recordLengthAfterHeader > byteArray.Length)
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
                byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(recordLengthAfterHeader);
            }

            _ = await stream.ReadAsync(byteArray.AsMemory(0, recordLengthAfterHeader), cancellationToken).ConfigureAwait(false);
        }

        var vlr = VariableLengthRecordProcessor.Instance.Process(header, records, position, byteArray.AsSpan(0, recordLengthAfterHeader));

        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

        return vlr;
    }

    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="extendedVariableLengthRecordPosition">The start of the first extended variable length record.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    internal ExtendedVariableLengthRecord GetExtendedVariableLengthRecord(IEnumerable<VariableLengthRecord> records, long extendedVariableLengthRecordPosition) =>
        this.IsInExtendedVariableLengthRecords(extendedVariableLengthRecordPosition) || this.MoveToExtendedVariableLengthRecords(extendedVariableLengthRecordPosition)
            ? GetExtendedVariableLengthRecord(stream, records)
            : throw new InvalidDataException("Failed to move to extended variable length records");

    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="records">The variable length records.</param>
    /// <param name="extendedVariableLengthRecordPosition">The start of the first extended variable length record.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    internal async Task<ExtendedVariableLengthRecord> GetExtendedVariableLengthRecordAsync(IEnumerable<VariableLengthRecord> records, long extendedVariableLengthRecordPosition, CancellationToken cancellationToken = default) =>
        this.IsInExtendedVariableLengthRecords(extendedVariableLengthRecordPosition) || await this.MoveToExtendedVariableLengthRecordsAsync(extendedVariableLengthRecordPosition, cancellationToken).ConfigureAwait(false)
            ? await GetExtendedVariableLengthRecordAsync(stream, records, cancellationToken).ConfigureAwait(false)
            : throw new InvalidDataException("Failed to move to extended variable length records");
#endif

    private static (HeaderBlockBuilder Builder, ushort HeaderSize) CreateHeaderBlockBuilder(ReadOnlySpan<byte> source)
    {
        var builder = new HeaderBlockBuilder
        {
            FileSourceId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[..2]),
#if LAS1_2_OR_GREATER
            GlobalEncoding = (GlobalEncoding)System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[2..4]),
#endif
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            ProjectId = new(source[4..20]),
#endif
            Version = new(source[20], source[21]),
            SystemIdentifier = System.Text.Encoding.UTF8.GetNullTerminatedString(source[22..54]),
            GeneratingSoftware = System.Text.Encoding.UTF8.GetNullTerminatedString(source[54..86]),
        };

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
        var guid1 = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[4..8]);
        var guid2 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[8..10]);
        var guid3 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[10..12]);
        builder.ProjectId = new(
            (int)guid1,
            (short)guid2,
            (short)guid3,
            source[12],
            source[13],
            source[14],
            source[15],
            source[16],
            source[17],
            source[18],
            source[19]);
#endif

        // file creation
        var days = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[86..88]);
        var years = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[88..90]);
        if (years is not 0)
        {
            var fileCreation = new DateTime(years, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            if (days > 0)
            {
                fileCreation = fileCreation.AddDays(days - 1);
            }

            builder.FileCreation = fileCreation;
        }

        return (builder, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[90..92]));
    }

#if LAS1_3_OR_GREATER
    private static HeaderValues ReadRestOfHeader(HeaderBlockBuilder builder, ReadOnlySpan<byte> source, ushort headerSize)
#else
    private static HeaderValues ReadRestOfHeader(HeaderBlockBuilder builder, ReadOnlySpan<byte> source)
#endif
    {
        var headerValues = new HeaderValues(
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[..4]),
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[4..8]),
            System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[9..11]));
        builder.PointDataFormat = source[8];
#if LAS1_4_OR_GREATER
        builder.LegacyNumberOfPointRecords = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[11..15]);
#else
        builder.NumberOfPointRecords = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[11..15]);
#endif

        // number of points by return
        for (var i = 0; i < 5; i++)
        {
            var start = 15 + (i * sizeof(uint));
#if LAS1_4_OR_GREATER
            builder.LegacyNumberOfPointsByReturn[i]
#else
            builder.NumberOfPointsByReturn[i]
#endif
                = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[start..]);
        }

        builder.ScaleFactor = new(System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[35..43]), System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[43..51]), System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[51..59]));
        builder.Offset = new(System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[59..67]), System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[67..75]), System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[75..83]));

        var maxX = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[83..91]);
        var minX = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[91..99]);

        var maxY = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[99..107]);
        var minY = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[107..115]);

        var maxZ = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[115..123]);
        var minZ = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[123..131]);

        builder.Max = new(maxX, maxY, maxZ);
        builder.Min = new(minX, minY, minZ);

#if LAS1_3_OR_GREATER
        if (headerSize > HeaderBlock.Size10)
        {
            headerValues = headerValues with { StartOfWaveformData = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source[131..139]) };
        }
#endif

#if LAS1_4_OR_GREATER
        if (headerSize > HeaderBlock.Size13)
        {
            headerValues = headerValues with
            {
                StartOfFirstExtendedVariableLengthRecord = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source[139..147]),
                ExtendedVariableLengthRecordCount = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[147..151]),
            };
        }

        if (headerSize > HeaderBlock.Size13 + sizeof(ulong) + sizeof(uint))
        {
            builder.NumberOfPointRecords = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source[151..159]);
            for (var i = 0; i < 15; i++)
            {
                var start = 159 + (i * sizeof(ulong));
                var end = start + sizeof(ulong);
                builder.NumberOfPointsByReturn[i] = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source[start..end]);
            }
        }
        else
        {
            builder.NumberOfPointRecords = builder.LegacyNumberOfPointRecords;
            for (var i = 0; i < 5; i++)
            {
                builder.NumberOfPointsByReturn[i] = builder.LegacyNumberOfPointsByReturn[i];
            }
        }
#endif

#if LAS1_5_OR_GREATER
        if (headerSize > HeaderBlock.Size15)
        {
            builder.MaxGpsTime = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[279..287]);
            builder.MinGpsTime = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[287..295]);
            builder.TimeOffset = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[295..297]);
        }
#endif

        return headerValues;
    }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use \'CompositeFormat\'", Justification = "This is for an exception")]
#endif
    private HeaderBlock GetHeaderBlockImpl(string fileSignature)
    {
        if (fileSignature is not "LASF")
        {
            throw new ArgumentException(string.Format(Properties.Resources.Culture, Properties.Resources.InvalidSignature, "LASF", fileSignature), nameof(fileSignature));
        }

        _ = stream.SwitchStreamIfMultiple(LasStreams.Header);
        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(92);
        var bytesRead = stream.Read(byteArray, 0, 92);

        (var builder, this.headerSize) = CreateHeaderBlockBuilder(byteArray.AsSpan(0, bytesRead));

        // read the rest of the header
        var bytesLeft = this.headerSize - 96;
        if (byteArray.Length < bytesLeft)
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
            byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(bytesLeft);
        }

        bytesRead = stream.Read(byteArray, 0, bytesLeft);

        var headerValues =
#if LAS1_3_OR_GREATER
            ReadRestOfHeader(builder, byteArray.AsSpan(0, bytesRead), this.headerSize);
#else
            ReadRestOfHeader(builder, byteArray.AsSpan(0, bytesRead));
#endif

        this.OffsetToPointData = headerValues.OffsetToPointData;
        this.VariableLengthRecordCount = headerValues.VariableLengthRecordCount;
        this.PointDataLength = headerValues.PointDataLength;

#if LAS1_3_OR_GREATER
        this.startOfWaveformData = headerValues.StartOfWaveformData;
#endif

#if LAS1_4_OR_GREATER
        this.startOfFirstExtendedVariableLengthRecord = headerValues.StartOfFirstExtendedVariableLengthRecord;
        this.ExtendedVariableLengthRecordCount = headerValues.ExtendedVariableLengthRecordCount;
#endif

        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

        return builder.HeaderBlock;
    }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use \'CompositeFormat\'", Justification = "This is for an exception")]
#endif
    private async Task<HeaderBlock> GetHeaderBlockImplAsync(string fileSignature, CancellationToken cancellationToken)
    {
        if (fileSignature is not "LASF")
        {
            throw new ArgumentException(string.Format(Properties.Resources.Culture, Properties.Resources.InvalidSignature, "LASF", fileSignature), nameof(fileSignature));
        }

        _ = stream.SwitchStreamIfMultiple(LasStreams.Header);
        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(92);
        var bytesRead = await stream.ReadAsync(byteArray.AsMemory(0, 92), cancellationToken).ConfigureAwait(false);

        (var builder, this.headerSize) = CreateHeaderBlockBuilder(byteArray.AsSpan(0, bytesRead));

        // read the rest of the header
        var bytesLeft = this.headerSize - 96;
        if (byteArray.Length < bytesLeft)
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
            byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(bytesLeft);
        }

        bytesRead = await stream.ReadAsync(byteArray.AsMemory(0, bytesLeft), cancellationToken).ConfigureAwait(false);

        var headerValues =
#if LAS1_3_OR_GREATER
            ReadRestOfHeader(builder, byteArray.AsSpan(0, bytesRead), this.headerSize);
#else
            ReadRestOfHeader(builder, byteArray.AsSpan(0, bytesRead));
#endif

        this.OffsetToPointData = headerValues.OffsetToPointData;
        this.VariableLengthRecordCount = headerValues.VariableLengthRecordCount;
        this.PointDataLength = headerValues.PointDataLength;

#if LAS1_3_OR_GREATER
        this.startOfWaveformData = headerValues.StartOfWaveformData;
#endif

#if LAS1_4_OR_GREATER
        this.startOfFirstExtendedVariableLengthRecord = headerValues.StartOfFirstExtendedVariableLengthRecord;
        this.ExtendedVariableLengthRecordCount = headerValues.ExtendedVariableLengthRecordCount;
#endif

        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

        return builder.HeaderBlock;
    }

    private void ThrowIfNotInitialized()
    {
        if (this.headerSize is 0)
        {
            // need to call GetHeaderBlock first
            throw new InvalidOperationException($"header size not initialized. {nameof(this.GetHeaderBlock)} or {nameof(this.GetHeaderBlockAsync)} must be called first");
        }
    }

    private bool IsInVariableLengthRecords() => stream.Position >= this.headerSize && stream.Position < this.OffsetToPointData;

#if LAS1_3_OR_GREATER
    private bool IsInExtendedVariableLengthRecords(long extendedVariableLengthRecordPosition) => extendedVariableLengthRecordPosition >= this.headerSize && stream.Position > extendedVariableLengthRecordPosition && stream.Position < stream.Length;
#endif

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct HeaderValues(uint OffsetToPointData, uint VariableLengthRecordCount, ushort PointDataLength)
    {
#if LAS1_3_OR_GREATER
        public ulong StartOfWaveformData { get; init; }
#endif

#if LAS1_4_OR_GREATER
        public ulong StartOfFirstExtendedVariableLengthRecord { get; init; }

        public uint ExtendedVariableLengthRecordCount { get; init; }
#endif
    }
}