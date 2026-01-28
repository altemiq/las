// -----------------------------------------------------------------------
// <copyright file="LasIndexCell.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// Represents a LAS index cell.
/// </summary>
public readonly record struct LasIndexCell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LasIndexCell"/> struct.
    /// </summary>
    /// <param name="minimum">The minimum coordinate.</param>
    /// <param name="maximum">The maximum coordinate.</param>
    /// <param name="ranges">The ranges.</param>
    internal LasIndexCell(System.Numerics.Vector2 minimum, System.Numerics.Vector2 maximum, IEnumerable<Range> ranges) => (this.Minimum, this.Maximum, this.Intervals) = (minimum, maximum, ranges);

    /// <summary>
    /// Gets the minimum coordinate.
    /// </summary>
    public System.Numerics.Vector2 Minimum { get; }

    /// <summary>
    /// Gets the maximum coordinate.
    /// </summary>
    public System.Numerics.Vector2 Maximum { get; }

    /// <summary>
    /// Gets the intervals.
    /// </summary>
    public IEnumerable<Range> Intervals { get; } = [];

    /// <summary>
    /// Determines whether the specified coordinate is in this cell.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns><see langword="true"/> if the <paramref name="x"/>,<paramref name="y"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool Contains(double x, double y)
    {
        var value = new System.Numerics.Vector2((float)x, (float)y);
        return VectorMath.LessThanOrEqualAll(this.Minimum, value) && VectorMath.LessThanAll(value, this.Maximum);
    }

    /// <summary>
    /// Determines whether the specified index is in this cell.
    /// </summary>
    /// <param name="index">The point index.</param>
    /// <returns><see langword="true"/> if the <paramref name="index"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool Contains(uint index) => this.Intervals.Any(range => index >= range.Start.Value && index < range.End.Value);

    /// <summary>
    /// Determines whether the specified x-coordinate is in this cell.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <returns><see langword="true"/> if the <paramref name="x"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool ContainsX(double x) => this.Minimum.X <= x && x < this.Maximum.X;

    /// <summary>
    /// Determines whether the specified y-coordinate is in this cell.
    /// </summary>
    /// <param name="y">The y-coordinate.</param>
    /// <returns><see langword="true"/> if the <paramref name="y"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool ContainsY(double y) => this.Minimum.X <= y && y < this.Maximum.X;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.Minimum, this.Maximum);

    /// <inheritdoc/>
    public override string ToString() => $"[ X: ({this.Minimum.X} -> {this.Maximum.X}), Y: ({this.Minimum.Y} -> {this.Maximum.Y}) ]";
}