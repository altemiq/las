// -----------------------------------------------------------------------
// <copyright file="ILazReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a LAZ file reader.
/// </summary>
public interface ILazReader
{
    /// <summary>
    /// Gets a value indicating whether this instance is compressed.
    /// </summary>
    bool IsCompressed { get; }
}