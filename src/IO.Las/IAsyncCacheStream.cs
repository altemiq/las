// -----------------------------------------------------------------------
// <copyright file="IAsyncCacheStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Interface for asynchronously caching a specific part in a stream.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is an extension for streams.")]
public interface IAsyncCacheStream
{
    /// <summary>
    /// Cashes the stream for reading using the specified start.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask CacheAsync(long start, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cashes the stream for reading using the specified start and length.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="length">The number of bytes to cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask CacheAsync(long start, int length, CancellationToken cancellationToken = default);
}