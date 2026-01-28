// -----------------------------------------------------------------------
// <copyright file="OccupancyGrid.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Cloud.Internals;

/// <summary>
/// Initializes a new instance of the <see cref="OccupancyGrid"/> class.
/// </summary>
/// <param name="quantizer">The converter.</param>
/// <param name="gridSpacing">The grid spacing.</param>
internal sealed class OccupancyGrid(PointDataRecordQuantizer quantizer, float gridSpacing)
{
#pragma warning disable IDE0079
#pragma warning disable RCS1222
#pragma warning disable S2933
    private int[] minusAnchors = [];
    private int[]?[] minusMinus = [];
    private int[]?[] minusPlus = [];
    private int[] plusAnchors = [];
    private int[]?[] plusMinus = [];
    private int[]?[] plusPlus = [];
#pragma warning restore IDE0079, RCS1222, S2933

    private float gridSpacing = -gridSpacing;
    private int anker;
    private int minX;
    private int minY;
    private int maxX;
    private int maxY;

    /// <summary>
    /// Gets the number of occupied grids.
    /// </summary>
    public uint NumOccupied { get; private set; }

    /// <summary>
    /// Adds the point.
    /// </summary>
    /// <param name="point">The point to add.</param>
    /// <returns><see langword="true"/> if the point was added; otherwise <see langword="false"/>.</returns>
    public bool Add(IBasePointDataRecord point)
    {
        int posX;
        int posY;
        if (this.gridSpacing < 0)
        {
            this.gridSpacing = -this.gridSpacing;
#if NET7_0_OR_GREATER
            var position = System.Runtime.Intrinsics.Vector256.Floor((quantizer.Get(point) / this.gridSpacing).AsVector256());
            posX = (int)position[0];
            posY = (int)position[1];
#else
            var position = quantizer.Get(point) / this.gridSpacing;
            posX = (int)Math.Floor(position.X);
            posY = (int)Math.Floor(position.Y);
#endif
            this.anker = posY;
            this.minX = this.maxX = posX;
            this.minY = this.maxY = posY;
        }
        else
        {
#if NET7_0_OR_GREATER
            var position = System.Runtime.Intrinsics.Vector256.Floor((quantizer.Get(point) / this.gridSpacing).AsVector256());
            posX = (int)position[0];
            posY = (int)position[1];
#else
            var position = quantizer.Get(point) / this.gridSpacing;
            posX = (int)Math.Floor(position.X);
            posY = (int)Math.Floor(position.Y);
#endif
            if (posX < this.minX)
            {
                this.minX = posX;
            }
            else if (posX > this.maxX)
            {
                this.maxX = posX;
            }

            if (posY < this.minY)
            {
                this.minY = posY;
            }
            else if (posY > this.maxY)
            {
                this.maxY = posY;
            }
        }

        return AddInternal(posX, posY);

        bool AddInternal(int x, int y)
        {
            y -= this.anker;
            var noXAnchor = false;
#pragma warning disable IDE0007
            ref int[] anchors = ref this.minusAnchors;
            ref int[]?[] array = ref this.minusMinus;
#pragma warning restore IDE0007
            if (y < 0)
            {
                y = -y - 1;
                anchors = ref this.minusAnchors;
                if (y < this.minusPlus.Length && this.minusPlus[y] is not null)
                {
                    x -= this.minusAnchors[y];
                    if (x < 0)
                    {
                        x = -x - 1;
                        array = ref this.minusMinus;
                    }
                    else
                    {
                        array = ref this.minusPlus;
                    }
                }
                else
                {
                    noXAnchor = true;
                    array = ref this.minusPlus;
                }
            }
            else
            {
                anchors = ref this.plusAnchors;
                if (y < this.plusPlus.Length && this.plusPlus[y] is not null)
                {
                    x -= this.plusAnchors[y];
                    if (x < 0)
                    {
                        x = -x - 1;
                        array = ref this.plusMinus;
                    }
                    else
                    {
                        array = ref this.plusPlus;
                    }
                }
                else
                {
                    noXAnchor = true;
                    array = ref this.plusPlus;
                }
            }

            // maybe grow banded grid in y direction
            if (y >= array.Length)
            {
                var size = ((y / 1024) + 1) * 1024;
                if (array == this.minusPlus || array == this.plusPlus)
                {
                    Array.Resize(ref anchors, size);
                }

                Array.Resize(ref array, size);
            }

            // is this the first x anchor for this y pos?
            if (noXAnchor)
            {
                anchors[y] = x;
                x = 0;
            }

            // maybe grow banded grid in x direction
            var xPos = x / 32;
            var arrayY = array[y];
            if (arrayY is null)
            {
                arrayY = new int[((xPos / 256) + 1) * 256];
                array[y] = arrayY;
            }
            else if (xPos >= arrayY.Length)
            {
                Array.Resize(ref arrayY, ((xPos / 256) + 1) * 256);
            }

            var xBit = 1 << (x % 32);
            var arrayX = arrayY[xPos];
            if ((arrayX & xBit) is not 0)
            {
                return false;
            }

            arrayY[xPos] = arrayX | xBit;
            this.NumOccupied++;
            return true;
        }
    }
}