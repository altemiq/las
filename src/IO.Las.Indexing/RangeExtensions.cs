// -----------------------------------------------------------------------
// <copyright file="RangeExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// <see cref="Range"/> extensions.
/// </summary>
public static class RangeExtensions
{
    /// <summary>
    /// Gets the indexes for the specified range.
    /// </summary>
    /// <param name="range">The range.</param>
    /// <returns>The indexes for <paramref name="range"/>.</returns>
    /// <remarks>The range is treated as inclusive of both <see cref="Range.Start"/> and <see cref="Range.End"/>, matching the on-disk LAX interval format.</remarks>
    public static IEnumerable<uint> GetIndexes(this Range range)
    {
        var start = range.Start.Value;
        var end = range.End.Value;

        while (start < end)
        {
            yield return start++;
        }

        // emit `end` outside the loop so we do not overflow when end == uint.MaxValue
        yield return end;
    }

    /// <summary>
    /// Gets the indexes for the specified ranges.
    /// </summary>
    /// <param name="ranges">The ranges.</param>
    /// <returns>The indexes for <paramref name="ranges"/>.</returns>
    public static IEnumerable<uint> GetIndexes(this IEnumerable<Range> ranges) => ranges.SelectMany(GetIndexes);
}