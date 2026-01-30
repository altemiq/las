// -----------------------------------------------------------------------
// <copyright file="Vector3D.Instrinsics.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if NETCOREAPP3_0_OR_GREATER
namespace Altemiq.IO.Las;

using System.Runtime.Intrinsics;
#if !NET7_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
#endif

#pragma warning disable SA1600

/// <content>
/// <see cref="System.Runtime.Intrinsics"/> implementations.
/// </content>
public partial struct Vector3D
{
#pragma warning disable SA1601

    public static partial Vector3D Add(Vector3D left, Vector3D right) => Vector256.Add(left.AsVector256(), right.AsVector256()).AsVector3D();

    public static partial Vector3D Subtract(Vector3D left, Vector3D right) => Vector256.Subtract(left.AsVector256(), right.AsVector256()).AsVector3D();

    public static partial Vector3D Multiply(Vector3D left, Vector3D right) => Vector256.Multiply(left.AsVector256(), right.AsVector256()).AsVector3D();

    public static partial Vector3D Multiply(Vector3D left, double right) => Vector256.Multiply(left.AsVector256(), Vector256.Create(right)).AsVector3D();

    public static partial Vector3D Multiply(double left, Vector3D right) => Vector256.Multiply(Vector256.Create(left), right.AsVector256()).AsVector3D();

    public static partial Vector3D Divide(Vector3D left, Vector3D right) => Vector256.Divide(left.AsVector256(), right.AsVector256()).AsVector3D();

    public static partial Vector3D Divide(Vector3D left, double divisor) => Vector256.Divide(left.AsVector256(), Vector256.Create(divisor)).AsVector3D();

    public static partial Vector3D Negate(Vector3D value) => Subtract(Zero, value);

    public static partial double Dot(Vector3D vector1, Vector3D vector2) => Vector256.Sum(Vector256.Multiply(vector1.AsVector256(), vector2.AsVector256()));

    public static partial Vector3D Min(Vector3D value1, Vector3D value2) => Vector256.Min(value1.AsVector256(), value2.AsVector256()).AsVector3D();

    public static partial Vector3D Max(Vector3D value1, Vector3D value2) => Vector256.Max(value1.AsVector256(), value2.AsVector256()).AsVector3D();

    public static partial Vector3D Abs(Vector3D value) => Vector256.Abs(value.AsVector256()).AsVector3D();

    public static partial Vector3D SquareRoot(Vector3D value) => Vector256.Sqrt(value.AsVector256()).AsVector3D();

    public static partial Vector3D Round(Vector3D vector, MidpointRounding mode) => Vector256.Round(vector.AsVector256Unsafe(), mode).AsVector3D();

    public static partial Vector3D Cross(Vector3D value1, Vector3D vector2)
#if NET7_0_OR_GREATER
    {
        var v1 = value1.AsVector256Unsafe();
        var v2 = vector2.AsVector256Unsafe();

        var temp1 = Vector256.Shuffle(v1, Vector256.Create(1, 2, 0, 0)) * Vector256.Shuffle(v2, Vector256.Create(2, 0, 1, 0));
        var temp2 = Vector256.Shuffle(v1, Vector256.Create(2, 0, 1, 0)) * Vector256.Shuffle(v2, Vector256.Create(1, 2, 0, 0));

        return (temp1 - temp2).AsVector3D();
    }
#else
        => new(
            (value1.Y * vector2.Z) - (value1.Z * vector2.Y),
            (value1.Z * vector2.X) - (value1.X * vector2.Z),
            (value1.X * vector2.Y) - (value1.Y * vector2.X));
#endif
}
#endif