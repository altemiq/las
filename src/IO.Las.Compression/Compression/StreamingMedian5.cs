// -----------------------------------------------------------------------
// <copyright file="StreamingMedian5.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The stream median of 5 values.
/// </summary>
internal sealed class StreamingMedian5
{
    private readonly int[] values = new int[5];

    private bool high = true;

    /// <summary>
    /// Add the value to the stream.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Add(int value)
    {
        if (this.high)
        {
            if (value < this.values[2])
            {
                this.values[4] = this.values[3];
                this.values[3] = this.values[2];
                if (value < this.values[0])
                {
                    this.values[2] = this.values[1];
                    this.values[1] = this.values[0];
                    this.values[0] = value;
                }
                else if (value < this.values[1])
                {
                    this.values[2] = this.values[1];
                    this.values[1] = value;
                }
                else
                {
                    this.values[2] = value;
                }
            }
            else
            {
                if (value < this.values[3])
                {
                    this.values[4] = this.values[3];
                    this.values[3] = value;
                }
                else
                {
                    this.values[4] = value;
                }

                this.high = false;
            }
        }
        else
        {
            if (this.values[2] < value)
            {
                this.values[0] = this.values[1];
                this.values[1] = this.values[2];
                if (this.values[4] < value)
                {
                    this.values[2] = this.values[3];
                    this.values[3] = this.values[4];
                    this.values[4] = value;
                }
                else if (this.values[3] < value)
                {
                    this.values[2] = this.values[3];
                    this.values[3] = value;
                }
                else
                {
                    this.values[2] = value;
                }
            }
            else
            {
                if (this.values[1] < value)
                {
                    this.values[0] = this.values[1];
                    this.values[1] = value;
                }
                else
                {
                    this.values[0] = value;
                }

                this.high = true;
            }
        }
    }

    /// <summary>
    /// Gets the median value.
    /// </summary>
    /// <returns>The median value.</returns>
    public int Get() => this.values[2];
}