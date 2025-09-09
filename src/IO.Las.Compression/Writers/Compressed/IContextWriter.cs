// -----------------------------------------------------------------------
// <copyright file="IContextWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The <see cref="IContext"/> writer.
/// </summary>
internal interface IContextWriter : IContext
{
    /// <summary>
    /// Writes the specified item, with a context.
    /// </summary>
    /// <param name="item">The item to write to.</param>
    /// <param name="context">The context.</param>
    void Write(Span<byte> item, ref uint context);

    /// <summary>
    /// Writes the specified item asynchronously, with a context.
    /// </summary>
    /// <param name="item">The item to write to.</param>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The modified context.</returns>
    ValueTask<uint> WriteAsync(Memory<byte> item, uint context, CancellationToken cancellationToken = default);
}