// -----------------------------------------------------------------------
// <copyright file="LasIntervalCell.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// The LAS interval cell.
/// </summary>
internal class LasIntervalCell
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalCell"/> class.
    /// </summary>
    public LasIntervalCell()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalCell"/> class.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    public LasIntervalCell(uint pointIndex) => this.Start = this.End = pointIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalCell"/> class.
    /// </summary>
    /// <param name="cell">The cell to copy from.</param>
    public LasIntervalCell(LasIntervalCell cell)
    {
        this.Start = cell.Start;
        this.End = cell.End;
    }

    /// <summary>
    /// Gets or sets the start.
    /// </summary>
    public uint Start { get; set; }

    /// <summary>
    /// Gets or sets the end.
    /// </summary>
    public uint End { get; set; }

    /// <summary>
    /// Gets or sets the next cell.
    /// </summary>
    public LasIntervalCell? Next { get; set; }
}