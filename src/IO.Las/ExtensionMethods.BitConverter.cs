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
        public static unsafe ulong DoubleToUInt64Bits(double value) => *(ulong*)&value;

        /// <summary>
        /// Converts the specified single-precision floating point number to a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 32-bit unsigned integer whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint SingleToUInt32Bits(float value) => *(uint*)&value;

        /// <summary>
        /// Converts the specified single-precision floating point number to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 32-bit signed integer whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int SingleToInt32Bits(float value) => *(int*)&value;

        /// <summary>
        /// Converts the specified 64-bit unsigned integer to a double-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A double-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double UInt64BitsToDouble(ulong value) => *(double*)&value;

        /// <summary>
        /// Converts the specified 32-bit signed integer to a single-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A single-precision floating point number whose value is equivalent to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float Int32BitsToSingle(int value) => *(float*)&value;

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
#endif
}