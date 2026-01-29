// -----------------------------------------------------------------------
// <copyright file="Vector.cs" company="Altemiq">
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
public static partial class Vector
{
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
}
#endif