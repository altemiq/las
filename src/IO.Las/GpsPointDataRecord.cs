// -----------------------------------------------------------------------
// <copyright file="GpsPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a point data record with GPS time.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = Size)]
public readonly record struct GpsPointDataRecord :
#if NET7_0_OR_GREATER
    IBasePointDataRecord<GpsPointDataRecord>,
#endif
    IPointDataRecord,
    IGpsPointDataRecord
{
    /// <summary>
    /// The size of a point data record.
    /// </summary>
    public const ushort Size = Constants.PointDataRecord.GpsTimeFieldOffset + sizeof(double);

    /// <summary>
    /// The point data format ID.
    /// </summary>
    public const byte Id = 1;

    [System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.FlagsFieldOffset)]
    private readonly byte flags;

    [System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.ClassificationFieldOffset)]
    private readonly byte classification;

    /// <summary>
    /// Initializes a new instance of the <see cref="GpsPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public GpsPointDataRecord(byte[] data)
        : this(new ReadOnlySpan<byte>(data))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GpsPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public GpsPointDataRecord(ReadOnlySpan<byte> data)
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
        this.GpsTime = BitManipulation.ReadDoubleLittleEndian(data[Constants.PointDataRecord.GpsTimeFieldOffset..Size]);
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

    /// <summary>
    /// Converts a <see cref="GpsPointDataRecord"/> instance to a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The point data record.</returns>
    public static explicit operator PointDataRecord(GpsPointDataRecord pointDataRecord) => pointDataRecord.ToPointDataRecord();

    /// <summary>
    /// Converts a <see cref="PointDataRecord"/> instance to a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static explicit operator GpsPointDataRecord(PointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(PointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(GpsPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(ColorPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="GpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(GpsColorPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(GpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="GpsColorWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(GpsColorWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="ExtendedGpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(ExtendedGpsPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="ExtendedGpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(ExtendedGpsColorPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="ExtendedGpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(ExtendedGpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="GpsPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredWaveformPointDataRecord pointDataRecord) => PointConverter.ToGpsPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a new instance of <see cref="GpsPointDataRecord"/> from the data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The instance of <see cref="GpsPointDataRecord"/>.</returns>
    public static GpsPointDataRecord Create(ReadOnlySpan<byte> data) => BitConverter.IsLittleEndian ? System.Runtime.InteropServices.MemoryMarshal.Read<GpsPointDataRecord>(data) : new(data);

    /// <inheritdoc />
    public void Write(Span<byte> destination)
    {
        if (BitConverter.IsLittleEndian)
        {
            System.Runtime.InteropServices.MemoryMarshal.Write(destination, ref System.Runtime.CompilerServices.Unsafe.AsRef(this));
            return;
        }

        this.WriteLittleEndian(destination);
    }

    /// <summary>
    /// Writes this instance  into a span of bytes, as little endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
    public void WriteLittleEndian(Span<byte> destination)
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
        BitManipulation.WriteDoubleLittleEndian(destination[Constants.PointDataRecord.GpsTimeFieldOffset..Size], this.GpsTime);
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
    public GpsWaveformPointDataRecord ToGpsWaveformPointDataRecord() => PointConverter.ToGpsWaveformPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public GpsColorWaveformPointDataRecord ToGpsColorWaveformPointDataRecord() => PointConverter.ToGpsColorWaveformPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsPointDataRecord ToExtendedGpsPointDataRecord() => PointConverter.ToExtendedGpsPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorPointDataRecord ToExtendedGpsColorPointDataRecord() => PointConverter.ToExtendedGpsColorPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorNearInfraredPointDataRecord ToExtendedGpsColorNearInfraredPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsWaveformPointDataRecord ToExtendedGpsWaveformPointDataRecord() => PointConverter.ToExtendedGpsWaveformPointDataRecord.FromGpsPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorNearInfraredWaveformPointDataRecord ToExtendedGpsColorNearInfraredWaveformPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredWaveformPointDataRecord.FromGpsPointDataRecord(this);
}