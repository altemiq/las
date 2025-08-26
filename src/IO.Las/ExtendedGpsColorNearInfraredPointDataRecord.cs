// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorNearInfraredPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents an extended point data record with GPS time.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = Size)]
public readonly record struct ExtendedGpsColorNearInfraredPointDataRecord :
#if NET7_0_OR_GREATER
    IBasePointDataRecord<ExtendedGpsColorNearInfraredPointDataRecord>,
#endif
    IExtendedPointDataRecord,
    INearInfraredPointDataRecord
{
    /// <summary>
    /// The size of a point data record.
    /// </summary>
    public const ushort Size = Constants.ExtendedPointDataRecord.NirFieldOffset + sizeof(ushort);

    /// <summary>
    /// The point data format ID.
    /// </summary>
    public const byte Id = 8;

    [System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.FlagsFieldOffset)]
    private readonly byte flags;

    [System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset)]
    private readonly byte classificationFlags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ExtendedGpsColorNearInfraredPointDataRecord(byte[] data)
        : this(new ReadOnlySpan<byte>(data))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ExtendedGpsColorNearInfraredPointDataRecord(ReadOnlySpan<byte> data)
    {
        this.X = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data[..Constants.ExtendedPointDataRecord.YFieldOffset]);
        this.Y = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data[Constants.ExtendedPointDataRecord.YFieldOffset..Constants.ExtendedPointDataRecord.ZFieldOffset]);
        this.Z = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data[Constants.ExtendedPointDataRecord.ZFieldOffset..Constants.ExtendedPointDataRecord.IntensityFieldOffset]);
        this.Intensity = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.ExtendedPointDataRecord.IntensityFieldOffset..Constants.ExtendedPointDataRecord.FlagsFieldOffset]);
        this.flags = data[Constants.ExtendedPointDataRecord.FlagsFieldOffset];
        this.classificationFlags = data[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset];
        this.Classification = (ExtendedClassification)data[Constants.ExtendedPointDataRecord.ClassificationFieldOffset];
        this.UserData = data[Constants.ExtendedPointDataRecord.UserDataFieldOffset];
        this.ScanAngle = System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(data[Constants.ExtendedPointDataRecord.ScanAngleFieldOffset..Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset]);
        this.PointSourceId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset..Constants.ExtendedPointDataRecord.GpsTimeFieldOffset]);
        this.GpsTime = BitManipulation.ReadDoubleLittleEndian(data[Constants.ExtendedPointDataRecord.GpsTimeFieldOffset..Constants.ExtendedPointDataRecord.ColorFieldOffset]);
        this.Color = new()
        {
            R = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.ExtendedPointDataRecord.ColorFieldOffset..]),
            G = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[(Constants.ExtendedPointDataRecord.ColorFieldOffset + sizeof(ushort))..]),
            B = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[(Constants.ExtendedPointDataRecord.ColorFieldOffset + sizeof(ushort) + sizeof(ushort))..]),
        };
        this.NearInfrared = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.ExtendedPointDataRecord.NirFieldOffset..Size]);
    }

#if NET7_0_OR_GREATER
    /// <inheritdoc />
    static ushort IBasePointDataRecord.Size => Size;

    /// <inheritdoc />
    static byte IBasePointDataRecord.Id => Id;
#endif

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.XFieldOffset)]
    public required int X { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.YFieldOffset)]
    public required int Y { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.ZFieldOffset)]
    public required int Z { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.IntensityFieldOffset)]
    public ushort Intensity { get; init; }

    /// <inheritdoc />
    public required byte ReturnNumber
    {
        get => BitManipulation.Get(this.flags, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3);
        init => this.flags = BitManipulation.Set(this.flags, value, Constants.BitMasks.Mask0 | Constants.BitMasks.Mask1 | Constants.BitMasks.Mask2 | Constants.BitMasks.Mask3);
    }

    /// <inheritdoc />
    public required byte NumberOfReturns
    {
        get => BitManipulation.Get(this.flags, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5 | Constants.BitMasks.Mask6 | Constants.BitMasks.Mask7, 4);
        init => this.flags = BitManipulation.Set(this.flags, value, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5 | Constants.BitMasks.Mask6 | Constants.BitMasks.Mask7, 4);
    }

    /// <inheritdoc />
    public bool Synthetic
    {
        get => BitManipulation.IsSet(this.classificationFlags, Constants.BitMasks.Mask0);
        init => this.classificationFlags = BitManipulation.Apply(this.classificationFlags, Constants.BitMasks.Mask0, value);
    }

    /// <inheritdoc />
    public bool KeyPoint
    {
        get => BitManipulation.IsSet(this.classificationFlags, Constants.BitMasks.Mask1);
        init => this.classificationFlags = BitManipulation.Apply(this.classificationFlags, Constants.BitMasks.Mask1, value);
    }

    /// <inheritdoc />
    public bool Withheld
    {
        get => BitManipulation.IsSet(this.classificationFlags, Constants.BitMasks.Mask2);
        init => this.classificationFlags = BitManipulation.Apply(this.classificationFlags, Constants.BitMasks.Mask2, value);
    }

    /// <inheritdoc />
    public bool Overlap
    {
        get => BitManipulation.IsSet(this.classificationFlags, Constants.BitMasks.Mask3);
        init => this.classificationFlags = BitManipulation.Apply(this.classificationFlags, Constants.BitMasks.Mask3, value);
    }

    /// <inheritdoc />
    public byte ScannerChannel
    {
        get => BitManipulation.Get(this.classificationFlags, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 4);
        init => this.classificationFlags = BitManipulation.Set(this.classificationFlags, value, Constants.BitMasks.Mask4 | Constants.BitMasks.Mask5, 4);
    }

    /// <inheritdoc />
    public required bool ScanDirectionFlag
    {
        get => BitManipulation.IsSet(this.classificationFlags, Constants.BitMasks.Mask6);
        init => this.classificationFlags = BitManipulation.Apply(this.classificationFlags, Constants.BitMasks.Mask6, value);
    }

    /// <inheritdoc />
    public required bool EdgeOfFlightLine
    {
        get => BitManipulation.IsSet(this.classificationFlags, Constants.BitMasks.Mask7);
        init => this.classificationFlags = BitManipulation.Apply(this.classificationFlags, Constants.BitMasks.Mask7, value);
    }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.ClassificationFieldOffset)]
    public required ExtendedClassification Classification { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.UserDataFieldOffset)]
    public byte UserData { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.ScanAngleFieldOffset)]
    public required short ScanAngle { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset)]
    public required ushort PointSourceId { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.GpsTimeFieldOffset)]
    public required double GpsTime { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.ColorFieldOffset)]
    public required Color Color { get; init; }

    /// <inheritdoc />
    [field: System.Runtime.InteropServices.FieldOffset(Constants.ExtendedPointDataRecord.NirFieldOffset)]
    public required ushort NearInfrared { get; init; }

    /// <summary>
    /// Converts a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> instance to a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</param>
    /// <returns>The <see cref="PointDataRecord"/>.</returns>
    public static explicit operator PointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => pointDataRecord.ToPointDataRecord();

    /// <summary>
    /// Converts a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> instance to a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</param>
    /// <returns>The <see cref="GpsPointDataRecord"/>.</returns>
    public static explicit operator GpsPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => pointDataRecord.ToGpsPointDataRecord();

    /// <summary>
    /// Converts a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> instance to a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static explicit operator ColorPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => pointDataRecord.ToColorPointDataRecord();

    /// <summary>
    /// Converts a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> instance to a <see cref="GpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</param>
    /// <returns>The <see cref="GpsColorPointDataRecord"/>.</returns>
    public static explicit operator GpsColorPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => pointDataRecord.ToGpsColorPointDataRecord();

    /// <summary>
    /// Converts a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> instance to a <see cref="ExtendedGpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</param>
    /// <returns>The <see cref="ExtendedGpsPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => pointDataRecord.ToExtendedGpsPointDataRecord();

    /// <summary>
    /// Converts a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> instance to a <see cref="ExtendedGpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</param>
    /// <returns>The <see cref="ExtendedGpsColorPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsColorPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => pointDataRecord.ToExtendedGpsColorPointDataRecord();

    /// <summary>
    /// Converts a <see cref="PointDataRecord"/> instance to a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The <see cref="PointDataRecord"/>.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsColorNearInfraredPointDataRecord(PointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Converts a <see cref="GpsPointDataRecord"/> instance to a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsColorNearInfraredPointDataRecord(GpsPointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Converts a <see cref="ColorPointDataRecord"/> instance to a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsColorNearInfraredPointDataRecord(ColorPointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Converts a <see cref="GpsColorPointDataRecord"/> instance to a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsColorNearInfraredPointDataRecord(GpsColorPointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Converts a <see cref="GpsColorPointDataRecord"/> instance to a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsColorNearInfraredPointDataRecord(ExtendedGpsPointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Converts a <see cref="GpsColorPointDataRecord"/> instance to a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static explicit operator ExtendedGpsColorNearInfraredPointDataRecord(ExtendedGpsColorPointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(PointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(GpsPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(ColorPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromColorPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="GpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(GpsColorPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromGpsColorPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(GpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromGpsPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="GpsColorWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(GpsColorWaveformPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromGpsColorPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="ExtendedGpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(ExtendedGpsPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="ExtendedGpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(ExtendedGpsColorPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromExtendedColorPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromExtendedNearInfraredPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="ExtendedGpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(ExtendedGpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredWaveformPointDataRecord pointDataRecord) => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromExtendedNearInfraredPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a new instance of <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/> from the data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The instance of <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.</returns>
    public static ExtendedGpsColorNearInfraredPointDataRecord Create(ReadOnlySpan<byte> data) => BitConverter.IsLittleEndian ? System.Runtime.InteropServices.MemoryMarshal.Read<ExtendedGpsColorNearInfraredPointDataRecord>(data) : new(data);

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
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[..Constants.ExtendedPointDataRecord.YFieldOffset], this.X);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.ExtendedPointDataRecord.YFieldOffset..Constants.ExtendedPointDataRecord.ZFieldOffset], this.Y);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination[Constants.ExtendedPointDataRecord.ZFieldOffset..Constants.ExtendedPointDataRecord.IntensityFieldOffset], this.Z);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.IntensityFieldOffset..Constants.ExtendedPointDataRecord.FlagsFieldOffset], this.Intensity);
        destination[Constants.ExtendedPointDataRecord.FlagsFieldOffset] = this.flags;
        destination[Constants.ExtendedPointDataRecord.ClassificationFlagsFieldOffset] = this.classificationFlags;
        destination[Constants.ExtendedPointDataRecord.ClassificationFieldOffset] = (byte)this.Classification;
        destination[Constants.ExtendedPointDataRecord.UserDataFieldOffset] = this.UserData;
        System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.ScanAngleFieldOffset..Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset], this.ScanAngle);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.PointSourceIdFieldOffset..Constants.ExtendedPointDataRecord.GpsTimeFieldOffset], this.PointSourceId);
        BitManipulation.WriteDoubleLittleEndian(destination[Constants.ExtendedPointDataRecord.GpsTimeFieldOffset..Constants.ExtendedPointDataRecord.ColorFieldOffset], this.GpsTime);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.ColorFieldOffset..], this.Color.R);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[(Constants.ExtendedPointDataRecord.ColorFieldOffset + sizeof(ushort))..], this.Color.G);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[(Constants.ExtendedPointDataRecord.ColorFieldOffset + sizeof(ushort) + sizeof(ushort))..], this.Color.B);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.ExtendedPointDataRecord.NirFieldOffset..Size], this.NearInfrared);
    }

    /// <inheritdoc/>
    public PointDataRecord ToPointDataRecord() => PointConverter.ToPointDataRecord.FromExtendedPointDataRecord(this);

    /// <inheritdoc/>
    public GpsPointDataRecord ToGpsPointDataRecord() => PointConverter.ToGpsPointDataRecord.FromExtendedPointDataRecord(this);

    /// <inheritdoc/>
    public ColorPointDataRecord ToColorPointDataRecord() => PointConverter.ToColorPointDataRecord.FromExtendedColorPointDataRecord(this);

    /// <inheritdoc/>
    public GpsColorPointDataRecord ToGpsColorPointDataRecord() => PointConverter.ToGpsColorPointDataRecord.FromExtendedColorPointDataRecord(this);

    /// <inheritdoc/>
    public GpsWaveformPointDataRecord ToGpsWaveformPointDataRecord() => PointConverter.ToGpsWaveformPointDataRecord.FromExtendedPointDataRecord(this);

    /// <inheritdoc/>
    public GpsColorWaveformPointDataRecord ToGpsColorWaveformPointDataRecord() => PointConverter.ToGpsColorWaveformPointDataRecord.FromExtendedColorPointDataRecord(this);

    /// <inheritdoc/>
    public ExtendedGpsPointDataRecord ToExtendedGpsPointDataRecord() => PointConverter.ToExtendedGpsPointDataRecord.FromExtendedPointDataRecord(this);

    /// <inheritdoc/>
    public ExtendedGpsColorPointDataRecord ToExtendedGpsColorPointDataRecord() => PointConverter.ToExtendedGpsColorPointDataRecord.FromExtendedColorPointDataRecord(this);

    /// <inheritdoc/>
    public ExtendedGpsColorNearInfraredPointDataRecord ToExtendedGpsColorNearInfraredPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromExtendedNearInfraredPointDataRecord(this);

    /// <inheritdoc/>
    public ExtendedGpsWaveformPointDataRecord ToExtendedGpsWaveformPointDataRecord() => PointConverter.ToExtendedGpsWaveformPointDataRecord.FromExtendedPointDataRecord(this);

    /// <inheritdoc/>
    public ExtendedGpsColorNearInfraredWaveformPointDataRecord ToExtendedGpsColorNearInfraredWaveformPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredWaveformPointDataRecord.FromExtendedNearInfraredPointDataRecord(this);
}