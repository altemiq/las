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
    private const int ByteMaxValuePlusOne = byte.MaxValue + 1;

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
    public static byte Clamp(this int n) => (byte)Math.Clamp(n, byte.MinValue, byte.MaxValue);

    /// <summary>
    /// Clamps the specified short into a byte.
    /// </summary>
    /// <param name="n">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static byte Clamp(this short n) => (byte)Math.Clamp(n, byte.MinValue, byte.MaxValue);

    /// <summary>
    /// Zero's the bit 0.
    /// </summary>
    /// <param name="n">The value.</param>
    /// <returns>The value with zero in the first bit.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static uint ZeroBit0(this uint n) => n & 0xFFFFFFFE;
}