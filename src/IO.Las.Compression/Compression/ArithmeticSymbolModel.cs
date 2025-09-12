// -----------------------------------------------------------------------
// <copyright file="ArithmeticSymbolModel.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic symbol model.
/// </summary>
internal sealed class ArithmeticSymbolModel : ISymbolModel
{
    /// <summary>
    /// Length bits discarded before mult.
    /// </summary>
    internal const uint LengthShift = 15;

    private const uint MaxCount = 1U << (int)LengthShift;

    private readonly bool compress;

    private readonly uint tableSize;

    private uint totalCount;

    private uint updateCycle;

    private uint symbolsUntilUpdate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArithmeticSymbolModel"/> class.
    /// </summary>
    /// <param name="symbols">The symbols.</param>
    /// <param name="compress">Whether this should compress.</param>
    public ArithmeticSymbolModel(uint symbols, bool compress)
    {
        if (symbols is < 2 or > 1 << 11)
        {
            // invalid number of symbols
            throw new ArgumentOutOfRangeException(nameof(symbols));
        }

        if ((!compress) && symbols > 16)
        {
            var tableBits = 3;
            while (symbols > (1U << (tableBits + 2)))
            {
                ++tableBits;
            }

            this.tableSize = 1U << tableBits;
            this.TableShift = LengthShift - (uint)tableBits;
            this.Distribution = new uint[symbols];
            this.DecoderTable = new uint[this.tableSize + 2];
        }
        else
        {
            // small alphabet: no table needed
            this.DecoderTable = default;
            this.tableSize = this.TableShift = default;
            this.Distribution = new uint[symbols];
        }

        this.Symbols = symbols;
        this.compress = compress;
        this.LastSymbol = symbols - 1;
        this.SymbolCount = new uint[symbols];
    }

    /// <inheritdoc/>
    public uint[] Distribution { get; }

    /// <inheritdoc/>
    public uint[] SymbolCount { get; }

    /// <inheritdoc/>
    public uint[]? DecoderTable { get; }

    /// <inheritdoc/>
    public uint Symbols { get; }

    /// <inheritdoc/>
    public uint LastSymbol { get; }

    /// <inheritdoc/>
    public uint TableShift { get; }

    /// <inheritdoc/>
    public bool Initialized { get; private set; }

    /// <inheritdoc/>
    public uint DecrementSymbolsUntilUpdate() => --this.symbolsUntilUpdate;

    /// <inheritdoc/>
    public bool Initialize(uint[]? table = null)
    {
        this.totalCount = default;
        this.updateCycle = this.Symbols;
        if (table is null)
        {
            for (var k = 0U; k < this.Symbols; k++)
            {
                this.SymbolCount[k] = 1;
            }
        }
        else
        {
            for (var k = 0U; k < this.Symbols; k++)
            {
                this.SymbolCount[k] = table[k];
            }
        }

        this.Update();
        this.symbolsUntilUpdate = this.updateCycle = (this.Symbols + 6) >> 1;

        return this.Initialized = true;
    }

    /// <summary>
    /// Updates this instance.
    /// </summary>
    /// <exception cref="CompressionNotInitializedException">The compression has not been initialized.</exception>
    public void Update()
    {
        // halve counts when a threshold is reached
        this.totalCount += this.updateCycle;
        if (this.totalCount > MaxCount)
        {
            this.totalCount = default;
            for (var n = 0U; n < this.Symbols; n++)
            {
                var value = this.SymbolCount[n] = (this.SymbolCount[n] + 1) >> 1;
                this.totalCount += value;
            }
        }

        // compute cumulative distribution, decoder table
        var scale = 0x80000000U / this.totalCount;
        const int DistributionLeftShift = (int)(31U - LengthShift);

        if (this.compress || (this.tableSize is default(uint)))
        {
            var sum = default(uint);
            for (var k = 0U; k < this.Symbols; k++)
            {
                this.Distribution[k] = (scale * sum) >> DistributionLeftShift;
                sum += this.SymbolCount[k];
            }
        }
        else if (this.DecoderTable is { } decoderTable)
        {
            var sum = default(uint);
            var s = default(uint);
            var tableShift = (int)this.TableShift;
            for (var k = 0U; k < this.Symbols; k++)
            {
                this.Distribution[k] = (scale * sum) >> DistributionLeftShift;
                sum += this.SymbolCount[k];
                var w = this.Distribution[k] >> tableShift;
                while (s < w)
                {
                    decoderTable[++s] = k - 1U;
                }
            }

            decoderTable[0] = default;
            while (s <= this.tableSize)
            {
                decoderTable[++s] = this.Symbols - 1U;
            }
        }
        else
        {
            throw new CompressionNotInitializedException();
        }

        // set frequency of model updates
        this.updateCycle = (5U * this.updateCycle) >> 2;
        var maxCycle = (this.Symbols + 6U) << 3;
        if (this.updateCycle > maxCycle)
        {
            this.updateCycle = maxCycle;
        }

        this.symbolsUntilUpdate = this.updateCycle;
    }
}