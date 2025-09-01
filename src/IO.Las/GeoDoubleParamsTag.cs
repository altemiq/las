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
    public GeoDoubleParamsTag(params IEnumerable<double> values)
        : this([.. values])
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

    private GeoDoubleParamsTag(IReadOnlyList<double> values)
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

    private GeoDoubleParamsTag(VariableLengthRecordHeader header, IReadOnlyList<double> values)
        : base(header) => this.values = values;

    /// <inheritdoc />
    public int Count => this.values.Count;

    /// <inheritdoc />
    public double this[int index] => this.values[index];

    /// <inheritdoc />
    public IEnumerator<double> GetEnumerator() => this.values.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc />
    public override int Write(Span<byte> destination)
    {
        this.Header.Write(destination);
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

    private static IReadOnlyList<double> GetValues(ReadOnlySpan<byte> data)
    {
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<double>();
        for (int i = 0; i < data.Length; i += sizeof(double))
        {
            builder.Add(System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data.Slice(i, sizeof(double))));
        }

        return builder.ToReadOnlyCollection();
    }
}