// -----------------------------------------------------------------------
// <copyright file="IntegerCompressor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The integer compressor.
/// </summary>
internal sealed class IntegerCompressor
{
    private readonly IEntropyEncoder encoder;

    private readonly uint bitsHigh;

    private readonly uint correctorRange;

    private readonly int correctorMin;

    private readonly int correctorMax;

    private readonly ISymbolModel[] bitsModels;

    private readonly ISymbolModel[] correctorModels;

    private readonly IBitModel correctorBitModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerCompressor"/> class.
    /// </summary>
    /// <param name="enc">The encoder.</param>
    /// <param name="bits">The bits.</param>
    /// <param name="contexts">The contexts.</param>
    /// <param name="bitsHigh">The high bits.</param>
    /// <param name="range">The range.</param>
    public IntegerCompressor(IEntropyEncoder enc, uint bits = 16, uint contexts = 1, uint bitsHigh = 8, uint range = 0)
    {
        this.encoder = enc;
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

            if (this.correctorRange == (1u << (int)(correctorBits - 1)))
            {
                correctorBits--;
            }

            // the corrector must fall into this interval
            this.correctorMin = -(int)(this.correctorRange / 2);
            this.correctorMax = (int)(this.correctorMin + this.correctorRange - 1);
        }
        else if (bits is > 0 and < 32)
        {
            correctorBits = bits;
            this.correctorRange = 1U << (int)bits;

            // the corrector must fall into this interval
            this.correctorMin = -(int)(this.correctorRange / 2);
            this.correctorMax = (int)(this.correctorMin + this.correctorRange - 1);
        }
        else
        {
            correctorBits = 32;
            this.correctorRange = default;

            // the corrector must fall into this interval
            this.correctorMin = int.MinValue;
            this.correctorMax = int.MaxValue;
        }

        this.K = default;
        this.bitsModels = new ISymbolModel[contexts];
        this.correctorModels = new ISymbolModel[correctorBits + 1];

        for (var i = 0; i < this.bitsModels.Length; i++)
        {
            this.bitsModels[i] = this.encoder.CreateSymbolModel(correctorBits + 1);
        }

        this.correctorBitModel = this.encoder.CreateBitModel();
        for (var i = 1; i < this.correctorModels.Length; i++)
        {
            this.correctorModels[i] = i <= this.bitsHigh
                ? this.encoder.CreateSymbolModel(1U << i)
                : this.encoder.CreateSymbolModel(1U << (int)this.bitsHigh);
        }
    }

    /// <summary>
    /// Gets the K value.
    /// </summary>
    public uint K { get; private set; }

    /// <summary>
    /// Initializes the compressor.
    /// </summary>
    public void Initialize()
    {
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
    /// Compresses the value.
    /// </summary>
    /// <param name="pred">The predicate.</param>
    /// <param name="real">The real value.</param>
    /// <param name="context">The context.</param>
    public void Compress(int pred, uint real, uint context = default) => this.Compress(pred, (int)real, context);

    /// <summary>
    /// Compresses the value.
    /// </summary>
    /// <param name="pred">The predicate.</param>
    /// <param name="real">The real value.</param>
    /// <param name="context">The context.</param>
    public void Compress(int pred, int real, uint context = default)
    {
        // the corrector will be within the interval [ - (corrRange - 1)  ...  + (corrRange - 1) ]
        var corr = real - pred;

        // we fold the corrector into the interval [ corrMin  ...  corrMax ]
        if (corr < this.correctorMin)
        {
            corr = (int)(corr + this.correctorRange);
        }
        else if (corr > this.correctorMax)
        {
            corr = (int)(corr - this.correctorRange);
        }

        WriteCorrector(corr, this.bitsModels[context]);

        void WriteCorrector(int c, ISymbolModel model)
        {
            // find the highest interval [ - (2^k - 1)  ...  + (2^k) ] that contains c
            this.K = default;

            // do this by checking the absolute value of c (adjusted for the case that c is 2^k)
            var c1 = (uint)(c <= 0 ? -c : c - 1);

            // this loop could be replaced with more efficient code
            while (c1 > 0)
            {
                c1 >>= 1;
                this.K++;
            }

            // the number k is between 0 and corrBits and describes the interval the corrector falls into
            // we can compress the exact location of c within this interval using k bits
            this.encoder.EncodeSymbol(model, this.K);

            if (this.K is not 0)
            {
                // then c is either smaller than 0 or bigger than 1
                if (this.K >= 32)
                {
                    return;
                }

                // translate the corrector c into the k-bit interval [ 0 ... 2^k - 1 ]
                if (c < 0)
                {
                    // then c is in the interval [ - (2^k - 1)  ...  - (2^(k-1)) ]
                    // so we translate c into the interval [ 0 ...  + 2^(k-1) - 1 ] by adding (2^k - 1)
                    c += (1 << (int)this.K) - 1;
                }
                else
                {
                    // then c is in the interval [ 2^(k-1) + 1  ...  2^k ]
                    // so we translate c into the interval [ 2^(k-1) ...  + 2^k - 1 ] by subtracting 1
                    c--;
                }

                if (this.K <= this.bitsHigh)
                {
                    // for small k we code the interval in one-step
                    // compress c with the range coder
                    this.encoder.EncodeSymbol(this.correctorModels[this.K], (uint)c);
                }
                else
                {
                    // for larger k we need to code the interval in two steps figure out how many lower bits there are
                    var k1 = (int)(this.K - this.bitsHigh);

                    // c1 represents the lowest k-bitsHigh+1 bits
                    c1 = (uint)(c & ((1 << k1) - 1));

                    // c represents the highest bitsHigh bits
                    c >>= k1;

                    // compress the higher bits using a context table
                    this.encoder.EncodeSymbol(this.correctorModels[this.K], (uint)c);

                    // store the lower k1 bits raw
                    this.encoder.WriteBits((uint)k1, c1);
                }
            }
            else
            {
                // then c is 0 or 1
                this.encoder.EncodeBit(this.correctorBitModel, (uint)c);
            }
        }
    }
}