// -----------------------------------------------------------------------
// <copyright file="Vector2D.Fallback.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if !NETCOREAPP3_0_OR_GREATER
namespace Altemiq.IO.Las;

#pragma warning disable SA1600, SA1601

/// <content>
/// Software implementations.
/// </content>
public partial struct Vector2D
{
    public static partial Vector2D Add(Vector2D left, Vector2D right) => new(left.X + right.X, left.Y + right.Y);

    public static partial Vector2D Subtract(Vector2D left, Vector2D right) => new(left.X - right.X, left.Y - right.Y);

    public static partial Vector2D Multiply(Vector2D left, Vector2D right) => new(left.X * right.X, left.Y * right.Y);

    public static partial Vector2D Multiply(Vector2D left, double right) => Multiply(left, new Vector2D(right));

    public static partial Vector2D Multiply(double left, Vector2D right) => Multiply(new Vector2D(left), right);

    public static partial Vector2D Divide(Vector2D left, Vector2D right) => new(left.X / right.X, left.Y / right.Y);

    public static partial Vector2D Divide(Vector2D left, double divisor) => Divide(left, new Vector2D(divisor));

    public static partial Vector2D Negate(Vector2D value) => Subtract(Zero, value);

    public static partial double Dot(Vector2D vector1, Vector2D vector2) => (vector1.X * vector2.X) + (vector1.Y * vector2.Y);

    public static partial Vector2D Min(Vector2D value1, Vector2D value2) => new(
        Math.Min(value1.X, value2.X),
        Math.Min(value1.Y, value2.Y));

    public static partial Vector2D Max(Vector2D value1, Vector2D value2) => new(
        Math.Max(value1.X, value2.X),
        Math.Max(value1.Y, value2.Y));

    public static partial Vector2D Abs(Vector2D value) => new(
        Math.Abs(value.X),
        Math.Abs(value.Y));

    public static partial Vector2D SquareRoot(Vector2D value) => new(
        Math.Sqrt(value.X),
        Math.Sqrt(value.Y));

    public static partial Vector2D Round(Vector2D vector, MidpointRounding mode) => new(
        Math.Round(vector.X, mode),
        Math.Round(vector.Y, mode));

    public static partial double Cross(Vector2D value1, Vector2D vector2) => (value1.X * vector2.Y) - (value1.Y * vector2.X);
}
#endif