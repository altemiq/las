// -----------------------------------------------------------------------
// <copyright file="CompressedTag.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using System.Collections;

/// <summary>
/// Represents the compressed tag.
/// </summary>
/// <remarks>
/// <code>
/// the data of the LASzip VLR
///     ushort  compressor              2 bytes<br/>
///     ushort  coder                   2 bytes<br/>
///     byte    version_major           1 byte<br/>
///     byte    version_minor           1 byte<br/>
///     ushort  version_revision        2 bytes<br/>
///     uint    options                 4 bytes<br/>
///     uint    chunk_size              4 bytes<br/>
///     long    number_of_special_evlrs 8 bytes<br/>
///     long    offset_to_special_evlrs 8 bytes<br/>
///     ushort  num_items               2 bytes<br/>
///        ushort type                      2 bytes * num_items<br/>
///        ushort size                      2 bytes * num_items<br/>
///        ushort version                   2 bytes * num_items<br/>
/// which totals 34+6*num_items
/// </code>
/// </remarks>
public sealed record CompressedTag : VariableLengthRecord, IReadOnlyList<LasItem>
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 22204;

    /// <summary>
    /// The variable chunk size.
    /// </summary>
    public const int VariableChunkSize = unchecked((int)LasZip.VariableChunkSize);

    private const int CompressorOffset = 0;

    private const int CoderOffset = CompressorOffset + sizeof(ushort);

    private const int VersionMajorOffset = CoderOffset + sizeof(ushort);

    private const int VersionMinorOffset = VersionMajorOffset + sizeof(byte);

    private const int VersionRevisionOffset = VersionMinorOffset + sizeof(byte);

    private const int OptionsOffset = VersionRevisionOffset + sizeof(ushort);

    private const int ChunkSizeOffset = OptionsOffset + sizeof(uint);

#if LAS1_4_OR_GREATER
    private const int NumOfSpecialEvlrsOffset = ChunkSizeOffset + sizeof(uint);

    private const int OffsetToSpecialEvlrsOffset = NumOfSpecialEvlrsOffset + sizeof(long);

    private const int NumItemsOffset = OffsetToSpecialEvlrsOffset + sizeof(long);
#else
    private const int NumPointsOffset = ChunkSizeOffset + sizeof(uint);

    private const int NumBytesOffset = NumPointsOffset + sizeof(long);

    private const int NumItemsOffset = NumBytesOffset + sizeof(long);
#endif

    private const int ItemOffset = NumItemsOffset + sizeof(ushort);

#pragma warning disable CA1859
    private readonly IReadOnlyList<LasItem> items;
#pragma warning restore CA1859

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="header">The header block.</param>
    /// <param name="records">The variable length records.</param>
    /// <param name="compressor">The compressor.</param>
    public CompressedTag(in HeaderBlock header, IEnumerable<VariableLengthRecord> records, Compressor compressor)
        : this(header, records.OfType<IExtraBytes>().SingleOrDefault(), compressor)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="header">The header block.</param>
    /// <param name="extraBytes">The extra bytes record.</param>
    /// <param name="compressor">The compressor.</param>
    public CompressedTag(in HeaderBlock header, IExtraBytes? extraBytes, Compressor compressor)
        : this(header, (ushort)(extraBytes?.Sum(ExtraBytes.GetByteCount) ?? 0), compressor)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="header">The header block.</param>
    /// <param name="extraBytesCount">The extra-bytes count.</param>
    /// <param name="compressor">The compressor.</param>
    public CompressedTag(in HeaderBlock header, ushort extraBytesCount, Compressor compressor)
        : this(new LasZip(header.PointDataFormatId, extraBytesCount, compressor, LasZip.GetValidVersion(header)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="pointDataFormatId">The point data format ID.</param>
    /// <param name="extraBytesCount">The extra-bytes count.</param>
    /// <param name="compressor">The compressor.</param>
    /// <param name="version">The LAS version.</param>
    public CompressedTag(byte pointDataFormatId, ushort extraBytesCount, Compressor compressor, Version version)
        : this(new LasZip(pointDataFormatId, extraBytesCount, compressor, LasZip.GetValidVersion(pointDataFormatId, version)))
    {
    }
#else
    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="header">The header block.</param>
    /// <param name="compressor">The compressor.</param>
    public CompressedTag(in HeaderBlock header, Compressor compressor)
        : this(new LasZip(header.PointDataFormatId, compressor, LasZip.GetValidVersion(header)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="pointDataFormatId">The point data format ID.</param>
    /// <param name="compressor">The compressor.</param>
    /// <param name="version">The LAS version.</param>
    public CompressedTag(byte pointDataFormatId, Compressor compressor, Version version)
        : this(new LasZip(pointDataFormatId, compressor, LasZip.GetValidVersion(pointDataFormatId, version)))
    {
    }
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal CompressedTag(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : base(header)
    {
        this.Compressor = (Compressor)System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[..CoderOffset]);
        this.Coder = (Coder)System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[CoderOffset..VersionMajorOffset]);
        this.Version = new(data[VersionMajorOffset], data[VersionMinorOffset], 0, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[VersionRevisionOffset..OptionsOffset]));
        this.Options = (LazOptions)System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[OptionsOffset..ChunkSizeOffset]);
#if LAS1_4_OR_GREATER
        this.ChunkSize = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[ChunkSizeOffset..NumOfSpecialEvlrsOffset]);
        this.NumOfSpecialEvlrs = System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(data[NumOfSpecialEvlrsOffset..OffsetToSpecialEvlrsOffset]);
        this.OffsetToSpecialEvlrs = System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(data[OffsetToSpecialEvlrsOffset..NumItemsOffset]);
#else
        this.ChunkSize = (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[ChunkSizeOffset..NumItemsOffset]);
        this.NumPoints = System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(data[NumPointsOffset..NumBytesOffset]);
        this.NumBytes = System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(data[NumBytesOffset..NumItemsOffset]);
#endif
        var count = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[NumItemsOffset..ItemOffset]);
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<LasItem>(count);
        for (var i = 0; i < count; i++)
        {
            var start = ItemOffset + (i * 6);
            var s = data[start..];
            builder.Add(new()
            {
                Type = (LasItemType)System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(s[..2]),
                Size = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(s[2..4]),
                Version = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(s[4..6]),
            });
        }

        this.items = builder.ToReadOnlyCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedTag"/> class.
    /// </summary>
    /// <param name="zip">The las-zip instance.</param>
    internal CompressedTag(LasZip zip)
        : base(new VariableLengthRecordHeader
        {
            UserId = "laszip encoded",
            RecordId = TagRecordId,
            RecordLengthAfterHeader = (ushort)(34 + (6 * zip.Items.Count)),
            Description = $"by {typeof(CompressedTag).Namespace}",
        })
    {
        this.Compressor = zip.Compressor;
        this.Coder = zip.Coder;
        this.Version = LasZip.Version;
        this.Options = zip.Options;
        this.ChunkSize = (int)zip.ChunkSize;
#if LAS1_4_OR_GREATER
        this.NumOfSpecialEvlrs = -1;
        this.OffsetToSpecialEvlrs = -1;
#else
        this.NumPoints = -1;
        this.NumBytes = -1;
#endif
        this.items = [.. zip.Items];
    }

    /// <summary>
    /// Gets the compressor.
    /// </summary>
    public Compressor Compressor { get; init; }

    /// <summary>
    /// Gets the coder.
    /// </summary>
    public Coder Coder { get; init; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    public Version Version { get; init; }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public LazOptions Options { get; init; }

    /// <summary>
    /// Gets the size of the chunk.
    /// </summary>
    public int ChunkSize { get; init; }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the number of special <see cref="ExtendedVariableLengthRecord"/> instances.
    /// </summary>
    public long NumOfSpecialEvlrs { get; init; }

    /// <summary>
    /// Gets the offset to the special <see cref="ExtendedVariableLengthRecord"/> instances.
    /// </summary>
    public long OffsetToSpecialEvlrs { get; init; }
#else
    /// <summary>
    /// Gets the number of points.
    /// </summary>
    public long NumPoints { get; init; }

    /// <summary>
    /// Gets the number of bytes.
    /// </summary>
    public long NumBytes { get; init; }
#endif

    /// <inheritdoc />
    public int Count => this.items.Count;

    /// <inheritdoc />
    public LasItem this[int index] => this.items[index];

    /// <inheritdoc />
    public IEnumerator<LasItem> GetEnumerator() => this.items.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        var bytesWritten = VariableLengthRecordHeader.Size;

        // write out the values
        var d = destination[bytesWritten..];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[..CoderOffset], (ushort)this.Compressor);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[CoderOffset..VersionMajorOffset], (ushort)this.Coder);
        d[VersionMajorOffset] = (byte)this.Version.Major;
        d[VersionMinorOffset] = (byte)this.Version.Minor;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[VersionRevisionOffset..OptionsOffset], (ushort)this.Version.Revision);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(d[OptionsOffset..ChunkSizeOffset], (uint)this.Options);
#if LAS1_4_OR_GREATER
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(d[ChunkSizeOffset..NumOfSpecialEvlrsOffset], (uint)this.ChunkSize);
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(d[NumOfSpecialEvlrsOffset..OffsetToSpecialEvlrsOffset], this.NumOfSpecialEvlrs);
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(d[OffsetToSpecialEvlrsOffset..NumItemsOffset], this.OffsetToSpecialEvlrs);
#else
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(d[ChunkSizeOffset..NumPointsOffset], (uint)this.ChunkSize);
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(d[NumPointsOffset..NumBytesOffset], this.NumPoints);
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(d[NumBytesOffset..NumItemsOffset], this.NumBytes);
#endif
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[NumItemsOffset..ItemOffset], (ushort)this.Count);
        bytesWritten += ItemOffset;
        for (var i = 0; i < this.Count; i++)
        {
            var item = this[i];
            d = destination[bytesWritten..];

            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[..2], (ushort)item.Type);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[2..4], item.Size);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[4..6], item.Version);

            bytesWritten += 6;
        }

        return bytesWritten;
    }
}