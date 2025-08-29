// -----------------------------------------------------------------------
// <copyright file="GpsWaveformPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a point data record.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = Size)]
public readonly record struct GpsWaveformPointDataRecord :
#if NET7_0_OR_GREATER
    IBasePointDataRecord<GpsWaveformPointDataRecord>,
#endif
    IPointDataRecord,
    IWaveformPointDataRecord
{
    /// <summary>
    /// The size of a point data record.
    /// </summary>
    public const ushort Size = ParametricDzFieldOffset + sizeof(float);

    /// <summary>
    /// The point data format ID.
    /// </summary>
    public const byte Id = 4;

    private const int ByteOffsetToWaveformDataFieldOffset = Constants.PointDataRecord.GpsWaveformFieldOffset + sizeof(byte);
    private const int WaveformPacketSizeInBytesFieldOffset = ByteOffsetToWaveformDataFieldOffset + sizeof(ulong);
    private const int ReturnPointWaveformLocationFieldOffset = WaveformPacketSizeInBytesFieldOffset + sizeof(uint);
    private const int ParametricDxFieldOffset = ReturnPointWaveformLocationFieldOffset + sizeof(float);
    private const int ParametricDyFieldOffset = ParametricDxFieldOffset + sizeof(float);
    private const int ParametricDzFieldOffset = ParametricDyFieldOffset + sizeof(float);

    [System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.FlagsFieldOffset)]
    private readonly byte flags;

    [System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.ClassificationFieldOffset)]
    private readonly byte classification;

    /// <summary>
    /// Initializes a new instance of the <see cref="GpsWaveformPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public GpsWaveformPointDataRecord(byte[] data)
        : this(new ReadOnlySpan<byte>(data))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpsWaveformPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public GpsWaveformPointDataRecord(ReadOnlySpan<byte> data)
    {
        this.X = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data[..Constants.PointDataRecord.YFieldOffset]);
        this.Y = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data[Constants.PointDataRecord.YFieldOffset..Constants.PointDataRecord.ZFieldOffset]);
        this.Z = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data[Constants.PointDataRecord.ZFieldOffset..Constants.PointDataRecord.IntensityFieldOffset]);
        this.Intensity = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.PointDataRecord.IntensityFieldOffset..Constants.PointDataRecord.FlagsFieldOffset]);
        this.flags = data[Constants.PointDataRecord.FlagsFieldOffset];
        this.classification = data[Constants.PointDataRecord.ClassificationFieldOffset];
        this.ScanAngleRank = (sbyte)data[Constants.PointDataRecord.ScanAngleRankFieldOffset];
        this.UserData = data[Constants.PointDataRecord.UserDataFieldOffset];
        this.PointSourceId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.PointDataRecord.PointSourceIdFieldOffset..Constants.PointDataRecord.GpsTimeFieldOffset]);
        this.GpsTime = BitManipulation.ReadDoubleLittleEndian(data[Constants.PointDataRecord.GpsTimeFieldOffset..Constants.PointDataRecord.GpsWaveformFieldOffset]);
        this.WavePacketDescriptorIndex = data[Constants.PointDataRecord.GpsWaveformFieldOffset];
        this.ByteOffsetToWaveformData = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset]);
        this.WaveformPacketSizeInBytes = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset]);
        this.ReturnPointWaveformLocation = BitManipulation.ReadSingleLittleEndian(data[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset]);
        this.ParametricDx = BitManipulation.ReadSingleLittleEndian(data[ParametricDxFieldOffset..ParametricDyFieldOffset]);
        this.ParametricDy = BitManipulation.ReadSingleLittleEndian(data[ParametricDyFieldOffset..ParametricDzFieldOffset]);
        this.ParametricDz = BitManipulation.ReadSingleLittleEndian(data[ParametricDzFieldOffset..Size]);
    }

#if NET7_0_OR_GREATER
    /// <inheritdoc />
    static ushort IBasePointDataRecord.Size => Size;

    /// <inheritdoc />
    static byte IBasePointDataRecord.Id => Id;
#endif

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.XFieldOffset)]
    public required int X { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.YFieldOffset)]
    public required int Y { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.ZFieldOffset)]
    public required int Z { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.IntensityFieldOffset)]
    public ushort Intensity { get; init; }

    /// <inheritdoc />
    public required byte ReturnNumber
    {
        get => BitManipulation.Get(this.flags, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2);
        init => this.flags = BitManipulation.Set(this.flags, value, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2);
    }

    /// <inheritdoc />
    public required byte NumberOfReturns
    {
        get => BitManipulation.Get(this.flags, Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 3);
        init => this.flags = BitManipulation.Set(this.flags, value, Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 3);
    }

    /// <inheritdoc />
    public required bool ScanDirectionFlag
    {
        get => BitManipulation.IsSet(this.flags, Constants.BitMasks.Mask6);
        init => this.flags = BitManipulation.Apply(this.flags, Constants.BitMasks.Mask6, value);
    }

    /// <inheritdoc />
    public required bool EdgeOfFlightLine
    {
        get => BitManipulation.IsSet(this.flags, Constants.BitMasks.Mask7);
        init => this.flags = BitManipulation.Apply(this.flags, Constants.BitMasks.Mask7, value);
    }

    /// <inheritdoc />
    public required Classification Classification
    {
        get => (Classification)BitManipulation.Get(this.classification, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4);
        init => this.classification = BitManipulation.Set(this.classification, (byte)value, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3 | Constants.BitMasks.Mask4);
    }

    /// <inheritdoc />
    public bool Synthetic
    {
        get => BitManipulation.IsSet(this.classification, Constants.BitMasks.Mask5);
        init => this.classification = BitManipulation.Apply(this.classification, Constants.BitMasks.Mask5, value);
    }

    /// <inheritdoc />
    public bool KeyPoint
    {
        get => BitManipulation.IsSet(this.classification, Constants.BitMasks.Mask6);
        init => this.classification = BitManipulation.Apply(this.classification, Constants.BitMasks.Mask6, value);
    }

    /// <inheritdoc />
    public bool Withheld
    {
        get => BitManipulation.IsSet(this.classification, Constants.BitMasks.Mask7);
        init => this.classification = BitManipulation.Apply(this.classification, Constants.BitMasks.Mask7, value);
    }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.ScanAngleRankFieldOffset)]
    public required sbyte ScanAngleRank { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.UserDataFieldOffset)]
    public byte UserData { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.PointSourceIdFieldOffset)]
    public required ushort PointSourceId { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.GpsTimeFieldOffset)]
    public required double GpsTime { get; init; }

    /// <inheritdoc/>
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.GpsWaveformFieldOffset)]
    public required byte WavePacketDescriptorIndex { get; init; }

    /// <inheritdoc/>
    [field: System.Runtime.InteropServices.FieldOffset(ByteOffsetToWaveformDataFieldOffset)]
    public required ulong ByteOffsetToWaveformData { get; init; }

    /// <inheritdoc/>
    [field: System.Runtime.InteropServices.FieldOffset(WaveformPacketSizeInBytesFieldOffset)]
    public required uint WaveformPacketSizeInBytes { get; init; }

    /// <inheritdoc/>
    [field: System.Runtime.InteropServices.FieldOffset(ReturnPointWaveformLocationFieldOffset)]
    public required float ReturnPointWaveformLocation { get; init; }

    /// <inheritdoc/>
    [field: System.Runtime.InteropServices.FieldOffset(ParametricDxFieldOffset)]
    public required float ParametricDx { get; init; }

    /// <inheritdoc/>
    [field: System.Runtime.InteropServices.FieldOffset(ParametricDyFieldOffset)]
    public required float ParametricDy { get; init; }

    /// <inheritdoc/>
    [field: System.Runtime.InteropServices.FieldOffset(ParametricDzFieldOffset)]
    public required float ParametricDz { get; init; }

    /// <summary>
    /// Converts a <see cref="GpsWaveformPointDataRecord"/> instance to a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The GPS waveform point data record.</param>
    /// <returns>The <see cref="PointDataRecord"/>.</returns>
    public static explicit operator PointDataRecord(GpsWaveformPointDataRecord pointDataRecord) => pointDataRecord.ToPointDataRecord();

    /// <summary>
    /// Converts a <see cref="GpsWaveformPointDataRecord"/> instance to a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The GPS waveform point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static explicit operator GpsPointDataRecord(GpsWaveformPointDataRecord pointDataRecord) => pointDataRecord.ToGpsPointDataRecord();

    /// <summary>
    /// Converts a <see cref="PointDataRecord"/> instance to a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static explicit operator GpsWaveformPointDataRecord(PointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Converts a <see cref="GpsPointDataRecord"/> instance to a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The GPS point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static explicit operator GpsWaveformPointDataRecord(GpsPointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(PointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(GpsPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(ColorPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="GpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(GpsColorPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(GpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromGpsWaveformPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="GpsColorWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(GpsColorWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromGpsWaveformPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="ExtendedGpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(ExtendedGpsPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="ExtendedGpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(ExtendedGpsColorPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="ExtendedGpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(ExtendedGpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromExtendedGpsWaveformPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsWaveformPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsWaveformPointDataRecord.FromExtendedGpsWaveformPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a new instance of <see cref="GpsWaveformPointDataRecord"/> from the data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The instance of <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord Create(ReadOnlySpan<byte> data) => BitConverter.IsLittleEndian ? System.Runtime.InteropServices.MemoryMarshal.Read<GpsWaveformPointDataRecord>(data) : new(data);

    /// <inheritdoc />
    public int Write(Span<byte> destination)
    {
        if (BitConverter.IsLittleEndian)
        {
            System.Runtime.InteropServices.MemoryMarshal.Write(destination, ref System.Runtime.CompilerServices.Unsafe.AsRef(this));
            return Size;
        }

        return this.WriteLittleEndian(destination);
    }

    /// <summary>
    /// Writes this instance into a span of bytes, as little endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
    /// <returns>The number of bytes written.</returns>
    public int WriteLittleEndian(Span<byte> destination)
    {
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[..Constants.PointDataRecord.YFieldOffset], this.X);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.PointDataRecord.YFieldOffset..Constants.PointDataRecord.ZFieldOffset], this.Y);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.PointDataRecord.ZFieldOffset..Constants.PointDataRecord.IntensityFieldOffset], this.Z);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.PointDataRecord.IntensityFieldOffset..Constants.PointDataRecord.FlagsFieldOffset], this.Intensity);
        destination[Constants.PointDataRecord.FlagsFieldOffset] = this.flags;
        destination[Constants.PointDataRecord.ClassificationFieldOffset] = this.classification;
        destination[Constants.PointDataRecord.ScanAngleRankFieldOffset] = (byte)this.ScanAngleRank;
        destination[Constants.PointDataRecord.UserDataFieldOffset] = this.UserData;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.PointDataRecord.PointSourceIdFieldOffset..Constants.PointDataRecord.ColorFieldOffset], this.PointSourceId);
        BitManipulation.WriteDoubleLittleEndian(destination[Constants.PointDataRecord.GpsTimeFieldOffset..Constants.PointDataRecord.GpsColorFieldOffset], this.GpsTime);
        destination[Constants.PointDataRecord.GpsWaveformFieldOffset] = this.WavePacketDescriptorIndex;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset], this.ByteOffsetToWaveformData);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset], this.WaveformPacketSizeInBytes);
        BitManipulation.WriteSingleLittleEndian(destination[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset], this.ReturnPointWaveformLocation);
        BitManipulation.WriteSingleLittleEndian(destination[ParametricDxFieldOffset..ParametricDyFieldOffset], this.ParametricDx);
        BitManipulation.WriteSingleLittleEndian(destination[ParametricDyFieldOffset..ParametricDzFieldOffset], this.ParametricDy);
        BitManipulation.WriteSingleLittleEndian(destination[ParametricDzFieldOffset..Size], this.ParametricDz);
        return Size;
    }

    /// <inheritdoc />
    public PointDataRecord ToPointDataRecord() => PointConverter.ToPointDataRecord.FromPointDataRecord(this);

    /// <inheritdoc />
    public GpsPointDataRecord ToGpsPointDataRecord() => PointConverter.ToGpsPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ColorPointDataRecord ToColorPointDataRecord() => PointConverter.ToColorPointDataRecord.FromPointDataRecord(this);

    /// <inheritdoc />
    public GpsColorPointDataRecord ToGpsColorPointDataRecord() => PointConverter.ToGpsColorPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public GpsWaveformPointDataRecord ToGpsWaveformPointDataRecord() => PointConverter.ToGpsWaveformPointDataRecord.FromGpsWaveformPointDataRecord(this);

    /// <inheritdoc />
    public GpsColorWaveformPointDataRecord ToGpsColorWaveformPointDataRecord() => PointConverter.ToGpsColorWaveformPointDataRecord.FromGpsWaveformPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsPointDataRecord ToExtendedGpsPointDataRecord() => PointConverter.ToExtendedGpsPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorPointDataRecord ToExtendedGpsColorPointDataRecord() => PointConverter.ToExtendedGpsColorPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorNearInfraredPointDataRecord ToExtendedGpsColorNearInfraredPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsWaveformPointDataRecord ToExtendedGpsWaveformPointDataRecord() => PointConverter.ToExtendedGpsWaveformPointDataRecord.FromGpsWaveformPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorNearInfraredWaveformPointDataRecord ToExtendedGpsColorNearInfraredWaveformPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredWaveformPointDataRecord.FromGpsWaveformPointDataRecord(this);
}