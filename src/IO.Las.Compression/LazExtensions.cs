// -----------------------------------------------------------------------
// <copyright file="LazExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable S2325, SA1101

/// <summary>
/// LAZ extensions.
/// </summary>
public static class LazExtensions
{
    /// <summary>
    /// The <see cref="ILasReader"/> extensions.
    /// </summary>
    extension(ILasReader reader)
    {
#if LAS1_4_OR_GREATER
        /// <summary>
        /// Copies the contents the current reader to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void CopyTo(ILasWriter writer) =>
            reader.CopyTo(writer, static vlr => !vlr.IsForCompression(), static evlr => !evlr.IsForCompression());

        /// <summary>
        /// Copies the contents the current reader to the specified writer asynchronously.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cancellationToken">The token for cancelling the task.</param>
        /// <returns>The asynchronous task for copying the contents to <paramref name="writer"/>.</returns>
        public Task CopyToAsync(ILasWriter writer, CancellationToken cancellationToken = default) =>
            reader.CopyToAsync(writer, static vlr => !vlr.IsForCompression(), static evlr => !evlr.IsForCompression(), cancellationToken);
#else
        /// <summary>
        /// Copies the contents the current reader to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void CopyTo(ILasWriter writer) =>
            reader.CopyTo(writer, static vlr => !vlr.IsForCompression());

        /// <summary>
        /// Copies the contents the current reader to the specified writer asynchronously.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="cancellationToken">The token for cancelling the task.</param>
        /// <returns>The asynchronous task for copying the contents to <paramref name="writer"/>.</returns>
        public Task CopyToAsync(ILasWriter writer, CancellationToken cancellationToken = default) =>
            reader.CopyToAsync(writer, static vlr => !vlr.IsForCompression(), cancellationToken);
#endif
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="HeaderBlock"/> is compressed.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="header"/> indicates that the file is compressed; otherwise <see langword="false"/>.</returns>
    public static bool IsCompressed(this in HeaderBlock header) => header.PointDataFormat.IsCompressed();

    /// <summary>
    /// Gets a value indicating whether this <see cref="HeaderBlockBuilder"/> is compressed.
    /// </summary>
    /// <param name="builder">The header.</param>
    /// <returns><see langword="true"/> if <paramref name="builder"/> indicates that the file is compressed; otherwise <see langword="false"/>.</returns>
    public static bool IsCompressed(this HeaderBlockBuilder builder) => builder.PointDataFormat.IsCompressed();

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