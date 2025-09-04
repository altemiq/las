// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// Compression extension methods.
/// </summary>
internal static class ExtensionMethods
{
    private const int ByteMaxValuePlusOne = byte.MaxValue + 1; // 256

    /// <summary>
    /// Folds the specified int into a byte.
    /// </summary>
    /// <param name="n">The value to fold.</param>
    /// <returns>The folded value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static byte Fold(this int n) => (byte)(n % ByteMaxValuePlusOne);

    /// <summary>
    /// Folds the specified unsigned int into a byte.
    /// </summary>
    /// <param name="n">The value to fold.</param>
    /// <returns>The folded value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static byte Fold(this uint n) => (byte)(n % ByteMaxValuePlusOne);

    /// <summary>
    /// Folds the specified unsigned short into a byte.
    /// </summary>
    /// <param name="n">The value to fold.</param>
    /// <returns>The folded value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static byte Fold(this ushort n) => (byte)(n % ByteMaxValuePlusOne);

    /// <summary>
    /// Clamps the specified int into a byte.
    /// </summary>
    /// <param name="n">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static byte Clamp(this int n)
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
         => (byte)Math.Clamp(n, byte.MinValue, byte.MaxValue);
#else
    {
        return n < byte.MinValue ? byte.MinValue : ClampMax(n);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static byte ClampMax(int n)
        {
            return n > byte.MaxValue ? byte.MaxValue : (byte)n;
        }
    }
#endif

    /// <summary>
    /// Clamps the specified short into a byte.
    /// </summary>
    /// <param name="n">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static byte Clamp(this short n)
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
         => (byte)Math.Clamp(n, byte.MinValue, byte.MaxValue);
#else
    {
        return n < byte.MinValue ? byte.MinValue : ClampMax(n);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static byte ClampMax(short n)
        {
            return n > byte.MaxValue ? byte.MaxValue : (byte)n;
        }
    }
#endif

    /// <summary>
    /// Zero's the bit 0.
    /// </summary>
    /// <param name="n">The value.</param>
    /// <returns>The value with zero in the first bit.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static uint ZeroBit0(this uint n) => n & 0xFFFFFFFE;
}