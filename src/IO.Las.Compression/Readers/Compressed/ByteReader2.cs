// -----------------------------------------------------------------------
// <copyright file="ByteReader2.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed byte reader, version 2.
/// </summary>
internal sealed class ByteReader2 : ISimpleReader
{
    private readonly IEntropyDecoder decoder;

    private readonly byte[] lastItem;

    private readonly ISymbolModel[] byteModels;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteReader2"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="number">The number.</param>
    public ByteReader2(IEntropyDecoder decoder, uint number)
    {
        this.decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));

        // create models and integer compressors
        this.byteModels = new ISymbolModel[number];
        for (var i = 0U; i < number; i++)
        {
            this.byteModels[i] = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        }

        // create last item
        this.lastItem = new byte[number];
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        foreach (var bitModel in this.byteModels)
        {
            _ = bitModel.Initialize();
        }

        item[..this.lastItem.Length].CopyTo(this.lastItem);

        return true;
    }

    /// <inheritdoc/>
    public void Read(Span<byte> item)
    {
        for (var i = 0; i < this.byteModels.Length; i++)
        {
            var value = (int)(this.lastItem[i] + this.decoder.DecodeSymbol(this.byteModels[i]));
            item[i] = value.Fold();
        }

        item[..this.lastItem.Length].CopyTo(this.lastItem);
    }
}