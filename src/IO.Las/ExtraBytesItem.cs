// -----------------------------------------------------------------------
// <copyright file="ExtraBytesItem.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

/// <summary>
/// The <see cref="ExtraBytes"/> item.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 192)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2292:Trivial properties should be auto-implemented", Justification = "This needs sequential layout")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1085:Use auto-implemented property", Justification = "This needs sequential layout")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable", Justification = "This needs sequential layout.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CollectionNeverQueried.Local", Justification = "This needs sequential layout.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible", Justification = "This needs sequential layout.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required for automated cleanup")]
public readonly record struct ExtraBytesItem
{
    /// <summary>
    /// The Beam ID extra bytes item.
    /// </summary>
    public static readonly ExtraBytesItem BeamId = new()
    {
        DataType = ExtraBytesDataType.UnsignedChar,
        Name = "beam id",
        Description = "Extended channel ID",
        Options = ExtraBytesOptions.NoData | ExtraBytesOptions.Min | ExtraBytesOptions.Max,
        Min = byte.MinValue,
        Max = byte.MaxValue - 1,
        NoData = byte.MaxValue,
    };

    /// <summary>
    /// The echo width extra bytes item.
    /// </summary>
    public static readonly ExtraBytesItem EchoWidth = new()
    {
        DataType = ExtraBytesDataType.UnsignedShort,
        Name = "echo width",
        Description = "Full width at half maximum [ns]",
        Offset = default,
        Scale = 0.1,
        Options = ExtraBytesOptions.All,
        Min = (ushort)1,
        Max = (ushort)10000,
        NoData = (ushort)0,
    };

    /// <summary>
    /// The height above ground extra bytes item.
    /// </summary>
    public static readonly ExtraBytesItem HeightAboveGround = new()
    {
        DataType = ExtraBytesDataType.Short,
        Name = "height above ground",
        Description = "Vertical point to TIN distance",
        Offset = default,
        Scale = 0.01,
        Options = ExtraBytesOptions.All,
        Min = short.MinValue,
        Max = short.MaxValue,
        NoData = unchecked((short)(short.MaxValue + 1)),
    };

    private const int ReservedSize = 2;
    private const int NameSize = 32;
    private const int UnusedSize = 4;
    private const int DeprecatedSize = 16;
    private const int AnyDataSize = 8;
    private const int DescriptionSize = 32;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = ReservedSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] reserved;

    private readonly ExtraBytesDataType dataType;

    private readonly ExtraBytesOptions options;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NameSize)]
    private readonly string name;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = UnusedSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] unused;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = AnyDataSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] noData;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DeprecatedSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] deprecated1;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = AnyDataSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] min;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DeprecatedSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] deprecated2;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = AnyDataSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] max;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DeprecatedSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] deprecated3;

    private readonly double scale;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DeprecatedSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] deprecated4;

    private readonly double offset;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DeprecatedSize, ArraySubType = UnmanagedType.U1)]
    private readonly byte[] deprecated5;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DescriptionSize)]
    private readonly string description;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtraBytesItem"/> struct.
    /// </summary>
    /// <param name="source">The source.</param>
    internal ExtraBytesItem(ReadOnlySpan<byte> source)
    {
        this.reserved = source[..2].ToArray();
        this.dataType = (ExtraBytesDataType)source[2];
        this.options = (ExtraBytesOptions)source[3];
        this.name = System.Text.Encoding.UTF8.GetNullTerminatedString(source[4..36]);
        this.unused = source[36..40].ToArray();
        this.noData = source[40..48].ToArray();
        this.deprecated1 = source[48..64].ToArray();
        this.min = source[64..72].ToArray();
        this.deprecated2 = source[72..88].ToArray();
        this.max = source[88..96].ToArray();
        this.deprecated3 = source[96..112].ToArray();
        this.scale = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[112..120]);
        this.deprecated4 = source[120..136].ToArray();
        this.offset = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source[136..144]);
        this.deprecated5 = source[144..160].ToArray();
        this.description = System.Text.Encoding.UTF8.GetNullTerminatedString(source[160..192]);
    }

    /// <summary>
    /// Gets the data type.
    /// </summary>
    public ExtraBytesDataType DataType { get => this.dataType; init => this.dataType = value; }

    /// <summary>
    /// Gets the options.
    /// </summary>
    public ExtraBytesOptions Options { get => this.options; init => this.options = value; }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get => this.name; init => this.name = value; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get => this.description; init => this.description = value; }

    /// <summary>
    /// Gets the no-data value.
    /// </summary>
    public ExtraBytesValue NoData
    {
        get => this.GetDataAsFull(this.noData);
        init
        {
            this.noData = new byte[AnyDataSize];
            this.SetDataAsFull(this.noData, value);
        }
    }

    /// <summary>
    /// Gets the minimum value.
    /// </summary>
    public ExtraBytesValue Min
    {
        get => this.GetDataAsFull(this.min);
        init
        {
            this.min ??= new byte[AnyDataSize];
            this.SetDataAsFull(this.min, value);
        }
    }

    /// <summary>
    /// Gets the maximum value.
    /// </summary>
    public ExtraBytesValue Max
    {
        get => this.GetDataAsFull(this.max);
        init
        {
            this.max ??= new byte[AnyDataSize];
            this.SetDataAsFull(this.max, value);
        }
    }

    /// <summary>
    /// Gets the scale.
    /// </summary>
    public double Scale { get => this.scale; init => this.scale = value; }

    /// <summary>
    /// Gets a value indicating whether this instance has a value <see cref="Scale"/> value.
    /// </summary>
    public bool HasScale => (this.Options & ExtraBytesOptions.Scale) is ExtraBytesOptions.Scale;

    /// <summary>
    /// Gets the offset.
    /// </summary>
    public double Offset { get => this.offset; init => this.offset = value; }

    /// <summary>
    /// Gets a value indicating whether this instance has a value <see cref="Offset"/> value.
    /// </summary>
    public bool HasOffset => (this.Options & ExtraBytesOptions.Offset) is ExtraBytesOptions.Offset;

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public ExtraBytesValue GetValue(ReadOnlyMemory<byte> source) => this.GetValue(source.Span);

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public ExtraBytesValue GetValue(ReadOnlySpan<byte> source)
    {
        return GetItemValue(this, source);

        static ExtraBytesValue GetItemValue(ExtraBytesItem item, ReadOnlySpan<byte> source)
        {
            return (item.HasScale, item.HasOffset, item.DataType) switch
            {
                (_, _, ExtraBytesDataType.Undocumented) => source[..(int)item.options].ToArray(),
                (false, false, ExtraBytesDataType.UnsignedChar) => source[0],
                (true, true, ExtraBytesDataType.UnsignedChar) => ScaleAndOffsetByte(item, source[0]),
                (true, false, ExtraBytesDataType.UnsignedChar) => ScaleByte(item, source[0]),
                (false, true, ExtraBytesDataType.UnsignedChar) => OffsetByte(item, source[0]),
                (false, false, ExtraBytesDataType.Char) => (sbyte)source[0],
                (true, true, ExtraBytesDataType.Char) => ScaleAndOffsetSByte(item, (sbyte)source[0]),
                (true, false, ExtraBytesDataType.Char) => ScaleSByte(item, (sbyte)source[0]),
                (false, true, ExtraBytesDataType.Char) => OffsetSByte(item, (sbyte)source[0]),
                (false, false, ExtraBytesDataType.UnsignedShort) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source),
                (true, true, ExtraBytesDataType.UnsignedShort) => ScaleAndOffsetUInt16(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source)),
                (true, false, ExtraBytesDataType.UnsignedShort) => ScaleUInt16(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source)),
                (false, true, ExtraBytesDataType.UnsignedShort) => OffsetUInt16(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(source)),
                (false, false, ExtraBytesDataType.Short) => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(source),
                (true, true, ExtraBytesDataType.Short) => ScaleAndOffsetInt16(item, System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(source)),
                (true, false, ExtraBytesDataType.Short) => ScaleInt16(item, System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(source)),
                (false, true, ExtraBytesDataType.Short) => OffsetInt16(item, System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(source)),
                (false, false, ExtraBytesDataType.UnsignedLong) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source),
                (true, true, ExtraBytesDataType.UnsignedLong) => ScaleAndOffsetUInt32(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source)),
                (true, false, ExtraBytesDataType.UnsignedLong) => ScaleUInt32(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source)),
                (false, true, ExtraBytesDataType.UnsignedLong) => OffsetUInt32(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source)),
                (false, false, ExtraBytesDataType.Long) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source),
                (true, true, ExtraBytesDataType.Long) => ScaleAndOffsetInt32(item, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source)),
                (true, false, ExtraBytesDataType.Long) => ScaleInt32(item, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source)),
                (false, true, ExtraBytesDataType.Long) => OffsetInt32(item, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source)),
                (false, false, ExtraBytesDataType.UnsignedLongLong) => System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source),
                (true, true, ExtraBytesDataType.UnsignedLongLong) => ScaleAndOffsetUInt64(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source)),
                (true, false, ExtraBytesDataType.UnsignedLongLong) => ScaleUInt64(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source)),
                (false, true, ExtraBytesDataType.UnsignedLongLong) => OffsetUInt64(item, System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(source)),
                (false, false, ExtraBytesDataType.LongLong) => System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(source),
                (true, true, ExtraBytesDataType.LongLong) => ScaleAndOffsetInt64(item, System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(source)),
                (true, false, ExtraBytesDataType.LongLong) => ScaleInt64(item, System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(source)),
                (false, true, ExtraBytesDataType.LongLong) => OffsetInt64(item, System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(source)),
                (false, false, ExtraBytesDataType.Float) => System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source),
                (true, true, ExtraBytesDataType.Float) => ScaleAndOffsetSingle(item, System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source)),
                (true, false, ExtraBytesDataType.Float) => ScaleSingle(item, System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source)),
                (false, true, ExtraBytesDataType.Float) => OffsetSingle(item, System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source)),
                (false, false, ExtraBytesDataType.Double) => System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source),
                (true, true, ExtraBytesDataType.Double) => ScaleAndOffsetDouble(item, System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source)),
                (true, false, ExtraBytesDataType.Double) => ScaleDouble(item, System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source)),
                (false, true, ExtraBytesDataType.Double) => OffsetDouble(item, System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source)),
                _ => throw new InvalidCastException(),
            };

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetByte(ExtraBytesItem item, byte value)
            {
                return ScaleByte(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetSByte(ExtraBytesItem item, sbyte value)
            {
                return ScaleSByte(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetUInt16(ExtraBytesItem item, ushort value)
            {
                return ScaleUInt16(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetInt16(ExtraBytesItem item, short value)
            {
                return ScaleInt16(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetUInt32(ExtraBytesItem item, uint value)
            {
                return ScaleUInt32(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetInt32(ExtraBytesItem item, int value)
            {
                return ScaleInt32(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetUInt64(ExtraBytesItem item, ulong value)
            {
                return ScaleUInt64(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetInt64(ExtraBytesItem item, long value)
            {
                return ScaleInt64(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetSingle(ExtraBytesItem item, float value)
            {
                return ScaleSingle(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleAndOffsetDouble(ExtraBytesItem item, double value)
            {
                return ScaleDouble(item, value) + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleByte(ExtraBytesItem item, byte value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleSByte(ExtraBytesItem item, sbyte value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleUInt16(ExtraBytesItem item, ushort value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleInt16(ExtraBytesItem item, short value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleUInt32(ExtraBytesItem item, uint value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleInt32(ExtraBytesItem item, int value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleUInt64(ExtraBytesItem item, ulong value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleInt64(ExtraBytesItem item, long value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleSingle(ExtraBytesItem item, float value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double ScaleDouble(ExtraBytesItem item, double value)
            {
                return value * item.Scale;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetByte(ExtraBytesItem item, byte value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetSByte(ExtraBytesItem item, sbyte value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetUInt16(ExtraBytesItem item, ushort value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetInt16(ExtraBytesItem item, short value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetUInt32(ExtraBytesItem item, uint value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetInt32(ExtraBytesItem item, int value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetUInt64(ExtraBytesItem item, ulong value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetInt64(ExtraBytesItem item, long value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetSingle(ExtraBytesItem item, float value)
            {
                return value + item.Offset;
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static double OffsetDouble(ExtraBytesItem item, double value)
            {
                return value + item.Offset;
            }
        }
    }

    /// <summary>
    /// Gets the data.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The value.</returns>
    public ValueTask<ExtraBytesValue> GetValueAsync(ReadOnlyMemory<byte> source) => new(this.GetValue(source.Span));

    /// <summary>
    /// Creates an instance of <see cref="ExtraBytesItem"/> from the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The instance of <see cref="ExtraBytesItem"/>.</returns>
    internal static ExtraBytesItem Create(ReadOnlySpan<byte> source) => BitConverter.IsLittleEndian
        ? Marshal.SpanToStructure<ExtraBytesItem>(source)
        : new(source);

    /// <summary>
    /// Writes this instance to the destination.
    /// </summary>
    /// <param name="destination">The destination.</param>
    internal void CopyTo(Span<byte> destination)
    {
        if (BitConverter.IsLittleEndian)
        {
            Marshal.StructureToSpan(this, destination);
        }
        else
        {
            destination[2] = (byte)this.dataType;
            destination[3] = (byte)this.options;
            System.Text.Encoding.UTF8.GetNullTerminatedBytes(this.name, destination[4..36]);
            this.noData.CopyTo(destination[40..48]);
            this.min.CopyTo(destination[64..72]);
            this.max.CopyTo(destination[88..96]);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[112..120], this.scale);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[136..142], this.offset);
            System.Text.Encoding.UTF8.GetNullTerminatedBytes(this.description, destination[160..192]);
        }
    }

#if NET8_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use \'CompositeFormat\'", Justification = "This is for an exception")]
#endif
    private static ExtraBytesValue GetDataAsFull(byte[] data, ExtraBytesDataType dataType) => dataType switch
    {
        ExtraBytesDataType.UnsignedChar or ExtraBytesDataType.UnsignedShort or ExtraBytesDataType.UnsignedLong or ExtraBytesDataType.UnsignedLongLong => System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data),
        ExtraBytesDataType.Char or ExtraBytesDataType.Short or ExtraBytesDataType.Long or ExtraBytesDataType.LongLong => System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(data),
        ExtraBytesDataType.Float or ExtraBytesDataType.Double => System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data),
        ExtraBytesDataType.Undocumented => data,
        _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, string.Format(Properties.Resources.Culture, Properties.Resources.CannotSetOn, "data-type", dataType)),
    };

    private static void SetDataAsFull(byte[] data, ExtraBytesValue value, ExtraBytesDataType dataType)
    {
        if (value.Value is null)
        {
            Array.Clear(data, 0, data.Length);
            return;
        }

        switch (value.Value, dataType)
        {
            case (byte @byte, ExtraBytesDataType.UnsignedChar):
                WriteUInt64(@byte);
                break;
            case (sbyte @sbyte, ExtraBytesDataType.Char):
                WriteInt64(@sbyte);
                break;
            case (ushort @ushort, ExtraBytesDataType.UnsignedShort):
                WriteUInt64(@ushort);
                break;
            case (short @short, ExtraBytesDataType.Short):
                WriteInt64(@short);
                break;
            case (uint @uint, ExtraBytesDataType.UnsignedLong):
                WriteUInt64(@uint);
                break;
            case (int @int, ExtraBytesDataType.Long):
                WriteInt64(@int);
                break;
            case (ulong @ulong, ExtraBytesDataType.UnsignedLongLong):
                WriteUInt64(@ulong);
                break;
            case (long @long, ExtraBytesDataType.LongLong):
                WriteInt64(@long);
                break;
            case (float @float, ExtraBytesDataType.Float):
                WriteDouble(@float);
                break;
            case (double @double, ExtraBytesDataType.Double):
                WriteDouble(@double);
                break;
            case (byte[] bytes, ExtraBytesDataType.Undocumented):
                bytes.CopyTo(data.AsSpan());
                Array.Clear(data, bytes.Length, 8 - bytes.Length);
                break;
        }

        void WriteInt64(long v)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(data, v);
        }

        void WriteUInt64(ulong v)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(data, v);
        }

        void WriteDouble(double v)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(data, v);
        }
    }

    private ExtraBytesValue GetDataAsFull(byte[] data) => GetDataAsFull(data, this.dataType);

    private void SetDataAsFull(byte[] data, ExtraBytesValue value) => SetDataAsFull(data, value, this.dataType);
}