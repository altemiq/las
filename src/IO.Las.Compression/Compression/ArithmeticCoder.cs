// -----------------------------------------------------------------------
// <copyright file="ArithmeticCoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic encoder.
/// </summary>
internal abstract class ArithmeticCoder : IEntropyCoder
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

    /// <inheritdoc/>
    public abstract void Done();

    /// <inheritdoc/>
    public IBitModel CreateBitModel() => new ArithmeticBitModel();

    /// <inheritdoc/>
    public ISymbolModel CreateSymbolModel(uint n) => new ArithmeticSymbolModel(n, compress: false);
}