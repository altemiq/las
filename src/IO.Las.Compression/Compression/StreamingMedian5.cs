// -----------------------------------------------------------------------
// <copyright file="StreamingMedian5.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using System.Runtime.CompilerServices;

/// <summary>
/// The streaming median of 5 values.
/// </summary>
/// <remarks>
/// This is a value type so that arrays of <see cref="StreamingMedian5"/> are stored
/// contiguously as a single heap allocation, rather than as an array of references
/// to separately-allocated instances each containing its own <c>int[5]</c>. For the
/// per-point LAZ decode/encode hot path this removes 16-96 heap allocations and
/// 16-96 pointer indirections per reader/writer instance.
/// <para>
/// A default-constructed (e.g. zero-initialised array element) instance behaves
/// identically to the previous class' parameterless constructor: all five values
/// are zero and the insertion phase starts in the "high" state. The latter is
/// encoded here as the inverted field <see cref="low"/> so that
/// <c>default(StreamingMedian5)</c> matches the class behaviour (<c>low == false</c>
/// corresponds to the class' <c>high = true</c>).
/// </para>
/// </remarks>
internal struct StreamingMedian5
{
    private int v0;
    private int v1;
    private int v2;
    private int v3;
    private int v4;

    // Inverted so that default(struct) (low == false) corresponds to the
    // original class' parameterless constructor state (high == true).
    private bool low;

    /// <summary>
    /// Add the value to the stream.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Add(int value)
    {
        if (!this.low)
        {
            if (value < this.v2)
            {
                this.v4 = this.v3;
                this.v3 = this.v2;
                if (value < this.v0)
                {
                    this.v2 = this.v1;
                    this.v1 = this.v0;
                    this.v0 = value;
                }
                else if (value < this.v1)
                {
                    this.v2 = this.v1;
                    this.v1 = value;
                }
                else
                {
                    this.v2 = value;
                }
            }
            else
            {
                if (value < this.v3)
                {
                    this.v4 = this.v3;
                    this.v3 = value;
                }
                else
                {
                    this.v4 = value;
                }

                this.low = true;
            }
        }
        else if (this.v2 < value)
        {
            this.v0 = this.v1;
            this.v1 = this.v2;
            if (this.v4 < value)
            {
                this.v2 = this.v3;
                this.v3 = this.v4;
                this.v4 = value;
            }
            else if (this.v3 < value)
            {
                this.v2 = this.v3;
                this.v3 = value;
            }
            else
            {
                this.v2 = value;
            }
        }
        else
        {
            if (this.v1 < value)
            {
                this.v0 = this.v1;
                this.v1 = value;
            }
            else
            {
                this.v0 = value;
            }

            this.low = false;
        }
    }

    /// <summary>
    /// Gets the median value.
    /// </summary>
    /// <returns>The median value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int Get() => this.v2;

    /// <summary>
    /// Gets the current median and adds a new value in one operation.
    /// This can reduce cache misses when both operations are needed.
    /// </summary>
    /// <param name="value">The value to add.</param>
    /// <returns>The median before adding the new value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAndAdd(int value)
    {
        var median = this.v2;
        this.Add(value);
        return median;
    }
}