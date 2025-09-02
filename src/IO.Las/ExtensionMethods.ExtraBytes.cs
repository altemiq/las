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
    /// Scales and offset the value, if required.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="value">The value to scale and offset.</param>
    /// <returns>The scaled, and offset value.</returns>
    public static object ScaleAndOffset(this ExtraBytesItem item, object value) => (item, value) switch
    {
        ({ HasScale: false, HasOffset: false }, var v) => v,
        ({ HasScale: true, HasOffset: true }, byte v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, byte v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, byte v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, sbyte v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, sbyte v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, sbyte v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, ushort v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, ushort v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, ushort v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, short v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, short v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, short v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, uint v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, uint v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, uint v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, int v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, int v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, int v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, ulong v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, ulong v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, ulong v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, long v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, long v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, long v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, float v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, float v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, float v) => Offset(item, v),
        ({ HasScale: true, HasOffset: true }, double v) => ScaleAndOffset(item, v),
        ({ HasScale: true, HasOffset: false }, double v) => Scale(item, v),
        ({ HasScale: false, HasOffset: true }, double v) => Offset(item, v),
        _ => value,
    };

    /// <summary>
    /// Gets the value with offset removed and descaled, if required.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="value">The value to descale and offset.</param>
    /// <returns>The descaled, and offsetted value.</returns>
    public static object DescaleAndOffset(this ExtraBytesItem item, object value)
    {
        var returnValue = (item, value) switch
        {
            ({ HasOffset: false, HasScale: false }, var v) => v,
            ({ HasOffset: true, HasScale: true }, byte v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, byte v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, byte v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, sbyte v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, sbyte v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, sbyte v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, ushort v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, ushort v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, ushort v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, short v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, short v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, short v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, uint v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, uint v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, uint v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, int v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, int v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, int v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, ulong v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, ulong v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, ulong v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, long v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, long v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, long v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, float v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, float v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, float v) => Descale(item, v),
            ({ HasOffset: true, HasScale: true }, double v) => DeoffsetAndDescale(item, v),
            ({ HasOffset: true, HasScale: false }, double v) => Deoffset(item, v),
            ({ HasOffset: false, HasScale: true }, double v) => Descale(item, v),
            _ => value,
        };

        return Convert.ChangeType(returnValue, item.DataType.ToType(), System.Globalization.CultureInfo.InvariantCulture);
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
    private static double ScaleAndOffset(ExtraBytesItem item, byte value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, sbyte value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, ushort value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, short value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, uint value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, int value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, ulong value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, long value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, float value) => Offset(item, Scale(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double ScaleAndOffset(ExtraBytesItem item, double value) => Offset(item, Scale(item, value));

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
    private static double Offset(ExtraBytesItem item, byte value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, sbyte value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, ushort value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, short value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, uint value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, int value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, ulong value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, long value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, float value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Offset(ExtraBytesItem item, double value) => value + item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, byte value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, sbyte value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, ushort value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, short value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, uint value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, int value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, ulong value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, long value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, float value) => Descale(item, Deoffset(item, value));

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double DeoffsetAndDescale(ExtraBytesItem item, double value) => Descale(item, Deoffset(item, value));

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
    private static double Deoffset(ExtraBytesItem item, byte value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, sbyte value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, ushort value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, short value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, uint value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, int value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, ulong value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, long value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, float value) => value - item.Offset;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double Deoffset(ExtraBytesItem item, double value) => value - item.Offset;
}