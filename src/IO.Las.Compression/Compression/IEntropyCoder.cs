// -----------------------------------------------------------------------
// <copyright file="IEntropyCoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The base entropy coder.
/// </summary>
internal interface IEntropyCoder
{
    /// <summary>
    /// Finalizes this instance.
    /// </summary>
    void Done();

    /// <summary>
    /// Creates the bit model.
    /// </summary>
    /// <returns>The bit model.</returns>
    IBitModel CreateBitModel();

    /// <summary>
    /// Creates an entropy model for n symbols.
    /// </summary>
    /// <param name="n">The number of symbols.</param>
    /// <returns>The symbol model.</returns>
    ISymbolModel CreateSymbolModel(uint n);
}