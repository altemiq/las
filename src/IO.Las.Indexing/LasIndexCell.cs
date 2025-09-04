// -----------------------------------------------------------------------
// <copyright file="LasIndexCell.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// Represents a LAS index cell.
/// </summary>
public readonly struct LasIndexCell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LasIndexCell"/> struct.
    /// </summary>
    /// <param name="minimumX">The minimum x-coordinate.</param>
    /// <param name="minimumY">The maximum x-coordinate.</param>
    /// <param name="maximumX">The minimum y-coordinate.</param>
    /// <param name="maximumY">The maximum y-coordinate.</param>
    /// <param name="ranges">The ranges.</param>
    internal LasIndexCell(double minimumX, double minimumY, double maximumX, double maximumY, IEnumerable<Range> ranges) => (this.MinimumX, this.MinimumY, this.MaximumX, this.MaximumY, this.Intervals) = (minimumX, minimumY, maximumX, maximumY, ranges);

    /// <summary>
    /// Gets the minimum x-coordinate.
    /// </summary>
    public double MinimumX { get; }

    /// <summary>
    /// Gets the maximum x-coordinate.
    /// </summary>
    public double MaximumX { get; }

    /// <summary>
    /// Gets the minimum y-coordinate.
    /// </summary>
    public double MinimumY { get; }

    /// <summary>
    /// Gets the maximum x-coordinate.
    /// </summary>
    public double MaximumY { get; }

    /// <summary>
    /// Gets the intervals.
    /// </summary>
    public IEnumerable<Range> Intervals { get; } = [];

    /// <summary>
    /// Implements the equals operator.
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(LasIndexCell left, LasIndexCell right) => left.Equals(right);

    /// <summary>
    /// Implements the not-equals operator.
    /// </summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(LasIndexCell left, LasIndexCell right) => !(left == right);

    /// <summary>
    /// Determines whether the specified coordinate is in this cell.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns><see langword="true"/> if the <paramref name="x"/>,<paramref name="y"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool Contains(double x, double y) => this.ContainsX(x) && this.ContainsY(y);

    /// <summary>
    /// Determines whether the specified index is in this cell.
    /// </summary>
    /// <param name="index">The point index.</param>
    /// <returns><see langword="true"/> if the <paramref name="index"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool Contains(uint index) => this.Intervals.Any(range => range.Start.Value >= index && range.End.Value < index);

    /// <summary>
    /// Determines whether the specified x-coordinate is in this cell.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <returns><see langword="true"/> if the <paramref name="x"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool ContainsX(double x) => this.MinimumX <= x && x < this.MaximumX;

    /// <summary>
    /// Determines whether the specified y-coordinate is in this cell.
    /// </summary>
    /// <param name="y">The y-coordinate.</param>
    /// <returns><see langword="true"/> if the <paramref name="y"/> is within this cell; otherwise <see langword="false"/>.</returns>
    public bool ContainsY(double y) => this.MinimumY <= y && y < this.MaximumY;

    /// <inheritdoc/>
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is LasIndexCell lic
                                                                                                    && this.MinimumX.Equals(lic.MinimumX)
                                                                                                    && this.MaximumX.Equals(lic.MaximumX)
                                                                                                    && this.MinimumY.Equals(lic.MinimumY)
                                                                                                    && this.MaximumY.Equals(lic.MaximumY)
                                                                                                    && this.Intervals.SequenceEqual(lic.Intervals);

    /// <inheritdoc/>
    public override int GetHashCode() =>
#if NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        HashCode.Combine(this.MinimumX, this.MaximumX, this.MinimumY, this.MaximumY);
#else
        (this.MinimumX, this.MaximumX, this.MinimumY, this.MaximumY).GetHashCode();
#endif

    /// <inheritdoc/>
    public override string ToString() => $"[ X: ({this.MinimumX} -> {this.MaximumX}), Y: ({this.MinimumY} -> {this.MaximumY}) ]";
}