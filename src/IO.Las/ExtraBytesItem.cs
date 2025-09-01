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
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2292:Trivial properties should be auto-implemented", Justification = "This needs sequential layout.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible", Justification = "This needs sequential layout.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1085:Use auto-implemented property", Justification = "This needs sequential layout.")]
public readonly record struct ExtraBytesItem
{
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
    /// Initialises a new instance of the <see cref="ExtraBytesItem"/> struct.
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
    public object NoData
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
    public object Min
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
    public object Max
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
    /// Reads an instance of <see cref="ExtraBytesItem"/> from the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The instance of <see cref="ExtraBytesItem"/>.</returns>
    internal static ExtraBytesItem Read(ReadOnlySpan<byte> source)
    {
        if (BitConverter.IsLittleEndian)
        {
            return Marshal.PtrToStructure<ExtraBytesItem>(GetIntPtrFromSpan(source));

            static unsafe IntPtr GetIntPtrFromSpan<T>(ReadOnlySpan<T> span)
            {
                // Cast the reference to an IntPtr
                return (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            }
        }

        return new(source);
    }

    /// <summary>
    /// Writes this instance to the destination.
    /// </summary>
    /// <param name="destination">The destination.</param>
    internal void Write(Span<byte> destination)
    {
        if (BitConverter.IsLittleEndian)
        {
            Marshal.StructureToPtr(this, GetIntPtrFromSpan(destination), fDeleteOld: false);

            static unsafe IntPtr GetIntPtrFromSpan<T>(Span<T> span)
            {
                // Cast the reference to an IntPtr
                return (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            }
        }
        else
        {
            destination[2] = (byte)this.dataType;
            destination[3] = (byte)this.options;
            WriteString(this.name, destination[4..36]);
            this.noData.CopyTo(destination[40..48]);
            this.min.CopyTo(destination[64..72]);
            this.max.CopyTo(destination[88..96]);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[112..120], this.scale);
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination[136..142], this.offset);
            WriteString(this.description, destination[160..192]);
        }

        static void WriteString(string value, Span<byte> destination)
        {
            var chars = value.AsSpan();
            if (chars.Length > destination.Length)
            {
                chars = chars[..destination.Length];
            }

            var written = System.Text.Encoding.UTF8.GetBytes(chars, destination);

            if (written < destination.Length)
            {
                destination[written..].Clear();
            }
        }
    }

    private static object GetDataAsFull(byte[] data, ExtraBytesDataType dataType) => dataType switch
    {
        ExtraBytesDataType.UnsignedChar or ExtraBytesDataType.UnsignedShort or ExtraBytesDataType.UnsignedLong or ExtraBytesDataType.UnsignedLongLong => System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data),
        ExtraBytesDataType.Char or ExtraBytesDataType.Short or ExtraBytesDataType.Long or ExtraBytesDataType.LongLong => System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(data),
        ExtraBytesDataType.Float or ExtraBytesDataType.Double => System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data),
        ExtraBytesDataType.Undocumented => data,
        _ => throw new ArgumentOutOfRangeException(nameof(dataType), string.Format(Properties.Resources.Culture, Properties.Resources.CannotSetOn, "data-type", dataType)),
    };

    private static void SetDataAsFull(byte[] data, object? value, ExtraBytesDataType dataType)
    {
        if (value is null)
        {
            Array.Clear(data, 0, data.Length);
            return;
        }

        switch (value, dataType)
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

    private object GetDataAsFull(byte[] data) => GetDataAsFull(data, this.dataType);

    private void SetDataAsFull(byte[] data, object? value) => SetDataAsFull(data, value, this.dataType);
}