// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.Encoding.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// Extension methods for <see cref="System.Text.Encoding"/>.
/// </content>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822: Mark members as static", Justification = "False positive")]
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
            var endIndex = bytes.IndexOf((byte)0);
            if (endIndex is -1)
            {
                endIndex = bytes.Length;
            }

            return encoding.GetString(bytes[..endIndex]);
        }

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Decodes all the bytes in the specified byte span into a string.
        /// </summary>
        /// <param name="bytes">A read-only byte span to decode to a Unicode string.</param>
        /// <returns>A string that contains the decoded bytes from the provided read-only span.</returns>
        public unsafe string GetString(ReadOnlySpan<byte> bytes)
        {
            var ptr = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bytes));
            return encoding.GetString(ptr, bytes.Length);
        }

        /// <summary>
        /// Calculates the number of bytes produced by encoding the characters in the specified character span.
        /// </summary>
        /// <param name="chars">The span of characters to encode.</param>
        /// <returns>The number of bytes produced by encoding the specified character span.</returns>
        public unsafe int GetByteCount(ReadOnlySpan<byte> chars)
        {
            var charsPtr = (char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(chars));
            return encoding.GetByteCount(charsPtr, chars.Length);
        }

        /// <summary>
        /// Encodes into a span of bytes a set of characters from the specified read-only span.
        /// </summary>
        /// <param name="chars">The span containing the set of characters to encode.</param>
        /// <param name="bytes">The byte span to hold the encoded bytes.</param>
        /// <returns>The number of encoded bytes.</returns>
        public unsafe int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            if (chars.Length is 0)
            {
                return 0;
            }

            var charsPtr = (char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(chars));
            var bytesPtr = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bytes));
            return encoding.GetBytes(charsPtr, chars.Length, bytesPtr, bytes.Length);
        }
#endif
    }
}