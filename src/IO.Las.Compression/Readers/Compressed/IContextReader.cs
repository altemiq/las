// -----------------------------------------------------------------------
// <copyright file="IContextReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// Represents a compressed <see cref="Readers.IPointDataRecordReader"/>.
/// </summary>
internal interface IContextReader : IContext
{
    /// <summary>
    /// Reads the value into the specified item at the start index, with a context.
    /// </summary>
    /// <param name="item">The item to write to.</param>
    /// <param name="context">The context.</param>
    void Read(Span<byte> item, uint context);
}