// -----------------------------------------------------------------------
// <copyright file="BitManipulation.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Bit manipulation methods.
/// </summary>
internal static class BitManipulation
{
    /// <summary>
    /// Gets the byte value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The bit mask.</param>
    /// <returns>The byte value.</returns>
    public static byte Get(byte value, byte mask) => (byte)(value & mask);

    /// <summary>
    /// Gets the byte value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The bit mask.</param>
    /// <param name="position">The position.</param>
    /// <returns>The byte value.</returns>
    public static byte Get(byte value, byte mask, int position) => (byte)((value & mask) >> position);

    /// <summary>
    /// Gets the byte value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The bit mask.</param>
    /// <returns>The byte value.</returns>
    public static bool IsSet(byte value, byte mask) => Get(value, mask) == mask;

    /// <summary>
    /// Sets the bit.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="mask">The mask.</param>
    /// <param name="set">Set to <see langword="true"/> to set the bit.</param>
    /// <returns>The byte with the mask applied.</returns>
    public static byte Apply(byte value, byte mask, bool set) => set ? Set(value, mask) : Clear(value, mask);

    /// <summary>
    /// Sets the bits.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="source">The source.</param>
    /// <param name="mask">The mask.</param>
    /// <returns>The byte with the bits from <paramref name="source"/> set.</returns>
    public static byte Set(byte value, byte source, byte mask) => Set(Clear(value, mask), (byte)(source & mask));

    /// <summary>
    /// Sets the bits.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="source">The source.</param>
    /// <param name="mask">The mask.</param>
    /// <param name="position">The position at which to set the bits.</param>
    /// <returns>The byte with the bits from <paramref name="source"/> set at <paramref name="position"/>.</returns>
    public static byte Set(byte value, byte source, byte mask, int position) => Set(Clear(value, mask), (byte)((source << position) & mask));

    /// <summary>
    /// Reads a <see cref="double"/> from the beginning of a read-only span of bytes, as little endian.
    /// </summary>
    /// <param name="source">The read-only span to read.</param>
    /// <returns>The little endian value.</returns>
    public static double ReadDoubleLittleEndian(ReadOnlySpan<byte> source) =>
#if NET5_0_OR_GREATER
        System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(source);
#else
        BitConverter.Int64BitsToDouble(System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(source));
#endif

    /// <summary>
    /// Reads a <see cref="float"/> from the beginning of a read-only span of bytes, as little endian.
    /// </summary>
    /// <param name="source">The read-only span to read.</param>
    /// <returns>The little endian value.</returns>
    public static float ReadSingleLittleEndian(ReadOnlySpan<byte> source)
#if NET5_0_OR_GREATER
        => System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source);
#elif NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
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
    public static void WriteDoubleLittleEndian(Span<byte> destination, double value) =>
#if NET5_0_OR_GREATER
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(destination, value);
#else
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(destination, BitConverter.DoubleToInt64Bits(value));
#endif

    /// <summary>
    /// Writes a <see cref="float"/> into a span of bytes, as little endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
    /// <param name="value">The value to write into the span of bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is too small to contain a <see cref="float"/>.</exception>
    /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
    public static void WriteSingleLittleEndian(Span<byte> destination, float value)
#if NET5_0_OR_GREATER
         => System.Buffers.Binary.BinaryPrimitives.WriteSingleLittleEndian(destination, value);
#elif NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
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

    private static byte Set(byte value, byte mask) => (byte)(value | mask);

    private static byte Clear(byte value, byte mask) => (byte)(value & (~mask));
}