// -----------------------------------------------------------------------
// <copyright file="NullContextReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The null <see cref="IContextReader"/>.
/// </summary>
internal sealed class NullContextReader : IContextReader
{
    /// <summary>
    /// The instance.
    /// </summary>
    public static readonly IContextReader Instance = new NullContextReader();

    /// <inheritdoc/>
    bool IContext.ChunkSizes() => true;

    /// <inheritdoc/>
    bool IContext.Initialize(ReadOnlySpan<byte> item, ref uint context) => true;

    /// <inheritdoc/>
    void IContextReader.Read(Span<byte> item, uint context)
    {
        // this should not do anything
    }
}