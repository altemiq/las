// -----------------------------------------------------------------------
// <copyright file="Tiling.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The tiling <see cref="VariableLengthRecord"/>.
/// </summary>
public sealed record Tiling : VariableLengthRecord
{
    /// <summary>
    /// The LASTools user ID.
    /// </summary>
    public const string LasToolsUserId = "LAStools";

    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 10;

    private const uint ImplicitLevelsMask = 0x3FFFFFFF;
    private const uint BufferMask = 0x40000000;
    private const uint ReversibleMask = 0x80000000;

    private const int LevelIdx = 0;
    private const int LevelIndexIdx = LevelIdx + sizeof(uint);
    private const int ImplicitLevelsIdx = LevelIndexIdx + sizeof(uint);
    private const int MinXIdx = ImplicitLevelsIdx + sizeof(uint);
    private const int MaxXIdx = MinXIdx + sizeof(float);
    private const int MinYIdx = MaxXIdx + sizeof(float);
    private const int MaxYIdx = MinYIdx + sizeof(float);
    private const int DataSize = MaxYIdx + sizeof(float);

    private readonly uint value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tiling"/> class.
    /// </summary>
    public Tiling()
        : base(new VariableLengthRecordHeader { UserId = LasToolsUserId, RecordId = TagRecordId, RecordLengthAfterHeader = DataSize, })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tiling"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal Tiling(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : base(header)
    {
        this.Level = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[..LevelIndexIdx]);
        this.LevelIndex = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[LevelIndexIdx..ImplicitLevelsIdx]);
        this.value = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[ImplicitLevelsIdx..MinXIdx]);
        this.MinX = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[MinXIdx..MaxXIdx]);
        this.MaxX = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[MaxXIdx..MinYIdx]);
        this.MinY = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[MinYIdx..MaxYIdx]);
        this.MaxY = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[MaxYIdx..DataSize]);
    }

    /// <summary>
    /// Gets the level.
    /// </summary>
    public uint Level { get; init; }

    /// <summary>
    /// Gets the level index.
    /// </summary>
    public uint LevelIndex { get; init; }

    /// <summary>
    /// Gets the implicit levels.
    /// </summary>
    public uint ImplicitLevels
    {
        get => this.value & ImplicitLevelsMask;
        init => this.value = (this.value & ~ImplicitLevelsMask) | (value & ImplicitLevelsMask);
    }

    /// <summary>
    /// Gets a value indicating whether this has a buffer.
    /// </summary>
    public bool Buffer
    {
        get => (this.value & BufferMask) is not 0;
        init
        {
            var current = this.value & ~BufferMask;
            if (value)
            {
                current |= BufferMask;
            }

            this.value = current;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this is reversible.
    /// </summary>
    public bool Reversible
    {
        get => (this.value & ReversibleMask) is not 0;
        init
        {
            var current = this.value & ~ReversibleMask;
            if (value)
            {
                current |= ReversibleMask;
            }

            this.value = current;
        }
    }

    /// <summary>
    /// Gets the minimum-X value.
    /// </summary>
    public float MinX { get; init; }

    /// <summary>
    /// Gets the maximum-X value.
    /// </summary>
    public float MaxX { get; init; }

    /// <summary>
    /// Gets the minimum-Y value.
    /// </summary>
    public float MinY { get; init; }

    /// <summary>
    /// Gets the maximum-Y value.
    /// </summary>
    public float MaxY { get; init; }

    /// <inheritdoc />
    public override int Write(Span<byte> destination)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[..LevelIndexIdx], this.Level);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[LevelIndexIdx..ImplicitLevelsIdx], this.LevelIndex);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[ImplicitLevelsIdx..MinXIdx], this.value);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[MinXIdx..MaxXIdx], this.MinX);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[MaxXIdx..MinYIdx], this.MaxX);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[MinYIdx..MaxYIdx], this.MinY);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[MaxYIdx..DataSize], this.MaxY);
        return DataSize;
    }
}