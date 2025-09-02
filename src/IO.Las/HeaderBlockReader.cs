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
    /// Gets the file signature.
    /// </summary>
    /// <returns>The file signature.</returns>
    public string GetFileSignature()
    {
        // do a cache on the stream
        if (stream is ICacheStream prepareStream)
        {
            prepareStream.Cache(0, 2024);
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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        Span<byte> buffer = stackalloc byte[4];
        _ = stream.Read(buffer);
        var stringValue = System.Text.Encoding.ASCII.GetString(buffer);
#else
        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(4);
        _ = stream.Read(byteArray, 0, 4);
        var stringValue = System.Text.Encoding.ASCII.GetString(byteArray, 0, 4);
        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
#endif
        return stringValue;
    }

    /// <summary>
    /// Moves to the variable length records.
    /// </summary>
    /// <returns><see langword="true"/> if the current position is at the start of the variable length records.</returns>
    public bool MoveToVariableLengthRecords()
    {
        if (stream is ICacheStream prepareStream)
        {
            prepareStream.Cache(this.headerSize, (int)(this.OffsetToPointData - this.headerSize));
        }

        stream.MoveToPositionForwardsOnly(this.headerSize);

        return stream.Position == this.headerSize;
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Moves to the extended variable length records.
    /// </summary>
    /// <returns><see langword="true"/> if the current position is at the start of the extended variable length records.</returns>
    public bool MoveToExtendedVariableLengthRecords() => this.MoveToExtendedVariableLengthRecords((long)this.startOfFirstExtendedVariableLengthRecord);
#endif

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Moves to the extended variable length records.
    /// </summary>
    /// <param name="startOfFirstExtendedVariableLengthRecord">The start of the first extended variable length record.</param>
    /// <returns><see langword="true"/> if the current position is at the start of the extended variable length records.</returns>
    public bool MoveToExtendedVariableLengthRecords(long startOfFirstExtendedVariableLengthRecord)
    {
        if (stream is ICacheStream prepareStream)
        {
            prepareStream.Cache(startOfFirstExtendedVariableLengthRecord);
        }

        stream.MoveToPositionForwardsOnly(startOfFirstExtendedVariableLengthRecord);

        return stream.Position == startOfFirstExtendedVariableLengthRecord;
    }
#endif

    /// <summary>
    /// Gets the variable length record.
    /// </summary>
    /// <returns>The next <see cref="VariableLengthRecord"/>.</returns>
    public VariableLengthRecord GetVariableLengthRecord()
    {
        _ = this.MoveToVariableLengthRecords();
        return GetVariableLengthRecord(stream);
    }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="vlrs">The variable length records.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    public ExtendedVariableLengthRecord GetExtendedVariableLengthRecord(IEnumerable<VariableLengthRecord> vlrs) => this.GetExtendedVariableLengthRecord(vlrs, (long)this.startOfFirstExtendedVariableLengthRecord);
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
        var byteArray = new byte[54];
        _ = stream.Read(byteArray, 0, byteArray.Length);
        var header = VariableLengthRecordHeader.Read(byteArray);

        byteArray = new byte[header.RecordLengthAfterHeader];
        _ = stream.Read(byteArray, 0, header.RecordLengthAfterHeader);

        return VariableLengthRecordProcessor.Instance.Process(header, byteArray);
    }

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="vlrs">The variable length records.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    internal static ExtendedVariableLengthRecord GetExtendedVariableLengthRecord(Stream stream, IEnumerable<VariableLengthRecord> vlrs)
    {
        var position = stream.Position;
        var byteArray = new byte[60];
        _ = stream.Read(byteArray, 0, byteArray.Length);
        var header = ExtendedVariableLengthRecordHeader.Read(byteArray);

        byteArray = new byte[header.RecordLengthAfterHeader];
        _ = stream.Read(byteArray, 0, byteArray.Length);

        return VariableLengthRecordProcessor.Instance.Process(header, vlrs, position, byteArray);
    }

    /// <summary>
    /// Gets the extended variable length record.
    /// </summary>
    /// <param name="vlrs">The variable length records.</param>
    /// <param name="startOfFirstExtendedVariableLengthRecord">The start of the first extended variable length record.</param>
    /// <returns>The next <see cref="ExtendedVariableLengthRecord"/>.</returns>
    internal ExtendedVariableLengthRecord GetExtendedVariableLengthRecord(IEnumerable<VariableLengthRecord> vlrs, long startOfFirstExtendedVariableLengthRecord)
    {
        _ = this.MoveToExtendedVariableLengthRecords(startOfFirstExtendedVariableLengthRecord);
        return GetExtendedVariableLengthRecord(stream, vlrs);
    }
#endif

    private HeaderBlock GetHeaderBlockImpl(string fileSignature)
    {
        if (fileSignature is not "LASF")
        {
            throw new ArgumentException(string.Format(Properties.Resources.Culture, Properties.Resources.InvalidSignature, "LASF", fileSignature), nameof(fileSignature));
        }

        var byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(92);
        _ = stream.Read(byteArray, 0, 92);
        ReadOnlySpan<byte> source = byteArray;

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

        this.headerSize = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[90..92]);

        // read the rest of the header
        var bytesLeft = this.headerSize - 92;
        if (byteArray.Length < bytesLeft)
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);
            byteArray = System.Buffers.ArrayPool<byte>.Shared.Rent(bytesLeft);
        }

        _ = stream.Read(byteArray, 0, bytesLeft);
        source = byteArray;

        this.OffsetToPointData = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[..4]);
        this.VariableLengthRecordCount = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[4..8]);
        builder.PointDataFormat = source[8];
        this.PointDataLength = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[9..11]);
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
        if (this.headerSize > HeaderBlock.Size10)
        {
            this.startOfWaveformData = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source[131..139]);
        }
#endif

#if LAS1_4_OR_GREATER
        if (this.headerSize > HeaderBlock.Size13)
        {
            this.startOfFirstExtendedVariableLengthRecord = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source[139..147]);
            this.ExtendedVariableLengthRecordCount = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[147..151]);
        }

        if (this.headerSize > HeaderBlock.Size13 + sizeof(ulong) + sizeof(uint))
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
        if (this.headerSize > HeaderBlock.Size15)
        {
            builder.MaxGpsTime = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[279..287]);
            builder.MinGpsTime = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[287..295]);
            builder.TimeOffset = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[295..297]);
        }
#endif

        System.Buffers.ArrayPool<byte>.Shared.Return(byteArray);

        return builder.HeaderBlock;
    }
}