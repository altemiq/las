// -----------------------------------------------------------------------
// <copyright file="ExtendedBitConverter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using System.Runtime.CompilerServices;

/// <summary>
/// Converts base data types to an array of bytes, and an array of bytes to base data types.
/// </summary>
internal static class ExtendedBitConverter
{
    /// <summary>
    /// Converts the specified double-precision floating point number to a 64-bit unsigned integer.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>A 64-bit unsigned integer whose value is equivalent to <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong DoubleToUInt64Bits(double value) => *(ulong*)&value;

    /// <summary>
    /// Converts the specified single-precision floating point number to a 32-bit unsigned integer.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>A 32-bit unsigned integer whose value is equivalent to <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint SingleToUInt32Bits(float value) => *(uint*)&value;

    /// <summary>
    /// Converts the specified 64-bit unsigned integer to a double-precision floating point number.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>A double-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double UInt64BitsToDouble(ulong value) => *(double*)&value;

    /// <summary>
    /// Converts the specified 32-bit unsigned integer to a single-precision floating point number.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>A single-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float UInt32BitsToSingle(uint value) => *(float*)&value;

    /// <summary>
    /// Converts the specified 64-bit signed integer to a 64-bit unsigned integer.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>A 64-bit unsigned integer whose value is equivalent to <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong Int64BitsToUInt64Bits(long value) => *(ulong*)&value;

    /// <summary>
    /// Converts the specified 64-bit unsigned integer to a 64-bit signed integer.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <returns>A 64-bit signed integer whose value is equivalent to <paramref name="value"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe long UInt64BitsToInt64Bits(ulong value) => *(long*)&value;
}