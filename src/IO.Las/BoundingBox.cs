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
public readonly struct BoundingBox : IEquatable<BoundingBox>
{
    /// <summary>
    /// Represents an instance of the <see cref="BoundingBox"/> class with its members uninitialized.
    /// </summary>
    public static readonly BoundingBox Empty = new(Vector3D.Zero, Vector3D.Zero);

    private readonly Vector3D lowerLeftFront;
    private readonly Vector3D upperRightBack;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct with the specified edge locations.
    /// </summary>
    /// <param name="left">The x-coordinate of the lower-left-front corner of the bounding box.</param>
    /// <param name="bottom">The y-coordinate of the lower-right-front corner of the bounding box.</param>
    /// <param name="front">The z-coordinate of the lower-left-front corner of the bounding box.</param>
    /// <param name="right">The x-coordinate of the upper-right-back corner of the bounding box.</param>
    /// <param name="top">The y-coordinate of the upper-right-back corner of the bounding box.</param>
    /// <param name="back">The z-coordinate of the upper-right-back corner of the bounding box.</param>
    public BoundingBox(double left, double bottom, double front, double right, double top, double back)
        : this(new(left, bottom, front), new(right, top, back))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct with the specified edge locations.
    /// </summary>
    /// <param name="lowerLeftFront">The coordinate of the lower-left-front corner of the bounding box.</param>
    /// <param name="upperRightBack">The coordinate of the upper-right-back corner of the bounding box.</param>
    public BoundingBox(Vector3D lowerLeftFront, Vector3D upperRightBack) => (this.lowerLeftFront, this.upperRightBack) = (lowerLeftFront, upperRightBack);

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
    public double Depth => this.Back - this.Front;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The y-coordinate of the bottom edge of this <see cref="BoundingBox"/> structure.</value>
    [System.ComponentModel.Browsable(false)]
    public double Bottom => this.lowerLeftFront.Y;

    /// <summary>
    /// Gets the x-coordinate of the left edge of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The x-coordinate of the left edge of this <see cref="BoundingBox"/> structure.</value>
    [System.ComponentModel.Browsable(false)]
    public double Left => this.lowerLeftFront.X;

    /// <summary>
    /// Gets the z-coordinate of the front edge of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The z-coordinate of the front edge of this <see cref="BoundingBox"/> structure.</value>
    [System.ComponentModel.Browsable(false)]
    public double Front => this.lowerLeftFront.Z;

    /// <summary>
    /// Gets the y-coordinate that is the sum of <see cref="Y"/> and <see cref="Height"/> property values of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The y-coordinate that is the sum of <see cref="Y"/> and <see cref="Height"/> of this <see cref="BoundingBox"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public double Top => this.upperRightBack.Y;

    /// <summary>
    /// Gets the x-coordinate that is the sum of <see cref="X"/> and <see cref="Width"/> property values of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The x-coordinate that is the sum of <see cref="X"/> and <see cref="Width"/> of this <see cref="BoundingBox"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public double Right => this.upperRightBack.X;

    /// <summary>
    /// Gets the z-coordinate that is the sum of <see cref="Z"/> and <see cref="Depth"/> property values of this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <value>The z-coordinate that is the sum of <see cref="Z"/> and <see cref="Depth"/> of this <see cref="BoundingBox"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public double Back => this.upperRightBack.Z;

    /// <summary>
    /// Gets a value indicating whether the <see cref="Width" />, <see cref="Height" />, or <see cref="Depth"/> property of this <see cref="BoundingBox" /> has a value of zero.
    /// </summary>
    /// <value>This property returns <see langword="true"/> if the <see cref="Width" />, <see cref="Height" />, or <see cref="Depth"/> property of this <see cref="BoundingBox" /> has a value of zero; otherwise, <see langword="false"/>.</value>
    [System.ComponentModel.Browsable(false)]
    public bool IsEmpty =>
#if NET7_0_OR_GREATER
        System.Runtime.Intrinsics.Vector256.GreaterThanOrEqualAny(this.lowerLeftFront.AsVector256(), this.upperRightBack.AsVector256(double.MaxValue));
#else
        (this.Width <= 0D) || (this.Height <= 0D) || (this.Depth <= 0D);
#endif

    /// <summary>
    /// Returns a value that indicates whether each pair of elements in two specified bounding boxes are equal.
    /// </summary>
    /// <param name="left">The first bounding box to compare.</param>
    /// <param name="right">The second bounding box to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(BoundingBox left, BoundingBox right) => left.Equals(right);

    /// <summary>
    /// Returns a value that indicates whether each pair of elements in two specified bounding boxes are not equal.
    /// </summary>
    /// <param name="left">The first bounding box to compare.</param>
    /// <param name="right">The second bounding box to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(BoundingBox left, BoundingBox right) => !left.Equals(right);

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
    /// <param name="envelope">The <see cref="BoundingBox"/> with which to start. This bounding box is not modified.</param>
    /// <param name="x">The amount to inflate this <see cref="BoundingBox"/> horizontally.</param>
    /// <param name="y">The amount to inflate this <see cref="BoundingBox"/> vertically.</param>
    /// <param name="z">The amount to inflate this <see cref="BoundingBox"/> int depth.</param>
    /// <returns>The inflated <see cref="BoundingBox"/>.</returns>
    public static BoundingBox Inflate(in BoundingBox envelope, double x, double y, double z) => envelope.Inflate(x, y, z);

    /// <summary>
    /// Returns a third <see cref="BoundingBox"/> structure that represents the intersection of two other <see cref="BoundingBox"/> structures. If there is no intersection, an empty <see cref="BoundingBox"/> is returned.
    /// </summary>
    /// <param name="a">The first bounding box to intersect.</param>
    /// <param name="b">The second bounding box to intersect.</param>
    /// <returns>A <see cref="BoundingBox"/> that represents the intersection of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static BoundingBox Intersect(in BoundingBox a, in BoundingBox b)
    {
#if NET7_0_OR_GREATER
        var min = System.Runtime.Intrinsics.Vector256.Max(a.lowerLeftFront.AsVector256(), b.lowerLeftFront.AsVector256());
        var max = System.Runtime.Intrinsics.Vector256.Min(a.upperRightBack.AsVector256(double.MaxValue), b.upperRightBack.AsVector256(double.MaxValue));

        return System.Runtime.Intrinsics.Vector256.GreaterThanOrEqualAll(max, min)
            ? new(min.AsVector3D(), max.AsVector3D())
            : Empty;
#else
        var x1 = Math.Max(a.Left, b.Left);
        var x2 = Math.Min(a.Right, b.Right);
        var y1 = Math.Max(a.Bottom, b.Bottom);
        var y2 = Math.Min(a.Top, b.Top);
        var z1 = Math.Max(a.Front, b.Front);
        var z2 = Math.Min(a.Back, b.Back);

        return x2 >= x1 && y2 >= y1 && z2 >= z1 ? new(x1, y1, z1, x2, y2, z2) : Empty;
#endif
    }

    /// <summary>
    /// Creates the smallest possible third bounding box that can contain both of two rectangles that form a union.
    /// </summary>
    /// <param name="a">The first bounding box to union.</param>
    /// <param name="b">The second bounding box to union.</param>
    /// <returns>A <see cref="BoundingBox"/> structure that bounds the union of the two <see cref="BoundingBox"/> structures.</returns>
    public static BoundingBox Union(in BoundingBox a, in BoundingBox b)
#if NET7_0_OR_GREATER
    {
        var min = System.Runtime.Intrinsics.Vector256.Min(a.lowerLeftFront.AsVector256(), b.lowerLeftFront.AsVector256());
        var max = System.Runtime.Intrinsics.Vector256.Max(a.upperRightBack.AsVector256(), b.upperRightBack.AsVector256());

        return new(min.AsVector3D(), max.AsVector3D());
    }
#else
        => new(
            Math.Min(a.Left, b.Left),
            Math.Min(a.Bottom, b.Bottom),
            Math.Min(a.Front, b.Front),
            Math.Max(a.Right, b.Right),
            Math.Max(a.Top, b.Top),
            Math.Max(a.Back, b.Back));
#endif

    /// <summary>
    /// Determines if the specified point is contained within this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <param name="x">The x-coordinate of the point to test.</param>
    /// <param name="y">The y-coordinate of the point to test.</param>
    /// <param name="z">The z-coordinate of the point to test.</param>
    /// <returns>This method returns <see langword="true"/> if the point defined by <paramref name="x"/>, <paramref name="y"/>, and <paramref name="z"/> is contained within this <see cref="BoundingBox"/> structure; otherwise, <see langword="false"/>.</returns>
    public bool Contains(double x, double y, double z)
#if NET7_0_OR_GREATER
    {
        var vector = System.Runtime.Intrinsics.Vector256.Create(x, y, z, default);
        return System.Runtime.Intrinsics.Vector256.LessThanOrEqualAll(this.lowerLeftFront.AsVector256(), vector)
            && System.Runtime.Intrinsics.Vector256.GreaterThanOrEqualAll(this.upperRightBack.AsVector256(), vector);
    }
#else
        => (this.Height > 0 ? this.Bottom <= y && y < this.Top : this.Bottom >= y && y > this.Top)
           && (this.Width > 0 ? this.Left <= x && x < this.Right : this.Left >= x && x > this.Right)
           && (this.Depth > 0 ? this.Front <= z && z < this.Back : this.Front >= z && z > this.Back);
#endif

    /// <summary>
    /// Determines if the rectangular region represented by <paramref name="envelope"/> is entirely contained within this <see cref="BoundingBox"/> structure.
    /// </summary>
    /// <param name="envelope">The <see cref="BoundingBox"/> to test.</param>
    /// <returns>This method returns <see langword="true"/> if the rectangular region represented by <paramref name="envelope"/> is entirely contained within this <see cref="BoundingBox"/> structure; otherwise, <see langword="false"/>.</returns>
    public bool Contains(BoundingBox envelope)
#if NET7_0_OR_GREATER
        => System.Runtime.Intrinsics.Vector256.LessThanOrEqualAll(this.lowerLeftFront.AsVector256(), envelope.lowerLeftFront.AsVector256())
           && System.Runtime.Intrinsics.Vector256.GreaterThanOrEqualAll(this.upperRightBack.AsVector256(), envelope.upperRightBack.AsVector256());
#else
        => this.Left <= envelope.Left && envelope.Right <= this.Right
            && this.Bottom <= envelope.Bottom && envelope.Top <= this.Top
            && this.Front <= envelope.Front && envelope.Back <= this.Back;
#endif

    /// <summary>
    /// Inflates this <see cref="BoundingBox"/> by the specified amount.
    /// </summary>
    /// <param name="width">The amount to inflate this <see cref="BoundingBox"/> horizontally.</param>
    /// <param name="height">The amount to inflate this <see cref="BoundingBox"/> vertically.</param>
    /// <param name="depth">The amount to inflate this <see cref="BoundingBox"/> in depth.</param>
    /// <returns>The inflated bounding box.</returns>
    public BoundingBox Inflate(double width, double height, double depth)
    {
        var amount = new Vector3D(width, height, depth);
        return new(this.lowerLeftFront - amount, this.upperRightBack + amount);
    }

    /// <summary>
    /// Returns a <see cref="BoundingBox"/> with the intersection of this instance and the specified <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="envelope">The <see cref="BoundingBox"/> to intersect.</param>
    /// <returns>The inflated bounding box.</returns>
    public BoundingBox Intersect(BoundingBox envelope) => Intersect(envelope, this);

    /// <summary>
    /// Determines if this bounding box intersects with <paramref name="rect"/>.
    /// </summary>
    /// <param name="rect">The bounding box to test.</param>
    /// <returns>This method returns <see langword="true"/> if there is any intersection, otherwise <see langword="false"/>.</returns>
    public bool IntersectsWith(BoundingBox rect)
#if NET7_0_OR_GREATER
    {
        var min = System.Runtime.Intrinsics.Vector256.Max(this.lowerLeftFront.AsVector256(), rect.lowerLeftFront.AsVector256());
        var max = System.Runtime.Intrinsics.Vector256.Min(this.upperRightBack.AsVector256(double.MaxValue), rect.upperRightBack.AsVector256(double.MaxValue));

        return System.Runtime.Intrinsics.Vector256.GreaterThanOrEqualAll(max, min);
    }
#else
        => (rect.Left < this.Right)
           && (this.Left < rect.Right)
           && (rect.Bottom < this.Top)
           && (this.Bottom < rect.Top)
           && (rect.Front < this.Back)
           && (this.Front < rect.Back);
#endif

    /// <summary>
    /// Adjusts the location of this bounding box by the specified amount.
    /// </summary>
    /// <param name="x">The horizontal offset.</param>
    /// <param name="y">The vertical offset.</param>
    /// <param name="z">The depth offset.</param>
    /// <returns>The offset bounding box.</returns>
    public BoundingBox Offset(double x, double y, double z) => this.Offset(new(x, y, z));

    /// <summary>
    /// Adjusts the location of this bounding box by the specified amount.
    /// </summary>
    /// <param name="value">The offset.</param>
    /// <returns>The offset bounding box.</returns>
    public BoundingBox Offset(Vector3D value) => new(this.lowerLeftFront + value, this.upperRightBack + value);

    /// <summary>
    /// Normalizes this instance ensuring that the <see cref="Width"/>, <see cref="Height"/>, and <see cref="Depth"/> are positive.
    /// </summary>
    /// <returns>The normalized bounding box.</returns>
    public BoundingBox Normalize() =>
#if NET7_0_OR_GREATER
        new(
            System.Runtime.Intrinsics.Vector256.Min(this.lowerLeftFront.AsVector256(), this.upperRightBack.AsVector256()).AsVector3D(),
            System.Runtime.Intrinsics.Vector256.Max(this.lowerLeftFront.AsVector256(), this.upperRightBack.AsVector256()).AsVector3D());
#else
        new(
            Math.Min(this.Left, this.Right),
            Math.Min(this.Bottom, this.Top),
            Math.Min(this.Front, this.Back),
            Math.Max(this.Left, this.Right),
            Math.Max(this.Bottom, this.Top),
            Math.Max(this.Front, this.Back));
#endif

    /// <inheritdoc />
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is BoundingBox other && this.Equals(other);

    /// <inheritdoc />
    public bool Equals(BoundingBox other)
#if NET7_0_OR_GREATER
        => this.lowerLeftFront.AsVector256() == other.lowerLeftFront.AsVector256() && this.upperRightBack.AsVector256() == other.upperRightBack.AsVector256();
#else
        => this.lowerLeftFront.Equals(other.lowerLeftFront) && this.upperRightBack.Equals(other.upperRightBack);
#endif

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(this.lowerLeftFront, this.upperRightBack);

    /// <inheritdoc />
    public override string ToString() => $"{{X={this.X},Y={this.Y},Z={this.Z},Width={this.Width},Height={this.Height},Depth={this.Depth}}}";
}