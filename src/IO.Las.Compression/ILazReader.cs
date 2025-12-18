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