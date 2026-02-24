// -----------------------------------------------------------------------
// <copyright file="LazReaderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable RCS1263, SA1101

/// <summary>
/// <see cref="ILazReader"/> extensions.
/// </summary>
internal static class LazReaderExtensions
{
    /// <content>
    /// <see cref="ILazReader"/> extensions.
    /// </content>
    /// <param name="reader">The reader.</param>
    extension(ILazReader reader)
    {
        /// <inheritdoc cref="ILazReader.ReadChunk()" />
        public ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk() => reader.ReadChunk();

        /// <inheritdoc cref="ILazReader.ReadChunk(int)" />
        public ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk(int chunk) => reader.ReadChunk(chunk);

        /// <inheritdoc cref="ILazReader.ReadChunkAsync()" />
        public ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync() => reader.ReadChunkAsync();

        /// <inheritdoc cref="ILazReader.ReadChunkAsync(int)" />
        public ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync(int chunk) => reader.ReadChunkAsync(chunk);
    }
}