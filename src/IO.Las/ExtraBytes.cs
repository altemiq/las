// -----------------------------------------------------------------------
// <copyright file="ExtraBytes.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The Extra Bytes VLR provides a mechanism whereby additional information can be added to the end of a standard Point Record.
/// </summary>
[System.Runtime.CompilerServices.CollectionBuilder(typeof(ExtraBytes), nameof(Create))]
public sealed record ExtraBytes : VariableLengthRecord, IReadOnlyList<ExtraBytesItem>
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 4;

    private readonly IReadOnlyList<ExtraBytesItem> items;

    private readonly int[] indexes;

    /// <summary>
    /// Initialises a new instance of the <see cref="ExtraBytes"/> class.
    /// </summary>
    /// <param name="items">The items.</param>
    public ExtraBytes(params IReadOnlyList<ExtraBytesItem> items)
        : this(CreateHeader(items.Count), items)
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="ExtraBytes"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The items.</param>
    internal ExtraBytes(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : this(header, GetEntries(data))
    {
    }

    private ExtraBytes(VariableLengthRecordHeader header, IReadOnlyList<ExtraBytesItem> items)
        : base(header)
    {
        this.items = items;
        this.indexes = this.CreateIndexes();
    }

    private ExtraBytes(ReadOnlySpan<ExtraBytesItem> items)
        : this(CreateHeader(items.Length), GetEntries(items))
    {
    }

    /// <inheritdoc />
    public int Count => this.items.Count;

    /// <inheritdoc />
    public ExtraBytesItem this[int index] => this.items[index];

    /// <summary>
    /// Creates an instance of <see cref="ExtraBytes"/>.
    /// </summary>
    /// <param name="items">The values.</param>
    /// <returns>The <see cref="ExtraBytes"/>.</returns>
    public static ExtraBytes Create(ReadOnlySpan<ExtraBytesItem> items) => new(items);

    /// <inheritdoc />
    public override int Write(Span<byte> destination)
    {
        this.Header.Write(destination);
        int bytesWritten = VariableLengthRecordHeader.Size;

        var d = destination[bytesWritten..];

        foreach (var item in this.items)
        {
            item.Write(d);
            bytesWritten += 192;
            d = destination[bytesWritten..];
        }

        return bytesWritten;
    }

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public object? GetData(int index, ReadOnlySpan<byte> source) => this.items[index].GetData(source[this.indexes[index]..]);

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public ValueTask<object?> GetDataAsync(int index, ReadOnlyMemory<byte> source) => this.items[index].GetDataAsync(source[this.indexes[index]..]);

    /// <inheritdoc />
    public IEnumerator<ExtraBytesItem> GetEnumerator() => this.items.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.items.GetEnumerator();

    /// <summary>
    /// Gets the byte count.
    /// </summary>
    /// <returns>The byte count.</returns>
    internal ushort GetByteCount() => (ushort)this.items.Sum(GetByteCount);

    private static System.Collections.ObjectModel.ReadOnlyCollection<ExtraBytesItem> GetEntries(ReadOnlySpan<byte> source)
    {
        var count = source.Length / 192;
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<ExtraBytesItem>(count);

        for (int i = 0; i < count; i++)
        {
            var index = i * 192;
            builder.Add(ExtraBytesItem.Read(source[index..]));
        }

        return builder.ToReadOnlyCollection();
    }

    private static System.Collections.ObjectModel.ReadOnlyCollection<ExtraBytesItem> GetEntries(ReadOnlySpan<ExtraBytesItem> source)
    {
        var count = source.Length;
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<ExtraBytesItem>(count);

        for (int i = 0; i < count; i++)
        {
            builder.Add(source[i]);
        }

        return builder.ToReadOnlyCollection();
    }

    private static int GetByteCount(ExtraBytesItem item) =>
        item switch
        {
            { DataType: ExtraBytesDataType.Undocumented, Options: var options } => (ushort)options,
            { DataType: ExtraBytesDataType.UnsignedChar or ExtraBytesDataType.Char } => sizeof(byte),
            { DataType: ExtraBytesDataType.UnsignedShort or ExtraBytesDataType.Short } => sizeof(short),
            { DataType: ExtraBytesDataType.UnsignedLong or ExtraBytesDataType.Long } => sizeof(int),
            { DataType: ExtraBytesDataType.UnsignedLongLong or ExtraBytesDataType.LongLong } => sizeof(long),
            { DataType: ExtraBytesDataType.Float } => sizeof(float),
            { DataType: ExtraBytesDataType.Double } => sizeof(double),
            _ => default,
        };

    private static VariableLengthRecordHeader CreateHeader(int count) =>
        new()
        {
            UserId = VariableLengthRecordHeader.SpecUserId,
            RecordId = TagRecordId,
            RecordLengthAfterHeader = (ushort)(count * 192),
        };

    private int[] CreateIndexes()
    {
        var result = new int[this.items.Count];
        for (var i = 1; i < this.items.Count; i++)
        {
            result[i] = result[i - 1] + GetByteCount(this.items[i - 1]);
        }

        return result;
    }
}