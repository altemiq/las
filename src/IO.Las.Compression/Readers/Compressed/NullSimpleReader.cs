// -----------------------------------------------------------------------
// <copyright file="NullSimpleReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The null <see cref="ISimpleReader"/>.
/// </summary>
internal sealed class NullSimpleReader : ISimpleReader
{
    /// <summary>
    /// The instance.
    /// </summary>
    public static readonly ISimpleReader Instance = new NullSimpleReader();

    /// <inheritdoc/>
    bool ISimple.Initialize(ReadOnlySpan<byte> item) => true;

    /// <inheritdoc/>
    void ISimpleReader.Read(Span<byte> item)
    {
        // this should not do anything
    }
}