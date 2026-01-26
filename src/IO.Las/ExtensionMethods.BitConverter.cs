// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.BitConverter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using System.Runtime.CompilerServices;

/// <content>
/// Extension methods for <see cref="BitConverter"/>.
/// </content>
public static partial class ExtensionMethods
{
#if !NET6_0_OR_LATER
    /// <summary>
    /// The <see cref="BitConverter"/> extensions.
    /// </summary>
    extension(BitConverter)
    {
        /// <summary>
        /// Converts the specified double-precision floating point number to a 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 64-bit unsigned integer whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DoubleToUInt64Bits(double value) => Unsafe.ReadUnaligned<ulong>(ref Unsafe.As<double, byte>(ref value));

        /// <summary>
        /// Converts the specified single-precision floating point number to a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 32-bit unsigned integer whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SingleToUInt32Bits(float value) => Unsafe.ReadUnaligned<uint>(ref Unsafe.As<float, byte>(ref value));

        /// <summary>
        /// Converts the specified single-precision floating point number to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 32-bit signed integer whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SingleToInt32Bits(float value) => Unsafe.ReadUnaligned<int>(ref Unsafe.As<float, byte>(ref value));

        /// <summary>
        /// Converts the specified 64-bit unsigned integer to a double-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A double-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double UInt64BitsToDouble(ulong value) => Unsafe.ReadUnaligned<double>(ref Unsafe.As<ulong, byte>(ref value));

        /// <summary>
        /// Converts the specified 32-bit signed integer to a single-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A single-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Int32BitsToSingle(int value) => Unsafe.ReadUnaligned<float>(ref Unsafe.As<int, byte>(ref value));

        /// <summary>
        /// Converts the specified 32-bit unsigned integer to a single-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A single-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float UInt32BitsToSingle(uint value) => Unsafe.ReadUnaligned<float>(ref Unsafe.As<uint, byte>(ref value));

        /// <summary>
        /// Converts the specified 64-bit signed integer to a 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 64-bit unsigned integer whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Int64BitsToUInt64Bits(long value) => Unsafe.ReadUnaligned<ulong>(ref Unsafe.As<long, byte>(ref value));

        /// <summary>
        /// Converts the specified 64-bit unsigned integer to a 64-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 64-bit signed integer whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long UInt64BitsToInt64Bits(ulong value) => Unsafe.ReadUnaligned<long>(ref Unsafe.As<ulong, byte>(ref value));
    }
#endif
}