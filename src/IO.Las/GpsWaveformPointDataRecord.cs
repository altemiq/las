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
        this.X = FieldAccessors.PointDataRecord.GetX(data);
        this.Y = FieldAccessors.PointDataRecord.GetY(data);
        this.Z = FieldAccessors.PointDataRecord.GetZ(data);
        this.Intensity = FieldAccessors.PointDataRecord.GetIntensity(data);
        this.flags = data[Constants.PointDataRecord.FlagsFieldOffset];
        this.classification = data[Constants.PointDataRecord.ClassificationFieldOffset];
        this.ScanAngleRank = (sbyte)data[Constants.PointDataRecord.ScanAngleRankFieldOffset];
        this.UserData = data[Constants.PointDataRecord.UserDataFieldOffset];
        this.PointSourceId = FieldAccessors.PointDataRecord.GetPointSourceId(data);
        this.GpsTime = FieldAccessors.PointDataRecord.GetGpsTime(data);
        this.WavePacketDescriptorIndex = data[Constants.PointDataRecord.GpsWaveformFieldOffset];
        this.ByteOffsetToWaveformData = System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset]);
        this.WaveformPacketSizeInBytes = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset]);
        this.ReturnPointWaveformLocation = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset]);
        this.ParametricDx = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[ParametricDxFieldOffset..ParametricDyFieldOffset]);
        this.ParametricDy = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[ParametricDyFieldOffset..ParametricDzFieldOffset]);
        this.ParametricDz = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(data[ParametricDzFieldOffset..Size]);
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

#if LAS1_4_OR_GREATER
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
#endif

    /// <summary>
    /// Creates a new instance of <see cref="GpsWaveformPointDataRecord"/> from the data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The instance of <see cref="GpsWaveformPointDataRecord"/>.</returns>
    public static GpsWaveformPointDataRecord Create(ReadOnlySpan<byte> data) => BitConverter.IsLittleEndian ? System.Runtime.InteropServices.MemoryMarshal.Read<GpsWaveformPointDataRecord>(data) : new(data);

    /// <inheritdoc />
    IBasePointDataRecord IBasePointDataRecord.Clone() => this;

#if NET7_0_OR_GREATER
    /// <inheritdoc />
    GpsWaveformPointDataRecord IBasePointDataRecord<GpsWaveformPointDataRecord>.Clone() => this;
#endif

    /// <inheritdoc />
    public int CopyTo(Span<byte> destination)
    {
        switch (BitConverter.IsLittleEndian)
        {
            case true:
#if NET8_0_OR_GREATER
                System.Runtime.InteropServices.MemoryMarshal.Write(destination, in this);
#else
                System.Runtime.InteropServices.MemoryMarshal.Write(destination, ref System.Runtime.CompilerServices.Unsafe.AsRef(this));
#endif
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
        FieldAccessors.PointDataRecord.SetGpsTime(destination, this.GpsTime);
        destination[Constants.PointDataRecord.GpsWaveformFieldOffset] = this.WavePacketDescriptorIndex;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination[ByteOffsetToWaveformDataFieldOffset..WaveformPacketSizeInBytesFieldOffset], this.ByteOffsetToWaveformData);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination[WaveformPacketSizeInBytesFieldOffset..ReturnPointWaveformLocationFieldOffset], this.WaveformPacketSizeInBytes);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[ReturnPointWaveformLocationFieldOffset..ParametricDxFieldOffset], this.ReturnPointWaveformLocation);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[ParametricDxFieldOffset..ParametricDyFieldOffset], this.ParametricDx);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[ParametricDyFieldOffset..ParametricDzFieldOffset], this.ParametricDy);
        System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination[ParametricDzFieldOffset..Size], this.ParametricDz);
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

#if LAS1_4_OR_GREATER
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
#endif
}