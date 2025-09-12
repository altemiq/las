// -----------------------------------------------------------------------
// <copyright file="LasItem.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a compressed tag item.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 6)]
public readonly record struct LasItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LasItem"/> struct.
    /// </summary>
    /// <param name="data">The data.</param>
    internal LasItem(ReadOnlySpan<byte> data)
        : this()
    {
        this.Type = (LasItemType)System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[..2]);
        this.Size = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[2..4]);
        this.Version = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[4..6]);
    }

    /// <summary>
    /// Gets the type.
    /// </summary>
    [field: System.Runtime.InteropServices.FieldOffset(0)]
    public LasItemType Type { get; init; }

    /// <summary>
    /// Gets the size.
    /// </summary>
    [field: System.Runtime.InteropServices.FieldOffset(2)]
    public ushort Size { get; init; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    [field: System.Runtime.InteropServices.FieldOffset(4)]
    public ushort Version { get; init; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name => this.Type.ToString().ToUpperInvariant();

    /// <summary>
    /// Tests if this instance if of the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><see langword="true"/> if this instance if of type <paramref name="type"/>; otherwise <see langword="false"/>.</returns>
    public bool IsType(LasItemType type) => this.Type == type && type switch
    {
        LasItemType.Point10 => this.Size is PointDataRecord.Size,
        LasItemType.GpsTime11 => this.Size is Constants.Size.GpsTime,
#if LAS1_2_OR_GREATER
        LasItemType.Rgb12 => this.Size is Constants.Size.Color,
#endif
        LasItemType.Byte => this.Size > 0,
        LasItemType.Short => this.Size % sizeof(ushort) is 0,
        LasItemType.Int => this.Size % sizeof(uint) is 0,
        LasItemType.Float => this.Size % sizeof(float) is 0,
        LasItemType.Long => this.Size % sizeof(ulong) is 0,
        LasItemType.Double => this.Size % sizeof(double) is 0,
#if LAS1_3_OR_GREATER
        LasItemType.WavePacket13 => this.Size is Constants.Size.Waveform,
#endif
#if LAS1_4_OR_GREATER
        LasItemType.Point14 => this.Size is ExtendedGpsPointDataRecord.Size,
        LasItemType.Rgb14 => this.Size is Constants.Size.Color,
        LasItemType.Byte14 => this.Size > 0,
        LasItemType.RgbNir14 => this.Size is Constants.Size.Color + Constants.Size.NearInfrared,
        LasItemType.WavePacket14 => this.Size is Constants.Size.Waveform,
#endif
        _ => false,
    };

    /// <inheritdoc/>
    public override string ToString() =>
        $$"""
        {
          {{nameof(this.Type)}}: {{this.Type}},
          {{nameof(this.Size)}}: {{this.Size}},
          {{nameof(this.Version)}}: {{this.Version}}
        }
        """;
}