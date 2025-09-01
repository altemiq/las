// -----------------------------------------------------------------------
// <copyright file="BitManipulation.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Bit manipulation methods.
/// </summary>
internal static class BitManipulation
{
    /// <summary>
    /// Gets the byte value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The bit mask.</param>
    /// <returns>The byte value.</returns>
    public static byte Get(byte value, byte mask) => (byte)(value & mask);

    /// <summary>
    /// Gets the byte value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The bit mask.</param>
    /// <param name="position">The position.</param>
    /// <returns>The byte value.</returns>
    public static byte Get(byte value, byte mask, int position) => (byte)((value & mask) >> position);

    /// <summary>
    /// Gets the byte value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The bit mask.</param>
    /// <returns>The byte value.</returns>
    public static bool IsSet(byte value, byte mask) => Get(value, mask) == mask;

    /// <summary>
    /// Sets the bit.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The mask.</param>
    /// <param name="set">Set to <see langword="true"/> to set the bit.</param>
    /// <returns>The byte with the mask applied.</returns>
    public static byte Apply(byte value, byte mask, bool set) => set ? Set(value, mask) : Clear(value, mask);

    /// <summary>
    /// Sets the bits.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="source">The source.</param>
    /// <param name="mask">The mask.</param>
    /// <returns>The byte with the bits from <paramref name="source"/> set.</returns>
    public static byte Set(byte value, byte source, byte mask) => Set(Clear(value, mask), (byte)(source & mask));

    /// <summary>
    /// Sets the bits.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="source">The source.</param>
    /// <param name="mask">The mask.</param>
    /// <param name="position">The position at which to set the bits.</param>
    /// <returns>The byte with the bits from <paramref name="source"/> set at <paramref name="position"/>.</returns>
    public static byte Set(byte value, byte source, byte mask, int position) => Set(Clear(value, mask), (byte)((source << position) & mask));

    private static byte Set(byte value, byte mask) => (byte)(value | mask);

    private static byte Clear(byte value, byte mask) => (byte)(value & (~mask));
}