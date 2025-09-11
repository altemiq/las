// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

using System.Runtime.CompilerServices;

/// <summary>
/// Extension methods.
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Quantize the specified float to int.
    /// </summary>
    /// <param name="n">The value to quantize.</param>
    /// <returns>The quantized value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Quantize(this float n) => (n >= 0) ? (int)(n + 0.5F) : (int)(n - 0.5F);

    /// <summary>
    /// Gets a value indicating whether the specified value is an <see cref="int"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> can be represented as an <see cref="int"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInt32(this long value) => value is <= int.MaxValue and >= int.MinValue;
}