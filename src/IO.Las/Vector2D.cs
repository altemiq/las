// -----------------------------------------------------------------------
// <copyright file="Vector2D.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using System.Runtime.CompilerServices;

/// <summary>
/// Represents a vector with three double-precision floating-point values.
/// </summary>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly partial struct Vector2D : IFormattable, IEquatable<Vector2D>
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
    /// Initializes a new instance of the <see cref="Vector2D"/> struct.
    /// Creates a new <see cref="Vector2D"></see> object whose three elements have the same value.
    /// </summary>
    /// <param name="value">The value to assign to all three elements.</param>
    public Vector2D(double value)
        : this(value, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2D"/> struct whose elements have the specified values.
    /// </summary>
    /// <param name="x">The value to assign to the <see cref="X">x</see> field.</param>
    /// <param name="y">The value to assign to the <see cref="Y">y</see> field.</param>
    public Vector2D(double x, double y)
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2D"/> struct from the given <see cref="ReadOnlySpan{Double}" />.
    /// </summary>
    /// <param name="values">The span of elements to assign to the vector.</param>
    public Vector2D(ReadOnlySpan<double> values)
    {
        if (values.Length < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(values));
        }

        this.X = values[0];
        this.Y = values[1];
    }

    /// <summary>
    /// Gets a vector whose 2 elements are equal to zero.
    /// </summary>
    /// <returns>A vector whose three elements are equal to zero (that is, it returns the vector <c>(0,0,0)</c>.</returns>
    public static Vector2D Zero => default;

    /// <summary>
    /// Gets a vector whose 2 elements are equal to one.
    /// </summary>
    /// <returns>A vector whose three elements are equal to one (that is, it returns the vector <c>(1,1,1)</c>.</returns>
    public static Vector2D One => new(1D);

    /// <summary>
    /// Gets the vector (1,0).
    /// </summary>
    /// <returns>The vector <c>(1,0)</c>.</returns>
    public static Vector2D UnitX => new(1D, 0D);

    /// <summary>
    /// Gets the vector (0,1).
    /// </summary>
    /// <returns>The vector <c>(0,1)</c>.</returns>
    public static Vector2D UnitY => new(0D, 1D);

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator +(Vector2D left, Vector2D right) => Add(left, right);

    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from subtracting <paramref name="right"/> from <paramref name="left"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator -(Vector2D left, Vector2D right) => Subtract(left, right);

    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator *(Vector2D left, Vector2D right) => Multiply(left, right);

    /// <summary>
    /// Multiples the specified vector by the specified scalar value.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator *(Vector2D left, double right) => Multiply(left, new Vector2D(right));

    /// <summary>
    /// Multiples the scalar value by the specified vector.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator *(double left, Vector2D right) => Multiply(new Vector2D(left), right);

    /// <summary>
    /// Divides the first vector by the second.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from dividing <paramref name="left"/> by <paramref name="right"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator /(Vector2D left, Vector2D right) => Divide(left, right);

    /// <summary>
    /// Divides the specified vector by a specified scalar value.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator /(Vector2D left, double right)
    {
        var num = 1D / right;
        return new(left.X * num, left.Y * num);
    }

    /// <summary>
    /// Negates the specified vector.
    /// </summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D operator -(Vector2D value) => Negate(value);

    /// <summary>
    /// Returns a value that indicates whether each pair of elements in two specified vectors is equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2D left, Vector2D right) => left.X.Equals(right.X) && left.Y.Equals(right.Y);

    /// <summary>
    /// Returns a value that indicates whether two specified vectors are not equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2D left, Vector2D right) => !left.X.Equals(right.X) || !left.Y.Equals(right.Y);

    /// <summary>
    /// Computes the Euclidean distance between the two given points.
    /// </summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Vector2D value1, Vector2D value2)
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
    public static double DistanceSquared(Vector2D value1, Vector2D value2)
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
    public static Vector2D Normalize(Vector2D value) => value / value.Length();

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The cross product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial double Cross(Vector2D value1, Vector2D vector2);

    /// <summary>
    /// Returns the reflection of a vector off a surface that has the specified normal.
    /// </summary>
    /// <param name="vector">The source vector.</param>
    /// <param name="normal">The normal of the surface being reflected off.</param>
    /// <returns>The reflected vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D Reflect(Vector2D vector, Vector2D normal)
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
    public static Vector2D Clamp(Vector2D value, Vector2D min, Vector2D max) => Min(Max(value, min), max);

    /// <summary>
    /// Performs a linear interpolation between two vectors based on the given weighting.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="amount">A value between 0 and 1 that indicates the weight of value2.</param>
    /// <returns>The interpolated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2D Lerp(Vector2D value1, Vector2D value2, double amount) => new(value1.X + ((value2.X - value1.X) * amount), value1.Y + ((value2.Y - value1.Y) * amount));

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Add(Vector2D left, Vector2D right);

    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Subtract(Vector2D left, Vector2D right);

    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Multiply(Vector2D left, Vector2D right);

    /// <summary>
    /// Multiplies a vector by a specified scalar.
    /// </summary>
    /// <param name="left">The vector to multiply.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Multiply(Vector2D left, double right);

    /// <summary>
    /// Multiplies a scalar value by a specified vector.
    /// </summary>
    /// <param name="left">The scaled value.</param>
    /// <param name="right">The vector.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Multiply(double left, Vector2D right);

    /// <summary>
    /// Divides the first vector by the second.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector resulting from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Divide(Vector2D left, Vector2D right);

    /// <summary>
    /// Divides the specified vector by a specified scalar value.
    /// </summary>
    /// <param name="left">The vector.</param>
    /// <param name="divisor">The scalar value.</param>
    /// <returns>The vector that results from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Divide(Vector2D left, double divisor);

    /// <summary>
    /// Negates a specified vector.
    /// </summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Negate(Vector2D value);

    /// <summary>
    /// Returns the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial double Dot(Vector2D vector1, Vector2D vector2);

    /// <summary>
    /// Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The minimized vector.</returns>
    public static partial Vector2D Min(Vector2D value1, Vector2D value2);

    /// <summary>
    /// Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The maximized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Max(Vector2D value1, Vector2D value2);

    /// <summary>
    /// Returns a vector whose elements are the absolute values of each of the specified vector's elements.
    /// </summary>
    /// <param name="value">A vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D Abs(Vector2D value);

    /// <summary>
    /// Returns a vector whose elements are the square root of each of a specified vector's elements.
    /// </summary>
    /// <param name="value">A vector.</param>
    /// <returns>The square root vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2D SquareRoot(Vector2D value);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.X, this.Y);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is Vector2D vector && this.Equals(vector);

    /// <inheritdoc/>
    public bool Equals(Vector2D other) => this == other;

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
    public void CopyTo(double[] array)
    {
        // We explicitly don't check for `null` because historically this has thrown `NullReferenceException` for perf reasons
        if (array.Length < 2)
        {
            throw new ArgumentException(Properties.Resources.ElementsInSourceIsGreaterThanDestination, nameof(array));
        }

        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref array[0]), this);
    }

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
        // We explicitly don't check for `null` because historically this has thrown `NullReferenceException` for perf reasons
        ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, array.Length);
        if (array.Length - index < 2)
        {
            throw new ArgumentException(Properties.Resources.ElementsInSourceIsGreaterThanDestination, nameof(index));
        }

        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref array[index]), this);
    }

    /// <summary>
    /// Copies the vector to the given <see cref="Span{T}" />. The length of the destination span must be at least 2.
    /// </summary>
    /// <param name="destination">The destination span which the values are copied into.</param>
    /// <exception cref="ArgumentException">If number of elements in source vector is greater than those available in destination span.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<double> destination)
    {
        if (destination.Length < 2)
        {
            throw new ArgumentException(Properties.Resources.ElementsInSourceIsGreaterThanDestination, nameof(destination));
        }

        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(destination)), this);
    }
}