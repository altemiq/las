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
            posX = (int)Math.Floor(quantizer.GetX(point.X) / this.gridSpacing);
            posY = (int)Math.Floor(quantizer.GetY(point.Y) / this.gridSpacing);
            this.anker = posY;
            this.minX = this.maxX = posX;
            this.minY = this.maxY = posY;
        }
        else
        {
            posX = (int)Math.Floor(quantizer.GetX(point.X) / this.gridSpacing);
            posY = (int)Math.Floor(quantizer.GetY(point.Y) / this.gridSpacing);
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

        return this.AddInternal(posX, posY);
    }

    private bool AddInternal(int posX, int posY)
    {
        posY -= this.anker;
        var noXAnchor = false;
#pragma warning disable IDE0007
        ref int[] anchors = ref this.minusAnchors;
        ref int[]?[] array = ref this.minusMinus;
#pragma warning restore IDE0007
        if (posY < 0)
        {
            posY = -posY - 1;
            anchors = ref this.minusAnchors;
            if (posY < this.minusPlus.Length && this.minusPlus[posY] is not null)
            {
                posX -= this.minusAnchors[posY];
                if (posX < 0)
                {
                    posX = -posX - 1;
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
            if (posY < this.plusPlus.Length && this.plusPlus[posY] is not null)
            {
                posX -= this.plusAnchors[posY];
                if (posX < 0)
                {
                    posX = -posX - 1;
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
        if (posY >= array.Length)
        {
            var newSize = ((posY / 1024) + 1) * 1024;
            if (array == this.minusPlus || array == this.plusPlus)
            {
                Array.Resize(ref anchors, newSize);
            }

            Array.Resize(ref array, newSize);
        }

        // is this the first x anchor for this y pos?
        if (noXAnchor)
        {
            anchors[posY] = posX;
            posX = 0;
        }

        // maybe grow banded grid in x direction
        var posXPos = posX / 32;
        var arrayPosY = array[posY];
        if (arrayPosY is null)
        {
            arrayPosY = new int[((posXPos / 256) + 1) * 256];
            array[posY] = arrayPosY;
        }
        else if (posXPos >= arrayPosY.Length)
        {
            Array.Resize(ref arrayPosY, ((posXPos / 256) + 1) * 256);
        }

        var posXBit = 1 << (posX % 32);
        var arrayPosXPos = arrayPosY[posXPos];
        if ((arrayPosXPos & posXBit) is not 0)
        {
            return false;
        }

        arrayPosY[posXPos] = arrayPosXPos | posXBit;
        this.NumOccupied++;
        return true;
    }
}