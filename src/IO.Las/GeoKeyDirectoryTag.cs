// -----------------------------------------------------------------------
// <copyright file="GeoKeyDirectoryTag.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// This record contains the key values that define the coordinate system.
/// A complete description can be found in the GeoTIFF format specification.
/// </summary>
[System.Runtime.CompilerServices.CollectionBuilder(typeof(GeoKeyDirectoryTag), nameof(Create))]
public sealed record GeoKeyDirectoryTag : VariableLengthRecord, IReadOnlyList<GeoKeyEntry>
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 34735;

    private readonly IReadOnlyList<GeoKeyEntry> values;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoKeyDirectoryTag"/> class.
    /// </summary>
    /// <param name="entries">The entries.</param>
    public GeoKeyDirectoryTag(params IReadOnlyList<GeoKeyEntry> entries)
        : this(
            new()
            {
                UserId = VariableLengthRecordHeader.ProjectionUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = (ushort)(4 * sizeof(ushort) * entries.Count),
            },
            new(1, 1),
            entries)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeoKeyDirectoryTag"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal GeoKeyDirectoryTag(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : this(header, GetVersion(data), GetEntries(data[8..], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[6..8])))
    {
    }

    private GeoKeyDirectoryTag(VariableLengthRecordHeader header, Version version, IReadOnlyList<GeoKeyEntry> values)
        : base(header)
    {
        this.Version = version;
        this.values = values;
    }

    /// <summary>
    /// Gets the key version.
    /// </summary>
    public Version Version { get; }

    /// <inheritdoc />
    public int Count => this.values.Count;

    /// <inheritdoc />
    public GeoKeyEntry this[int index] => this.values[index];

    /// <summary>
    /// Gets the <see cref="GeoKeyEntry"/> with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get.</param>
    /// <returns>The <see cref="GeoKeyEntry"/> with the specified key.</returns>
    public GeoKeyEntry this[GeoKey key] => this.values.FirstOrDefault(x => x.KeyId == key);

    /// <summary>
    /// Creates an instance of <see cref="GeoKeyDirectoryTag"/>.
    /// </summary>
    /// <param name="items">The values.</param>
    /// <returns>The <see cref="GeoKeyDirectoryTag"/>.</returns>
    public static GeoKeyDirectoryTag Create(ReadOnlySpan<GeoKeyEntry> items) => new(items.ToReadOnlyList());

    /// <inheritdoc />
    public override int Write(Span<byte> destination)
    {
        this.Header.Write(destination);
        int bytesWritten = VariableLengthRecordHeader.Size;
        var d = destination[bytesWritten..];

        foreach (var value in this.values)
        {
            if (BitConverter.IsLittleEndian)
            {
                System.Runtime.InteropServices.MemoryMarshal.Write(d, ref System.Runtime.CompilerServices.Unsafe.AsRef(value));
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[..2], (ushort)value.KeyId);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[2..4], value.TiffTagLocation);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[4..6], value.Count);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(d[6..8], value.ValueOffset);
            }

            bytesWritten += 4 * sizeof(ushort);
            d = destination[bytesWritten..];
        }

        return bytesWritten;
    }

    /// <inheritdoc />
    public IEnumerator<GeoKeyEntry> GetEnumerator() => this.values.GetEnumerator();

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    private static System.Collections.ObjectModel.ReadOnlyCollection<GeoKeyEntry> GetEntries(ReadOnlySpan<byte> source, int count)
    {
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<GeoKeyEntry>(count);

        for (var i = 0; i < count; i++)
        {
            var index = i * 8;
            var entry = BitConverter.IsLittleEndian
                ? System.Runtime.InteropServices.MemoryMarshal.Read<GeoKeyEntry>(source[index..])
                : new()
                {
                    KeyId = (GeoKey)System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[index..]),
                    TiffTagLocation = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[(index + 2)..]),
                    Count = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[(index + 4)..]),
                    ValueOffset = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[(index + 6)..]),
                };

            builder.Add(entry);
        }

        return builder.ToReadOnlyCollection();
    }

    private static Version GetVersion(ReadOnlySpan<byte> source) => new(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[2..4]), System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[2..4]), System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[4..6]));
}