// -----------------------------------------------------------------------
// <copyright file="InterlockedExtension.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Internals;

/// <summary>
/// <see cref="Interlocked"/> extension.
/// </summary>
internal static class InterlockedExtension
{
    /// <summary>
    /// Exchanges the value if it is less.
    /// </summary>
    /// <param name="location1">The target.</param>
    /// <param name="value">The new value.</param>
    /// <returns><see langword="true"/> if the value is updated; otherwise <see langword="false"/>.</returns>
    public static bool ExchangeIfLessThan(ref double location1, double value)
    {
        double snapshot;
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
    /// <param name="location1">The target.</param>
    /// <param name="value">The new value.</param>
    /// <returns><see langword="true"/> if the value is updated; otherwise <see langword="false"/>.</returns>
    public static bool ExchangeIfGreaterThan(ref double location1, double value)
    {
        double snapshot;
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