// -----------------------------------------------------------------------
// <copyright file="IStreamProvider.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Stream"/> provider.
/// </summary>
internal interface IStreamProvider
{
    /// <summary>
    /// Gets a value indicating whether this instance can read.
    /// </summary>
    bool CanRead { get; }

    /// <summary>
    /// Gets a value indicating whether this instance can write.
    /// </summary>
    bool CanWrite { get; }

    /// <summary>
    /// Tests whether the specified URI can be processed by this instance.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> exists.</returns>
    bool IsValid(Uri uri);

    /// <summary>
    /// Tests whether the specified URI exists.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> exists.</returns>
    bool Exists(Uri uri);

    /// <summary>
    /// Tests whether the specified URI exists asynchronously.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if <paramref name="uri"/> exists.</returns>
    ValueTask<bool> ExistsAsync(Uri uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the specified URI for reading.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns>The <see cref="Stream"/> from <paramref name="uri"/>.</returns>
    Stream OpenRead(Uri uri);

    /// <summary>
    /// Opens the specified URI for reading asynchronously.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="Stream"/> from <paramref name="uri"/>.</returns>
    ValueTask<Stream> OpenReadAsync(Uri uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the specified URI for writing.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <returns>The <see cref="Stream"/> from <paramref name="uri"/>.</returns>
    Stream OpenWrite(Uri uri);

    /// <summary>
    /// Opens the specified URI for writing asynchronously.
    /// </summary>
    /// <param name="uri">The URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="Stream"/> from <paramref name="uri"/>.</returns>
    ValueTask<Stream> OpenWriteAsync(Uri uri, CancellationToken cancellationToken = default);
}