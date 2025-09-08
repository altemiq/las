// -----------------------------------------------------------------------
// <copyright file="LazExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// LAZ extensions.
/// </summary>
public static class LazExtensions
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="HeaderBlock"/> is compressed.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="header"/> indicates that the file is compressed; otherwise <see langword="false"/>.</returns>
    public static bool IsCompressed(this in HeaderBlock header) => IsCompressed(header.PointDataFormat);

    /// <summary>
    /// Gets a value indicating whether this <see cref="HeaderBlockBuilder"/> is compressed.
    /// </summary>
    /// <param name="builder">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="builder"/> indicates that the file is compressed; otherwise <see langword="false"/>.</returns>
    public static bool IsCompressed(this HeaderBlockBuilder builder) => IsCompressed(builder.PointDataFormat);

    /// <summary>
    /// Gets a value indicating whether this <see cref="byte"/> represents a compressed point data format.
    /// </summary>
    /// <param name="pointDataFormat">The point data format byte.</param>
    /// <returns><see langword="true"/> if <paramref name="pointDataFormat"/> indicates that the file is compressed; otherwise <see langword="false"/>.</returns>
    public static bool IsCompressed(this byte pointDataFormat) => (((pointDataFormat & 0x80) >> 7) is not 0) && (((pointDataFormat & 0x40) >> 6) is 0);

    /// <summary>
    /// Sets the compressed indicator in the specified header.
    /// </summary>
    /// <param name="builder">The header builder.</param>
    public static void SetCompressed(this HeaderBlockBuilder builder) => builder.PointDataFormat = SetCompressed(builder.PointDataFormat);

    private static byte SetCompressed(byte pointDataFormat)
    {
        BitManipulation.Apply(ref pointDataFormat, Constants.BitMasks.Mask6, set: false);
        BitManipulation.Apply(ref pointDataFormat, Constants.BitMasks.Mask7, set: true);
        return pointDataFormat;
    }
}