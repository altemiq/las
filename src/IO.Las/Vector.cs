// -----------------------------------------------------------------------
// <copyright file="Vector.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable CA1708, RCS1263, SA1101, S2325

using System.Runtime.CompilerServices;

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
#if !NET9_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif
#endif

/// <summary>
/// Vector extensions.
/// </summary>
public static partial class Vector
{
#if NETCOREAPP3_0_OR_GREATER
    /// <content>
    /// The <see cref="Vector128{Double}"/> extensions.
    /// </content>
    /// <param name="value">The vector to reinterpret.</param>
    extension(Vector128<double> value)
    {
        /// <summary>
        /// Reinterprets a <see langword="Vector128{double}" /> as a new <see cref="Vector2D" />.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector2D" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D AsVector2D()
        {
            ref var address = ref Unsafe.As<Vector128<double>, byte>(ref value);
            return Unsafe.ReadUnaligned<Vector2D>(ref address);
        }
    }

    /// <content>
    /// The <see cref="Vector256{Double}"/> extensions.
    /// </content>
    /// <param name="value">The vector to reinterpret.</param>
    extension(Vector256<double> value)
    {
        /// <summary>
        /// Reinterprets a <see langword="Vector256{double}" /> as a new <see cref="Vector2D" />.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector2D" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2D AsVector2D()
        {
            ref var address = ref Unsafe.As<Vector256<double>, byte>(ref value);
            return Unsafe.ReadUnaligned<Vector2D>(ref address);
        }

        /// <summary>
        /// Reinterprets a <see langword="Vector256{double}" /> as a new <see cref="Vector3D" />.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector3D" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D AsVector3D()
        {
            ref var address = ref Unsafe.As<Vector256<double>, byte>(ref value);
            return Unsafe.ReadUnaligned<Vector3D>(ref address);
        }
    }

    extension(Vector128)
    {
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Rounds each element in a vector to the nearest integer using the specified rounding mode.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="mode">The mode under which vector should be rounded.</param>
        /// <returns>The result of rounding each element to the nearest integer using <paramref name="mode"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<float> Round(Vector128<float> vector, MidpointRounding mode)
        {
            if (Sse41.IsSupported)
            {
                return mode switch
                {
                    MidpointRounding.ToEven => Sse41.RoundToNearestInteger(vector),
                    MidpointRounding.AwayFromZero => Sse41.RoundCurrentDirection(vector),
                    MidpointRounding.ToZero => Sse41.RoundToZero(vector),
                    MidpointRounding.ToNegativeInfinity => Sse41.RoundToNegativeInfinity(vector),
                    MidpointRounding.ToPositiveInfinity => Sse41.RoundToPositiveInfinity(vector),
                    _ => throw new ArgumentOutOfRangeException(nameof(mode)),
                };
            }

            if (AdvSimd.IsSupported)
            {
                return mode switch
                {
                    MidpointRounding.ToEven => AdvSimd.RoundToNearest(vector),
                    MidpointRounding.AwayFromZero => AdvSimd.RoundAwayFromZero(vector),
                    MidpointRounding.ToZero => AdvSimd.RoundToZero(vector),
                    MidpointRounding.ToNegativeInfinity => AdvSimd.RoundToNegativeInfinity(vector),
                    MidpointRounding.ToPositiveInfinity => AdvSimd.RoundToPositiveInfinity(vector),
                    _ => throw new ArgumentOutOfRangeException(nameof(mode)),
                };
            }

            return Vector128.Create(
                MathF.Round(vector.GetElement(0), mode),
                MathF.Round(vector.GetElement(1), mode),
                MathF.Round(vector.GetElement(2), mode),
                MathF.Round(vector.GetElement(3), mode));
        }

        /// <summary>
        /// Rounds each element in a vector to the nearest integer using the specified rounding mode.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="mode">The mode under which vector should be rounded.</param>
        /// <returns>The result of rounding each element to the nearest integer using <paramref name="mode"/>.</returns>
        public static Vector128<double> Round(Vector128<double> vector, MidpointRounding mode)
        {
            if (Sse41.IsSupported)
            {
                return mode switch
                {
                    MidpointRounding.ToEven => Sse41.RoundToNearestInteger(vector),
                    MidpointRounding.AwayFromZero => Sse41.RoundCurrentDirection(vector),
                    MidpointRounding.ToZero => Sse41.RoundToZero(vector),
                    MidpointRounding.ToNegativeInfinity => Sse41.RoundToNegativeInfinity(vector),
                    MidpointRounding.ToPositiveInfinity => Sse41.RoundToPositiveInfinity(vector),
                    _ => throw new ArgumentOutOfRangeException(nameof(mode)),
                };
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return mode switch
                {
                    MidpointRounding.ToEven => AdvSimd.Arm64.RoundToNearest(vector),
                    MidpointRounding.AwayFromZero => AdvSimd.Arm64.RoundAwayFromZero(vector),
                    MidpointRounding.ToZero => AdvSimd.Arm64.RoundToZero(vector),
                    MidpointRounding.ToNegativeInfinity => AdvSimd.Arm64.RoundToNegativeInfinity(vector),
                    MidpointRounding.ToPositiveInfinity => AdvSimd.Arm64.RoundToPositiveInfinity(vector),
                    _ => throw new ArgumentOutOfRangeException(nameof(mode)),
                };
            }

            return Vector128.Create(
                Math.Round(vector.GetElement(0), mode),
                Math.Round(vector.GetElement(1), mode));
        }
#endif

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Adds two vectors to compute their sum.
        /// </summary>
        /// <param name="left">The vector to add with <paramref name="right"/>.</param>
        /// <param name="right">The vector to add with <paramref name="left"/>.</param>
        /// <returns>The sum of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector128<double> Add(Vector128<double> left, Vector128<double> right)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Add(left, right);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.Add(left, right);
            }

            return Vector128.Create(
                GetElementUnsafe(left, 0) + GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) + GetElementUnsafe(right, 1));
        }

        /// <summary>
        /// Subtracts two vectors to compute their difference.
        /// </summary>
        /// <param name="left">The vector from which <paramref name="right"/> will be subtracted.</param>
        /// <param name="right">The vector to subtract from <paramref name="left"/>.</param>
        /// <returns>The difference of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector128<double> Subtract(Vector128<double> left, Vector128<double> right)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Subtract(left, right);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.Subtract(left, right);
            }

            return Vector128.Create(
                GetElementUnsafe(left, 0) - GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) - GetElementUnsafe(right, 1));
        }

        /// <summary>
        /// Multiplies two vectors to compute their element-wise product.
        /// </summary>
        /// <param name="left">The vector to multiply with <paramref name="right"/>.</param>
        /// <param name="right">The vector to multiply with <paramref name="left"/>.</param>
        /// <returns>The element-wise product of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector128<double> Multiply(Vector128<double> left, Vector128<double> right)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Multiply(left, right);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.Multiply(left, right);
            }

            return Vector128.Create(
                GetElementUnsafe(left, 0) * GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) * GetElementUnsafe(right, 1));
        }

        /// <summary>
        /// Divides two vectors to compute their quotient.
        /// </summary>
        /// <param name="left">The vector that will be divided by <paramref name="right"/>.</param>
        /// <param name="right">The vector that will divide <paramref name="left"/>.</param>
        /// <returns>The quotient of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector128<double> Divide(Vector128<double> left, Vector128<double> right)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Divide(left, right);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.Divide(left, right);
            }

            return Vector128.Create(
                GetElementUnsafe(left, 0) / GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) / GetElementUnsafe(right, 1));
        }

        /// <summary>
        /// Computes the sum of all elements in a vector.
        /// </summary>
        /// <param name="vector">The vector whose elements will be summed.</param>
        /// <returns>The sum of all elements in <paramref name="vector"/>.</returns>
        public static double Sum(Vector128<double> vector) => GetElementUnsafe(vector, 0) + GetElementUnsafe(vector, 1) + GetElementUnsafe(vector, 2) + GetElementUnsafe(vector, 3);

        /// <summary>
        /// Computes the minimum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>A vector whose elements are the minimum of the corresponding elements in <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static Vector128<double> Min(Vector128<double> left, Vector128<double> right)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Min(left, right);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.Min(left, right);
            }

            return Vector128.Create(
                Math.Min(GetElementUnsafe(left, 0), GetElementUnsafe(right, 0)),
                Math.Min(GetElementUnsafe(left, 1), GetElementUnsafe(right, 1)));
        }

        /// <summary>
        /// Computes the maximum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>A vector whose elements are the maximum of the corresponding elements in <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static Vector128<double> Max(Vector128<double> left, Vector128<double> right)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Max(left, right);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.Max(left, right);
            }

            return Vector128.Create(
                Math.Max(GetElementUnsafe(left, 0), GetElementUnsafe(right, 0)),
                Math.Max(GetElementUnsafe(left, 1), GetElementUnsafe(right, 1)));
        }

        /// <summary>
        /// Computes the absolute value of each element in a vector.
        /// </summary>
        /// <param name="vector">The vector that will have its absolute value computed.</param>
        /// <returns>A vector whose elements are the absolute value of the elements in <paramref name="vector"/>.</returns>
        public static Vector128<double> Abs(Vector128<double> vector)
            => AdvSimd.Arm64.IsSupported
                ? AdvSimd.Arm64.Abs(vector)
                : Vector128.Create(Math.Abs(GetElementUnsafe(vector, 0)), Math.Abs(GetElementUnsafe(vector, 1)));

        /// <summary>
        /// Computes the square root of a vector on a per-element basis.
        /// </summary>
        /// <param name="vector">The vector whose square root is to be computed.</param>
        /// <returns>A vector whose elements are the square root of the corresponding elements in <paramref name="vector"/>.</returns>
        public static Vector128<double> Sqrt(Vector128<double> vector)
        {
            if (Sse2.IsSupported)
            {
                return Sse2.Sqrt(vector);
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return AdvSimd.Arm64.Sqrt(vector);
            }

            return Vector128.Create(
                Math.Sqrt(vector.GetElement(0)),
                Math.Sqrt(vector.GetElement(1)));
        }
#endif
    }

    extension(Vector256)
    {
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Rounds each element in a vector to the nearest integer using the specified rounding mode.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="mode">The mode under which vector should be rounded.</param>
        /// <returns>The result of rounding each element to the nearest integer using <paramref name="mode"/>.</returns>
        public static Vector256<double> Round(Vector256<double> vector, MidpointRounding mode)
        {
            if (Avx.IsSupported)
            {
                return mode switch
                {
                    MidpointRounding.ToEven => Avx.RoundToNearestInteger(vector),
                    MidpointRounding.AwayFromZero => Avx.RoundCurrentDirection(vector),
                    MidpointRounding.ToZero => Avx.RoundToZero(vector),
                    MidpointRounding.ToNegativeInfinity => Avx.RoundToNegativeInfinity(vector),
                    MidpointRounding.ToPositiveInfinity => Avx.RoundToPositiveInfinity(vector),
                    _ => throw new ArgumentOutOfRangeException(nameof(mode)),
                };
            }

            return Vector256.Create(
                Math.Round(vector.GetElement(0), mode),
                Math.Round(vector.GetElement(1), mode),
                Math.Round(vector.GetElement(2), mode),
                Math.Round(vector.GetElement(3), mode));
        }

        /// <summary>
        /// Truncates each element in a vector.
        /// </summary>
        /// <param name="vector">The vector to truncate.</param>
        /// <returns>The truncation of each element in <paramref name="vector"/>.</returns>
        public static Vector256<double> Truncate(Vector256<double> vector)
        {
            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.ConvertToVector128Double(Sse2.ConvertToVector128Int32WithTruncation(vector.GetLower())),
                    Sse2.ConvertToVector128Double(Sse2.ConvertToVector128Int32WithTruncation(vector.GetUpper())));
            }

            if (AdvSimd.Arm64.IsSupported)
            {
                return Vector256.Create(
                    AdvSimd.Arm64.ConvertToDouble(AdvSimd.Arm64.ConvertToInt64RoundToZero(vector.GetLower())),
                    AdvSimd.Arm64.ConvertToDouble(AdvSimd.Arm64.ConvertToInt64RoundToZero(vector.GetUpper())));
            }

            return Vector256.Create(
                Math.Truncate(vector.GetElement(0)),
                Math.Truncate(vector.GetElement(1)),
                Math.Truncate(vector.GetElement(2)),
                Math.Truncate(vector.GetElement(3)));
        }
#endif

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Adds two vectors to compute their sum.
        /// </summary>
        /// <param name="left">The vector to add with <paramref name="right"/>.</param>
        /// <param name="right">The vector to add with <paramref name="left"/>.</param>
        /// <returns>The sum of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector256<double> Add(Vector256<double> left, Vector256<double> right)
        {
            if (Avx.IsSupported)
            {
                return Avx.Add(left, right);
            }

            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.Add(left.GetLower(), right.GetLower()),
                    Sse2.Add(left.GetUpper(), right.GetUpper()));
            }

            return Vector256.Create(
                GetElementUnsafe(left, 0) + GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) + GetElementUnsafe(right, 1),
                GetElementUnsafe(left, 2) + GetElementUnsafe(right, 2),
                GetElementUnsafe(left, 3) + GetElementUnsafe(right, 3));
        }

        /// <summary>
        /// Subtracts two vectors to compute their difference.
        /// </summary>
        /// <param name="left">The vector from which <paramref name="right"/> will be subtracted.</param>
        /// <param name="right">The vector to subtract from <paramref name="left"/>.</param>
        /// <returns>The difference of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector256<double> Subtract(Vector256<double> left, Vector256<double> right)
        {
            if (Avx.IsSupported)
            {
                return Avx.Subtract(left, right);
            }

            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.Subtract(left.GetLower(), right.GetLower()),
                    Sse2.Subtract(left.GetUpper(), right.GetUpper()));
            }

            return Vector256.Create(
                GetElementUnsafe(left, 0) - GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) - GetElementUnsafe(right, 1),
                GetElementUnsafe(left, 2) - GetElementUnsafe(right, 2),
                GetElementUnsafe(left, 3) - GetElementUnsafe(right, 3));
        }

        /// <summary>
        /// Multiplies two vectors to compute their element-wise product.
        /// </summary>
        /// <param name="left">The vector to multiply with <paramref name="right"/>.</param>
        /// <param name="right">The vector to multiply with <paramref name="left"/>.</param>
        /// <returns>The element-wise product of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector256<double> Multiply(Vector256<double> left, Vector256<double> right)
        {
            if (Avx.IsSupported)
            {
                return Avx.Multiply(left, right);
            }

            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.Multiply(left.GetLower(), right.GetLower()),
                    Sse2.Multiply(left.GetUpper(), right.GetUpper()));
            }

            return Vector256.Create(
                GetElementUnsafe(left, 0) * GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) * GetElementUnsafe(right, 1),
                GetElementUnsafe(left, 2) * GetElementUnsafe(right, 2),
                GetElementUnsafe(left, 3) * GetElementUnsafe(right, 3));
        }

        /// <summary>
        /// Divides two vectors to compute their quotient.
        /// </summary>
        /// <param name="left">The vector that will be divided by <paramref name="right"/>.</param>
        /// <param name="right">The vector that will divide <paramref name="left"/>.</param>
        /// <returns>The quotient of <paramref name="right"/> and <paramref name="left"/>.</returns>
        public static Vector256<double> Divide(Vector256<double> left, Vector256<double> right)
        {
            if (Avx.IsSupported)
            {
                return Avx.Divide(left, right);
            }

            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.Divide(left.GetLower(), right.GetLower()),
                    Sse2.Divide(left.GetUpper(), right.GetUpper()));
            }

            return Vector256.Create(
                GetElementUnsafe(left, 0) / GetElementUnsafe(right, 0),
                GetElementUnsafe(left, 1) / GetElementUnsafe(right, 1),
                GetElementUnsafe(left, 2) / GetElementUnsafe(right, 2),
                GetElementUnsafe(left, 3) / GetElementUnsafe(right, 3));
        }

        /// <summary>
        /// Computes the sum of all elements in a vector.
        /// </summary>
        /// <param name="vector">The vector whose elements will be summed.</param>
        /// <returns>The sum of all elements in <paramref name="vector"/>.</returns>
        public static double Sum(Vector256<double> vector) => GetElementUnsafe(vector, 0) + GetElementUnsafe(vector, 1) + GetElementUnsafe(vector, 2) + GetElementUnsafe(vector, 3);

        /// <summary>
        /// Computes the minimum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>A vector whose elements are the minimum of the corresponding elements in <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static Vector256<double> Min(Vector256<double> left, Vector256<double> right)
        {
            if (Avx.IsSupported)
            {
                return Avx.Min(left, right);
            }

            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.Min(left.GetLower(), right.GetLower()),
                    Sse2.Min(left.GetUpper(), right.GetUpper()));
            }

            return Vector256.Create(
                Math.Min(GetElementUnsafe(left, 0), GetElementUnsafe(right, 0)),
                Math.Min(GetElementUnsafe(left, 1), GetElementUnsafe(right, 1)),
                Math.Min(GetElementUnsafe(left, 2), GetElementUnsafe(right, 2)),
                Math.Min(GetElementUnsafe(left, 3), GetElementUnsafe(right, 3)));
        }

        /// <summary>
        /// Computes the maximum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>A vector whose elements are the maximum of the corresponding elements in <paramref name="left"/> and <paramref name="right"/>.</returns>
        public static Vector256<double> Max(Vector256<double> left, Vector256<double> right)
        {
            if (Avx.IsSupported)
            {
                return Avx.Max(left, right);
            }

            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.Max(left.GetLower(), right.GetLower()),
                    Sse2.Max(left.GetUpper(), right.GetUpper()));
            }

            return Vector256.Create(
                Math.Max(GetElementUnsafe(left, 0), GetElementUnsafe(right, 0)),
                Math.Max(GetElementUnsafe(left, 1), GetElementUnsafe(right, 1)),
                Math.Max(GetElementUnsafe(left, 2), GetElementUnsafe(right, 2)),
                Math.Max(GetElementUnsafe(left, 3), GetElementUnsafe(right, 3)));
        }

        /// <summary>
        /// Computes the absolute value of each element in a vector.
        /// </summary>
        /// <param name="vector">The vector that will have its absolute value computed.</param>
        /// <returns>A vector whose elements are the absolute value of the elements in <paramref name="vector"/>.</returns>
        public static Vector256<double> Abs(Vector256<double> vector) =>
            Vector256.Create(
                Math.Abs(GetElementUnsafe(vector, 0)),
                Math.Abs(GetElementUnsafe(vector, 1)),
                Math.Abs(GetElementUnsafe(vector, 2)),
                Math.Abs(GetElementUnsafe(vector, 3)));

        /// <summary>
        /// Computes the square root of a vector on a per-element basis.
        /// </summary>
        /// <param name="vector">The vector whose square root is to be computed.</param>
        /// <returns>A vector whose elements are the square root of the corresponding elements in <paramref name="vector"/>.</returns>
        public static Vector256<double> Sqrt(Vector256<double> vector)
        {
            if (Avx.IsSupported)
            {
                return Avx.Sqrt(vector);
            }

            if (Sse2.IsSupported)
            {
                return Vector256.Create(
                    Sse2.Sqrt(vector.GetLower()),
                    Sse2.Sqrt(vector.GetUpper()));
            }

            return Vector256.Create(
                Math.Sqrt(GetElementUnsafe(vector, 0)),
                Math.Sqrt(GetElementUnsafe(vector, 1)),
                Math.Sqrt(GetElementUnsafe(vector, 2)),
                Math.Sqrt(GetElementUnsafe(vector, 4)));
        }
#endif
    }
#endif

#if !NET10_0_OR_GREATER
    extension(System.Numerics.Vector2)
    {
        /// <summary>
        /// Rounds each element in a vector to the nearest integer using the specified rounding mode.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="mode">The mode under which vector should be rounded.</param>
        /// <returns>The result of rounding each element to the nearest integer using <paramref name="mode"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Vector2 Round(System.Numerics.Vector2 vector, MidpointRounding mode)
#if NETCOREAPP3_0_OR_GREATER
            => Vector128.Round(vector.AsVector128(), mode).AsVector2();
#else
            => new((float)Math.Round(vector.X, mode), (float)Math.Round(vector.Y, mode));
#endif
    }

    extension(System.Numerics.Vector3)
    {
        /// <summary>
        /// Rounds each element in a vector to the nearest integer using the specified rounding mode.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="mode">The mode under which vector should be rounded.</param>
        /// <returns>The result of rounding each element to the nearest integer using <paramref name="mode"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Vector3 Round(System.Numerics.Vector3 vector, MidpointRounding mode)
#if NETCOREAPP3_0_OR_GREATER
            => Vector128.Round(vector.AsVector128(), mode).AsVector3();
#else
            => new((float)Math.Round(vector.X, mode), (float)Math.Round(vector.Y, mode), (float)Math.Round(vector.Z, mode));
#endif
    }
#endif

#if NETCOREAPP3_0_OR_GREATER && !NET7_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetElementUnsafe<T>(in Vector128<T> vector, int index)
        where T : struct => Unsafe.Add(ref Unsafe.As<Vector128<T>, T>(ref Unsafe.AsRef(in vector)), index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetElementUnsafe<T>(in Vector256<T> vector, int index)
        where T : struct => Unsafe.Add(ref Unsafe.As<Vector256<T>, T>(ref Unsafe.AsRef(in vector)), index);
#endif
}