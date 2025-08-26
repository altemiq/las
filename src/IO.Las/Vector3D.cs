// -----------------------------------------------------------------------
// <copyright file="Vector3D.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using System.Runtime.CompilerServices;

/// <summary>
/// Represents a vector with three double-precision floating-point values.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly struct Vector3D : IFormattable, IEquatable<Vector3D>
{
    /// <summary>
    /// The X component of the vector.
    /// </summary>
    public readonly double X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    public readonly double Y;

    /// <summary>
    /// The Z component of the vector.
    /// </summary>
    public readonly double Z;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3D"/> struct.
    /// Creates a new <see cref="Vector3D"></see> object whose three elements have the same value.
    /// </summary>
    /// <param name="value">The value to assign to all three elements.</param>
    public Vector3D(double value)
        : this(value, value, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3D"/> struct whose elements have the specified values.
    /// </summary>
    /// <param name="x">The value to assign to the <see cref="X">x</see> field.</param>
    /// <param name="y">The value to assign to the <see cref="Y">y</see> field.</param>
    /// <param name="z">The value to assign to the <see cref="Z">z</see> field.</param>
    public Vector3D(double x, double y, double z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3D"/> struct from the given <see cref="ReadOnlySpan{Double}" />.
    /// </summary>
    /// <param name="values">The span of elements to assign to the vector.</param>
    public Vector3D(ReadOnlySpan<double> values)
    {
        if (values.Length < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(values));
        }

        this.X = values[0];
        this.Y = values[1];
        this.Z = values[2];
    }

    /// <summary>
    /// Gets a vector whose 3 elements are equal to zero.
    /// </summary>
    /// <returns>A vector whose three elements are equal to zero (that is, it returns the vector <c>(0,0,0)</c>.</returns>
    public static Vector3D Zero => default;

    /// <summary>
    /// Gets a vector whose 3 elements are equal to one.
    /// </summary>
    /// <returns>A vector whose three elements are equal to one (that is, it returns the vector <c>(1,1,1)</c>.</returns>
    public static Vector3D One => new(1D);

    /// <summary>
    /// Gets the vector (1,0,0).
    /// </summary>
    /// <returns>The vector <c>(1,0,0)</c>.</returns>
    public static Vector3D UnitX => new(1D, 0D, 0D);

    /// <summary>
    /// Gets the vector (0,1,0).
    /// </summary>
    /// <returns>The vector <c>(0,1,0)</c>.</returns>
    public static Vector3D UnitY => new(0D, 1D, 0D);

    /// <summary>
    /// Gets the vector (0,0,1).
    /// </summary>
    /// <returns>The vector <c>(0,0,1)</c>.</returns>
    public static Vector3D UnitZ => new(0D, 0D, 1D);

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator +(Vector3D left, Vector3D right) => Add(left, right);

    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D left, Vector3D right) => Subtract(left, right);

    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(Vector3D left, Vector3D right) => Multiply(left, right.Y);

    /// <summary>
    /// Multiples the specified vector by the specified scalar value.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(Vector3D left, double right) => Multiply(left, new Vector3D(right));

    /// <summary>
    /// Multiples the scalar value by the specified vector.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(double left, Vector3D right) => Multiply(new Vector3D(left), right);

    /// <summary>
    /// Divides the first vector by the second.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator /(Vector3D left, Vector3D right) => Divide(left, right);

    /// <summary>
    /// Divides the specified vector by a specified scalar value.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator /(Vector3D left, double right)
    {
        var num = 1D / right;
        return new Vector3D(left.X * num, left.Y * num, left.Z * num);
    }

    /// <summary>
    /// Negates the specified vector.
    /// </summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D value) => Negate(value);

    /// <summary>
    /// Returns a value that indicates whether each pair of elements in two specified vectors is equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector3D left, Vector3D right) => left.X.Equals(right.X) && left.Y.Equals(right.Y) && left.Z.Equals(right.Z);

    /// <summary>
    /// Returns a value that indicates whether two specified vectors are not equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector3D left, Vector3D right) => !left.X.Equals(right.X) || !left.Y.Equals(right.Y) || !left.Z.Equals(right.Z);

    /// <summary>
    /// Computes the Euclidean distance between the two given points.
    /// </summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Vector3D value1, Vector3D value2)
    {
        var distanceSquared = DistanceSquared(value1, value2);
        return Math.Sqrt(distanceSquared);
    }

    /// <summary>
    /// Returns the Euclidean distance squared between two specified points.
    /// </summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DistanceSquared(Vector3D value1, Vector3D value2)
    {
        var difference = Subtract(value1, value2);
        return Dot(difference, difference);
    }

    /// <summary>
    /// Returns a vector with the same direction as the specified vector, but with a length of one.
    /// </summary>
    /// <param name="value">The vector to normalize.</param>
    /// <returns>The normalized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Normalize(Vector3D value) => value / value.Length();

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The cross product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Cross(Vector3D value1, Vector3D vector2) => new(
        (value1.Y * vector2.Z) - (value1.Z * vector2.Y),
        (value1.Z * vector2.X) - (value1.X * vector2.Z),
        (value1.X * vector2.Y) - (value1.Y * vector2.X));

    /// <summary>
    /// Returns the reflection of a vector off a surface that has the specified normal.
    /// </summary>
    /// <param name="vector">The source vector.</param>
    /// <param name="normal">The normal of the surface being reflected off.</param>
    /// <returns>The reflected vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Reflect(Vector3D vector, Vector3D normal)
    {
        var dot = Dot(vector, normal);
        return vector - (2D * (dot * normal));
    }

    /// <summary>
    /// Restricts a vector between a minimum and a maximum value.
    /// </summary>
    /// <param name="value">The vector to restrict.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The restricted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Clamp(Vector3D value, Vector3D min, Vector3D max) => Min(Max(value, min), max);

    /// <summary>
    /// Performs a linear interpolation between two vectors based on the given weighting.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="amount">A value between 0 and 1 that indicates the weight of value2.</param>
    /// <returns>The interpolated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Lerp(Vector3D value1, Vector3D value2, double amount) => new(value1.X + ((value2.X - value1.X) * amount), value1.Y + ((value2.Y - value1.Y) * amount), value1.Z + ((value2.Z - value1.Z) * amount));

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Add(Vector3D left, Vector3D right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Subtract(Vector3D left, Vector3D right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Multiply(Vector3D left, Vector3D right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    /// <summary>
    /// Multiplies a vector by a specified scalar.
    /// </summary>
    /// <param name="left">The vector to multiply.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Multiply(Vector3D left, double right) => Multiply(left, new Vector3D(right));

    /// <summary>
    /// Multiplies a scalar value by a specified vector.
    /// </summary>
    /// <param name="left">The scaled value.</param>
    /// <param name="right">The vector.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Multiply(double left, Vector3D right) => Multiply(new Vector3D(left), right);

    /// <summary>
    /// Divides the first vector by the second.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector resulting from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Divide(Vector3D left, Vector3D right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    /// <summary>
    /// Divides the specified vector by a specified scalar value.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="divisor">The scalar value.</param>
    /// <returns>The vector that results from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Divide(Vector3D left, double divisor) => Divide(left, new Vector3D(divisor));

    /// <summary>
    /// Negates a specified vector.
    /// </summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Negate(Vector3D value) => Subtract(Zero, value);

    /// <summary>
    /// Returns the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vector3D vector1, Vector3D vector2) => (vector1.X * vector2.X) + (vector1.Y * vector2.Y) + (vector1.Z * vector2.Z);

    /// <summary>
    /// Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The minimized vector.</returns>
    public static Vector3D Min(Vector3D value1, Vector3D value2) => new(
        Math.Min(value1.X, value2.X),
        Math.Min(value1.Y, value2.Y),
        Math.Min(value1.Z, value2.Z));

    /// <summary>
    /// Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The maximized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Max(Vector3D value1, Vector3D value2) => new(
        Math.Max(value1.X, value2.X),
        Math.Max(value1.Y, value2.Y),
        Math.Max(value1.Z, value2.Z));

    /// <summary>
    /// Returns a vector whose elements are the absolute values of each of the specified vector's elements.
    /// </summary>
    /// <param name="value">A vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Abs(Vector3D value) => new(
        Math.Abs(value.X),
        Math.Abs(value.Y),
        Math.Abs(value.Z));

    /// <summary>
    /// Returns a vector whose elements are the square root of each of a specified vector's elements.
    /// </summary>
    /// <param name="value">A vector.</param>
    /// <returns>The square root vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D SquareRoot(Vector3D value) => new(
        Math.Sqrt(value.X),
        Math.Sqrt(value.Y),
        Math.Sqrt(value.Z));

    /// <inheritdoc/>
    public override int GetHashCode()
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER
        => HashCode.Combine(this.X, this.Y, this.Z);
#else
    {
        return Combine(Combine(this.X.GetHashCode(), this.Y.GetHashCode()), this.Z.GetHashCode());

        static int Combine(int h1, int h2)
        {
            var num = (uint)(h1 << 5) | ((uint)h1 >> 27);
            return ((int)num + h1) ^ h2;
        }
    }
#endif

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is Vector3D vector && this.Equals(vector);

    /// <inheritdoc/>
    public bool Equals(Vector3D other) => this == other;

    /// <summary>
    /// Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the value of this instance.</returns>>
    public override string ToString() => this.ToString("G", System.Globalization.CultureInfo.CurrentCulture);

    /// <summary>
    /// Returns the string representation of the current instance using the specified format string to format individual elements.
    /// </summary>
    /// <param name="format">The format to for individual elements or a <see langword="null"/> to use the default format.</param>
    /// <returns>The string representation of the current instance.</returns>
    public string ToString([System.Diagnostics.CodeAnalysis.StringSyntax(System.Diagnostics.CodeAnalysis.StringSyntaxAttribute.NumericFormat)] string? format) => this.ToString(format, System.Globalization.CultureInfo.CurrentCulture);

    /// <inheritdoc/>
    public string ToString([System.Diagnostics.CodeAnalysis.StringSyntax(System.Diagnostics.CodeAnalysis.StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        var stringBuilder = new System.Text.StringBuilder();
        var numberGroupSeparator = System.Globalization.NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        _ = stringBuilder.Append('<');
        _ = stringBuilder.Append(this.X.ToString(format, formatProvider));
        _ = stringBuilder.Append(numberGroupSeparator);
        _ = stringBuilder.Append(' ');
        _ = stringBuilder.Append(this.Y.ToString(format, formatProvider));
        _ = stringBuilder.Append(numberGroupSeparator);
        _ = stringBuilder.Append(' ');
        _ = stringBuilder.Append(this.Z.ToString(format, formatProvider));
        _ = stringBuilder.Append('>');
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Returns the length of this vector object.
    /// </summary>
    /// <returns>The vector's length.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Length() => Math.Sqrt(this.LengthSquared());

    /// <summary>
    /// Returns the length of the vector squared.
    /// </summary>
    /// <returns>The vector's length squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double LengthSquared() => Dot(this, this);

    /// <summary>
    /// Copies the elements of the vector to a specified array.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The number of elements in the current instance is greater than in the array.</exception>
    /// <exception cref="RankException"><paramref name="array"/> is multidimensional.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(double[] array) => this.CopyTo(array, 0);

    /// <summary>Copies the elements of the vector to a specified array starting at a specified index position.</summary>
    /// <param name="array">The destination array.</param>
    /// <param name="index">The index at which to copy the first element of the vector.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The number of elements in the current instance is greater than in the array.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.
    /// <para>-or-</para>
    /// <paramref name="index"/> is greater than or equal to the array length.</exception>
    /// <exception cref="RankException"><paramref name="array"/> is multidimensional.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(double[] array, int index)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(array);
#else
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }
#endif

        if (index < 0 || index >= array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (array.Length - index < 3)
        {
            throw new ArgumentException(Properties.Resources.ElementsInSourceIsGreaterThanDestination, nameof(index));
        }

        array[index] = this.X;
        array[index + 1] = this.Y;
        array[index + 2] = this.Z;
    }
}