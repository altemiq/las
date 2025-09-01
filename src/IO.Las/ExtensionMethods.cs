// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable SA1201
/// <summary>
/// Extension methods.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// Moves to <paramref name="stream"/> to the specified <paramref name="position"/>.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="position">The position.</param>
    internal static void MoveToPositionForwardsOnly(this Stream stream, long position)
    {
        var streamPosition = stream.Position;
        if (streamPosition < position)
        {
            MoveForward(stream, streamPosition, position);
        }
    }

    private static void MoveForward(Stream stream, long baseStreamPosition, long position)
    {
        if (stream.CanSeek)
        {
            stream.Position = position;
        }
        else
        {
            var delta = position - baseStreamPosition;
            while (delta > 0)
            {
                _ = stream.ReadByte();
                delta--;
            }
        }
    }

#pragma warning disable SA1101, SA1137, SA1400
    /// <summary>
    /// <see cref="System.Text.Encoding"/> extensions.
    /// </summary>
    extension(System.Text.Encoding encoding)
    {
#pragma warning disable S2325
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
        /// Calculates the number of bytes produced by encoding the characters in the specified character span..
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
            var charsPtr = (char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(chars));
            var bytesPtr = (byte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bytes));
            return encoding.GetBytes(charsPtr,  chars.Length, bytesPtr, bytes.Length);
        }
#endif
#pragma warning restore S2325
    }

#if !NET5_0_OR_GREATER
    /// <summary>
    /// <see cref="System.Buffers.Binary.BinaryPrimitives"/> extensions.
    /// </summary>
    extension(System.Buffers.Binary.BinaryPrimitives)
    {
        /// <summary>
        /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        public static double ReadDoubleLittleEndian(ReadOnlySpan<byte> source) => BitConverter.Int64BitsToDouble(System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(source));

        /// <summary>
        /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as little endian.
        /// </summary>
        /// <param name="source">The read-only span to read.</param>
        /// <returns>The little endian value.</returns>
        public static float ReadSingleLittleEndian(ReadOnlySpan<byte> source)
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            => BitConverter.Int32BitsToSingle(System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source));
#else
        {
            return ToSingle(source);

            static unsafe float ToSingle(ReadOnlySpan<byte> source)
            {
                var tmpBuffer = (uint)(source[0] | (source[1] << 8) | (source[2] << 16) | (source[3] << 24));
                return *(float*)&tmpBuffer;
            }
        }
#endif

        /// <summary>
        /// Writes a <see cref="double"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="double"/>.</exception>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        public static void WriteDoubleLittleEndian(Span<byte> destination, double value) => System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(destination, BitConverter.DoubleToInt64Bits(value));

        /// <summary>
        /// Writes a <see cref="float"/> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="float"/>.</exception>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        public static void WriteSingleLittleEndian(Span<byte> destination, float value)
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            => System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(destination, BitConverter.SingleToInt32Bits(value));
#else
    {
        CopyTo(value, destination);
        static unsafe void CopyTo(float value, Span<byte> bytes)
        {
            var tmp = *(uint*)&value;
            bytes[0] = (byte)tmp;
            bytes[1] = (byte)(tmp >> 8);
            bytes[2] = (byte)(tmp >> 16);
            bytes[3] = (byte)(tmp >> 24);
        }
    }
#endif
    }
#endif
#pragma warning restore SA1101, SA1137, SA1201, SA1400
}
#pragma warning restore SA1201