// -----------------------------------------------------------------------
// <copyright file="Vector3D.Instrinsics.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if NETCOREAPP3_0_OR_GREATER
namespace Altemiq.IO.Las;

using System.Runtime.CompilerServices;
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

    public static partial Vector3D Add(Vector3D left, Vector3D right)
#if NET7_0_OR_GREATER
        => (left.AsVector256() + right.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Add(left.AsVector256(), right.AsVector256()).AsVector3D()
            : new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
#endif

    public static partial Vector3D Subtract(Vector3D left, Vector3D right)
#if NET7_0_OR_GREATER
        => (left.AsVector256() - right.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Subtract(left.AsVector256(), right.AsVector256()).AsVector3D()
            : new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
#endif

    public static partial Vector3D Multiply(Vector3D left, Vector3D right)
#if NET7_0_OR_GREATER
        => (left.AsVector256() * right.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Multiply(left.AsVector256(), right.AsVector256()).AsVector3D()
            : new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
#endif

    public static partial Vector3D Multiply(Vector3D left, double right)
#if NET7_0_OR_GREATER
        => (left.AsVector256() * Vector256.Create(right)).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Multiply(left.AsVector256(), Vector256.Create(right)).AsVector3D()
            : new(left.X * right, left.Y * right, left.Z * right);
#endif

    public static partial Vector3D Multiply(double left, Vector3D right)
#if NET7_0_OR_GREATER
        => (Vector256.Create(left) * right.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Multiply(Vector256.Create(left), right.AsVector256()).AsVector3D()
            : new(left * right.X, left * right.Y, left * right.Z);
#endif

    public static partial Vector3D Divide(Vector3D left, Vector3D right)
#if NET7_0_OR_GREATER
        => (left.AsVector256() / right.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Divide(left.AsVector256(), right.AsVector256()).AsVector3D()
            : new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
#endif

    public static partial Vector3D Divide(Vector3D left, double divisor)
#if NET7_0_OR_GREATER
        => (left.AsVector256() / Vector256.Create(divisor)).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Divide(left.AsVector256(), Vector256.Create(divisor)).AsVector3D()
            : new(left.X / divisor, left.Y / divisor, left.Z / divisor);
#endif

    public static partial Vector3D Negate(Vector3D value) => Subtract(Zero, value);

    public static partial double Dot(Vector3D vector1, Vector3D vector2)
#if NET7_0_OR_GREATER
        => Vector256.Sum(vector1.AsVector256() * vector2.AsVector256());
#else
    {
        if (Avx.IsSupported)
        {
            var dot = Avx.Multiply(vector1.AsVector256(), vector2.AsVector256());
            return GetElementUnsafe(dot, 0) + GetElementUnsafe(dot, 1) + GetElementUnsafe(dot, 2);
        }

        return (vector1.X * vector2.X) + (vector1.Y * vector2.Y) + (vector1.Z * vector2.Z);
    }
#endif

    public static partial Vector3D Min(Vector3D value1, Vector3D value2)
#if NET7_0_OR_GREATER
        => Vector256.Min(value1.AsVector256(), value2.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Min(value1.AsVector256(), value2.AsVector256()).AsVector3D()
            : new(Math.Min(value1.X, value2.X), Math.Min(value1.Y, value2.Y), Math.Min(value1.Z, value2.Z));
#endif

    public static partial Vector3D Max(Vector3D value1, Vector3D value2)
#if NET7_0_OR_GREATER
        => Vector256.Max(value1.AsVector256(), value2.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Max(value1.AsVector256(), value2.AsVector256()).AsVector3D()
            : new(Math.Max(value1.X, value2.X), Math.Max(value1.Y, value2.Y), Math.Max(value1.Z, value2.Z));
#endif

    public static partial Vector3D Abs(Vector3D value)
#if NET7_0_OR_GREATER
        => Vector256.Abs(value.AsVector256()).AsVector3D();
#else
        => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));
#endif

    public static partial Vector3D SquareRoot(Vector3D value)
#if NET7_0_OR_GREATER
        => Vector256.Sqrt(value.AsVector256()).AsVector3D();
#else
        => Avx.IsSupported
            ? Avx.Sqrt(value.AsVector256()).AsVector3D()
            : new(
                Math.Sqrt(value.X),
                Math.Sqrt(value.Y),
                Math.Sqrt(value.Z));
#endif

#if !NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T GetElementUnsafe<T>(in Vector256<T> vector, int index)
        where T : struct => Unsafe.Add(ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector)), index);
#endif
}
#endif