// -----------------------------------------------------------------------
// <copyright file="LasIntervalStartCell.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// The start <see cref="LasIntervalCell"/>.
/// </summary>
internal sealed class LasIntervalStartCell : LasIntervalCell
{
    private LasIntervalCell? last;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalStartCell"/> class.
    /// </summary>
    public LasIntervalStartCell()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalStartCell"/> class.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    public LasIntervalStartCell(uint pointIndex)
        : base(pointIndex) => this.Full = this.Total = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIntervalStartCell"/> class.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="endIndex">The end index.</param>
    public LasIntervalStartCell(uint startIndex, uint endIndex)
        : base(startIndex)
    {
        this.Full = this.Total = endIndex - startIndex + 1;
        this.End = endIndex;
    }

    /// <summary>
    /// Gets or sets the full number.
    /// </summary>
    public uint Full { get; set; }

    /// <summary>
    /// Gets or sets the total.
    /// </summary>
    public uint Total { get; set; }

    /// <summary>
    /// Adds the point index.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    /// <param name="threshold">The threshold.</param>
    /// <returns><see langword="true"/> if a new interval is created for this point; otherwise <see langword="false" />.</returns>
    public bool Add(uint pointIndex, int threshold)
    {
        var currentEnd = this.last?.End ?? this.End;

        System.Diagnostics.Debug.Assert(pointIndex > currentEnd, "Point index is beyond the current end.");
        var diff = pointIndex - currentEnd;
        this.Full++;
        if (diff > threshold)
        {
            if (this.last is not null)
            {
                this.last.Next = new(pointIndex);
                this.last = this.last.Next;
            }
            else
            {
                this.Next = new(pointIndex);
                this.last = this.Next;
            }

            this.Total++;

            return true; // created new interval
        }

        if (this.last is not null)
        {
            this.last.End = pointIndex;
        }
        else
        {
            this.End = pointIndex;
        }

        this.Total += diff;

        return false; // added to interval
    }
}