// -----------------------------------------------------------------------
// <copyright file="Vector3D.Fallback.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if !NETCOREAPP3_0_OR_GREATER
namespace Altemiq.IO.Las;

#pragma warning disable SA1600, SA1601

/// <content>
/// Software implementations.
/// </content>
public partial struct Vector3D
{
    public static partial Vector3D Add(Vector3D left, Vector3D right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static partial Vector3D Subtract(Vector3D left, Vector3D right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static partial Vector3D Multiply(Vector3D left, Vector3D right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);

    public static partial Vector3D Multiply(Vector3D left, double right) => Multiply(left, new Vector3D(right));

    public static partial Vector3D Multiply(double left, Vector3D right) => Multiply(new Vector3D(left), right);

    public static partial Vector3D Divide(Vector3D left, Vector3D right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);

    public static partial Vector3D Divide(Vector3D left, double divisor) => Divide(left, new Vector3D(divisor));

    public static partial Vector3D Negate(Vector3D value) => Subtract(Zero, value);

    public static partial double Dot(Vector3D vector1, Vector3D vector2) => (vector1.X * vector2.X) + (vector1.Y * vector2.Y) + (vector1.Z * vector2.Z);

    public static partial Vector3D Min(Vector3D value1, Vector3D value2) => new(
        Math.Min(value1.X, value2.X),
        Math.Min(value1.Y, value2.Y),
        Math.Min(value1.Z, value2.Z));

    public static partial Vector3D Max(Vector3D value1, Vector3D value2) => new(
        Math.Max(value1.X, value2.X),
        Math.Max(value1.Y, value2.Y),
        Math.Max(value1.Z, value2.Z));

    public static partial Vector3D Abs(Vector3D value) => new(
        Math.Abs(value.X),
        Math.Abs(value.Y),
        Math.Abs(value.Z));

    public static partial Vector3D SquareRoot(Vector3D value) => new(
        Math.Sqrt(value.X),
        Math.Sqrt(value.Y),
        Math.Sqrt(value.Z));

    public static partial Vector3D Round(Vector3D vector, MidpointRounding mode) => new(
        Math.Round(vector.X, mode),
        Math.Round(vector.Y, mode),
        Math.Round(vector.Z, mode));

    public static partial Vector3D Cross(Vector3D value1, Vector3D vector2) => new(
        (value1.Y * vector2.Z) - (value1.Z * vector2.Y),
        (value1.Z * vector2.X) - (value1.X * vector2.Z),
        (value1.X * vector2.Y) - (value1.Y * vector2.X));
}
#endif