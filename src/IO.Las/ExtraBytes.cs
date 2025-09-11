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
public sealed record ExtraBytes : VariableLengthRecord, IExtraBytes
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
        : this(
            new()
            {
                UserId = VariableLengthRecordHeader.SpecUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = (ushort)(items.Count * 192),
            },
            items)
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

    /// <inheritdoc />
    public int Count => this.items.Count;

    /// <inheritdoc />
    public ExtraBytesItem this[int index] => this.items[index];

    /// <summary>
    /// Creates an instance of <see cref="ExtraBytes"/>.
    /// </summary>
    /// <param name="items">The values.</param>
    /// <returns>The <see cref="ExtraBytes"/>.</returns>
    public static ExtraBytes Create(ReadOnlySpan<ExtraBytesItem> items) => new(items.ToReadOnlyList());

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
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
    /// Gets the data for the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public object? GetValue(int index, ReadOnlySpan<byte> source) => this.items[index].GetValue(source[this.indexes[index]..]);

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public IReadOnlyList<object?> GetValues(ReadOnlySpan<byte> source)
    {
        var values = new object?[this.items.Count];
        for (int i = 0; i < this.items.Count; i++)
        {
            values[i] = this.items[i].GetValue(source[this.indexes[i]..]);
        }

        return values;
    }

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public ValueTask<object?> GetValueAsync(int index, ReadOnlyMemory<byte> source) => this.items[index].GetValueAsync(source[this.indexes[index]..]);

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public async ValueTask<IReadOnlyList<object?>> GetValuesAsync(ReadOnlyMemory<byte> source)
    {
        var values = new object?[this.items.Count];
        for (int i = 0; i < this.items.Count; i++)
        {
            values[i] = await this.items[i].GetValueAsync(source[this.indexes[i]..]).ConfigureAwait(false);
        }

        return values;
    }

    /// <inheritdoc />
    public IEnumerator<ExtraBytesItem> GetEnumerator() => this.items.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.items.GetEnumerator();

    /// <summary>
    /// Gets the byte count.
    /// </summary>
    /// <returns>The byte count.</returns>
    internal ushort GetByteCount() => (ushort)this.items.Sum(GetByteCount);

    /// <summary>
    /// Gets the byte count.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The byte count.</returns>
    internal static int GetByteCount(ExtraBytesItem item) =>
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