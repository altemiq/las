// -----------------------------------------------------------------------
// <copyright file="IndexingExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Index;

/// <summary>
/// The <see cref="Indexing"/> extensions.
/// </summary>
internal static class IndexingExtensions
{
#if LAS1_4_OR_GREATER
    /// <summary>
    /// Reads or creates the LAX from the LAS reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The LAS index.</returns>
    public static Indexing.LasIndex ReadOrCreateIndex(this LasReader reader) => reader.ReadIndex() ?? Indexing.LasIndex.Create(reader);

    /// <summary>
    /// Reads the index from the reader.
    /// </summary>
    /// <param name="reader">The LAS reader.</param>
    /// <returns>The LAS index.</returns>
    public static Indexing.LasIndex? ReadIndex(this LasReader reader) =>
        reader.ExtendedVariableLengthRecords.OfType<Indexing.LaxTag>().FirstOrDefault() is { } laxTag
            ? laxTag.GetIndex()
            : throw new InvalidOperationException();
#endif
}