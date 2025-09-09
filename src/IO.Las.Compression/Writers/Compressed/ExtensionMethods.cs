// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

using System.Runtime.CompilerServices;

/// <summary>
/// Extension methods.
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Copies a certain amount of bytes from a stream to a writer.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="byteCount">The bytes count.</param>
    public static void CopyToStream(this Stream source, Stream destination, int byteCount)
    {
        var position = source.Position;

        source.Position = 0;
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(ArithmeticCoder.BufferSize);
        var bytesLeft = byteCount;

        while (bytesLeft > 0)
        {
            var bytesToRead = bytesLeft > buffer.Length ? buffer.Length : bytesLeft;
            var bytesRead = source.Read(buffer, 0, bytesToRead);

            destination.Write(buffer, 0, bytesRead);

            bytesLeft -= bytesRead;
        }

        source.Position = position;

        System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
    }

    /// <summary>
    /// Quantize the specified float to int.
    /// </summary>
    /// <param name="n">The value to quantize.</param>
    /// <returns>The quantized value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Quantize(this float n) => (n >= 0) ? (int)(n + 0.5F) : (int)(n - 0.5F);

    /// <summary>
    /// Gets a value indicating whether the specified value is an <see cref="int"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> can be represented as an <see cref="int"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInt32(this long value)
    {
        const long MaxInt32Value = int.MaxValue;
        const long MinInt32Value = int.MinValue;

        return value is <= MaxInt32Value and >= MinInt32Value;
    }
}