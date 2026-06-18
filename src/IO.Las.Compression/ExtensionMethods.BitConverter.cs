// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.BitConverter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using System.Runtime.CompilerServices;

/// <summary>
/// Extensions for <see cref="BitConverter"/>.
/// </summary>
internal static partial class ExtensionMethods
{
    /// <summary>
    /// The <see cref="BitConverter"/> extensions.
    /// </summary>
    extension(BitConverter)
    {
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
}