// -----------------------------------------------------------------------
// <copyright file="GeoDoubleParamsTag.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// This record is simply an array of ASCII data.
/// It contains many strings separated by null terminator characters, which are referenced by position from data in the <see cref="GeoKeyDirectoryTag"/> record.
/// </summary>
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GeoDoubleParamsTag), nameof(Create))]
public record GeoDoubleParamsTag : VariableLengthRecord, IReadOnlyList<double>
{
    /// <summary>
    /// The tag record id.
    /// </summary>
    public const ushort TagRecordId = 34736;

    private readonly IReadOnlyList<double> values;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoDoubleParamsTag"/> class.
    /// </summary>
    /// <param name="values">The values.</param>
    public GeoDoubleParamsTag(params IReadOnlyList<double> values)
        : this(
            new()
            {
                UserId = VariableLengthRecordHeader.ProjectionUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = (ushort)(sizeof(double) * values.Count),
            },
            values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoDoubleParamsTag"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal GeoDoubleParamsTag(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : this(header, GetValues(data))
    {
    }

    private GeoDoubleParamsTag(VariableLengthRecordHeader header, IReadOnlyList<double> values)
        : base(header) => this.values = values;

    /// <inheritdoc />
    public int Count => this.values.Count;

    /// <inheritdoc />
    public double this[int index] => this.values[index];

    /// <summary>
    /// Gets the <see cref="double"/> with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get.</param>
    /// <returns>The <see cref="double"/> with the specified key.</returns>
    public double this[GeoKeyEntry key] => this.TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();

    /// <summary>
    /// Creates an instance of <see cref="GeoDoubleParamsTag"/>.
    /// </summary>
    /// <param name="items">The values.</param>
    /// <returns>The <see cref="GeoDoubleParamsTag"/>.</returns>
    public static GeoAsciiParamsTag Create(ReadOnlySpan<string> items) => new(items.ToReadOnlyList());

    /// <inheritdoc />
    public IEnumerator<double> GetEnumerator() => this.values.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Tries to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns><see langword="true"/> if <paramref name="key"/> is found; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue(GeoKeyEntry key, out double value)
    {
        if (key.TiffTagLocation is TagRecordId && key.ValueOffset < this.values.Count)
        {
            value = this.values[key.ValueOffset];
            return true;
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        int bytesWritten = VariableLengthRecordHeader.Size;
        var d = destination[bytesWritten..];

        foreach (var s in this.values)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(d, s);
            bytesWritten += sizeof(double);
            d = destination[bytesWritten..];
        }

        return bytesWritten;
    }

    private static System.Collections.ObjectModel.ReadOnlyCollection<double> GetValues(ReadOnlySpan<byte> data)
    {
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<double>();
        for (var i = 0; i < data.Length; i += sizeof(double))
        {
            builder.Add(System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(i, sizeof(double))));
        }

        return builder.ToReadOnlyCollection();
    }
}