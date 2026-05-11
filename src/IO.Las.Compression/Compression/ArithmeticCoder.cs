// -----------------------------------------------------------------------
// <copyright file="ArithmeticCoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic encoder.
/// </summary>
internal abstract class ArithmeticCoder
{
    /// <summary>
    /// The default model count.
    /// </summary>
    public const uint ModelCount = 256U;

    /// <summary>
    /// Half the <see cref="ModelCount"/>.
    /// </summary>
    public const uint HalfModelCount = ModelCount / 2;

    /// <summary>
    /// The buffer size.
    /// </summary>
    protected internal const int BufferSize = 4096;

    /// <summary>
    /// Threshold for renormalization.
    /// </summary>
    protected const uint MinLength = 0x01000000U;

    /// <summary>
    /// Maximum AC interval length.
    /// </summary>
    protected const uint MaxLength = 0xFFFFFFFFU;

    /// <summary>
    /// Finalizes this instance.
    /// </summary>
    public abstract void Done();

    /// <summary>
    /// Creates the bit model.
    /// </summary>
    /// <returns>The bit model.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Factory method kept as instance for uniform call syntax across encoder/decoder.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Factory method kept as instance for uniform call syntax across encoder/decoder.")]
    public ArithmeticBitModel CreateBitModel() => new();

    /// <summary>
    /// Creates an entropy model for n symbols.
    /// </summary>
    /// <param name="n">The number of symbols.</param>
    /// <returns>The symbol model.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Factory method kept as instance for uniform call syntax across encoder/decoder.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Factory method kept as instance for uniform call syntax across encoder/decoder.")]
    public ArithmeticSymbolModel CreateSymbolModel(uint n) => new(n, compress: false);
}