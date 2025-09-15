// -----------------------------------------------------------------------
// <copyright file="InterlockedExtension.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Internals;

using System.Numerics;

/// <summary>
/// <see cref="Interlocked"/> extension.
/// </summary>
internal static class InterlockedExtension
{
    /// <summary>
    /// Exchanges the value if it is less.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="location1">The target.</param>
    /// <param name="value">The new value.</param>
    /// <returns><see langword="true"/> if the value is updated; otherwise <see langword="false"/>.</returns>
    public static bool ExchangeIfLessThan<T>(ref T location1, T value)
        where T : IComparisonOperators<T, T, bool>
    {
        T snapshot;
        bool stillLess;
        do
        {
            snapshot = location1;
            stillLess = value < snapshot;
        }
        while (stillLess && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

        return stillLess;
    }

    /// <summary>
    /// Exchanges the value if it is greater.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="location1">The target.</param>
    /// <param name="value">The new value.</param>
    /// <returns><see langword="true"/> if the value is updated; otherwise <see langword="false"/>.</returns>
    public static bool ExchangeIfGreaterThan<T>(ref T location1, T value)
        where T : IComparisonOperators<T, T, bool>
    {
        T snapshot;
        bool stillMore;
        do
        {
            snapshot = location1;
            stillMore = value > snapshot;
        }
        while (stillMore && !Interlocked.CompareExchange(ref location1, value, snapshot).Equals(snapshot));

        return stillMore;
    }
}