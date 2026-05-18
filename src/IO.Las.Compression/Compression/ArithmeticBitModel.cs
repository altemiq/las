// -----------------------------------------------------------------------
// <copyright file="ArithmeticBitModel.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The arithmetic bit model.
/// </summary>
internal sealed class ArithmeticBitModel
{
    /// <summary>
    /// Length bits discarded before mult.
    /// </summary>
    internal const int LengthShift = 13;

    private const uint MaxCount = 1U << LengthShift;

    private uint updateCycle;
    private uint bitsUntilUpdate;
    private uint bitCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArithmeticBitModel"/> class.
    /// </summary>
    public ArithmeticBitModel() => this.Initialize();

    /// <summary>
    /// Gets the bit-0 count.
    /// </summary>
    public uint BitZeroProb { get; private set; }

    /// <summary>
    /// Gets the bit-0 prob.
    /// </summary>
    public uint BitZeroCount { get; private set; }

    /// <summary>
    /// Decrements the bits until update.
    /// </summary>
    /// <returns>The bits until update.</returns>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public uint DecrementBitsUntilUpdate() => --this.bitsUntilUpdate;

    /// <summary>
    /// Increments the <see cref="BitZeroCount"/>.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public void IncrementBitZeroCount() => this.BitZeroCount++;

    /// <summary>
    /// Initializes the entropy model.
    /// </summary>
    /// <returns>The return.</returns>
    public bool Initialize()
    {
        // initialization to equiprobable model
        this.BitZeroCount = 1;
        this.bitCount = 2;
        this.BitZeroProb = 1U << (LengthShift - 1);

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
        this.BitZeroProb = (this.BitZeroCount * scale) >> (31 - LengthShift);

        // set frequency of model updates
        this.updateCycle = (5 * this.updateCycle) >> 2;
        if (this.updateCycle > 64)
        {
            this.updateCycle = 64;
        }

        this.bitsUntilUpdate = this.updateCycle;
    }
}