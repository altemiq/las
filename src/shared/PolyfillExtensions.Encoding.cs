// -----------------------------------------------------------------------
// <copyright file="PolyfillExtensions.Encoding.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable SA1101

/// <content>
/// Extension methods for <see cref="System.Text.Encoding"/>.
/// </content>
internal static partial class PolyfillExtensions
{
    /// <summary>
    /// <see cref="System.Text.Encoding"/> extensions.
    /// </summary>
    extension(System.Text.Encoding encoding)
    {
        /// <summary>
        /// Decodes all the bytes in the specified byte span into a string.
        /// </summary>
        /// <param name="bytes">A read-only byte span to decode to a Unicode string.</param>
        /// <returns>A string that contains the decoded bytes from the provided read-only span.</returns>
        public string GetString(ReadOnlySpan<byte> bytes)
        {
#pragma warning disable S6640
            unsafe
            {
                var ptr = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bytes));
                return encoding.GetString(ptr, bytes.Length);
            }
#pragma warning restore S6640
        }

        /// <summary>
        /// Calculates the number of bytes produced by encoding the characters in the specified character span.
        /// </summary>
        /// <param name="chars">The span of characters to encode.</param>
        /// <returns>The number of bytes produced by encoding the specified character span.</returns>
        public int GetByteCount(ReadOnlySpan<byte> chars)
        {
#pragma warning disable S6640
            unsafe
            {
                var ptr = (char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(chars));
                return encoding.GetByteCount(ptr, chars.Length);
            }
#pragma warning restore S6640
        }

        /// <summary>
        /// Encodes into a span of bytes a set of characters from the specified read-only span.
        /// </summary>
        /// <param name="chars">The span containing the set of characters to encode.</param>
        /// <param name="bytes">The byte span to hold the encoded bytes.</param>
        /// <returns>The number of encoded bytes.</returns>
        public int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            if (chars.Length is 0)
            {
                return 0;
            }

#pragma warning disable S6640
            unsafe
            {
                var charsPtr = (char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(chars));
                var bytesPtr = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bytes));
                return encoding.GetBytes(charsPtr, chars.Length, bytesPtr, bytes.Length);
            }
#pragma warning restore S6640
        }
    }
}