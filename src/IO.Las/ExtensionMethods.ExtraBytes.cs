// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.ExtraBytes.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable CA1708, S2325, SA1101

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
        public int Write(Span<byte> destination, ExtraBytesValue value)
        {
            return item.RemoveOffsetAndDescale(value) switch
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
        /// <exception cref="System.Diagnostics.UnreachableException"><paramref name="value"/> is not a valid <see cref="ExtraBytesValue"/>.</exception>
        public ExtraBytesValue ScaleAndApplyOffset(ExtraBytesValue value)
        {
            return value.TryGetValue(out byte[]? _)
                ? value
                : item switch
                {
                    { HasScale: false, HasOffset: false } => value,
                    { HasScale: true, HasOffset: true } => ScaleAndApplyOffsetCore(item, value),
                    { HasScale: true, HasOffset: false } => ScaleCore(item, value),
                    { HasScale: false, HasOffset: true } => ApplyOffsetCore(item, value),
                };

            static double ScaleAndApplyOffsetCore(ExtraBytesItem item, ExtraBytesValue value)
            {
                return value switch
                {
                    byte v => ScaleAndApplyOffset(item, v),
                    sbyte v => ScaleAndApplyOffset(item, v),
                    ushort v => ScaleAndApplyOffset(item, v),
                    short v => ScaleAndApplyOffset(item, v),
                    uint v => ScaleAndApplyOffset(item, v),
                    int v => ScaleAndApplyOffset(item, v),
                    ulong v => ScaleAndApplyOffset(item, v),
                    long v => ScaleAndApplyOffset(item, v),
                    float v => ScaleAndApplyOffset(item, v),
                    double v => ScaleAndApplyOffset(item, v),
                    _ => throw new System.Diagnostics.UnreachableException(),
                };
            }

            static double ScaleCore(ExtraBytesItem item, ExtraBytesValue value)
            {
                return value switch
                {
                    byte v => Scale(item, v),
                    sbyte v => Scale(item, v),
                    ushort v => Scale(item, v),
                    short v => Scale(item, v),
                    uint v => Scale(item, v),
                    int v => Scale(item, v),
                    ulong v => Scale(item, v),
                    long v => Scale(item, v),
                    float v => Scale(item, v),
                    double v => Scale(item, v),
                    _ => throw new System.Diagnostics.UnreachableException(),
                };
            }

            static double ApplyOffsetCore(ExtraBytesItem item, ExtraBytesValue value)
            {
                return value switch
                {
                    byte v => ApplyOffset(item, v),
                    sbyte v => ApplyOffset(item, v),
                    ushort v => ApplyOffset(item, v),
                    short v => ApplyOffset(item, v),
                    uint v => ApplyOffset(item, v),
                    int v => ApplyOffset(item, v),
                    ulong v => ApplyOffset(item, v),
                    long v => ApplyOffset(item, v),
                    float v => ApplyOffset(item, v),
                    double v => ApplyOffset(item, v),
                    _ => throw new System.Diagnostics.UnreachableException(),
                };
            }
        }

        /// <summary>
        /// Gets the value with offset removed and descaled, if required.
        /// </summary>
        /// <param name="value">The value to descale and offset.</param>
        /// <returns>The descaled, and offset value.</returns>
        /// <exception cref="System.Diagnostics.UnreachableException"><paramref name="value"/> is not a valid <see cref="ExtraBytesValue"/>.</exception>
        public ExtraBytesValue RemoveOffsetAndDescale(ExtraBytesValue value)
        {
            if (value is byte[] || item is { HasOffset: false, HasScale: false })
            {
                return value;
            }

            var doubleValue = item switch
            {
                { HasOffset: true, HasScale: true } => RemoveOffsetAndDescaleCore(item, value),
                { HasOffset: true, HasScale: false } => RemoveOffsetCore(item, value),
                { HasOffset: false, HasScale: true } => DescaleCore(item, value),
                _ => throw new System.Diagnostics.UnreachableException(),
            };

            return new((ulong)(long)doubleValue, item.DataType);

            static double RemoveOffsetAndDescaleCore(ExtraBytesItem item, ExtraBytesValue value)
            {
                return value switch
                {
                    byte v => RemoveOffsetAndDescale(item, v),
                    sbyte v => RemoveOffsetAndDescale(item, v),
                    ushort v => RemoveOffsetAndDescale(item, v),
                    short v => RemoveOffsetAndDescale(item, v),
                    uint v => RemoveOffsetAndDescale(item, v),
                    int v => RemoveOffsetAndDescale(item, v),
                    ulong v => RemoveOffsetAndDescale(item, v),
                    long v => RemoveOffsetAndDescale(item, v),
                    float v => RemoveOffsetAndDescale(item, v),
                    double v => RemoveOffsetAndDescale(item, v),
                    _ => throw new System.Diagnostics.UnreachableException(),
                };
            }

            static double DescaleCore(ExtraBytesItem item, ExtraBytesValue value)
            {
                return value switch
                {
                    byte v => Descale(item, v),
                    sbyte v => Descale(item, v),
                    ushort v => Descale(item, v),
                    short v => Descale(item, v),
                    uint v => Descale(item, v),
                    int v => Descale(item, v),
                    ulong v => Descale(item, v),
                    long v => Descale(item, v),
                    float v => Descale(item, v),
                    double v => Descale(item, v),
                    _ => throw new System.Diagnostics.UnreachableException(),
                };
            }

            static double RemoveOffsetCore(ExtraBytesItem item, ExtraBytesValue value)
            {
                return value switch
                {
                    byte v => RemoveOffset(item, v),
                    sbyte v => RemoveOffset(item, v),
                    ushort v => RemoveOffset(item, v),
                    short v => RemoveOffset(item, v),
                    uint v => RemoveOffset(item, v),
                    int v => RemoveOffset(item, v),
                    ulong v => RemoveOffset(item, v),
                    long v => RemoveOffset(item, v),
                    float v => RemoveOffset(item, v),
                    double v => RemoveOffset(item, v),
                    _ => throw new System.Diagnostics.UnreachableException(),
                };
            }
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
    public static int Write(this ExtraBytes extraBytes, Span<byte> destination, params IReadOnlyList<ExtraBytesValue> values)
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