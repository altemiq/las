// -----------------------------------------------------------------------
// <copyright file="WavePacket13.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The wave packet structure.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
internal readonly struct WavePacket13
{
    /// <summary>
    /// The offset.
    /// </summary>
    public readonly ulong Offset;

    /// <summary>
    /// The packet size.
    /// </summary>
    public readonly uint PacketSize;

    /// <summary>
    /// The return point.
    /// </summary>
    public readonly float ReturnPoint;

    /// <summary>
    /// The x-value.
    /// </summary>
    public readonly float X;

    /// <summary>
    /// The y-value.
    /// </summary>
    public readonly float Y;

    /// <summary>
    /// The z-value.
    /// </summary>
    public readonly float Z;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavePacket13"/> struct.
    /// </summary>
    /// <param name="item">The item to initialize from.</param>
    /// <param name="startIndex">The start index.</param>
    public WavePacket13(byte[] item, int startIndex = default)
        : this(item.AsSpan(startIndex))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WavePacket13"/> struct.
    /// </summary>
    /// <param name="source">The item to initialize from.</param>
    public WavePacket13(ReadOnlySpan<byte> source)
        : this(
            System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source),
            System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[sizeof(ulong)..]),
            System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[(sizeof(ulong) + sizeof(uint))..]),
            System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[(sizeof(ulong) + sizeof(uint) + sizeof(float))..]),
            System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[(sizeof(ulong) + sizeof(uint) + (2 * sizeof(float)))..]),
            System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[(sizeof(ulong) + sizeof(uint) + (3 * sizeof(float)))..]))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WavePacket13"/> struct.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <param name="packetSize">The packet size.</param>
    /// <param name="returnPoint">The return point.</param>
    /// <param name="x">The x-value.</param>
    /// <param name="y">The y-value.</param>
    /// <param name="z">The z-value.</param>
    public WavePacket13(
        ulong offset,
        uint packetSize,
        float returnPoint,
        float x,
        float y,
        float z) => (this.Offset, this.PacketSize, this.ReturnPoint, this.X, this.Y, this.Z) = (offset, packetSize, returnPoint, x, y, z);

    /// <summary>
    /// Packs this instance into the specified array and start index.
    /// </summary>
    /// <param name="item">The item to write to.</param>
    /// <param name="startIndex">The start index.</param>
    public void WriteTo(byte[] item, int startIndex) => this.WriteTo(item.AsSpan(startIndex));

    /// <summary>
    /// Packs this instance into the destination.
    /// </summary>
    /// <param name="destination">The destination.</param>
    public void WriteTo(Span<byte> destination)
    {
        // pack a LAS wave-packet into raw memory
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination, this.Offset);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[sizeof(ulong)..], this.PacketSize);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[(sizeof(ulong) + sizeof(uint))..], this.ReturnPoint);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[(sizeof(ulong) + sizeof(uint) + sizeof(float))..], this.X);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[(sizeof(ulong) + sizeof(uint) + (2 * sizeof(float)))..], this.Y);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[(sizeof(ulong) + sizeof(uint) + (3 * sizeof(float)))..], this.Z);
    }

    /// <inheritdoc/>
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is WavePacket13 wp
                                                                                                    && this.Offset == wp.Offset
                                                                                                    && this.PacketSize == wp.PacketSize
                                                                                                    && this.ReturnPoint.Equals(wp.ReturnPoint)
                                                                                                    && this.X.Equals(wp.X)
                                                                                                    && this.Y.Equals(wp.Y)
                                                                                                    && this.Z.Equals(wp.Z);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.Offset, this.PacketSize, this.ReturnPoint, this.X, this.Y, this.Z);

    /// <inheritdoc/>
    public override string ToString() => $$"""
        {
          {{nameof(this.Offset)}}: {{this.Offset}},
          {{nameof(this.PacketSize)}}: {{this.PacketSize}},
          {{nameof(this.ReturnPoint)}}: {{this.ReturnPoint}},
          {{nameof(this.X)}}: {{this.X}},
          {{nameof(this.Y)}}: {{this.Y}},
          {{nameof(this.Z)}}: {{this.Z}}
        }
        """;
}