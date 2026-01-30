// -----------------------------------------------------------------------
// <copyright file="Vector2D.Instrinsics.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if NETCOREAPP3_0_OR_GREATER
namespace Altemiq.IO.Las;

using System.Runtime.Intrinsics;

#pragma warning disable SA1600

/// <content>
/// <see cref="System.Runtime.Intrinsics"/> implementations.
/// </content>
public partial struct Vector2D
{
#pragma warning disable SA1601

    public static partial Vector2D Add(Vector2D left, Vector2D right) => Vector128.Add(left.AsVector128(), right.AsVector128()).AsVector2D();

    public static partial Vector2D Subtract(Vector2D left, Vector2D right) => Vector128.Subtract(left.AsVector128(), right.AsVector128()).AsVector2D();

    public static partial Vector2D Multiply(Vector2D left, Vector2D right) => Vector128.Multiply(left.AsVector128(), right.AsVector128()).AsVector2D();

    public static partial Vector2D Multiply(Vector2D left, double right) => Vector128.Multiply(left.AsVector128(), Vector128.Create(right)).AsVector2D();

    public static partial Vector2D Multiply(double left, Vector2D right) => Vector128.Multiply(Vector128.Create(left), right.AsVector128()).AsVector2D();

    public static partial Vector2D Divide(Vector2D left, Vector2D right) => Vector128.Divide(left.AsVector128(), right.AsVector128()).AsVector2D();

    public static partial Vector2D Divide(Vector2D left, double divisor) => Vector128.Divide(left.AsVector128(), Vector128.Create(divisor)).AsVector2D();

    public static partial Vector2D Negate(Vector2D value) => Subtract(Zero, value);

    public static partial double Dot(Vector2D vector1, Vector2D vector2) => Vector128.Sum(Vector128.Multiply(vector1.AsVector128(), vector2.AsVector128()));

    public static partial Vector2D Min(Vector2D value1, Vector2D value2) => Vector128.Min(value1.AsVector128(), value2.AsVector128()).AsVector2D();

    public static partial Vector2D Max(Vector2D value1, Vector2D value2) => Vector128.Max(value1.AsVector128(), value2.AsVector128()).AsVector2D();

    public static partial Vector2D Abs(Vector2D value) => Vector128.Abs(value.AsVector128()).AsVector2D();

    public static partial Vector2D SquareRoot(Vector2D value) => Vector128.Sqrt(value.AsVector128()).AsVector2D();

    public static partial Vector2D Round(Vector2D vector, MidpointRounding mode) => Vector128.Round(vector.AsVector128Unsafe(), mode).AsVector2D();

    public static partial double Cross(Vector2D value1, Vector2D vector2)
#if NET7_0_OR_GREATER
    {
        var mul =
            Vector256.Shuffle(value1.AsVector256Unsafe(), Vector256.Create(0, 1, 0, 1)) *
            Vector256.Shuffle(vector2.AsVector256Unsafe(), Vector256.Create(1, 0, 1, 0));

        return (mul - Vector256.Shuffle(mul, Vector256.Create(1, 0, 1, 0))).ToScalar();
    }
#else
        => (value1.X * vector2.Y) - (value1.Y * vector2.X);
#endif
}
#endif