// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.Encoding.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable CA1708, S2325, SA1101

/// <content>
/// Extension methods for <see cref="System.Text.Encoding"/>.
/// </content>
public static partial class ExtensionMethods
{
    /// <summary>
    /// <see cref="System.Text.Encoding"/> extensions.
    /// </summary>
    extension(System.Text.Encoding encoding)
    {
        /// <summary>
        /// Decodes the bytes up until the first null terminator in the specified byte span into a string.
        /// </summary>
        /// <param name="bytes">A read-only byte span to decode to a Unicode string.</param>
        /// <returns>A string that contains the decoded bytes from the provided read-only span.</returns>
        public string GetNullTerminatedString(ReadOnlySpan<byte> bytes)
        {
#if NET6_0_OR_GREATER
#pragma warning disable S6640
            unsafe
            {
                var ptr = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bytes));
                return encoding.GetString(System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpanFromNullTerminated(ptr));
            }
#pragma warning restore S6640
#else
            var endIndex = bytes.IndexOf((byte)0);
            if (endIndex is -1)
            {
                endIndex = bytes.Length;
            }

            return encoding.GetString(bytes[..endIndex]);
#endif
        }

        /// <summary>
        /// Encodes into a span of bytes a set of characters from the specified read-only span.
        /// </summary>
        /// <param name="chars">The span containing the set of characters to encode.</param>
        /// <param name="bytes">The byte span to hold the encoded bytes.</param>
        public void GetNullTerminatedBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            var length = encoding.GetBytes(chars[..Math.Min(bytes.Length, chars.Length)], bytes);
            bytes[length..].Clear();
        }
    }
}