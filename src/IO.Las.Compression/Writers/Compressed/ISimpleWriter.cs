// -----------------------------------------------------------------------
// <copyright file="ISimpleWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// Represents a compressed Writer.
/// </summary>
internal interface ISimpleWriter : ISimple
{
    /// <summary>
    /// Writes the specified item.
    /// </summary>
    /// <param name="item">The item to write.</param>
    void Write(Span<byte> item);

    /// <summary>
    /// Writes the specified item asynchronously.
    /// </summary>
    /// <param name="item">The item to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default);
}