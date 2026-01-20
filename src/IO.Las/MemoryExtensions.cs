// -----------------------------------------------------------------------
// <copyright file="MemoryExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// Memory extensions.
/// </summary>
internal static class MemoryExtensions
{
    extension(System.Runtime.InteropServices.Marshal)
    {
        /// <summary>
        /// Marshals data from a block of memory to a newly allocated managed object of the type specified by a generic type parameter.
        /// </summary>
        /// <typeparam name="T">The type of the object to which the data is to be copied. This must be a formatted class or a structure.</typeparam>
        /// <param name="span">The span of memory.</param>
        /// <returns>A managed object that contains the data that the <paramref name="span"/> parameter points to.</returns>
        public static T? SpanToStructure<[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>(ReadOnlySpan<byte> span) =>
            System.Runtime.InteropServices.Marshal.PtrToStructure<T>(span.GetIntPtr());

        /// <summary>
        /// Marshals data from a managed object of a specified type to a block of memory.
        /// </summary>
        public static void StructureToSpan<T>([System.Diagnostics.CodeAnalysis.DisallowNull] T structure, Span<byte> span, bool fDeleteOld = false) =>
            System.Runtime.InteropServices.Marshal.StructureToPtr(structure, span.GetIntPtr(), fDeleteOld);
    }

    /// <content>
    /// <see cref="Span{T}"/> extensions.
    /// </content>
    /// <param name="span">The span to operate on.</param>
    extension<T>(Span<T> span)
    {
        private unsafe IntPtr GetIntPtr() => (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(span));
    }

    /// <content>
    /// <see cref="ReadOnlySpan{T}"/> extensions.
    /// </content>
    /// <param name="span">The span to operate on.</param>
    extension<T>(ReadOnlySpan<T> span)
    {
        private unsafe IntPtr GetIntPtr() => (IntPtr)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(span));
    }
}