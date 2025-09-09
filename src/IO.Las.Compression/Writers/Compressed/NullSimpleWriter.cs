// -----------------------------------------------------------------------
// <copyright file="NullSimpleWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The null <see cref="ISimpleWriter"/>.
/// </summary>
internal sealed class NullSimpleWriter : ISimpleWriter
{
    /// <summary>
    /// The instance.
    /// </summary>
    public static readonly ISimpleWriter Instance = new NullSimpleWriter();

    /// <inheritdoc/>
    bool ISimple.Initialize(ReadOnlySpan<byte> item) => true;

    /// <inheritdoc/>
    void ISimpleWriter.Write(Span<byte> item)
    {
        // This should not do anything
    }

    /// <inheritdoc/>
    public ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default) => default;
}