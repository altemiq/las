// -----------------------------------------------------------------------
// <copyright file="Vector3D.Extensions.cs" company="Altemiq">
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
/// <see cref="Vector3D"/> extensions.
/// </summary>
public static partial class Vector
{
    /// <summary>The <see cref="Vector3D"/> extensions.</summary>
    /// <param name="value">The vector to reinterpret.</param>
    extension(Vector3D value)
    {
        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector2D" />.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector2D" />.</returns>
        public Vector2D AsVector2D() =>
#if NETCOREAPP3_0_OR_GREATER
            value.AsVector256().AsVector2D();
#else
            new(value.X, value.Y);
#endif

#if NETCOREAPP3_0_OR_GREATER
        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector256{Double}" /> with the new elements zeroed.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see cref="Vector256{Double}" /> with the new elements zeroed.</returns>
        public Vector256<double> AsVector256() => value.AsVector256Unsafe().WithElement(3, 0);

        /// <summary>
        /// Reinterprets a <see cref="Vector3D" /> as a new <see cref="Vector256{Double}" /> with the new elements set to <paramref name="v"/>.
        /// </summary>
        /// <param name="v">The value of the final element.</param>
        /// <returns>The input reinterpreted as a new <see cref="Vector256{Double}" /> with the new elements set to <paramref name="v"/>.</returns>
        public Vector256<double> AsVector256(double v) => value.AsVector256Unsafe().WithElement(3, v);

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