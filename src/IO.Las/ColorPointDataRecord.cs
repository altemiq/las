// -----------------------------------------------------------------------
// <copyright file="ColorPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a point data record.
/// </summary>
[Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = Size)]
public readonly record struct ColorPointDataRecord :
#if NET7_0_OR_GREATER
    IBasePointDataRecord<ColorPointDataRecord>,
#endif
    IPointDataRecord,
    IColorPointDataRecord
{
    /// <summary>
    /// The size of a point data record.
    /// </summary>
    public const ushort Size = Constants.PointDataRecord.ColorFieldOffset + Constants.Size.Color;

    /// <summary>
    /// The point data format ID.
    /// </summary>
    public const byte Id = 2;

    [System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.FlagsFieldOffset)]
    private readonly byte flags;

    [System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.ClassificationFieldOffset)]
    private readonly byte classification;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ColorPointDataRecord(byte[] data)
        : this(new ReadOnlySpan<byte>(data))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPointDataRecord"/> struct from the byte array.
    /// </summary>
    /// <param name="data">The point data record bytes.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ColorPointDataRecord(ReadOnlySpan<byte> data)
    {
        this.X = FieldAccessors.PointDataRecord.GetX(data);
        this.Y = FieldAccessors.PointDataRecord.GetY(data);
        this.Z = FieldAccessors.PointDataRecord.GetZ(data);
        this.Intensity = FieldAccessors.PointDataRecord.GetIntensity(data);
        this.flags = data[Constants.PointDataRecord.FlagsFieldOffset];
        this.classification = data[Constants.PointDataRecord.ClassificationFieldOffset];
        this.ScanAngleRank = (sbyte)data[Constants.PointDataRecord.ScanAngleRankFieldOffset];
        this.UserData = data[Constants.PointDataRecord.UserDataFieldOffset];
        this.PointSourceId = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.PointDataRecord.PointSourceIdFieldOffset..Constants.PointDataRecord.ColorFieldOffset]);
        this.Color = new()
        {
            R = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[Constants.PointDataRecord.ColorFieldOffset..]),
            G = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[(Constants.PointDataRecord.ColorFieldOffset + sizeof(ushort))..]),
            B = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data[(Constants.PointDataRecord.ColorFieldOffset + sizeof(ushort) + sizeof(ushort))..]),
        };
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
        get => FieldAccessors.PointDataRecord.GetReturnNumber(this.flags);
        init => FieldAccessors.PointDataRecord.SetReturnNumber(ref this.flags, value);
    }

    /// <inheritdoc />
    public required byte NumberOfReturns
    {
        get => FieldAccessors.PointDataRecord.GetNumberOfReturns(this.flags);
        init => FieldAccessors.PointDataRecord.SetNumberOfReturns(ref this.flags, value);
    }

    /// <inheritdoc />
    public required bool ScanDirectionFlag
    {
        get => FieldAccessors.PointDataRecord.GetScanDirectionFlag(this.flags);
        init => FieldAccessors.PointDataRecord.SetScanDirectionFlag(ref this.flags, value);
    }

    /// <inheritdoc />
    public required bool EdgeOfFlightLine
    {
        get => FieldAccessors.PointDataRecord.GetEdgeOfFlightLine(this.flags);
        init => FieldAccessors.PointDataRecord.SetEdgeOfFlightLine(ref this.flags, value);
    }

    /// <inheritdoc />
    public required Classification Classification
    {
        get => FieldAccessors.PointDataRecord.GetClassification(this.classification);
        init => FieldAccessors.PointDataRecord.SetClassification(ref this.classification, value);
    }

    /// <inheritdoc />
    public bool Synthetic
    {
        get => FieldAccessors.PointDataRecord.GetSynthetic(this.classification);
        init => FieldAccessors.PointDataRecord.SetSynthetic(ref this.classification, value);
    }

    /// <inheritdoc />
    public bool KeyPoint
    {
        get => FieldAccessors.PointDataRecord.GetKeyPoint(this.classification);
        init => FieldAccessors.PointDataRecord.SetKeyPoint(ref this.classification, value);
    }

    /// <inheritdoc />
    public bool Withheld
    {
        get => FieldAccessors.PointDataRecord.GetWithheld(this.classification);
        init => FieldAccessors.PointDataRecord.SetWithheld(ref this.classification, value);
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
    [field: System.Runtime.InteropServices.FieldOffset(Constants.PointDataRecord.ColorFieldOffset)]
    public required Color Color { get; init; }

    /// <summary>
    /// Converts a <see cref="ColorPointDataRecord"/> instance to a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The point data record.</returns>
    public static explicit operator PointDataRecord(ColorPointDataRecord pointDataRecord) => pointDataRecord.ToPointDataRecord();

    /// <summary>
    /// Converts a <see cref="PointDataRecord"/> instance to a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The color point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static explicit operator ColorPointDataRecord(PointDataRecord pointDataRecord) => FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="PointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(PointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="GpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(GpsPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="ColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(ColorPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromColorPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="GpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(GpsColorPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromColorPointDataRecord(pointDataRecord);

#if LAS1_3_OR_GREATER
    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="GpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(GpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="GpsColorWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(GpsColorWaveformPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromColorPointDataRecord(pointDataRecord);
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="ExtendedGpsPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(ExtendedGpsPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="ExtendedGpsColorPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(ExtendedGpsColorPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromExtendedColorPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromExtendedColorPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="ExtendedGpsWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(ExtendedGpsWaveformPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromExtendedPointDataRecord(pointDataRecord);

    /// <summary>
    /// Creates a <see cref="ColorPointDataRecord"/> from a <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/>.
    /// </summary>
    /// <param name="pointDataRecord">The point data record.</param>
    /// <returns>The <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord FromPointDataRecord(ExtendedGpsColorNearInfraredWaveformPointDataRecord pointDataRecord) => PointConverter.ToColorPointDataRecord.FromExtendedColorPointDataRecord(pointDataRecord);
#endif

    /// <summary>
    /// Creates a new instance of <see cref="ColorPointDataRecord"/> from the data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The instance of <see cref="ColorPointDataRecord"/>.</returns>
    public static ColorPointDataRecord Create(ReadOnlySpan<byte> data) => BitConverter.IsLittleEndian ? System.Runtime.InteropServices.MemoryMarshal.Read<ColorPointDataRecord>(data) : new(data);

    /// <inheritdoc />
    IBasePointDataRecord IBasePointDataRecord.Clone() => this;

#if NET7_0_OR_GREATER
    /// <inheritdoc />
    ColorPointDataRecord IBasePointDataRecord<ColorPointDataRecord>.Clone() => this;
#endif

    /// <inheritdoc />
    public int CopyTo(Span<byte> destination)
    {
        switch (BitConverter.IsLittleEndian)
        {
            case true:
                System.Runtime.InteropServices.MemoryMarshal.Write(destination, ref System.Runtime.CompilerServices.Unsafe.AsRef(this));
                return Size;
            default:
                return this.WriteLittleEndian(destination);
        }
    }

    /// <summary>
    /// Writes this instance into a span of bytes, as little endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
    /// <returns>The number of bytes written.</returns>
    public int WriteLittleEndian(Span<byte> destination)
    {
        FieldAccessors.PointDataRecord.SetX(destination, this.X);
        FieldAccessors.PointDataRecord.SetY(destination, this.Y);
        FieldAccessors.PointDataRecord.SetZ(destination, this.Z);
        FieldAccessors.PointDataRecord.SetIntensity(destination, this.Intensity);
        destination[Constants.PointDataRecord.FlagsFieldOffset] = this.flags;
        destination[Constants.PointDataRecord.ClassificationFieldOffset] = this.classification;
        destination[Constants.PointDataRecord.ScanAngleRankFieldOffset] = (byte)this.ScanAngleRank;
        destination[Constants.PointDataRecord.UserDataFieldOffset] = this.UserData;
        FieldAccessors.PointDataRecord.SetPointSourceId(destination, this.PointSourceId);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[Constants.PointDataRecord.ColorFieldOffset..], this.Color.R);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[(Constants.PointDataRecord.ColorFieldOffset + sizeof(ushort))..], this.Color.G);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination[(Constants.PointDataRecord.ColorFieldOffset + sizeof(ushort) + sizeof(ushort))..], this.Color.B);
        return Size;
    }

    /// <inheritdoc />
    public PointDataRecord ToPointDataRecord() => PointConverter.ToPointDataRecord.FromPointDataRecord(this);

    /// <inheritdoc />
    public GpsPointDataRecord ToGpsPointDataRecord() => PointConverter.ToGpsPointDataRecord.FromPointDataRecord(this);

    /// <inheritdoc />
    public ColorPointDataRecord ToColorPointDataRecord() => PointConverter.ToColorPointDataRecord.FromColorPointDataRecord(this);

    /// <inheritdoc />
    public GpsColorPointDataRecord ToGpsColorPointDataRecord() => PointConverter.ToGpsColorPointDataRecord.FromColorPointDataRecord(this);

#if LAS1_3_OR_GREATER
    /// <inheritdoc />
    public GpsWaveformPointDataRecord ToGpsWaveformPointDataRecord() => PointConverter.ToGpsWaveformPointDataRecord.FromPointDataRecord(this);

    /// <inheritdoc />
    public GpsColorWaveformPointDataRecord ToGpsColorWaveformPointDataRecord() => PointConverter.ToGpsColorWaveformPointDataRecord.FromPointDataRecord(this);
#endif

#if LAS1_4_OR_GREATER
    /// <inheritdoc />
    public ExtendedGpsPointDataRecord ToExtendedGpsPointDataRecord() => PointConverter.ToExtendedGpsPointDataRecord.FromPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorPointDataRecord ToExtendedGpsColorPointDataRecord() => PointConverter.ToExtendedGpsColorPointDataRecord.FromColorPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorNearInfraredPointDataRecord ToExtendedGpsColorNearInfraredPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredPointDataRecord.FromColorPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsWaveformPointDataRecord ToExtendedGpsWaveformPointDataRecord() => PointConverter.ToExtendedGpsWaveformPointDataRecord.FromPointDataRecord(this);

    /// <inheritdoc />
    public ExtendedGpsColorNearInfraredWaveformPointDataRecord ToExtendedGpsColorNearInfraredWaveformPointDataRecord() => PointConverter.ToExtendedGpsColorNearInfraredWaveformPointDataRecord.FromColorPointDataRecord(this);
#endif
}