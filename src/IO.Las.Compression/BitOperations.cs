// -----------------------------------------------------------------------
// <copyright file="BitOperations.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if !NETCOREAPP3_0_OR_GREATER

#pragma warning disable IDE0130, CheckNamespace
namespace System.Numerics;
#pragma warning restore IDE0130, CheckNamespace

/// <summary>
/// Provides utility methods for intrinsic bit-twiddling operations.
/// The methods use hardware intrinsics when available on the underlying platform; otherwise, they use optimized software fallbacks.
/// </summary>
internal static class BitOperations
{
    private static ReadOnlySpan<byte> Log2DeBruijn =>
    [
        00, 09, 01, 10, 13, 21, 02, 29,
        11, 14, 16, 18, 22, 25, 03, 30,
        08, 12, 20, 28, 15, 17, 24, 07,
        19, 27, 23, 06, 26, 05, 04, 31,
    ];

    /// <summary>
    /// Returns the integer (floor) log of the specified value, base 2.
    /// </summary>
    /// <param name="value">The number from which to obtain the logarithm.</param>
    /// <returns>The log of the specified value, base 2.</returns>
    public static int Log2(uint value)
    {
        value |= 1;

        // Fill trailing zeros with ones, e.g. 00010010 becomes 00011111
        value |= value >> 01;
        value |= value >> 02;
        value |= value >> 04;
        value |= value >> 08;
        value |= value >> 16;

        // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
        return Runtime.CompilerServices.Unsafe.AddByteOffset(
            ref Runtime.InteropServices.MemoryMarshal.GetReference(Log2DeBruijn), // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
            (nint)((value * 0x07C4ACDDu) >> 27)); // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
    }
}
#endif