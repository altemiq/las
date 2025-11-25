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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        {
            Span<byte> buffer = stackalloc byte[sizeof(byte)];
            stream.ReadExactly(buffer);
            return buffer[0];
        }
#else
            => (byte)stream.ReadByte();
#endif

        /// <summary>
        /// Reads an <see cref="sbyte"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public sbyte ReadSByteLittleEndian()
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        {
            Span<byte> buffer = stackalloc byte[sizeof(sbyte)];
            stream.ReadExactly(buffer);
            return (sbyte)buffer[0];
        }
#else
            => (sbyte)stream.ReadByte();
#endif

        /// <summary>
        /// Reads an <see cref="short"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public short ReadInt16LittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(short)];
#else
            var buffer = new byte[sizeof(short)];
#endif
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ushort ReadUInt16LittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
#else
            var buffer = new byte[sizeof(ushort)];
#endif
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public int ReadInt32LittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(int)];
#else
            var buffer = new byte[sizeof(int)];
#endif
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="uint"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public uint ReadUInt32LittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
#else
            var buffer = new byte[sizeof(uint)];
#endif
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="long"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public long ReadInt64LittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(long)];
#else
            var buffer = new byte[sizeof(long)];
#endif
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        /// <summary>
        /// Reads an <see cref="ulong"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ulong ReadUInt64LittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
#else
            var buffer = new byte[sizeof(ulong)];
#endif
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        /// <summary>
        /// Reads a <see cref="float"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public float ReadSingleLittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(float)];
#else
            var buffer = new byte[sizeof(float)];
#endif
            stream.ReadExactly(buffer);
            return System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        /// <summary>
        /// Reads a <see cref="double"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public double ReadDoubleLittleEndian()
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(double)];
#else
            var buffer = new byte[sizeof(double)];
#endif
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
#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(byte)];
            buffer[0] = value;
#else
            var buffer = new[] { value };
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
#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(sbyte)];
            buffer[0] = (byte)value;
#else
            var buffer = new[] { (byte)value };
#endif
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="short"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt16LittleEndian(short value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(short)];
#else
            var buffer = new byte[sizeof(short)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="ushort"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteUInt16LittleEndian(ushort value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
#else
            var buffer = new byte[sizeof(ushort)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="int"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt32LittleEndian(int value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(int)];
#else
            var buffer = new byte[sizeof(int)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="uint"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteUInt32LittleEndian(uint value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
#else
            var buffer = new byte[sizeof(uint)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="long"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt64LittleEndian(long value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(long)];
#else
            var buffer = new byte[sizeof(long)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an <see cref="ulong"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteUInt64LittleEndian(ulong value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
#else
            var buffer = new byte[sizeof(ulong)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="float"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteSingleLittleEndian(float value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(float)];
#else
            var buffer = new byte[sizeof(float)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes a <see cref="double"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteDoubleLittleEndian(double value)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Span<byte> buffer = stackalloc byte[sizeof(double)];
#else
            var buffer = new byte[sizeof(double)];
#endif
            System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            stream.Write(buffer);
        }

        /// <summary>
        /// Writes an array of <see cref="byte"/> into the stream.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        public void Write(byte[] buffer) => stream.Write(buffer, 0, buffer.Length);

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Reads bytes from the current stream and advances the position within the stream until the <paramref name="buffer"/> is filled.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current stream.</param>
        /// <param name="offset">The byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The number of bytes to be read from the current stream.</param>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before reading <paramref name="count"/> number of bytes.</exception>
        public void ReadExactly(byte[] buffer, int offset, int count)
        {
            if (stream.Read(buffer, offset, count) < count)
            {
                throw new EndOfStreamException();
            }
        }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Reads bytes from the current stream and advances the position within the stream until the <paramref name="buffer"/> is filled.
        /// </summary>
        /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the current stream.</param>
        /// <exception cref="EndOfStreamException">The end of the stream is reached before filling the <paramref name="buffer"/>.</exception>
        public void ReadExactly(Span<byte> buffer)
        {
            if (stream.Read(buffer) < buffer.Length)
            {
                throw new EndOfStreamException();
            }
        }
#else
        private void ReadExactly(byte[] buffer) => stream.ReadExactly(buffer, 0, buffer.Length);
#endif
#endif
    }
}