// -----------------------------------------------------------------------
// <copyright file="VectorMath.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

using System.Numerics;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;

#pragma warning disable SA1101
#endif

/// <summary>
/// <see cref="Vector2"/> extensions.
/// </summary>
internal static class VectorMath
{
#if NET7_0_OR_GREATER
    extension(Vector2 value)
    {
        /// <summary>
        /// Reinterprets a <see cref="Vector2"/> as a new <see cref="Vector64{Single}"/>.
        /// </summary>
        /// <returns>value reinterpreted as a new <see cref="Vector64{Single}"/>.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private Vector64<float> AsVector64() => System.Runtime.CompilerServices.Unsafe.As<Vector2, Vector64<float>>(ref value);

        /// <summary>
        /// Reinterprets a <see cref="Vector2"/> as a new <see cref="Vector64{Single}"/>, leaving the new elements undefined.
        /// </summary>
        /// <returns>value reinterpreted as a new <see cref="Vector64{Single}"/>.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private Vector64<float> AsVector64Unsafe()
        {
            System.Runtime.CompilerServices.Unsafe.SkipInit(out Vector64<float> result);
            System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref System.Runtime.CompilerServices.Unsafe.As<Vector64<float>, byte>(ref result), value);
            return result;
        }
    }
#endif

    /// <summary>
    /// Compares two vectors to determine if any elements are greater.
    /// </summary>
    /// <param name="left">The vector to compare with <paramref name="left"/>.</param>
    /// <param name="right">The vector to compare with <paramref name="right"/>.</param>
    /// <returns><see langword="true"/> if any elements in <paramref name="left"/> was greater than the corresponding element in <paramref name="right"/>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static bool GreaterThanAny(Vector2 left, Vector2 right) =>
#if NET7_0_OR_GREATER
        Vector64.GreaterThanAny(left.AsVector64Unsafe(), right.AsVector64Unsafe());
#else
        left.X > right.X || left.Y > right.Y;
#endif

    /// <summary>
    /// Compares two vectors to determine if all elements are less.
    /// </summary>
    /// <param name="left">The vector to compare with <paramref name="left"/>.</param>
    /// <param name="right">The vector to compare with <paramref name="right"/>.</param>
    /// <returns><see langword="true"/> if all elements in <paramref name="left"/> was less than the corresponding element in <paramref name="right"/>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static bool LessThanAll(Vector2 left, Vector2 right) =>
#if NET7_0_OR_GREATER
        Vector64.LessThanAll(left.AsVector64Unsafe(), right.AsVector64Unsafe());
#else
        left.X < right.X && left.Y < right.Y;
#endif

    /// <summary>
    /// Compares two vectors to determine if any elements are less or equal.
    /// </summary>
    /// <param name="left">The vector to compare with <paramref name="left"/>.</param>
    /// <param name="right">The vector to compare with <paramref name="right"/>.</param>
    /// <returns><see langword="true"/> if any elements in <paramref name="left"/> was less or equal than the corresponding element in <paramref name="right"/>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static bool LessThanOrEqualAny(Vector2 left, Vector2 right) =>
#if NET7_0_OR_GREATER
        Vector64.LessThanOrEqualAny(left.AsVector64Unsafe(), right.AsVector64Unsafe());
#else
        left.X <= right.X || left.Y <= right.Y;
#endif

    /// <summary>
    /// Compares two vectors to determine if any elements are less or equal.
    /// </summary>
    /// <param name="left">The vector to compare with <paramref name="left"/>.</param>
    /// <param name="right">The vector to compare with <paramref name="right"/>.</param>
    /// <returns><see langword="true"/> if any elements in <paramref name="left"/> was less or equal than the corresponding element in <paramref name="right"/>.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    internal static bool LessThanOrEqualAll(Vector2 left, Vector2 right) =>
#if NET7_0_OR_GREATER
        Vector64.LessThanOrEqualAll(left.AsVector64Unsafe(), right.AsVector64Unsafe());
#else
        left.X <= right.X && left.Y <= right.Y;
#endif
}