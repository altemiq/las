// -----------------------------------------------------------------------
// <copyright file="IntegerDecompressor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The integer decompressor.
/// </summary>
internal sealed class IntegerDecompressor
{
    private readonly IEntropyDecoder decoder;
    private readonly uint bitsHigh;
    private readonly uint correctorRange;
    private readonly int correctorMin;
    private readonly ISymbolModel[] bitsModels;
    private readonly ISymbolModel[] correctorModels;
    private readonly IBitModel correctorBitModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerDecompressor"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="bits">The bits.</param>
    /// <param name="contexts">The contexts.</param>
    /// <param name="bitsHigh">The high bits.</param>
    /// <param name="range">The range.</param>
    public IntegerDecompressor(IEntropyDecoder decoder, uint bits = 16, uint contexts = 1, uint bitsHigh = 8, uint range = 0)
    {
        this.decoder = decoder;
        this.bitsHigh = bitsHigh;
        uint correctorBits;

        if (range is not 0)
        {
            // the corrector's significant bits and range
            correctorBits = default;
            this.correctorRange = range;
            while (range > 0)
            {
                range >>= 1;
                correctorBits++;
            }

            if (this.correctorRange == (1U << (int)(correctorBits - 1)))
            {
                correctorBits--;
            }

            // the corrector must fall into this interval
            this.correctorMin = -(int)(this.correctorRange / 2);
        }
        else if (bits is > 0 and < 32)
        {
            correctorBits = bits;
            this.correctorRange = 1U << (int)bits;

            // the corrector must fall into this interval
            this.correctorMin = -(int)(this.correctorRange / 2);
        }
        else
        {
            correctorBits = 32;
            this.correctorRange = default;

            // the corrector must fall into this interval
            this.correctorMin = int.MinValue;
        }

        this.K = default;

        this.bitsModels = new ISymbolModel[contexts];
        this.correctorModels = new ISymbolModel[correctorBits + 1];

        for (var i = 0; i < this.bitsModels.Length; i++)
        {
            this.bitsModels[i] = this.decoder.CreateSymbolModel(correctorBits + 1);
        }

        this.correctorBitModel = this.decoder.CreateBitModel();
        for (var i = 1; i < this.correctorModels.Length; i++)
        {
            this.correctorModels[i] = i <= this.bitsHigh
                ? this.decoder.CreateSymbolModel(1U << i)
                : this.decoder.CreateSymbolModel(1U << (int)this.bitsHigh);
        }
    }

    /// <summary>
    /// Gets the k-value.
    /// </summary>
    public uint K { get; private set; }

    /// <summary>
    /// Initializes the compressor.
    /// </summary>
    public void Initialize()
    {
        // certainly init the models
        foreach (var bitModel in this.bitsModels)
        {
            _ = bitModel.Initialize();
        }

        _ = this.correctorBitModel.Initialize();
        foreach (var correctorModel in this.correctorModels.Skip(1))
        {
            _ = correctorModel.Initialize();
        }
    }

    /// <summary>
    /// Decompresses the value.
    /// </summary>
    /// <param name="pred">The predicate.</param>
    /// <param name="context">The context.</param>
    /// <returns>The decompressed value.</returns>
    public int Decompress(int pred, uint context = default)
    {
        var real = pred + ReadCorrector(this.bitsModels[(int)context]);
        if (real < 0)
        {
            real += (int)this.correctorRange;
        }
        else if ((uint)real >= this.correctorRange)
        {
            real -= (int)this.correctorRange;
        }

        return real;

        int ReadCorrector(ISymbolModel bitsModel)
        {
            // decode within which interval the corrector is falling
            this.K = this.decoder.DecodeSymbol(bitsModel);

            switch (this.K)
            {
                // decode the exact location of the corrector within the interval\
                // then c is either smaller than 0 or bigger than 1
                case 0:
                    return (int)this.decoder.DecodeBit(this.correctorBitModel);
                case >= 32:
                    return this.correctorMin;
            }

            var k = (int)this.K;

            // for small k we can do this in one step
            var c = this.K <= this.bitsHigh
                ? (int)this.decoder.DecodeSymbol(this.correctorModels[k]) // decompress c with the range coder
                : GetLargeK(this.correctorModels[k]); // for larger k we need to do this in two steps

            // translate c back into its correct interval
            // if c is in the interval [ 2^(k-1)  ...  + 2^k - 1 ]
            return c >= (1 << (k - 1))

                // so we translate c back into the interval [ 2^(k-1) + 1  ...  2^k ] by adding 1
                ? c + 1

                // otherwise c is in the interval [ 0 ...  + 2^(k-1) - 1 ] so we translate c back into the interval [ - (2^k - 1)  ...  - (2^(k-1)) ] by subtracting (2^k - 1)
                : c - ((1 << k) - 1);

            int GetLargeK(ISymbolModel model)
            {
                var k1 = this.K - this.bitsHigh;

                // decompress higher bits with table
                var high = (int)this.decoder.DecodeSymbol(model);

                // read lower bits raw
                var low = (int)this.decoder.ReadBits(k1);

                // put the corrector back together
                return (high << (int)k1) | low;
            }
        }
    }
}