// -----------------------------------------------------------------------
// <copyright file="GeoAsciiParamsTag.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// This record is simply an array of ASCII data.
/// It contains many strings separated by null terminator characters, which are referenced by position from data in the <see cref="GeoKeyDirectoryTag"/> record.
/// </summary>
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GeoAsciiParamsTag), nameof(Create))]
public record GeoAsciiParamsTag : VariableLengthRecord, IReadOnlyList<string>
{
    /// <summary>
    /// The tag record id.
    /// </summary>
    public const ushort TagRecordId = 34737;

    private readonly IReadOnlyList<string> strings;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoAsciiParamsTag"/> class.
    /// </summary>
    /// <param name="strings">The strings.</param>
    public GeoAsciiParamsTag(params IReadOnlyList<string> strings)
        : this(
            new()
            {
                UserId = VariableLengthRecordHeader.ProjectionUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = (ushort)strings.Sum(s => System.Text.Encoding.UTF8.GetByteCount(s) + 1),
            },
            strings)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoAsciiParamsTag"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal GeoAsciiParamsTag(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : this(header, GetStrings(data))
    {
    }

    private GeoAsciiParamsTag(VariableLengthRecordHeader header, IReadOnlyList<string> strings)
        : base(header) => this.strings = strings;

    /// <inheritdoc />
    public int Count => this.strings.Count;

    /// <inheritdoc />
    public string this[int index] => this.strings[index];

    /// <summary>
    /// Creates an instance of <see cref="GeoAsciiParamsTag"/>.
    /// </summary>
    /// <param name="items">The values.</param>
    /// <returns>The <see cref="GeoAsciiParamsTag"/>.</returns>
    public static GeoAsciiParamsTag Create(ReadOnlySpan<string> items) => new(items.ToReadOnlyList());

    /// <inheritdoc />
    public IEnumerator<string> GetEnumerator() => this.strings.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc />
    public override int Write(Span<byte> destination)
    {
        this.Header.Write(destination);
        int bytesWritten = VariableLengthRecordHeader.Size;
        var d = destination[bytesWritten..];

        foreach (var s in this.strings)
        {
            bytesWritten += System.Text.Encoding.ASCII.GetBytes(s, d);
            d[bytesWritten] = 0;
            bytesWritten++;
            d = destination[bytesWritten..];
        }

        return bytesWritten;
    }

    private static IReadOnlyList<string> GetStrings(ReadOnlySpan<byte> data)
    {
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<string>();
        var current = 0;
        var nextIndex = GetNextIndex(data, current);
        while (nextIndex is not -1)
        {
            builder.Add(System.Text.Encoding.ASCII.GetString(data[current..nextIndex]));
            current = nextIndex + 1;
            nextIndex = GetNextIndex(data, current);
        }

        builder.Add(System.Text.Encoding.ASCII.GetString(data[current..]));

        return builder.ToReadOnlyCollection();

        static int GetNextIndex(ReadOnlySpan<byte> data, int startIndex)
        {
            for (var i = startIndex; i < data.Length; i++)
            {
                if (data[i] is 0 or 124)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}