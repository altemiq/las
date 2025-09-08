// -----------------------------------------------------------------------
// <copyright file="ISimpleReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// Represents a compressed reader.
/// </summary>
internal interface ISimpleReader : ISimple
{
    /// <summary>
    /// Reads the value into the specified item at the start index, with a context.
    /// </summary>
    /// <param name="item">The item to write to.</param>
    void Read(Span<byte> item);
}