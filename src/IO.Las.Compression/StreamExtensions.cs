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
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(byte), out var cacheBuffer) => cacheBuffer[0],
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(byte), out var memoryBuffer) => memoryBuffer[0],
                _ => ReadByteLittleEndianCore(stream),
            };

            static byte ReadByteLittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(byte)];
                return stream.Read(buffer) is sizeof(byte) ? buffer[0] : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads an <see cref="sbyte"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public sbyte ReadSByteLittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(sbyte), out var cacheBuffer) => (sbyte)cacheBuffer[0],
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(sbyte), out var memoryBuffer) => (sbyte)memoryBuffer[0],
                _ => ReadSByteLittleEndianCore(stream),
            };

            static sbyte ReadSByteLittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(sbyte)];
                return stream.Read(buffer) is sizeof(sbyte)
                    ? (sbyte)buffer[0]
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads an <see cref="short"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public short ReadInt16LittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(short), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(short), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(memoryBuffer),
                _ => ReadInt16LittleEndianCore(stream),
            };

            static short ReadInt16LittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(short)];
                return stream.Read(buffer) is sizeof(short)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ushort ReadUInt16LittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(ushort), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(ushort), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(memoryBuffer),
                _ => ReadUInt16LittleEndianCore(stream),
            };

            static ushort ReadUInt16LittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(ushort)];
                return stream.Read(buffer) is sizeof(ushort)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads an <see cref="int"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public int ReadInt32LittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(int), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(int), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(memoryBuffer),
                _ => ReadInt32LittleEndianCore(stream),
            };

            static int ReadInt32LittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(int)];
                return stream.Read(buffer) is sizeof(int)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads an <see cref="uint"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public uint ReadUInt32LittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(uint), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(uint), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(memoryBuffer),
                _ => ReadUInt32LittleEndianCore(stream),
            };

            static uint ReadUInt32LittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(uint)];
                return stream.Read(buffer) is sizeof(uint)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads an <see cref="long"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public long ReadInt64LittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(long), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(long), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(memoryBuffer),
                _ => ReadInt64LittleEndianCore(stream),
            };

            static long ReadInt64LittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(long)];
                return stream.Read(buffer) is sizeof(long)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads an <see cref="ulong"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public ulong ReadUInt64LittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(ulong), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(ulong), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(memoryBuffer),
                _ => ReadUInt64LittleEndianCore(stream),
            };

            static ulong ReadUInt64LittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(ulong)];
                return stream.Read(buffer) is sizeof(ulong)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads a <see cref="float"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public float ReadSingleLittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(float), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(float), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(memoryBuffer),
                _ => ReadSingleLittleEndianCore(stream),
            };

            static float ReadSingleLittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(float)];
                return stream.Read(buffer) is sizeof(float)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Reads a <see cref="double"/> from the stream, as little endian.
        /// </summary>
        /// <returns>The little endian value.</returns>
        public double ReadDoubleLittleEndian()
        {
            return stream switch
            {
                CachedStream cachedStream when cachedStream.TryGetSpan(sizeof(double), out var cacheBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(cacheBuffer),
                MemoryStream memoryStream when memoryStream.TryGetSpan(sizeof(double), out var memoryBuffer) => System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(memoryBuffer),
                _ => ReadDoubleLittleEndianCore(stream),
            };

            static double ReadDoubleLittleEndianCore(Stream stream)
            {
                Span<byte> buffer = stackalloc byte[sizeof(double)];
                return stream.Read(buffer) is sizeof(double)
                    ? System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(buffer)
                    : throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Writes a <see cref="byte"/> into the stream, as little endian.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteByteLittleEndian(byte value)
        {
#if NET8_0_OR_GREATER
            var buffer = new ReadOnlySpan<byte>(in value);
#elif NET7_0_OR_GREATER
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
#if NET8_0_OR_GREATER
            var byteValue = (byte)value;
            var buffer = new ReadOnlySpan<byte>(in byteValue);
#elif NET7_0_OR_GREATER
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

    extension(MemoryStream memoryStream)
    {
        /// <summary>
        /// Tries to read the span of the required length.
        /// </summary>
        /// <param name="length">The required length.</param>
        /// <param name="output">The output span.</param>
        /// <returns><see langword="true"/> if the span was read correctly; otherwise <see langword="false"/>.</returns>
        /// <remarks>When the return if <see langword="true"/> the stream has been moved forward by <paramref name="length"/>.</remarks>
        public bool TryGetSpan(int length, out ReadOnlySpan<byte> output)
        {
#if NET8_0_OR_GREATER
            var position = memoryStream.Position;
            if (position + length < memoryStream.Length)
            {
                output = InternalReadSpan(memoryStream, length);
                return true;

                [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = nameof(InternalReadSpan))]
                static extern ReadOnlySpan<byte> InternalReadSpan(MemoryStream stream, int count);
            }
#else
            if (memoryStream.TryGetBuffer(out var memoryBuffer))
            {
                var position = (int)memoryStream.Position;
                if (position + length < memoryBuffer.Count)
                {
                    output = memoryBuffer.AsSpan(position, length);
                    memoryStream.Position += length;
                    return true;
                }
            }
#endif

            output = default;
            return false;
        }

        /// <summary>
        /// Retargets <paramref name="memoryStream"/> at a new window of
        /// <paramref name="buffer"/> in-place, without allocating a new
        /// <see cref="MemoryStream"/> instance.
        /// </summary>
        /// <param name="buffer">The new backing buffer.</param>
        /// <param name="index">The start offset within <paramref name="buffer"/>.</param>
        /// <param name="count">The length of the window.</param>
        /// <remarks>
        /// <para>
        /// <see cref="MemoryStream"/> has no public API to change the backing
        /// buffer after construction: the constructor parameters are all
        /// stored in private (one of which, <c>_origin</c>, is
        /// <see langword="readonly"/>). This extension reaches into those
        /// fields using <c>System.Runtime.CompilerServices.UnsafeAccessor</c>
        /// on net8+, and cached <see cref="System.Reflection.FieldInfo"/>
        /// on older TFMs (which can still assign readonly fields).
        /// </para>
        /// <para>
        /// The result is equivalent to constructing
        /// <c>new MemoryStream(buffer, index, count, writable: false, publiclyVisible: true)</c>
        /// but reuses the existing instance, so hot paths that re-initialise a
        /// stream per chunk allocate nothing. The caller is responsible for
        /// ensuring <paramref name="memoryStream"/> was originally constructed
        /// as a non-expandable, non-writable byte-array-backed stream (the
        /// typical shape produced by the
        /// <c>new MemoryStream(byte[], int, int, bool, bool)</c> constructor).
        /// </para>
        /// </remarks>
        public void SetBuffer(byte[] buffer, int index, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            if (buffer.Length - index < count)
            {
                throw new ArgumentException(Compression.Properties.Resources.OffsetLengthOutOfBounds, nameof(buffer));
            }

#if NET8_0_OR_GREATER
            BufferRef(memoryStream) = buffer;
            OriginRef(memoryStream) = index;
            PositionRef(memoryStream) = index;
            LengthRef(memoryStream) = index + count;
            CapacityRef(memoryStream) = index + count;

            [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_buffer")]
            static extern ref byte[] BufferRef(MemoryStream stream);

            [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_origin")]
            static extern ref int OriginRef(MemoryStream stream);

            [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_position")]
            static extern ref int PositionRef(MemoryStream stream);

            [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_length")]
            static extern ref int LengthRef(MemoryStream stream);

            [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = "_capacity")]
            static extern ref int CapacityRef(MemoryStream stream);
#else
            MemoryStreamFields.Buffer.SetValue(memoryStream, buffer);
            MemoryStreamFields.Origin.SetValue(memoryStream, index);
            MemoryStreamFields.Position.SetValue(memoryStream, index);
            MemoryStreamFields.Length.SetValue(memoryStream, index + count);
            MemoryStreamFields.Capacity.SetValue(memoryStream, index + count);
#endif
        }
    }

#if !NET8_0_OR_GREATER
    /// <summary>
    /// Cached <see cref="System.Reflection.FieldInfo"/> handles for the
    /// private <see cref="MemoryStream"/> backing fields. Used by the
    /// pre-net8 implementation of <c>SetBuffer</c>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Deliberate: MemoryStream has no public API to swap its backing buffer; reflection is the documented fallback for pre-net8 TFMs that lack UnsafeAccessor.")]
    private static class MemoryStreamFields
    {
        public static readonly System.Reflection.FieldInfo Buffer = GetField("_buffer");
        public static readonly System.Reflection.FieldInfo Origin = GetField("_origin");
        public static readonly System.Reflection.FieldInfo Position = GetField("_position");
        public static readonly System.Reflection.FieldInfo Length = GetField("_length");
        public static readonly System.Reflection.FieldInfo Capacity = GetField("_capacity");

        private static System.Reflection.FieldInfo GetField(string name) =>
            typeof(MemoryStream).GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?? throw new MissingFieldException(nameof(MemoryStream), name);
    }
#endif
}