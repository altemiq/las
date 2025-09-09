// -----------------------------------------------------------------------
// <copyright file="NullContextWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The null <see cref="IContextWriter"/>.
/// </summary>
internal sealed class NullContextWriter : IContextWriter
{
    /// <summary>
    /// The instance.
    /// </summary>
    public static readonly IContextWriter Instance = new NullContextWriter();

    /// <inheritdoc/>
    bool IContext.ChunkBytes() => true;

    /// <inheritdoc/>
    bool IContext.ChunkSizes() => true;

    /// <inheritdoc/>
    bool IContext.Initialize(ReadOnlySpan<byte> item, ref uint context) => true;

    /// <inheritdoc/>
    void IContextWriter.Write(Span<byte> item, ref uint context)
    {
        // this should do nothing
    }

    /// <inheritdoc/>
    public ValueTask<uint> WriteAsync(Memory<byte> item, uint context, CancellationToken cancellationToken = default) => new(context);
}