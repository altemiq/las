// -----------------------------------------------------------------------
// <copyright file="Vector2D.Exensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable CA1708, RCS1263, SA1101, S2325

#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
#endif

/// <summary>
/// <see cref="Vector2D"/> extensions.
/// </summary>
public static partial class Vector
{
    /// <summary>The <see cref="Vector3D"/> extensions.</summary>
    /// <param name="value">The vector to reinterpret.</param>
    extension(Vector2D value)
    {
        /// <summary>
        /// Reinterprets a <see cref="Vector2D" /> as a new <see cref="Vector3D" />.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector3D" />.</returns>
        public Vector3D AsVector3D() =>
#if NETCOREAPP3_0_OR_GREATER
            value.AsVector256().AsVector3D();
#else
            new(value.X, value.Y, default);
#endif

#if NETCOREAPP3_0_OR_GREATER
        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector128" /> with the new elements zeroed.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector128{Double}" /> with the new elements zeroed.</returns>
        public Vector128<double> AsVector128() => value.AsVector128Unsafe();

        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector256{Double}" /> with the new elements zeroed.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector256{Double}" /> with the new elements zeroed.</returns>
        public Vector256<double> AsVector256() => value.AsVector256Unsafe().WithElement(2, 0).WithElement(3, 0);

        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector256{Double}" /> with the new elements set to <paramref name="v"/>.
        /// </summary>
        /// <param name="v">The value of the final element.</param>
        /// <returns>The input reinterpreted as a new <see cref="Vector256{Double}" /> with the new elements set to <paramref name="v"/>.</returns>
        public Vector256<double> AsVector256(double v) => value.AsVector256Unsafe().WithElement(2, v).WithElement(3, v);

        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector128{Double}" />, leaving the new elements undefined.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector128{Double}" />.</returns>
        internal Vector128<double> AsVector128Unsafe()
        {
            // This relies on us stripping the "init" flag from the ".locals" declaration to let the upper bits be uninitialized.
            Unsafe.SkipInit(out Vector128<double> result);
            Unsafe.WriteUnaligned(ref Unsafe.As<Vector128<double>, byte>(ref result), value);
            return result;
        }

        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector256{Double}" />, leaving the new elements undefined.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector256{Double}" />.</returns>
        internal Vector256<double> AsVector256Unsafe()
        {
            // This relies on us stripping the "init" flag from the ".locals" declaration to let the upper bits be uninitialized.
            Unsafe.SkipInit(out Vector256<double> result);
            Unsafe.WriteUnaligned(ref Unsafe.As<Vector256<double>, byte>(ref result), value);
            return result;
        }
#endif
    }
}