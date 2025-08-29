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
    public GeoKeyDirectoryTag(params IEnumerable<GeoKeyEntry> entries)
        : this([.. entries])
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

    private GeoKeyDirectoryTag(IReadOnlyList<GeoKeyEntry> values)
        : this(
            new()
            {
                UserId = VariableLengthRecordHeader.ProjectionUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = (ushort)(4 * sizeof(ushort) * values.Count),
            },
            new(1, 1),
            values)
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

    private static IReadOnlyList<GeoKeyEntry> GetEntries(ReadOnlySpan<byte> source, int count)
    {
        var builder = new System.Runtime.CompilerServices.ReadOnlyCollectionBuilder<GeoKeyEntry>(count);

        for (int i = 0; i < count; i++)
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

    private static Version GetVersion(ReadOnlySpan<byte> source)
    {
        var keyDirectoryVersion = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source);
        var keyRevision = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[2..]);
        var minorRevision = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source[4..]);
        return new(keyDirectoryVersion, keyRevision, 0, minorRevision);
    }
}