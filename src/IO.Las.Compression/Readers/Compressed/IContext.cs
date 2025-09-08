// -----------------------------------------------------------------------
// <copyright file="IContext.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// Represents a reader with context.
/// </summary>
internal interface IContext
{
    /// <summary>
    /// Initializes the reader with the specified data.
    /// </summary>
    /// <param name="item">The item to initialize from.</param>
    /// <param name="context">The context.</param>
    /// <returns><see langword="true"/> if successfully initialized; otherwise <see langword="false"/>.</returns>
    bool Initialize(ReadOnlySpan<byte> item, ref uint context);

    /// <summary>
    /// Creates the chunk sizes.
    /// </summary>
    /// <returns><see langword="true"/> if successfully initialized; otherwise <see langword="false"/>.</returns>
    bool ChunkSizes();
}