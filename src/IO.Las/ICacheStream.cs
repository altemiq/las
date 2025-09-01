// -----------------------------------------------------------------------
// <copyright file="ICacheStream.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Interface for caching a specific part in a stream.
/// </summary>
internal interface ICacheStream
{
    /// <summary>
    /// Cashes the stream for reading using the specified start.
    /// </summary>
    /// <param name="start">The start index.</param>
    void Cache(long start);

    /// <summary>
    /// Cashes the stream for reading using the specified start and length.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="length">The number of bytes to cache.</param>
    void Cache(long start, int length);
}