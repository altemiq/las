// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.ExtraBytes.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// Extension methods for <see cref="ExtraBytes"/>.
/// </content>
public static partial class ExtensionMethods
{
    /// <summary>
    /// The <see cref="ExtraBytesItem"/> extensions.
    /// </summary>
    extension(ExtraBytesItem item)
    {
        /// <summary>
        /// Writes the extra byte value to the destination.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="value">The value.</param>
        /// <returns>The number of bytes written.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The type of <paramref name="value"/> is invalid.</exception>
        public int Write(Span<byte> destination, object value)
        {
            return item.DescaleAndRemoveOffset(value) switch
            {
                byte v => WriteByte(destination, v),
                sbyte v => WriteSByte(destination, v),
                short v => WriteInt16(destination, v),
                ushort v => WriteUInt16(destination, v),
                int v => WriteInt32(destination, v),
                uint v => WriteUInt32(destination, v),
                long v => WriteInt64(destination, v),
                ulong v => WriteUInt64(destination, v),
                float v => WriteSingle(destination, v),
                double v => WriteDouble(destination, v),
                byte[] v => WriteBytes(destination, v),
                _ => throw new ArgumentOutOfRangeException(nameof(value)),
            };

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteByte(Span<byte> destination, byte value)
            {
                destination[0] = value;
                return sizeof(byte);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteSByte(Span<byte> destination, sbyte value)
            {
                destination[0] = (byte)value;
                return sizeof(sbyte);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteInt16(Span<byte> destination, short value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(destination, value);
                return sizeof(short);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteUInt16(Span<byte> destination, ushort value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination, value);
                return sizeof(ushort);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteInt32(Span<byte> destination, int value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination, value);
                return sizeof(int);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteUInt32(Span<byte> destination, uint value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
                return sizeof(uint);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteInt64(Span<byte> destination, long value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(destination, value);
                return sizeof(long);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteUInt64(Span<byte> destination, ulong value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(destination, value);
                return sizeof(ulong);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteSingle(Span<byte> destination, float value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination, value);
                return sizeof(ulong);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteDouble(Span<byte> destination, double value)
            {
                System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination, value);
                return sizeof(ulong);
            }

            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            static int WriteBytes(Span<byte> destination, byte[] value)
            {
                value.AsSpan().CopyTo(destination);
                return value.Length;
            }
        }

        /// <summary>
        /// Scales and applies the offset the value, if required.
        /// </summary>
        /// <param name="value">The value to scale and offset.</param>
        /// <returns>The scaled, and offset value.</returns>
        public object ScaleAndApplyOffset(object value) => (item, value) switch
        {
            ({ HasScale: false, HasOffset: false }, var v) => v,
            ({ HasScale: true, HasOffset: true }, byte v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, byte v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, byte v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, sbyte v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, sbyte v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, sbyte v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, ushort v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, ushort v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, ushort v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, short v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, short v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, short v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, uint v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, uint v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, uint v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, int v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, int v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, int v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, ulong v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, ulong v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, ulong v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, long v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, long v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, long v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, float v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, float v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, float v) => ApplyOffset(item, v),
            ({ HasScale: true, HasOffset: true }, double v) => ScaleAndApplyOffset(item, v),
            ({ HasScale: true, HasOffset: false }, double v) => Scale(item, v),
            ({ HasScale: false, HasOffset: true }, double v) => ApplyOffset(item, v),
            _ => value,
        };

        /// <summary>
        /// Gets the value with offset removed and descaled, if required.
        /// </summary>
        /// <param name="value">The value to descale and offset.</param>
        /// <returns>The descaled, and offset value.</returns>
        public object DescaleAndRemoveOffset(object value)
        {
            var returnValue = (item, value) switch
            {
                ({ HasOffset: false, HasScale: false }, var v) => v,
                ({ HasOffset: true, HasScale: true }, byte v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, byte v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, byte v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, sbyte v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, sbyte v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, sbyte v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, ushort v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, ushort v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, ushort v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, short v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, short v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, short v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, uint v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, uint v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, uint v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, int v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, int v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, int v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, ulong v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, ulong v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, ulong v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, long v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, long v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, long v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, float v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, float v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, float v) => Descale(item, v),
                ({ HasOffset: true, HasScale: true }, double v) => RemoveOffsetAndDescale(item, v),
                ({ HasOffset: true, HasScale: false }, double v) => RemoveOffset(item, v),
                ({ HasOffset: false, HasScale: true }, double v) => Descale(item, v),
                _ => value,
            };

            return Convert.ChangeType(returnValue, item.DataType.ToType(), System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Writes the extra byte values to the destination.
    /// </summary>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="values">The values.</param>
    /// <returns>The number of bytes written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The number of values does not match the extra byte items.</exception>
    public static int Write(this ExtraBytes extraBytes, Span<byte> destination, params IReadOnlyList<object> values)
    {
        // get the value
        if (extraBytes.Count != values.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(values));
        }

        var written = 0;
        for (var i = 0; i < values.Count; i++)
        {
            written += extraBytes[i].Write(destination[written..], values[i]);
        }

        return written;
    }

    /// <summary>
    /// Get the type from the specified extra bytes data type.
    /// </summary>
    /// <param name="extraBytesDataType">The extra bytes data type.</param>
    /// <returns>The <see cref="Type"/> relating to <paramref name="extraBytesDataType"/>.</returns>
    internal static Type ToType(this ExtraBytesDataType extraBytesDataType) => extraBytesDataType switch
    {
        ExtraBytesDataType.UnsignedChar => typeof(byte),
        ExtraBytesDataType.Char => typeof(sbyte),
        ExtraBytesDataType.UnsignedShort => typeof(ushort),
        ExtraBytesDataType.Short => typeof(short),
        ExtraBytesDataType.UnsignedLong => typeof(uint),
        ExtraBytesDataType.Long => typeof(int),
        ExtraBytesDataType.UnsignedLongLong => typeof(ulong),
        ExtraBytesDataType.LongLong => typeof(long),
        ExtraBytesDataType.Float => typeof(float),
        ExtraBytesDataType.Double => typeof(double),
        ExtraBytesDataType.Undocumented => typeof(byte[]),
        _ => typeof(object),
    };

    /// <summary>
    /// Get the extra bytes data type from the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The <see cref="ExtraBytesDataType"/> relating to <paramref name="type"/>.</returns>
    internal static ExtraBytesDataType ToDataType(this Type type) => type switch
    {
        { } t when t == typeof(byte) => ExtraBytesDataType.UnsignedChar,
        { } t when t == typeof(sbyte) => ExtraBytesDataType.Char,
        { } t when t == typeof(ushort) => ExtraBytesDataType.UnsignedShort,
        { } t when t == typeof(short) => ExtraBytesDataType.Short,
        { } t when t == typeof(uint) => ExtraBytesDataType.UnsignedLong,
        { } t when t == typeof(int) => ExtraBytesDataType.Long,
        { } t when t == typeof(ulong) => ExtraBytesDataType.UnsignedLongLong,
        { } t when t == typeof(long) => ExtraBytesDataType.LongLong,
        { } t when t == typeof(float) => ExtraBytesDataType.Float,
        { } t when t == typeof(double) => ExtraBytesDataType.Double,
        _ => ExtraBytesDataType.Undocumented,
    };

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, byte value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, sbyte value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, ushort value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, short value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, uint value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, int value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, ulong value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, long value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, float value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndApplyOffset(ExtraBytesItem item, double value) => ApplyOffset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, byte value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, sbyte value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, ushort value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, short value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, uint value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, int value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, ulong value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, long value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, float value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Scale(ExtraBytesItem item, double value) => value * item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, byte value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, sbyte value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, ushort value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, short value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, uint value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, int value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, ulong value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, long value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, float value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ApplyOffset(ExtraBytesItem item, double value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, byte value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, sbyte value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, ushort value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, short value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, uint value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, int value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, ulong value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, long value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, float value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffsetAndDescale(ExtraBytesItem item, double value) => Descale(item, RemoveOffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, byte value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, sbyte value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, ushort value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, short value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, uint value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, int value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, ulong value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, long value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, float value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Descale(ExtraBytesItem item, double value) => value / item.Scale;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, byte value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, sbyte value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, ushort value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, short value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, uint value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, int value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, ulong value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, long value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, float value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double RemoveOffset(ExtraBytesItem item, double value) => value - item.Offset;
}