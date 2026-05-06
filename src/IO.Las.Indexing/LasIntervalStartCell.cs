// -----------------------------------------------------------------------
// <copyright file="LasIntervalStartCell.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// The head of an interval chain owned by a <see cref="LasInterval"/>.
/// </summary>
/// <remarks>
/// The first interval's range is stored inline (<see cref="Start"/>, <see cref="End"/>, guarded by
/// <see cref="HasInlineInterval"/>); any subsequent intervals live in the owning
/// <see cref="LasInterval"/>'s <see cref="IntervalArena"/> and are reached by following
/// <see cref="FirstTail"/>.
/// </remarks>
internal sealed class LasIntervalStartCell
{
    private int lastTail = IntervalArena.NullIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalStartCell"/> class with no populated interval.
    /// </summary>
    public LasIntervalStartCell()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalStartCell"/> class with a single-point interval.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    public LasIntervalStartCell(uint pointIndex)
    {
        this.Start = this.End = pointIndex;
        this.Full = this.Total = 1;
        this.HasInlineInterval = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalStartCell"/> class with a given range.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="endIndex">The end index.</param>
    public LasIntervalStartCell(uint startIndex, uint endIndex)
    {
        this.Start = startIndex;
        this.End = endIndex;
        this.Full = this.Total = endIndex - startIndex + 1;
        this.HasInlineInterval = true;
    }

    /// <summary>
    /// Gets or sets the inclusive start point index of the first (inline) interval.
    /// </summary>
    public uint Start { get; set; }

    /// <summary>
    /// Gets or sets the inclusive end point index of the first (inline) interval.
    /// </summary>
    public uint End { get; set; }

    /// <summary>
    /// Gets or sets the total number of points that were added to the cell.
    /// </summary>
    public uint Full { get; set; }

    /// <summary>
    /// Gets or sets the total point span covered by the chained intervals.
    /// </summary>
    public uint Total { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Start"/> and <see cref="End"/> carry a real
    /// interval (rather than being the default uninitialized values of an empty cell).
    /// </summary>
    public bool HasInlineInterval { get; set; }

    /// <summary>
    /// Gets or sets the arena index of the first tail cell, or <see cref="IntervalArena.NullIndex"/>
    /// if the chain has only the inline head interval.
    /// </summary>
    public int FirstTail { get; set; } = IntervalArena.NullIndex;

    /// <summary>
    /// Sets the cached tail index; used after the chain has been appended to.
    /// </summary>
    /// <param name="index">The arena index of the new tail.</param>
    public void SetLastTail(int index) => this.lastTail = index;

    /// <summary>
    /// Gets the arena index of the tail cell, rehydrating it lazily when the chain was reloaded
    /// from disk via <see cref="LasInterval.ReadFrom(System.ReadOnlySpan{byte})"/>.
    /// </summary>
    /// <param name="arena">The arena backing this chain.</param>
    /// <returns>The arena index of the tail cell, or <see cref="IntervalArena.NullIndex"/> when the chain has only the head.</returns>
    public int GetLastTail(IntervalArena arena)
    {
        if (this.lastTail is not IntervalArena.NullIndex || this.FirstTail is IntervalArena.NullIndex)
        {
            return this.lastTail;
        }

        // chain was deserialized; walk once to find the tail and cache it
        var cursor = this.FirstTail;
        while (arena[cursor].Next is not IntervalArena.NullIndex)
        {
            cursor = arena[cursor].Next;
        }

        this.lastTail = cursor;
        return cursor;
    }

    /// <summary>
    /// Adds the specified <paramref name="pointIndex"/> to this chain.
    /// </summary>
    /// <param name="pointIndex">The point index to append; must be strictly greater than the current end.</param>
    /// <param name="threshold">The maximum gap before a new interval is started.</param>
    /// <param name="arena">The arena used to allocate tail cells.</param>
    /// <returns><see langword="true"/> if a new interval cell was allocated; otherwise <see langword="false"/>.</returns>
    public bool Add(uint pointIndex, int threshold, IntervalArena arena)
    {
        // first point ever added to an empty cell simply populates the inline head
        if (!this.HasInlineInterval)
        {
            this.Start = this.End = pointIndex;
            this.HasInlineInterval = true;
            this.Full++;
            this.Total++;
            return true;
        }

        var currentEnd = this.FirstTail is IntervalArena.NullIndex
            ? this.End
            : arena[this.GetLastTail(arena)].End;

        System.Diagnostics.Debug.Assert(pointIndex > currentEnd, "Point index is beyond the current end.");
        var diff = pointIndex - currentEnd;
        this.Full++;
        if (diff > threshold)
        {
            var newIndex = arena.Allocate(pointIndex, pointIndex);
            if (this.FirstTail is IntervalArena.NullIndex)
            {
                this.FirstTail = newIndex;
            }
            else
            {
                arena[this.GetLastTail(arena)].Next = newIndex;
            }

            this.lastTail = newIndex;
            this.Total++;

            return true; // created new interval
        }

        if (this.FirstTail is IntervalArena.NullIndex)
        {
            this.End = pointIndex;
        }
        else
        {
            arena[this.GetLastTail(arena)].End = pointIndex;
        }

        this.Total += diff;

        return false; // added to interval
    }
}