// -----------------------------------------------------------------------
// <copyright file="LasQuadTree.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// The LAS quad tree.
/// </summary>
public sealed class LasQuadTree : IEquatable<LasQuadTree>
{
    private const uint LasSpatialQuadTree = default;
    private const string Signature = "LASQ";

    private static readonly int[] LevelOffset = CreateLevelOffset();

    private readonly int sublevel;
    private readonly int sublevelIndex;
    private readonly float minimumX;
    private readonly float maximumX;
    private readonly float minimumY;
    private readonly float maximumY;
    private readonly int levels;

    private uint[]? adaptive;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasQuadTree"/> class.
    /// </summary>
    /// <param name="boundingBoxMinX">The bounding box minimum x-coordinate.</param>
    /// <param name="boundingBoxMaxX">The bounding box maximum x-coordinate.</param>
    /// <param name="boundingBoxMinY">The bounding box minimum y-coordinate.</param>
    /// <param name="boundingBoxMaxY">The bounding box maximum y-coordinate.</param>
    /// <param name="cellSize">The cell size.</param>
    /// <param name="offsetX">The x-offset.</param>
    /// <param name="offsetY">The y-offset.</param>
    public LasQuadTree(double boundingBoxMinX, double boundingBoxMaxX, double boundingBoxMinY, double boundingBoxMaxY, float cellSize, float offsetX = default, float offsetY = default)
    {
        // enlarge bounding box to units of cells
        this.minimumX = boundingBoxMinX >= offsetX
            ? (cellSize * ((int)((boundingBoxMinX - offsetX) / cellSize))) + offsetX
            : (cellSize * ((int)((boundingBoxMinX - offsetX) / cellSize) - 1)) + offsetX;
        this.maximumX = boundingBoxMaxX >= offsetX
            ? (cellSize * ((int)((boundingBoxMaxX - offsetX) / cellSize) + 1)) + offsetX
            : (cellSize * ((int)((boundingBoxMaxX - offsetX) / cellSize))) + offsetX;
        this.minimumY = boundingBoxMinY >= offsetY
            ? (cellSize * ((int)((boundingBoxMinY - offsetY) / cellSize))) + offsetY
            : (cellSize * ((int)((boundingBoxMinY - offsetY) / cellSize) - 1)) + offsetY;
        this.maximumY = boundingBoxMaxY >= offsetY
            ? (cellSize * ((int)((boundingBoxMaxY - offsetY) / cellSize) + 1)) + offsetY
            : (cellSize * ((int)((boundingBoxMaxY - offsetY) / cellSize))) + offsetY;

        // how many cells minimally in each direction
        var horizonalCells = UInt32Quantize((this.maximumX - this.minimumX) / cellSize);
        if (horizonalCells is 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, Properties.Resources.NoHorizontalCellsFound);
        }

        var verticalCells = UInt32Quantize((this.maximumY - this.minimumY) / cellSize);
        if (verticalCells is 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, Properties.Resources.NoVerticalCellsFound);
        }

        // how many quad tree levels to get to that many cells
        var c = (horizonalCells > verticalCells) ? horizonalCells - 1 : verticalCells - 1;
        while (c is not 0)
        {
            c >>= 1;
            this.levels++;
        }

        // enlarge bounding box to quad tree size
        c = (uint)(1 << this.levels) - horizonalCells;
        this.minimumX -= (c - (c / 2F)) * cellSize;
        this.maximumX += c / 2F * cellSize;
        c = (uint)(1 << this.levels) - verticalCells;
        this.minimumY -= (c - (c / 2F)) * cellSize;
        this.maximumY += c / 2F * cellSize;

        static uint UInt32Quantize(float n)
        {
            return (n >= 0) ? (uint)(n + 0.5) : 0;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LasQuadTree"/> class.
    /// </summary>
    /// <param name="boundingBoxMinX">The bounding box minimum x-coordinate.</param>
    /// <param name="boundingBoxMaxX">The bounding box maximum x-coordinate.</param>
    /// <param name="boundingBoxMinY">The bounding box minimum y-coordinate.</param>
    /// <param name="boundingBoxMaxY">The bounding box maximum y-coordinate.</param>
    /// <param name="subLevel">The sublevel.</param>
    /// <param name="subLevelIndex">The sublevel index.</param>
    /// <param name="levels">The number of levels.</param>
    public LasQuadTree(float boundingBoxMinX, float boundingBoxMaxX, float boundingBoxMinY, float boundingBoxMaxY, int subLevel, int subLevelIndex, int levels)
    {
        this.minimumX = boundingBoxMinX;
        this.maximumX = boundingBoxMaxX;
        this.minimumY = boundingBoxMinY;
        this.maximumY = boundingBoxMaxY;

        // get the cell bounding box
        (this.minimumX, this.minimumY, this.maximumX, this.maximumY) = this.GetBounds(subLevel, subLevelIndex);

        this.levels = levels;
        this.sublevel = subLevel;
        this.sublevelIndex = subLevelIndex;
    }

    private LasQuadTree(float minX, float maxX, float minY, float maxY, int levels)
    {
        this.minimumX = minX;
        this.maximumX = maxX;
        this.minimumY = minY;
        this.maximumY = maxY;
        this.levels = levels;
        this.sublevel = default;
        this.sublevelIndex = default;
    }

    /// <summary>
    /// Implements the equal operator.
    /// </summary>
    /// <param name="lhs">The left-hand side.</param>
    /// <param name="rhs">The right-hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(LasQuadTree? lhs, LasQuadTree? rhs) => lhs?.Equals(rhs) ?? rhs is null;

    /// <summary>
    /// Implements the not-equal operator.
    /// </summary>
    /// <param name="lhs">The left-hand side.</param>
    /// <param name="rhs">The right-hand side.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(LasQuadTree? lhs, LasQuadTree? rhs) => !lhs?.Equals(rhs) ?? rhs is not null;

    /// <summary>
    /// Reads the quad-tree from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The LAS quad-tree.</returns>
    public static LasQuadTree ReadFrom(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[44];
        _ = stream.Read(bytes);
        return ReadFrom(bytes);
    }

    /// <summary>
    /// Reads the quad-tree from the specified source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The LAS quad-tree.</returns>
    public static LasQuadTree ReadFrom(ReadOnlySpan<byte> source)
    {
        var signature = System.Text.Encoding.UTF8.GetString(source[..4]);
        if (signature is not "LASS")
        {
            ThrowInvalidSignature(signature, "LASS", nameof(source));
        }

        if (System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[4..8]) is not LasSpatialQuadTree)
        {
            throw new ArgumentException(Properties.Resources.IncorrectQuadTreeSignature, nameof(source));
        }

        signature = System.Text.Encoding.UTF8.GetString(source[8..12]);
        if (signature is not Signature)
        {
            ThrowInvalidSignature(signature, Signature, nameof(source));
        }

        // ignore 12..16
        var tempLevels = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[16..20]);

        // ignore 20..24
        // ignore 24..28
        var tempMinX = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[28..32]);
        var tempMaxX = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[32..36]);
        var tempMinY = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[36..40]);
        var tempMaxY = System.Buffers.Binary.BinaryPrimitives.ReadSingleLittleEndian(source[40..44]);

        return new(tempMinX, tempMaxX, tempMinY, tempMaxY, (int)tempLevels);

#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use 'CompositeFormat'", Justification = "This is a formatted string for an exception.")]
#endif
        [System.Diagnostics.CodeAnalysis.DoesNotReturn]
        static void ThrowInvalidSignature(string signature, string expected, string paramName)
        {
            throw new ArgumentException(string.Format(Las.Properties.Resources.Culture, Las.Properties.Resources.InvalidSignature, expected, signature), paramName);
        }
    }

    /// <summary>
    /// Clones this into an empty instance.
    /// </summary>
    /// <returns>The empty instance.</returns>
    public LasQuadTree CloneEmpty() => new(this.minimumX, this.maximumX, this.minimumY, this.maximumY, this.sublevel, this.sublevelIndex, this.levels);

    /// <summary>
    /// Writes this instance to the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void WriteTo(Stream stream)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        this.WriteTo(writer);
    }

    /// <summary>
    /// Writes this instance to the specified writer.
    /// </summary>
    /// <param name="writer">The writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write("LASS".ToCharArray());
        writer.Write(LasSpatialQuadTree);

        writer.Write(Signature.ToCharArray());
        writer.Write(0U); // version

        writer.Write((uint)this.levels);
        writer.Write(0U); // level index
        writer.Write(0U); // implicit_levels
        writer.Write(this.minimumX);
        writer.Write(this.maximumX);
        writer.Write(this.minimumY);
        writer.Write(this.maximumY);
    }

    /// <inheritdoc/>
    public bool Equals(LasQuadTree? other)
    {
        return other switch
        {
            null => false,
            not null when ReferenceEquals(this, other) => true,
            not null => CheckSequence(other.adaptive, this.adaptive)
                        && other.levels == this.levels
                        && other.maximumX.Equals(this.maximumX)
                        && other.maximumY.Equals(this.maximumY)
                        && other.minimumX.Equals(this.minimumX)
                        && other.minimumY.Equals(this.minimumY)
                        && other.sublevel == this.sublevel
                        && other.sublevelIndex == this.sublevelIndex,
        };

        static bool CheckSequence<T>(IEnumerable<T>? first, IEnumerable<T>? second)
        {
            return first is null
                ? second is null
                : second is not null && first.SequenceEqual(second);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as LasQuadTree);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(this.levels);
        hashCode.Add(this.maximumX);
        hashCode.Add(this.maximumY);
        hashCode.Add(this.minimumX);
        hashCode.Add(this.minimumY);
        hashCode.Add(this.sublevel);
        hashCode.Add(this.sublevelIndex);
        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() =>
    $$"""
      {
        "Levels": {{this.levels}},
        "MaxX": {{this.maximumX}},
        "MaxY": {{this.maximumY}},
        "MinX": {{this.minimumX}},
        "MinY": {{this.minimumY}},
        "Sublevel": {{this.sublevel}},
        "SublevelIndex": {{this.sublevelIndex}}
      }
      """;

    /// <summary>
    /// Gets the bounds of the cell that the coordinates are within.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The bounds of the cell that <paramref name="x"/> and <paramref name="y"/> are within.</returns>
    internal (float MinimumX, float MinimumY, float MaximumX, float MaximumY) GetBounds(double x, double y) => this.GetBounds(x, y, this.levels);

    /// <summary>
    /// Gets the bounds of the cell that the coordinates are within at the required level.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="level">The required level.</param>
    /// <returns>The bounds of the cell that <paramref name="x"/> and <paramref name="y"/> are within.</returns>
    internal (float MinimumX, float MinimumY, float MaximumX, float MaximumY) GetBounds(double x, double y, int level)
    {
        var cellMinX = this.minimumX;
        var cellMaxX = this.maximumX;
        var cellMinY = this.minimumY;
        var cellMaxY = this.maximumY;

        while (level > 0)
        {
            var cellMidX = (cellMinX + cellMaxX) * 0.5F;
            var cellMidY = (cellMinY + cellMaxY) * 0.5F;
            if (x < cellMidX)
            {
                cellMaxX = cellMidX;
            }
            else
            {
                cellMinX = cellMidX;
            }

            if (y < cellMidY)
            {
                cellMaxY = cellMidY;
            }
            else
            {
                cellMinY = cellMidY;
            }

            level--;
        }

        return (cellMinX, cellMinY, cellMaxX, cellMaxY);
    }

    /// <summary>
    /// Gets the bounds of the specified cell.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <returns>The bounds of the cell that <paramref name="cellIndex"/> represents.</returns>
    internal (float MinimumX, float MinimumY, float MaximumX, float MaximumY) GetBounds(int cellIndex)
    {
        var level = GetLevel(cellIndex);
        return this.GetBounds(level, this.GetLevelIndex(cellIndex, level));
    }

    /// <summary>
    /// Gets the bounds of the cell from the level and index.
    /// </summary>
    /// <param name="level">The cell level.</param>
    /// <param name="levelIndex">The index of the cell within <paramref name="level"/>.</param>
    /// <returns>The bounds of the cell from the level and index.</returns>
    internal (float MinimumX, float MinimumY, float MaximumX, float MaximumY) GetBounds(int level, int levelIndex)
    {
        var cellMinX = this.minimumX;
        var cellMaxX = this.maximumX;
        var cellMinY = this.minimumY;
        var cellMaxY = this.maximumY;

        while (level is not 0)
        {
            var index = (levelIndex >> (2 * (level - 1))) & 3;
            var cellMidX = (cellMinX + cellMaxX) * 0.5F;
            var cellMidY = (cellMinY + cellMaxY) * 0.5F;
            if ((index & 1) is not 0)
            {
                cellMinX = cellMidX;
            }
            else
            {
                cellMaxX = cellMidX;
            }

            if ((index & 2) is not 0)
            {
                cellMinY = cellMidY;
            }
            else
            {
                cellMaxY = cellMidY;
            }

            level--;
        }

        return (cellMinX, cellMinY, cellMaxX, cellMaxY);
    }

    /// <summary>
    /// Gets all the cells.
    /// </summary>
    /// <returns>The cell indexes.</returns>
    internal IList<int> AllCells() => this.CellsWithinRectangle(this.minimumX, this.minimumY, this.maximumX, this.maximumY);

    /// <summary>
    /// Intersects the spatial quad-tree with the rectangle.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinRectangle(double minX, double minY, double maxX, double maxY) => this.CellsWithinRectangle(minX, minY, maxX, maxY, this.levels);

    /// <summary>
    /// Intersects the spatial quad-tree with the rectangle.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    /// <param name="level">The level.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinRectangle(double minX, double minY, double maxX, double maxY, int level)
    {
        if (maxX <= this.minimumX || minX > this.maximumX || maxY <= this.minimumY || minY > this.maximumY)
        {
            return [];
        }

        var cellsInRectangle = new List<int>();
        if (this.adaptive is not null)
        {
            IntersectRectangleWithCellsAdaptive(cellsInRectangle, minX, minY, maxX, maxY, this.minimumX, this.minimumY, this.maximumX, this.maximumY, 0, 0);
        }
        else
        {
            IntersectRectangleWithCells(cellsInRectangle, LevelOffset[level], minX, minY, maxX, maxY, this.minimumX, this.minimumY, this.maximumX, this.maximumY, level, 0);
        }

        return cellsInRectangle;

        void IntersectRectangleWithCellsAdaptive(ICollection<int> cells, double rectangleMinX, double rectangleMinY, double rectangleMaxX, double rectangleMaxY, float cellMinX, float cellMinY, float cellMaxX, float cellMaxY, int currentLevel, int levelIndex)
        {
            while (true)
            {
                var cellIndex = this.GetCellIndex(levelIndex, currentLevel);
                var adaptivePos = cellIndex / 32;
                var adaptiveBit = 1U << (cellIndex % 32);
                if ((currentLevel < this.levels) && ((this.adaptive[adaptivePos] & adaptiveBit) is not 0))
                {
                    currentLevel++;
                    levelIndex <<= 2;

                    var cellMidX = (cellMinX + cellMaxX) * 0.5F;
                    var cellMidY = (cellMinY + cellMaxY) * 0.5F;

                    if (rectangleMaxX <= cellMidX)
                    {
                        if (rectangleMaxY <= cellMidY)
                        {
                            cellMaxX = cellMidX;
                            cellMaxY = cellMidY;
                            continue;
                        }

                        if (rectangleMinY >= cellMidY)
                        {
                            cellMinY = cellMidY;
                            cellMaxX = cellMidX;
                            levelIndex |= 2;
                            continue;
                        }

                        IntersectRectangleWithCellsAdaptive(cells, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinY = cellMidY;
                        cellMaxX = cellMidX;
                        levelIndex |= 2;
                        continue;
                    }

                    if (rectangleMinX >= cellMidX)
                    {
                        if (rectangleMaxY <= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMaxY = cellMidY;
                            levelIndex |= 1;
                            continue;
                        }

                        if (rectangleMinY >= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMinY = cellMidY;
                            levelIndex |= 3;
                            continue;
                        }

                        IntersectRectangleWithCellsAdaptive(cells, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    if (rectangleMaxY <= cellMidY)
                    {
                        IntersectRectangleWithCellsAdaptive(cells, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinX = cellMidX;
                        cellMaxY = cellMidY;
                        levelIndex |= 1;
                        continue;
                    }

                    if (rectangleMinY >= cellMidY)
                    {
                        IntersectRectangleWithCellsAdaptive(cells, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectRectangleWithCellsAdaptive(cells, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                    IntersectRectangleWithCellsAdaptive(cells, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                    IntersectRectangleWithCellsAdaptive(cells, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                    cellMinX = cellMidX;
                    cellMinY = cellMidY;
                    levelIndex |= 3;
                    continue;
                }

                cells.Add(cellIndex);

                break;
            }
        }

        static void IntersectRectangleWithCells(ICollection<int> cells, int levelOffset, double rectangleMinX, double rectangleMinY, double rectangleMaxX, double rectangleMaxY, float cellMinX, float cellMinY, float cellMaxX, float cellMaxY, int currentLevel, int levelIndex)
        {
            while (true)
            {
                if (currentLevel is not 0)
                {
                    currentLevel--;
                    levelIndex <<= 2;

                    var cellMidX = (cellMinX + cellMaxX) * 0.5F;
                    var cellMidY = (cellMinY + cellMaxY) * 0.5F;

                    if (rectangleMaxX <= cellMidX)
                    {
                        if (rectangleMaxY <= cellMidY)
                        {
                            cellMaxX = cellMidX;
                            cellMaxY = cellMidY;
                            continue;
                        }

                        if (rectangleMinY >= cellMidY)
                        {
                            cellMinY = cellMidY;
                            cellMaxX = cellMidX;
                            levelIndex |= 2;
                            continue;
                        }

                        IntersectRectangleWithCells(cells, levelOffset, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinY = cellMidY;
                        cellMaxX = cellMidX;
                        levelIndex |= 2;
                        continue;
                    }

                    if (rectangleMinX >= cellMidX)
                    {
                        if (rectangleMaxY <= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMaxY = cellMidY;
                            levelIndex |= 1;
                            continue;
                        }

                        if (rectangleMinY >= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMinY = cellMidY;
                            levelIndex |= 3;
                            continue;
                        }

                        IntersectRectangleWithCells(cells, levelOffset, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    if (rectangleMaxY <= cellMidY)
                    {
                        IntersectRectangleWithCells(cells, levelOffset, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinX = cellMidX;
                        cellMaxY = cellMidY;
                        levelIndex |= 1;
                        continue;
                    }

                    if (rectangleMinY >= cellMidY)
                    {
                        IntersectRectangleWithCells(cells, levelOffset, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectRectangleWithCells(cells, levelOffset, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                    IntersectRectangleWithCells(cells, levelOffset, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                    IntersectRectangleWithCells(cells, levelOffset, rectangleMinX, rectangleMinY, rectangleMaxX, rectangleMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                    cellMinX = cellMidX;
                    cellMinY = cellMidY;
                    levelIndex |= 3;
                    continue;
                }

                cells.Add(levelOffset + levelIndex);

                break;
            }
        }
    }

    /// <summary>
    /// Intersects the spatial quad-tree with the tile.
    /// </summary>
    /// <param name="left">The lower-left x-coordinate.</param>
    /// <param name="bottom">The lower-right y-coordinate.</param>
    /// <param name="size">The size of the tile.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinTile(float left, float bottom, float size) => this.CellsWithinTile(left, bottom, size, this.levels);

    /// <summary>
    /// Intersects the spatial quad-tree with the tile.
    /// </summary>
    /// <param name="left">The lower-left x-coordinate.</param>
    /// <param name="bottom">The lower-right y-coordinate.</param>
    /// <param name="size">The size of the tile.</param>
    /// <param name="level">The level.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinTile(float left, float bottom, float size, int level)
    {
        var right = left + size;
        var top = bottom + size;

        if (right <= this.minimumX || left > this.maximumX || top <= this.minimumY || bottom > this.maximumY)
        {
            return [];
        }

        var cellsWithinTile = new List<int>();
        if (this.adaptive is not null)
        {
            IntersectTileWithCellsAdaptive(cellsWithinTile, left, bottom, right, top, this.minimumX, this.maximumX, this.minimumY, this.maximumY, 0, 0);
        }
        else
        {
            IntersectTileWithCells(cellsWithinTile, LevelOffset[level], left, bottom, right, top, this.minimumX, this.maximumX, this.minimumY, this.maximumY, level, 0);
        }

        return cellsWithinTile;

        void IntersectTileWithCellsAdaptive(ICollection<int> cells, float tileMinX, float tileMinY, float tileMaxX, float tileMaxY, float cellMinX, float cellMinY, float cellMaxX, float cellMaxY, int currentLevel, int levelIndex)
        {
            while (true)
            {
                var cellIndex = this.GetCellIndex(levelIndex, currentLevel);
                var adaptivePos = cellIndex / 32;
                var adaptiveBit = 1U << (cellIndex % 32);
                if ((currentLevel < this.levels) && ((this.adaptive[adaptivePos] & adaptiveBit) is not 0))
                {
                    currentLevel++;
                    levelIndex <<= 2;

                    var cellMidX = (cellMinX + cellMaxX) * 0.5F;
                    var cellMidY = (cellMinY + cellMaxY) * 0.5F;

                    if (tileMaxX <= cellMidX)
                    {
                        if (tileMaxY <= cellMidY)
                        {
                            cellMaxX = cellMidX;
                            cellMaxY = cellMidY;
                            continue;
                        }

                        if (tileMinY >= cellMidY)
                        {
                            cellMinY = cellMidY;
                            cellMaxX = cellMidX;
                            levelIndex |= 2;
                            continue;
                        }

                        IntersectTileWithCellsAdaptive(cells, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinY = cellMidY;
                        cellMaxX = cellMidX;
                        levelIndex |= 2;
                        continue;
                    }

                    if (tileMinX >= cellMidX)
                    {
                        if (tileMaxY <= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMaxY = cellMidY;
                            levelIndex |= 1;
                            continue;
                        }

                        if (tileMinY >= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMinY = cellMidY;
                            levelIndex |= 3;
                            continue;
                        }

                        IntersectTileWithCellsAdaptive(cells, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    if (tileMaxY <= cellMidY)
                    {
                        IntersectTileWithCellsAdaptive(cells, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinX = cellMidX;
                        cellMaxY = cellMidY;
                        levelIndex |= 1;
                        continue;
                    }

                    if (tileMinY >= cellMidY)
                    {
                        IntersectTileWithCellsAdaptive(cells, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectTileWithCellsAdaptive(cells, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                    IntersectTileWithCellsAdaptive(cells, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                    IntersectTileWithCellsAdaptive(cells, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                    cellMinX = cellMidX;
                    cellMinY = cellMidY;
                    levelIndex |= 3;
                    continue;
                }

                cells.Add(cellIndex);

                break;
            }
        }

        static void IntersectTileWithCells(IList<int> cells, int levelOffset, float tileMinX, float tileMinY, float tileMaxX, float tileMaxY, float cellMinX, float cellMinY, float cellMaxX, float cellMaxY, int currentLevel, int levelIndex)
        {
            while (true)
            {
                if (currentLevel is not 0)
                {
                    currentLevel--;
                    levelIndex <<= 2;

                    var cellMidX = (cellMinX + cellMaxX) * 0.5F;
                    var cellMidY = (cellMinY + cellMaxY) * 0.5F;

                    if (tileMaxX <= cellMidX)
                    {
                        if (tileMaxY <= cellMidY)
                        {
                            cellMaxX = cellMidX;
                            cellMaxY = cellMidY;
                            continue;
                        }

                        if (tileMinY >= cellMidY)
                        {
                            cellMinY = cellMidY;
                            cellMaxX = cellMidX;
                            levelIndex |= 2;
                            continue;
                        }

                        IntersectTileWithCells(cells, levelOffset, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinY = cellMidY;
                        cellMaxX = cellMidX;
                        levelIndex |= 2;
                        continue;
                    }

                    if (tileMinX >= cellMidX)
                    {
                        if (tileMaxY <= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMaxY = cellMidY;
                            levelIndex |= 1;
                            continue;
                        }

                        if (tileMinY >= cellMidY)
                        {
                            cellMinX = cellMidX;
                            cellMinY = cellMidY;
                            levelIndex |= 3;
                            continue;
                        }

                        IntersectTileWithCells(cells, levelOffset, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    if (tileMaxY <= cellMidY)
                    {
                        IntersectTileWithCells(cells, levelOffset, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                        cellMinX = cellMidX;
                        cellMaxY = cellMidY;
                        levelIndex |= 1;
                        continue;
                    }

                    if (tileMinY >= cellMidY)
                    {
                        IntersectTileWithCells(cells, levelOffset, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                        cellMinX = cellMidX;
                        cellMinY = cellMidY;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectTileWithCells(cells, levelOffset, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMinY, cellMidX, cellMidY, currentLevel, levelIndex);
                    IntersectTileWithCells(cells, levelOffset, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMidX, cellMinY, cellMaxX, cellMidY, currentLevel, levelIndex | 1);
                    IntersectTileWithCells(cells, levelOffset, tileMinX, tileMinY, tileMaxX, tileMaxY, cellMinX, cellMidY, cellMidX, cellMaxY, currentLevel, levelIndex | 2);
                    cellMinX = cellMidX;
                    cellMinY = cellMidY;
                    levelIndex |= 3;
                    continue;
                }

                cells.Add(levelOffset + levelIndex);

                break;
            }
        }
    }

    /// <summary>
    /// Gets the cell index for the specified x, y coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The cell index.</returns>
    internal int GetCellIndex(double x, double y) => this.GetCellIndex(x, y, this.levels);

    /// <summary>
    /// Gets the cell index for the specified x, y coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="width">The width of the cell.</param>
    /// <param name="height">The height of the cell.</param>
    /// <returns>The cell index.</returns>
    internal int GetCellIndex(double x, double y, double width, double height)
    {
        return this.GetCellIndex(x, y, GetLevelCore(width, height));

        int GetLevelCore(double requiredWidth, double requiredHeight)
        {
            double currentWidth = this.maximumX - this.minimumY;
            double currentHeight = this.maximumY - this.minimumY;

            var level = 0;
            while (requiredWidth < currentWidth && requiredHeight < currentHeight)
            {
                level++;
                currentHeight /= 2;
                currentWidth /= 2;
            }

            return level;
        }
    }

    /// <summary>
    /// Coarsens the quad-tree.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <param name="coarserCellIndex">The index for the coarser cell.</param>
    /// <param name="cellIndices">The cell indices.</param>
    /// <returns><see langword="true"/> if the cell was coarsened; otherwise <see langword="false"/>.</returns>
    internal bool Coarsen(int cellIndex, out int coarserCellIndex, out int[] cellIndices)
    {
        if (cellIndex < 0)
        {
            coarserCellIndex = default;
            cellIndices = [];
            return false;
        }

        var level = GetLevel(cellIndex);
        if (level is 0)
        {
            coarserCellIndex = default;
            cellIndices = [];
            return false;
        }

        var levelIndex = this.GetLevelIndex(cellIndex, level);

        levelIndex >>= 2;
        coarserCellIndex = this.GetCellIndex(levelIndex, level - 1);

        levelIndex <<= 2;
        cellIndices =
        [
            this.GetCellIndex(levelIndex + 0, level),
            this.GetCellIndex(levelIndex + 1, level),
            this.GetCellIndex(levelIndex + 2, level),
            this.GetCellIndex(levelIndex + 3, level),
        ];

        return true;
    }

    /// <summary>
    /// Create or finalize the cell (in the spatial hierarchy).
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <returns><see langword="true"/> if successful; otherwise <see langword="false"/>.</returns>
    internal bool ManageCell(int cellIndex)
    {
        var adaptivePosition = cellIndex / 32;
        this.adaptive ??= new uint[adaptivePosition + 1];
        if (adaptivePosition >= this.adaptive.Length)
        {
            // increase by 2
            Array.Resize(ref this.adaptive, adaptivePosition * 2);
        }

        var adaptiveBit = 1U << (cellIndex % 32);
        this.adaptive[adaptivePosition] &= ~adaptiveBit;
        var level = GetLevel(cellIndex);
        var levelIndex = this.GetLevelIndex(cellIndex, level);
        while (level is not 0)
        {
            level--;
            levelIndex >>= 2;
            var index = this.GetCellIndex(levelIndex, level);
            adaptivePosition = index / 32;
            adaptiveBit = 1U << (index % 32);
            if ((this.adaptive[adaptivePosition] & adaptiveBit) is not 0)
            {
                break;
            }

            this.adaptive[adaptivePosition] |= adaptiveBit;
        }

        return true;
    }

    private static int[] CreateLevelOffset()
    {
        var offsets = new int[20];
        for (var i = 0; i < 16; i++)
        {
            offsets[i + 1] = offsets[i] + ((1 << i) * (1 << i));
        }

        return offsets;
    }

    private static int GetLevel(int cellIndex)
    {
        var level = default(int);
        while (cellIndex >= LevelOffset[level + 1])
        {
            level++;
        }

        return level;
    }

    private int GetCellIndex(double x, double y, int level) => this.sublevel is not 0
        ? LevelOffset[this.sublevel + level] + (this.sublevelIndex << (level * 2)) + this.GetLevelIndex(x, y, level)
        : LevelOffset[level] + this.GetLevelIndex(x, y, level);

    private int GetCellIndex(int levelIndex, int level) => this.sublevel is not 0
        ? levelIndex + (this.sublevelIndex << (level * 2)) + LevelOffset[this.sublevel + level]
        : levelIndex + LevelOffset[level];

    private int GetLevelIndex(int cellIndex, int level) => this.sublevel is not 0
        ? cellIndex - (this.sublevelIndex << (level * 2)) - LevelOffset[this.sublevel + level]
        : cellIndex - LevelOffset[level];

    private int GetLevelIndex(double x, double y, int level)
    {
        double cellMinX = this.minimumX;
        double cellMaxX = this.maximumX;
        double cellMinY = this.minimumY;
        double cellMaxY = this.maximumY;

        var levelIndex = default(int);

        while (level is not 0)
        {
            levelIndex <<= 2;

            var cellMidX = (cellMinX + cellMaxX) / 2;
            var cellMidY = (cellMinY + cellMaxY) / 2;

            if (x < cellMidX)
            {
                cellMaxX = cellMidX;
            }
            else
            {
                cellMinX = cellMidX;
                levelIndex |= 1;
            }

            if (y < cellMidY)
            {
                cellMaxY = cellMidY;
            }
            else
            {
                cellMinY = cellMidY;
                levelIndex |= 2;
            }

            level--;
        }

        return levelIndex;
    }
}