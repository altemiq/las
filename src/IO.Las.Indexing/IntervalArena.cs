// -----------------------------------------------------------------------
// <copyright file="IntervalArena.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// Append-only backing store for <see cref="LasIntervalCell"/> records, used to avoid per-interval
/// GC allocations when reading or building a large LAX index.
/// </summary>
/// <remarks>
/// Cells are returned by integer index; <see cref="NullIndex"/> represents the end of a chain.
/// The arena owns the backing array and will grow it on demand; indices remain stable for the
/// lifetime of the arena. A single arena is shared across all <see cref="LasIntervalStartCell"/>s
/// owned by the same <see cref="LasInterval"/>.
/// </remarks>
internal sealed class IntervalArena
{
    /// <summary>
    /// Sentinel representing a missing / null cell index.
    /// </summary>
    public const int NullIndex = -1;

    private LasIntervalCell[] cells;

    private int count;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntervalArena"/> class.
    /// </summary>
    /// <param name="capacity">The initial capacity.</param>
    public IntervalArena(int capacity = 16) => this.cells = new LasIntervalCell[Math.Max(capacity, 4)];

    /// <summary>
    /// Gets the number of cells allocated so far.
    /// </summary>
    public int Count => this.count;

    /// <summary>
    /// Gets a reference to the cell at the specified arena index.
    /// </summary>
    /// <param name="index">The arena index.</param>
    /// <returns>A mutable reference to the cell.</returns>
    public ref LasIntervalCell this[int index] => ref this.cells[index];

    /// <summary>
    /// Allocates a new cell in the arena and returns its index.
    /// </summary>
    /// <param name="start">The start point index.</param>
    /// <param name="end">The end point index.</param>
    /// <returns>The arena index of the newly allocated cell.</returns>
    public int Allocate(uint start, uint end)
    {
        if (this.count == this.cells.Length)
        {
            Array.Resize(ref this.cells, this.cells.Length * 2);
        }

        var index = this.count++;
        this.cells[index] = new(start, end);
        return index;
    }
}