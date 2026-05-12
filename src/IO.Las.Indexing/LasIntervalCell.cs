// -----------------------------------------------------------------------
// <copyright file="LasIntervalCell.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// A LAS interval cell stored in an <see cref="IntervalArena"/>.
/// </summary>
/// <remarks>
/// This is a mutable struct for allocation-free storage in the arena's backing array; always
/// mutate through <see langword="ref"/> via the arena indexer (<see cref="IntervalArena.this"/>) to avoid
/// silently modifying a copy.
/// </remarks>
/// <param name="start">The start point index.</param>
/// <param name="end">The end point index.</param>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
internal struct LasIntervalCell(uint start, uint end)
{
    /// <summary>
    /// Gets or sets the inclusive start point index of this interval.
    /// </summary>
    public uint Start { get; set; } = start;

    /// <summary>
    /// Gets or sets the inclusive end point index of this interval.
    /// </summary>
    public uint End { get; set; } = end;

    /// <summary>
    /// Gets or sets the arena index of the next cell in the chain, or <see cref="IntervalArena.NullIndex"/>
    /// if this is the last cell.
    /// </summary>
    public int Next { get; set; } = IntervalArena.NullIndex;
}