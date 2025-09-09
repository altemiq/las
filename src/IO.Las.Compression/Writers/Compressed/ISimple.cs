// -----------------------------------------------------------------------
// <copyright file="ISimple.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// Represents a simple writer without context.
/// </summary>
internal interface ISimple
{
    /// <summary>
    /// Initializes the reader with the specified data.
    /// </summary>
    /// <param name="item">The item to initialize from.</param>
    /// <returns><see langword="true"/> if successfully initialized; otherwise <see langword="false"/>.</returns>
    bool Initialize(ReadOnlySpan<byte> item);
}