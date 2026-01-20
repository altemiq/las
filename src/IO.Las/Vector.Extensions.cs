// -----------------------------------------------------------------------
// <copyright file="Vector.Extensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#if NETCOREAPP3_0_OR_GREATER
namespace Altemiq.IO.Las;

#pragma warning disable CA1708, RCS1263, SA1101, S2325

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

/// <summary>
/// Vector extensions.
/// </summary>
public static class Vector
{
    /// <content>
    /// The <see cref="Vector256{Double}"/> extensions.
    /// </content>
    /// <param name="value">The vector to reinterpret.</param>
    extension(Vector256<double> value)
    {
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

    /// <summary>The <see cref="Vector3D"/> extensions.</summary>
    /// <param name="value">The vector to reinterpret.</param>
    extension(Vector3D value)
    {
        /// <summary>
        /// Reinterprets a <see langword="Numerics.Vector3D" /> as a new <see cref="Vector256{Double}" /> with the new elements zeroed.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see langword="Vector256{Double}" /> with the new elements zeroed.</returns>
        public Vector256<double> AsVector256() => value.AsVector256Unsafe().WithElement(3, 0);

        /// <summary>
        /// Reinterprets a <see langword="Numerics.Vector3D" /> as a new <see cref="Vector256{Double}" />, leaving the new elements undefined.
        /// </summary>
        /// <returns>The input reinterpreted as a new <see langword="Vector256{Double}" />.</returns>
        internal Vector256<double> AsVector256Unsafe()
        {
            // This relies on us stripping the "init" flag from the ".locals" declaration to let the upper bits be uninitialized.
            Unsafe.SkipInit(out Vector256<double> result);
            Unsafe.WriteUnaligned(ref Unsafe.As<Vector256<double>, byte>(ref result), value);
            return result;
        }
    }
}
#endif