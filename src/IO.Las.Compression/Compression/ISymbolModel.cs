// -----------------------------------------------------------------------
// <copyright file="ISymbolModel.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The symbol <see cref="IEntropyModel"/>.
/// </summary>
internal interface ISymbolModel : IEntropyModel
{
    /// <summary>
    /// Gets the distribution.
    /// </summary>
    uint[] Distribution { get; }

    /// <summary>
    /// Gets the decoder table.
    /// </summary>
    uint[]? DecoderTable { get; }

    /// <summary>
    /// Gets the last symbol.
    /// </summary>
    uint LastSymbol { get; }

    /// <summary>
    /// Gets the symbol count.
    /// </summary>
    uint[] SymbolCount { get; }

    /// <summary>
    /// Gets the symbols.
    /// </summary>
    uint Symbols { get; }

    /// <summary>
    /// Gets the table shift.
    /// </summary>
    uint TableShift { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is initialized.
    /// </summary>
    bool Initialized { get; }

    /// <summary>
    /// Decrements the symbols until update.
    /// </summary>
    /// <returns>The symbols until update.</returns>
    uint DecrementSymbolsUntilUpdate();
}