// -----------------------------------------------------------------------
// <copyright file="OccupancyGrid.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// Represents a grid for tracking point occupancy with optimized point counting.
/// </summary>
/// <param name="quantizer">The converter.</param>
/// <param name="gridSpacing">The grid spacing.</param>
internal sealed class OccupancyGrid(PointDataRecordQuantizer quantizer, float gridSpacing)
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<long, int> gridCells = [];
    private readonly double gridSpacing = Math.Abs(gridSpacing);
    private readonly PointDataRecordQuantizer quantizer = quantizer;

    /// <summary>
    /// Gets the number of occupied grids.
    /// </summary>
    public uint NumOccupied => (uint)this.gridCells.Count;

    /// <summary>
    /// Adds the point to the grid.
    /// </summary>
    /// <param name="point">The point to add.</param>
    /// <returns><see langword="true"/> if the point was added; otherwise <see langword="false"/>.</returns>
    public bool Add(IBasePointDataRecord point)
    {
        // Vectorized coordinate calculation with truncation
        var position = (this.quantizer.Get(point) / this.gridSpacing).AsVector2D().AsVector128Unsafe();
        var gridKey = 0L;

        if (System.Runtime.Intrinsics.X86.Sse2.IsSupported)
        {
            var floor = System.Runtime.Intrinsics.X86.Sse2.ConvertToVector128Int32WithTruncation(position);

            // Pack x and y into a single long (x high bits, y low bits)
            gridKey = ((long)floor[0] << 32) | (uint)floor[1];
        }
        else
        {
            var floor = System.Runtime.Intrinsics.Vector128.Floor(position);

            // Pack x and y into a single long (x high bits, y low bits)
            gridKey = ((long)floor[0] << 32) | (uint)floor[1];
        }

        // Use the concurrent dictionary to safely increment cell counts
        this.gridCells.AddOrUpdate(gridKey, 1, (key, value) => value + 1);

        return true;
    }

    /// <summary>
    /// Gets the point count for all grid cells.
    /// </summary>
    /// <returns>A dictionary of cell positions and their point counts.</returns>
    public IReadOnlyDictionary<long, int> GetCellCounts() => this.gridCells;

    /// <summary>
    /// Gets the total number of points in all grid cells.
    /// </summary>
    /// <returns>The total count of points.</returns>
    public int GetTotalPointCount() => this.gridCells.Values.Sum();

    /// <summary>
    /// Gets the point count for all grid cells as vectors.
    /// </summary>
    /// <returns>A dictionary of cell positions and their point counts.</returns>
    public IReadOnlyDictionary<System.Numerics.Vector2, int> GetCellCountsAsVectors() => this.gridCells.ToDictionary(
        static kvp => new System.Numerics.Vector2((int)(kvp.Key >> 32), (int)(kvp.Key & 0xFFFFFFFF)),
        static kvp => kvp.Value);
}