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
    public static Indexing.LasIndex ReadOrCreateIndex(this LasReader reader) =>
        TryReadIndex(reader, out var index)
            ? index
            : Indexing.LasIndex.Create(reader);

    /// <summary>
    /// Reads the index from the reader.
    /// </summary>
    /// <param name="reader">The LAS reader.</param>
    /// <returns>The LAS index.</returns>
    public static Indexing.LasIndex ReadIndex(this LasReader reader) =>
        TryReadIndex(reader, out var index)
            ? index
            : throw new InvalidOperationException();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S1751:Loops with at most one iteration should be refactored", Justification = "Checked")]
    private static bool TryReadIndex(LasReader reader, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Indexing.LasIndex? index)
    {
        foreach (var tag in reader.ExtendedVariableLengthRecords.OfType<Indexing.LaxTag>())
        {
            index = tag.GetIndex();
            return true;
        }

        index = default;
        return false;
    }
#endif
}