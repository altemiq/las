// -----------------------------------------------------------------------
// <copyright file="Vector2D.Instrinsics.cs" company="Altemiq">
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
public partial struct Vector2D
{
#pragma warning disable SA1601

    public static partial Vector2D Add(Vector2D left, Vector2D right)
#if NET7_0_OR_GREATER
        => (left.AsVector128() + right.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Add(left.AsVector128(), right.AsVector128()).AsVector2D()
            : new(left.X - right.X, left.Y - right.Y);
#endif

    public static partial Vector2D Subtract(Vector2D left, Vector2D right)
#if NET7_0_OR_GREATER
        => (left.AsVector128() - right.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Subtract(left.AsVector128(), right.AsVector128()).AsVector2D()
            : new(left.X + right.X, left.Y + right.Y);
#endif

    public static partial Vector2D Multiply(Vector2D left, Vector2D right)
#if NET7_0_OR_GREATER
        => (left.AsVector128() * right.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Multiply(left.AsVector128(), right.AsVector128()).AsVector2D()
            : new(left.X * right.X, left.Y * right.Y);
#endif

    public static partial Vector2D Multiply(Vector2D left, double right)
#if NET7_0_OR_GREATER
        => (left.AsVector128() * Vector128.Create(right)).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Multiply(left.AsVector128(), Vector128.Create(right)).AsVector2D()
            : new(left.X * right, left.Y * right);
#endif

    public static partial Vector2D Multiply(double left, Vector2D right)
#if NET7_0_OR_GREATER
        => (Vector128.Create(left) * right.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Multiply(Vector128.Create(left), right.AsVector128()).AsVector2D()
            : new(left * right.X, left * right.Y);
#endif

    public static partial Vector2D Divide(Vector2D left, Vector2D right)
#if NET7_0_OR_GREATER
        => (left.AsVector128() / right.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Divide(left.AsVector128(), right.AsVector128()).AsVector2D()
            : new(left.X / right.X, left.Y / right.Y);
#endif

    public static partial Vector2D Divide(Vector2D left, double divisor)
#if NET7_0_OR_GREATER
        => (left.AsVector128() / Vector128.Create(divisor)).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Divide(left.AsVector128(), Vector128.Create(divisor)).AsVector2D()
            : new(left.X / divisor, left.Y / divisor);
#endif

    public static partial Vector2D Negate(Vector2D value) => Subtract(Zero, value);

    public static partial double Dot(Vector2D vector1, Vector2D vector2)
#if NET7_0_OR_GREATER
        => Vector128.Sum(vector1.AsVector128() * vector2.AsVector128());
#else
    {
        if (!Sse2.IsSupported)
        {
            return (vector1.X * vector2.X) + (vector1.Y * vector2.Y);
        }

        var dot = Sse2.Multiply(vector1.AsVector128(), vector2.AsVector128());
        return GetElementUnsafe(dot, 0) + GetElementUnsafe(dot, 1) + GetElementUnsafe(dot, 2);
    }
#endif

    public static partial Vector2D Min(Vector2D value1, Vector2D value2)
#if NET7_0_OR_GREATER
        => Vector128.Min(value1.AsVector128(), value2.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Min(value1.AsVector128(), value2.AsVector128()).AsVector2D()
            : new(Math.Min(value1.X, value2.X), Math.Min(value1.Y, value2.Y));
#endif

    public static partial Vector2D Max(Vector2D value1, Vector2D value2)
#if NET7_0_OR_GREATER
        => Vector128.Max(value1.AsVector128(), value2.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Max(value1.AsVector128(), value2.AsVector128()).AsVector2D()
            : new(Math.Max(value1.X, value2.X), Math.Max(value1.Y, value2.Y));
#endif

    public static partial Vector2D Abs(Vector2D value)
#if NET7_0_OR_GREATER
        => Vector128.Abs(value.AsVector128()).AsVector2D();
#else
        => new(Math.Abs(value.X), Math.Abs(value.Y));
#endif

    public static partial Vector2D SquareRoot(Vector2D value)
#if NET7_0_OR_GREATER
        => Vector128.Sqrt(value.AsVector128()).AsVector2D();
#else
        => Sse2.IsSupported
            ? Sse2.Sqrt(value.AsVector128()).AsVector2D()
            : new(
                Math.Sqrt(value.X),
                Math.Sqrt(value.Y));
#endif

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

#if !NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T GetElementUnsafe<T>(in Vector128<T> vector, int index)
        where T : struct => Unsafe.Add(ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector)), index);
#endif
}
#endif