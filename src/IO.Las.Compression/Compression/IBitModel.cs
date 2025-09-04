// -----------------------------------------------------------------------
// <copyright file="IBitModel.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The bit <see cref="IEntropyModel"/>.
/// </summary>
internal interface IBitModel : IEntropyModel
{
    /// <summary>
    /// Gets the bit-0 count.
    /// </summary>
    uint BitZeroCount { get; }

    /// <summary>
    /// Gets the bit-0 prob.
    /// </summary>
    uint BitZeroProb { get; }

    /// <summary>
    /// Decrements the bits until update.
    /// </summary>
    /// <returns>The bits until update.</returns>
    uint DecrementBitsUntilUpdate();

    /// <summary>
    /// Increments the <see cref="BitZeroCount"/>.
    /// </summary>
    void IncrementBitZeroCount();
}