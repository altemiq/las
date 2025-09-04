// -----------------------------------------------------------------------
// <copyright file="ArithmeticBitModel.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic bit model.
/// </summary>
internal sealed class ArithmeticBitModel : IBitModel
{
    /// <summary>
    /// Length bits discarded before mult.
    /// </summary>
    internal const uint LengthShift = 13;

    // for adaptive models
    private const uint MaxCount = 1U << (int)LengthShift;

    private uint updateCycle;
    private uint bitsUntilUpdate;
    private uint bitCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArithmeticBitModel"/> class.
    /// </summary>
    public ArithmeticBitModel() => this.Initialize();

    /// <inheritdoc/>
    public uint BitZeroProb { get; private set; }

    /// <inheritdoc/>
    public uint BitZeroCount { get; private set; }

    /// <inheritdoc/>
    public uint DecrementBitsUntilUpdate() => --this.bitsUntilUpdate;

    /// <inheritdoc/>
    public void IncrementBitZeroCount() => this.BitZeroCount++;

    /// <inheritdoc/>
    public bool Initialize(uint[]? table = null)
    {
        // initialization to equiprobable model
        this.BitZeroCount = 1;
        this.bitCount = 2;
        this.BitZeroProb = 1U << (int)(LengthShift - 1);

        // start with frequent updates
        this.updateCycle = this.bitsUntilUpdate = 4;

        return true;
    }

    /// <summary>
    /// Updates this instance.
    /// </summary>
    public void Update()
    {
        // halve counts when a threshold is reached
        if ((this.bitCount += this.updateCycle) > MaxCount)
        {
            this.bitCount = (this.bitCount + 1) >> 1;
            this.BitZeroCount = (this.BitZeroCount + 1) >> 1;
            if (this.BitZeroCount == this.bitCount)
            {
                ++this.bitCount;
            }
        }

        // compute scaled bit 0 probability
        var scale = 0x80000000U / this.bitCount;
        this.BitZeroProb = (this.BitZeroCount * scale) >> (int)(31 - LengthShift);

        // set frequency of model updates
        this.updateCycle = (5 * this.updateCycle) >> 2;
        if (this.updateCycle > 64)
        {
            this.updateCycle = 64;
        }

        this.bitsUntilUpdate = this.updateCycle;
    }
}