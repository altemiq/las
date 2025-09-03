// -----------------------------------------------------------------------
// <copyright file="BoundingBox.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Stores a set of six doubles that represent the location and size of a box.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct BoundingBox
{
    /// <summary>
    /// Represents an instance of the <see cref="BoundingBox"/> class with its members uninitialized.
    /// </summary>
    public static readonly BoundingBox Empty = new(0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct with the specified edge locations.
    /// </summary>
    /// <param name="left">The x-coordinate of the lower-left-front corner of the envelope.</param>
    /// <param name="bottom">The y-coordinate of the lower-right-front corner of the envelope.</param>
    /// <param name="front">The z-coordinate of the lower-left-front corner of the envelope.</param>
    /// <param name="right">The x-coordinate of the upper-right-back corner of the envelope.</param>
    /// <param name="top">The y-coordinate of the upper-right-back corner of the envelope.</param>
    /// <param name="back">The z-coordinate of the upper-right-back corner of the envelope.</param>
    public BoundingBox(double left, double bottom, double front, double right, double top, double back) => (this.Left, this.Bottom, this.Front, this.Right, this.Top, this.Back) = (left, bottom, front, right, top, back);

    /// <summary>
    /// Gets the x-coordinate of the lower-left corner of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The x-coordinate of the lower-left corner of this <see cref="BoundingBox"/> structure.</value>
    public double X => this.Left;

    /// <summary>
    /// Gets the y-coordinate of the lower-left corner of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The y-coordinate of the lower-left corner of this <see cref="BoundingBox"/> structure.</value>
    public double Y => this.Bottom;

    /// <summary>
    /// Gets the z-coordinate of the lower-left corner of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The z-coordinate of the lower-left corner of this <see cref="BoundingBox"/> structure.</value>
    public double Z => this.Front;

    /// <summary>
    /// Gets the width of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The width of this <see cref="BoundingBox"/> structure.</value>
    public double Width => this.Right - this.Left;

    /// <summary>
    /// Gets the height of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The height of this <see cref="BoundingBox"/> structure.</value>
    public double Height => this.Top - this.Bottom;

    /// <summary>
    /// Gets the depth of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The depth of this <see cref="BoundingBox"/> structure.</value>
    public double Depth => this.Bottom - this.Front;

    /// <summary>
    /// Gets the y-coordinate that is the sum of <see cref="Y"/> and <see cref="Height"/> property values of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The y-coordinate that is the sum of <see cref="Y"/> and <see cref="Height"/> of this <see cref="BoundingBox"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public double Bottom { get; }

    /// <summary>
    /// Gets the x-coordinate of the left edge of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The x-coordinate of the left edge of this <see cref="BoundingBox"/> structure.</value>
    [System.ComponentModel.Browsable(false)]
    public double Left { get; }

    /// <summary>
    /// Gets the z-coordinate of the front edge of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The z-coordinate of the front edge of this <see cref="BoundingBox"/> structure.</value>
    [System.ComponentModel.Browsable(false)]
    public double Front { get; }

    /// <summary>
    /// Gets the y-coordinate of the top edge of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The y-coordinate of the top edge of this <see cref="BoundingBox"/> structure.</value>
    [System.ComponentModel.Browsable(false)]
    public double Top { get; }

    /// <summary>
    /// Gets the x-coordinate that is the sum of <see cref="X"/> and <see cref="Width"/> property values of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The x-coordinate that is the sum of <see cref="X"/> and <see cref="Width"/> of this <see cref="BoundingBox"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public double Right { get; }

    /// <summary>
    /// Gets the z-coordinate that is the sum of <see cref="Z"/> and <see cref="Depth"/> property values of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The z-coordinate that is the sum of <see cref="Z"/> and <see cref="Depth"/> of this <see cref="BoundingBox"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public double Back { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="Width" />, <see cref="Height" />, or <see cref="Depth"/> property of this <see cref="BoundingBox" /> has a value of zero.
    /// </summary>
    /// <value>This property returns <see langword="true"/> if the <see cref="Width" />, <see cref="Height" />, or <see cref="Depth"/> property of this <see cref="BoundingBox" /> has a value of zero; otherwise, <see langword="false"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public bool IsEmpty => (this.Width <= 0D) || (this.Height <= 0D) || (this.Depth <= 0D);

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct with the specified location and size.
    /// </summary>
    /// <param name="x">The x-coordinate of the lower-left-front corner of this <see cref="BoundingBox"/> structure.</param>
    /// <param name="y">The y-coordinate of the lower-left-front corner of this <see cref="BoundingBox"/> structure.</param>
    /// <param name="z">The z-coordinate of the lower-left-front corner of this <see cref="BoundingBox"/> structure.</param>
    /// <param name="width">The width of this <see cref="BoundingBox"/> structure.</param>
    /// <param name="height">The height of this <see cref="BoundingBox"/> structure.</param>
    /// <param name="depth">The depth of this <see cref="BoundingBox"/> structure.</param>
    /// <returns>The new <see cref="BoundingBox"/> that this method creates.</returns>
    public static BoundingBox FromXYZWHD(double x, double y, double z, double width, double height, double depth) => new(x, y, z, x + width, y + height, z + depth);

    /// <summary>
    /// Creates and returns an inflated copy of the specified <see cref="BoundingBox"/> structure. The copy is inflated by the specified amount. The original <see cref="BoundingBox"/> structure remains unmodified.
    /// </summary>
    /// <param name="envelope">The <see cref="BoundingBox"/> with which to start. This envelope is not modified.</param>
    /// <param name="x">The amount to inflate this <see cref="BoundingBox"/> horizontally.</param>
    /// <param name="y">The amount to inflate this <see cref="BoundingBox"/> vertically.</param>
    /// <param name="z">The amount to inflate this <see cref="BoundingBox"/> int depth.</param>
    /// <returns>The inflated <see cref="BoundingBox"/>.</returns>
    public static BoundingBox Inflate(in BoundingBox envelope, double x, double y, double z) => envelope.Inflate(x, y, z);

    /// <summary>
    /// Returns a third <see cref="BoundingBox"/> structure that represents the intersection of two other <see cref="BoundingBox"/> structures. If there is no intersection, an empty <see cref="BoundingBox"/> is returned.
    /// </summary>
    /// <param name="a">The first envelope to intersect.</param>
    /// <param name="b">The second envelope to intersect.</param>
    /// <returns>A <see cref="BoundingBox"/> that represents the intersection of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static BoundingBox Intersect(in BoundingBox a, in BoundingBox b)
    {
        var x1 = Math.Max(a.Left, b.Left);
        var x2 = Math.Min(a.Right, b.Right);
        var y1 = Math.Max(a.Bottom, b.Bottom);
        var y2 = Math.Min(a.Top, b.Top);
        var z1 = Math.Max(a.Front, b.Front);
        var z2 = Math.Min(a.Back, b.Back);

        return x2 >= x1 && y2 >= y1 && z2 >= z1 ? new(x1, y1, z1, x2, y2, z2) : Empty;
    }

    /// <summary>
    /// Creates the smallest possible third envelope that can contain both of two rectangles that form a union.
    /// </summary>
    /// <param name="a">The first envelope to union.</param>
    /// <param name="b">The second envelope to union.</param>
    /// <returns>A <see cref="BoundingBox"/> structure that bounds the union of the two <see cref="BoundingBox"/> structures.</returns>
    public static BoundingBox Union(in BoundingBox a, in BoundingBox b) => new(
        Math.Min(a.Left, b.Left),
        Math.Min(a.Bottom, b.Bottom),
        Math.Min(a.Front, b.Front),
        Math.Max(a.Right, b.Right),
        Math.Max(a.Top, b.Top),
        Math.Max(a.Back, b.Back));

    /// <summary>
    /// Determines if the specified point is contained within this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to test.</param>
    /// <param name="y">The y-coordinate of the point to test.</param>
    /// <param name="z">The z-coordinate of the point to test.</param>
    /// <returns>This method returns <see langword="true"/> if the point defined by <paramref name="x"/>, <paramref name="y"/>, and <paramref name="z"/> is contained within this <see cref="BoundingBox"/> structure; otherwise, <see langword="false"/>.</returns>
    public bool Contains(double x, double y, double z) =>
        (this.Height > 0 ? this.Bottom <= y && y < this.Top : this.Bottom >= y && y > this.Top)
        && (this.Width > 0 ? this.Left <= x && x < this.Right : this.Left >= x && x > this.Right)
        && (this.Depth > 0 ? this.Front <= z && z < this.Back : this.Front >= z && z > this.Back);

    /// <summary>
    /// Determines if the rectangular region represented by <paramref name="envelope"/> is entirely contained within this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <param name="envelope">The <see cref="BoundingBox"/> to test.</param>
    /// <returns>This method returns <see langword="true"/> if the rectangular region represented by <paramref name="envelope"/> is entirely contained within this <see cref="BoundingBox"/> structure; otherwise, <see langword="false"/>.</returns>
    public bool Contains(BoundingBox envelope) => this.Left <= envelope.Left && envelope.Right <= this.Right
        && this.Bottom <= envelope.Bottom && envelope.Top <= this.Top
        && this.Front <= envelope.Front && envelope.Back <= this.Back;

    /// <summary>
    /// Inflates this <see cref="BoundingBox"/> by the specified amount.
    /// </summary>
    /// <param name="width">The amount to inflate this <see cref="BoundingBox"/> horizontally.</param>
    /// <param name="height">The amount to inflate this <see cref="BoundingBox"/> vertically.</param>
    /// <param name="depth">The amount to inflate this <see cref="BoundingBox"/> in depth.</param>
    /// <returns>The inflated envelope.</returns>
    public BoundingBox Inflate(double width, double height, double depth) => new(this.Left - width, this.Bottom - height, this.Front - depth, this.Right + width, this.Top + height, this.Back + depth);

    /// <summary>
    /// Returns a <see cref="BoundingBox"/> with the intersection of this instance and the specified <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="envelope">The <see cref="BoundingBox"/> to intersect.</param>
    /// <returns>The inflated envelope.</returns>
    public BoundingBox Intersect(BoundingBox envelope) => Intersect(envelope, this);

    /// <summary>
    /// Determines if this envelope intersects with <paramref name="rect"/>.
    /// </summary>
    /// <param name="rect">The envelope to test.</param>
    /// <returns>This method returns <see langword="true"/> if there is any intersection, otherwise <see langword="false"/>.</returns>
    public bool IntersectsWith(BoundingBox rect) =>
        (rect.Left < this.Right)
        && (this.Left < rect.Right)
        && (rect.Bottom < this.Top)
        && (this.Bottom < rect.Top)
        && (rect.Front < this.Back)
        && (this.Front < rect.Back);

    /// <summary>
    /// Adjusts the location of this envelope by the specified amount.
    /// </summary>
    /// <param name="x">The horizontal offset.</param>
    /// <param name="y">The vertical offset.</param>
    /// <param name="z">The depth offset.</param>
    /// <returns>The offset envelope.</returns>
    public BoundingBox Offset(double x, double y, double z) => new(this.Left + x, this.Bottom + y, this.Z + z, this.Right + x, this.Top + y, this.Back + z);

    /// <summary>
    /// Adjusts the location of this envelope by the specified amount.
    /// </summary>
    /// <param name="value">The offset.</param>
    /// <returns>The offset envelope.</returns>
    public BoundingBox Offset(Vector3D value) => new(this.Left + value.X, this.Bottom + value.Y, this.Z + value.Z, this.Right + value.X, this.Top + value.Y, this.Back + value.Z);
}