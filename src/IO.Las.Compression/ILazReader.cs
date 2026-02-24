// -----------------------------------------------------------------------
// <copyright file="ILazReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a LAZ file reader.
/// </summary>
public interface ILazReader
{
    /// <summary>
    /// Gets a value indicating whether this instance is compressed.
    /// </summary>
    bool IsCompressed { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is chunked.
    /// </summary>
    internal bool IsChunked { get; }

    /// <summary>
    /// Reads the rest of the chunk as an enumerable.
    /// </summary>
    /// <returns>The enumerable.</returns>
    internal ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk();

    /// <summary>
    /// Reads the rest of the specified chunk as an enumerable.
    /// </summary>
    /// <param name="chunk">The chunk to read.</param>
    /// <returns>The enumerable.</returns>
    internal ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk(int chunk);

    /// <summary>
    /// Reads the rest of the chunk as an asynchronous enumerable.
    /// </summary>
    /// <returns>The asynchronous enumerable.</returns>
    internal ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync();

    /// <summary>
    /// Reads the rest of the specified chunk as an asynchronous enumerable.
    /// </summary>
    /// <param name="chunk">The chunk to read.</param>
    /// <returns>The asynchronous enumerable.</returns>
    internal ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync(int chunk);

    /// <summary>
    /// Moves to the specified chunk.
    /// </summary>
    /// <param name="index">The chunk index.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal bool MoveToChunk(int index);

    /// <summary>
    /// Moves to the specified chunk start.
    /// </summary>
    /// <param name="chunkStart">The chunk start.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal bool MoveToChunk(long chunkStart);

    /// <summary>
    /// Moves to the specified chunk.
    /// </summary>
    /// <param name="index">The chunk index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal ValueTask<bool> MoveToChunkAsync(int index, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves to the specified chunk start.
    /// </summary>
    /// <param name="chunkStart">The chunk start.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the move was successful; otherwise <see langword="false"/>.</returns>
    internal ValueTask<bool> MoveToChunkAsync(long chunkStart, CancellationToken cancellationToken = default);
}