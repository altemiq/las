// -----------------------------------------------------------------------
// <copyright file="StreamExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Extension methods for <see cref="Stream"/>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822: Mark members as static", Justification = "False positive")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required for automated cleanup")]
internal static class StreamExtensions
{
    /// <summary>
    /// <see cref="Stream"/> extensions.
    /// </summary>
    extension(Stream stream)
    {
        /// <summary>
        /// Reads an <see cref="byte"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public byte ReadByteLittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(byte)];
            stream.ReadExactly(buffer);
            return buffer[0];
        }

        /// <summary>
        /// Reads an <see cref="sbyte"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public sbyte ReadSByteLittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(sbyte)];
            stream.ReadExactly(buffer);
            return (sbyte)buffer[0];
        }

        /// <summary>
        /// Reads an <see cref="short"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public short ReadInt16LittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ushort ReadUInt16LittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public int ReadInt32LittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="uint"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public uint ReadUInt32LittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="long"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public long ReadInt64LittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="ulong"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ulong ReadUInt64LittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        /// <summary>
        /// Reads a <see cref="float"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public float ReadSingleLittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        /// <summary>
        /// Reads a <see cref="double"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public double ReadDoubleLittleEndian()
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(buffer);
        }

        /// <summary>
        /// Writes a <see cref="byte"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteByteLittleEndian(byte value)
        {
#if NET7_0_OR_GREATER
            var buffer = new ReadOnlySpan<byte>(value);
#else
            Span<byte> buffer = stackalloc byte[sizeof(byte)];
            buffer[0] = value;
#endif
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="sbyte"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteSByteLittleEndian(sbyte value)
        {
#if NET7_0_OR_GREATER
            var buffer = new ReadOnlySpan<byte>((byte)value);
#else
            Span<byte> buffer = stackalloc byte[sizeof(sbyte)];
            buffer[0] = (byte)value;
#endif
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="short"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt16LittleEndian(short value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="ushort"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteUInt16LittleEndian(ushort value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="int"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt32LittleEndian(int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="uint"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteUInt32LittleEndian(uint value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="long"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt64LittleEndian(long value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="ulong"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteUInt64LittleEndian(ulong value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="float"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteSingleLittleEndian(float value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="double"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteDoubleLittleEndian(double value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Reads bytes from the current stream and advances the position within the stream until the <paramref name="buffer"/> is filled.
        /// </summary>
        /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the current stream.</param>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before filling the <paramref name="buffer"/>.</exception>
        public void ReadExactly(Span<byte> buffer)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            if (stream.Read(buffer) < buffer.Length)
            {
                throw new EndOfStreamException();
            }
#else
            var bufferTemp = new byte[buffer.Length];
            if (stream.Read(bufferTemp, 0, bufferTemp.Length) < bufferTemp.Length)
            {
                throw new EndOfStreamException();
            }

            bufferTemp.AsSpan().CopyTo(buffer);
#endif
        }
#endif
    }
}