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
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(byte), out var cacheBuffer))
            {
                return cacheBuffer[0];
            }

            Span<byte> buffer = stackalloc byte[sizeof(byte)];
            var bytesRead = stream.Read(buffer);
            if (bytesRead is sizeof(byte))
            {
                return buffer[0];
            }

            throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads an <see cref="sbyte"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public sbyte ReadSByteLittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(sbyte), out var cacheBuffer))
            {
                return (sbyte)cacheBuffer[0];
            }

            Span<byte> buffer = stackalloc byte[sizeof(sbyte)];
            return stream.Read(buffer) is sizeof(sbyte) ? (sbyte)buffer[0] : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads an <see cref="short"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public short ReadInt16LittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(short), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(short)];
            return stream.Read(buffer) is sizeof(short)
                ? System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(buffer)
                : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ushort ReadUInt16LittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(ushort), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            return stream.Read(buffer) is sizeof(ushort)
                ? System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(buffer)
                : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public int ReadInt32LittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(int), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(int)];
            return stream.Read(buffer) is sizeof(int)
                ? System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(buffer)
                : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads an <see cref="uint"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public uint ReadUInt32LittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(uint), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            return stream.Read(buffer) is sizeof(uint)
                ? System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(buffer)
                : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads an <see cref="long"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public long ReadInt64LittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(long), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(long)];
            return stream.Read(buffer) is sizeof(long)
                ? System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(buffer)
                : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads an <see cref="ulong"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ulong ReadUInt64LittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(ulong), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            return stream.Read(buffer) is sizeof(ulong)
                ? System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(buffer)
                : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads a <see cref="float"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public float ReadSingleLittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(float), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(float)];
            return stream.Read(buffer) is sizeof(float)
                ? System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(buffer)
                : throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads a <see cref="double"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public double ReadDoubleLittleEndian()
        {
            if (stream is CachedStream cachedStream && cachedStream.TryGetSpan(sizeof(double), out var cacheBuffer))
            {
                return System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(cacheBuffer);
            }

            Span<byte> buffer = stackalloc byte[sizeof(double)];
            return stream.Read(buffer) is sizeof(double)
                ? System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(buffer)
                : throw new EndOfStreamException();
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
    }
}