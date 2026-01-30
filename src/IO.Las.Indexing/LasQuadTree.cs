// -----------------------------------------------------------------------
// <copyright file="LasQuadTree.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

using System.Numerics;

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
    private readonly Vector2 minimum;
    private readonly Vector2 maximum;
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
        this.minimum = new(
            boundingBoxMinX >= offsetX ? (cellSize * ((int)((boundingBoxMinX - offsetX) / cellSize))) + offsetX : (cellSize * ((int)((boundingBoxMinX - offsetX) / cellSize) - 1)) + offsetX,
            boundingBoxMinY >= offsetY ? (cellSize * ((int)((boundingBoxMinY - offsetY) / cellSize))) + offsetY : (cellSize * ((int)((boundingBoxMinY - offsetY) / cellSize) - 1)) + offsetY);
        this.maximum = new(
            boundingBoxMaxX >= offsetX ? (cellSize * ((int)((boundingBoxMaxX - offsetX) / cellSize) + 1)) + offsetX : (cellSize * ((int)((boundingBoxMaxX - offsetX) / cellSize))) + offsetX,
            boundingBoxMaxY >= offsetY ? (cellSize * ((int)((boundingBoxMaxY - offsetY) / cellSize) + 1)) + offsetY : (cellSize * ((int)((boundingBoxMaxY - offsetY) / cellSize))) + offsetY);

        // how many cells minimally in each direction
        var (horizonalCells, verticalCells) = UInt32Quantize((this.maximum - this.minimum) / cellSize);
        if (horizonalCells is 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, Properties.Resources.NoHorizontalCellsFound);
        }

        if (verticalCells is 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, Properties.Resources.NoVerticalCellsFound);
        }

        // how many quad tree levels to get to that many cells
        var cells = Math.Max(horizonalCells, verticalCells) - 1;
        while (cells is not 0)
        {
            cells >>= 1;
            this.levels++;
        }

        // enlarge bounding box to quad tree size
        var xc = (1U << this.levels) - horizonalCells;
        var xc1 = xc / 2;
        var xc2 = xc - xc1;

        var yc = (1U << this.levels) - verticalCells;
        var yc1 = yc / 2;
        var yc2 = yc - yc1;

        this.minimum -= new Vector2(xc2 * cellSize, yc2 * cellSize);
        this.maximum += new Vector2(xc1 * cellSize, yc1 * cellSize);

        static (uint X, uint Y) UInt32Quantize(Vector2 n)
        {
            var rounded = Vector2.Round(
                Vector2.Clamp(n, Vector2.Zero, new(float.MaxValue)),
                MidpointRounding.AwayFromZero);
            return ((uint)rounded.X, (uint)rounded.Y);
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
        : this(new(boundingBoxMinX, boundingBoxMinY), new(boundingBoxMaxX, boundingBoxMaxY), subLevel, subLevelIndex, levels)
    {
    }

    private LasQuadTree(Vector2 min, Vector2 max, int subLevel, int subLevelIndex, int levels)
    {
        this.minimum = min;
        this.maximum = max;

        // get the cell bounding box
        (this.minimum, this.maximum) = this.GetBounds(subLevel, subLevelIndex);

        this.levels = levels;
        this.sublevel = subLevel;
        this.sublevelIndex = subLevelIndex;
    }

    private LasQuadTree(float minX, float maxX, float minY, float maxY, int levels)
    {
        this.minimum = new(minX, minY);
        this.maximum = new(maxX, maxY);
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
    public LasQuadTree CloneEmpty() => new(this.minimum, this.maximum, this.sublevel, this.sublevelIndex, this.levels);

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
        writer.Write(this.minimum.X);
        writer.Write(this.maximum.X);
        writer.Write(this.minimum.Y);
        writer.Write(this.maximum.Y);
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
                        && other.maximum == this.maximum
                        && other.minimum == this.minimum
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
        hashCode.Add(this.maximum);
        hashCode.Add(this.minimum);
        hashCode.Add(this.sublevel);
        hashCode.Add(this.sublevelIndex);
        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString() =>
    $$"""
      {
        "Levels": {{this.levels}},
        "MaxX": {{this.maximum.X}},
        "MaxY": {{this.maximum.Y}},
        "MinX": {{this.minimum.X}},
        "MinY": {{this.minimum.Y}},
        "Sublevel": {{this.sublevel}},
        "SublevelIndex": {{this.sublevelIndex}}
      }
      """;

    /// <summary>
    /// Gets the bounds of the cell that the coordinates are within.
    /// </summary>
    /// <param name="vector">The coordinate.</param>
    /// <returns>The bounds of the cell that <paramref name="vector"/> is within.</returns>
    internal (Vector2 Minimum, Vector2 Maximum) GetBounds(Vector2D vector) => this.GetBounds(vector, this.levels);

    /// <summary>
    /// Gets the bounds of the cell that the coordinates are within at the required level.
    /// </summary>
    /// <param name="vector">The coordinate.</param>
    /// <param name="level">The required level.</param>
    /// <returns>The bounds of the cell that <paramref name="vector"/> is within.</returns>
    internal (Vector2 Minimum, Vector2 Maximum) GetBounds(Vector2D vector, int level)
    {
        var cellMin = this.minimum;
        var cellMax = this.maximum;

        while (level > 0)
        {
            var cellMid = (cellMin + cellMax) * 0.5F;
            if (vector.X < cellMid.X)
            {
                cellMax.X = cellMid.X;
            }
            else
            {
                cellMin.X = cellMid.X;
            }

            if (vector.Y < cellMid.Y)
            {
                cellMax.Y = cellMid.Y;
            }
            else
            {
                cellMin.Y = cellMid.Y;
            }

            level--;
        }

        return (cellMin, cellMax);
    }

    /// <summary>
    /// Gets the bounds of the specified cell.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <returns>The bounds of the cell that <paramref name="cellIndex"/> represents.</returns>
    internal (Vector2 Minimum, Vector2 MaximumY) GetBounds(int cellIndex)
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
    internal (Vector2 Minimum, Vector2 MaximumY) GetBounds(int level, int levelIndex)
    {
        var cellMin = this.minimum;
        var cellMax = this.maximum;

        while (level is not 0)
        {
            var index = (levelIndex >> (2 * (level - 1))) & 3;
            var cellMid = (cellMin + cellMax) * 0.5F;
            if ((index & 1) is not 0)
            {
                cellMin.X = cellMid.X;
            }
            else
            {
                cellMax.X = cellMid.X;
            }

            if ((index & 2) is not 0)
            {
                cellMin.Y = cellMid.Y;
            }
            else
            {
                cellMax.Y = cellMid.Y;
            }

            level--;
        }

        return (cellMin, cellMax);
    }

    /// <summary>
    /// Gets all the cells.
    /// </summary>
    /// <returns>The cell indexes.</returns>
    internal IList<int> AllCells() => this.CellsWithinRectangle(this.minimum, this.maximum, this.levels);

    /// <summary>
    /// Intersects the spatial quad-tree with the rectangle.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinRectangle(float minX, float minY, float maxX, float maxY) => this.CellsWithinRectangle(minX, minY, maxX, maxY, this.levels);

    /// <summary>
    /// Intersects the spatial quad-tree with the rectangle.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    /// <param name="level">The level.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinRectangle(float minX, float minY, float maxX, float maxY, int level) => this.CellsWithinRectangle(new(minX, minY), new(maxX, maxY), level);

    /// <summary>
    /// Intersects the spatial quad-tree with the rectangle.
    /// </summary>
    /// <param name="min">The minimum coordinate.</param>
    /// <param name="max">The maximum coordinate.</param>
    /// <param name="level">The level.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinRectangle(Vector2 min, Vector2 max, int level)
    {
        if (VectorMath.LessThanOrEqualAny(max, this.minimum) || VectorMath.GreaterThanAny(min, this.maximum))
        {
            return [];
        }

        var cellsInRectangle = new List<int>();
        if (this.adaptive is not null)
        {
            IntersectRectangleWithCellsAdaptive(cellsInRectangle, min, max, this.minimum, this.maximum, 0, 0);
        }
        else
        {
            IntersectRectangleWithCells(cellsInRectangle, LevelOffset[level], min, max, this.minimum, this.maximum, level, 0);
        }

        return cellsInRectangle;

        void IntersectRectangleWithCellsAdaptive(ICollection<int> cells, Vector2 rectangleMin, Vector2 rectangleMax, Vector2 cellMin, Vector2 cellMax, int currentLevel, int levelIndex)
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

                    var cellMid = (cellMin + cellMax) * 0.5F;

                    if (rectangleMax.X <= cellMid.X)
                    {
                        if (rectangleMax.Y <= cellMid.Y)
                        {
                            cellMax = cellMid;
                            continue;
                        }

                        if (rectangleMin.Y >= cellMid.Y)
                        {
                            cellMin.Y = cellMid.Y;
                            cellMax.X = cellMid.X;
                            levelIndex |= 2;
                            continue;
                        }

                        IntersectRectangleWithCellsAdaptive(cells, rectangleMin, rectangleMax, cellMin, cellMid, currentLevel, levelIndex);
                        cellMin.Y = cellMid.Y;
                        cellMax.X = cellMid.X;
                        levelIndex |= 2;
                        continue;
                    }

                    if (rectangleMin.X >= cellMid.X)
                    {
                        if (rectangleMax.Y <= cellMid.Y)
                        {
                            cellMin.X = cellMid.X;
                            cellMax.Y = cellMid.Y;
                            levelIndex |= 1;
                            continue;
                        }

                        if (rectangleMin.Y >= cellMid.Y)
                        {
                            cellMin = cellMid;
                            levelIndex |= 3;
                            continue;
                        }

                        IntersectRectangleWithCellsAdaptive(cells, rectangleMin, rectangleMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                        cellMin = cellMid;
                        levelIndex |= 3;
                        continue;
                    }

                    if (rectangleMax.Y <= cellMid.Y)
                    {
                        IntersectRectangleWithCellsAdaptive(cells, rectangleMin, rectangleMax, cellMin, cellMid, currentLevel, levelIndex);
                        cellMin.X = cellMid.X;
                        cellMax.Y = cellMid.Y;
                        levelIndex |= 1;
                        continue;
                    }

                    if (rectangleMin.Y >= cellMid.Y)
                    {
                        IntersectRectangleWithCellsAdaptive(cells, rectangleMin, rectangleMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                        cellMin = cellMid;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectRectangleWithCellsAdaptive(cells, rectangleMin, rectangleMax, cellMin, cellMid, currentLevel, levelIndex);
                    IntersectRectangleWithCellsAdaptive(cells, rectangleMin, rectangleMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                    IntersectRectangleWithCellsAdaptive(cells, rectangleMin, rectangleMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                    cellMin = cellMid;
                    levelIndex |= 3;
                    continue;
                }

                cells.Add(cellIndex);

                break;
            }
        }

        static void IntersectRectangleWithCells(ICollection<int> cells, int levelOffset, Vector2 rectangleMin, Vector2 rectangleMax, Vector2 cellMin, Vector2 cellMax, int currentLevel, int levelIndex)
        {
            while (currentLevel is not 0)
            {
                currentLevel--;
                levelIndex <<= 2;

                var cellMid = (cellMin + cellMax) * 0.5F;

                if (rectangleMax.X <= cellMid.X)
                {
                    if (rectangleMax.Y <= cellMid.Y)
                    {
                        cellMax = cellMid;
                        continue;
                    }

                    if (rectangleMin.Y >= cellMid.Y)
                    {
                        cellMin.Y = cellMid.Y;
                        cellMax.X = cellMid.X;
                        levelIndex |= 2;
                        continue;
                    }

                    IntersectRectangleWithCells(cells, levelOffset, rectangleMin, rectangleMax, cellMin, cellMid, currentLevel, levelIndex);
                    cellMin.Y = cellMid.Y;
                    cellMax.X = cellMid.X;
                    levelIndex |= 2;
                    continue;
                }

                if (rectangleMin.X >= cellMid.X)
                {
                    if (rectangleMax.Y <= cellMid.Y)
                    {
                        cellMin.X = cellMid.X;
                        cellMax.Y = cellMid.Y;
                        levelIndex |= 1;
                        continue;
                    }

                    if (rectangleMin.Y >= cellMid.Y)
                    {
                        cellMin = cellMid;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectRectangleWithCells(cells, levelOffset, rectangleMin, rectangleMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                    cellMin = cellMid;
                    levelIndex |= 3;
                    continue;
                }

                if (rectangleMax.Y <= cellMid.Y)
                {
                    IntersectRectangleWithCells(cells, levelOffset, rectangleMin, rectangleMax, cellMin, cellMid, currentLevel, levelIndex);
                    cellMin.X = cellMid.X;
                    cellMax.Y = cellMid.Y;
                    levelIndex |= 1;
                    continue;
                }

                if (rectangleMin.Y >= cellMid.Y)
                {
                    IntersectRectangleWithCells(cells, levelOffset, rectangleMin, rectangleMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                    cellMin = cellMid;
                    levelIndex |= 3;
                    continue;
                }

                IntersectRectangleWithCells(cells, levelOffset, rectangleMin, rectangleMax, cellMin, cellMid, currentLevel, levelIndex);
                IntersectRectangleWithCells(cells, levelOffset, rectangleMin, rectangleMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                IntersectRectangleWithCells(cells, levelOffset, rectangleMin, rectangleMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                cellMin = cellMid;
                levelIndex |= 3;
            }

            cells.Add(levelOffset + levelIndex);
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
        var bottomLeft = new Vector2(left, bottom);
        return this.CellsWithinTile(bottomLeft, bottomLeft + new Vector2(size), level);
    }

    /// <summary>
    /// Intersects the spatial quad-tree with the tile.
    /// </summary>
    /// <param name="bottomLeft">The lower-left coordinate.</param>
    /// <param name="topRight">The upper-right coordinate.</param>
    /// <param name="level">The level.</param>
    /// <returns>The intersected cell indexes.</returns>
    internal IList<int> CellsWithinTile(Vector2 bottomLeft, Vector2 topRight, int level)
    {
        if (VectorMath.LessThanOrEqualAny(topRight, this.minimum) || VectorMath.GreaterThanAny(bottomLeft, this.maximum))
        {
            return [];
        }

        var cellsWithinTile = new List<int>();
        if (this.adaptive is not null)
        {
            IntersectTileWithCellsAdaptive(cellsWithinTile, bottomLeft, topRight, this.minimum, this.maximum, 0, 0);
        }
        else
        {
            IntersectTileWithCells(cellsWithinTile, LevelOffset[level], bottomLeft, topRight, this.minimum, this.maximum, level, 0);
        }

        return cellsWithinTile;

        void IntersectTileWithCellsAdaptive(ICollection<int> cells, Vector2 tileMin, Vector2 tileMax, Vector2 cellMin, Vector2 cellMax, int currentLevel, int levelIndex)
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

                    var cellMid = (cellMin + cellMax) * 0.5F;

                    if (tileMax.X <= cellMid.X)
                    {
                        if (tileMax.Y <= cellMid.Y)
                        {
                            cellMax = cellMid;
                            continue;
                        }

                        if (tileMin.Y >= cellMid.Y)
                        {
                            cellMin.Y = cellMid.Y;
                            cellMax.X = cellMid.X;
                            levelIndex |= 2;
                            continue;
                        }

                        IntersectTileWithCellsAdaptive(cells, tileMin, tileMax, cellMin, cellMid, currentLevel, levelIndex);
                        cellMin.Y = cellMid.Y;
                        cellMax.X = cellMid.X;
                        levelIndex |= 2;
                        continue;
                    }

                    if (tileMin.X >= cellMid.X)
                    {
                        if (tileMax.Y <= cellMid.Y)
                        {
                            cellMin.X = cellMid.X;
                            cellMax.Y = cellMid.Y;
                            levelIndex |= 1;
                            continue;
                        }

                        if (tileMin.Y >= cellMid.Y)
                        {
                            cellMin = cellMid;
                            levelIndex |= 3;
                            continue;
                        }

                        IntersectTileWithCellsAdaptive(cells, tileMin, tileMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                        cellMin = cellMid;
                        levelIndex |= 3;
                        continue;
                    }

                    if (tileMax.Y <= cellMid.Y)
                    {
                        IntersectTileWithCellsAdaptive(cells, tileMin, tileMax, cellMin, cellMid, currentLevel, levelIndex);
                        cellMin.X = cellMid.X;
                        cellMax.Y = cellMid.Y;
                        levelIndex |= 1;
                        continue;
                    }

                    if (tileMin.Y >= cellMid.Y)
                    {
                        IntersectTileWithCellsAdaptive(cells, tileMin, tileMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                        cellMin = cellMid;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectTileWithCellsAdaptive(cells, tileMin, tileMax, cellMin, cellMid, currentLevel, levelIndex);
                    IntersectTileWithCellsAdaptive(cells, tileMin, tileMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                    IntersectTileWithCellsAdaptive(cells, tileMin, tileMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                    cellMin = cellMid;
                    levelIndex |= 3;
                    continue;
                }

                cells.Add(cellIndex);

                break;
            }
        }

        static void IntersectTileWithCells(ICollection<int> cells, int levelOffset, Vector2 tileMin, Vector2 tileMax, Vector2 cellMin, Vector2 cellMax, int currentLevel, int levelIndex)
        {
            while (currentLevel is not 0)
            {
                currentLevel--;
                levelIndex <<= 2;

                var cellMid = (cellMin + cellMax) * 0.5F;

                if (tileMax.X <= cellMid.X)
                {
                    if (tileMax.Y <= cellMid.Y)
                    {
                        cellMax = cellMid;
                        continue;
                    }

                    if (tileMin.Y >= cellMid.Y)
                    {
                        cellMin.Y = cellMid.Y;
                        cellMax.X = cellMid.X;
                        levelIndex |= 2;
                        continue;
                    }

                    IntersectTileWithCells(cells, levelOffset, tileMin, tileMax, cellMin, cellMid, currentLevel, levelIndex);
                    cellMin.Y = cellMid.Y;
                    cellMax.X = cellMid.X;
                    levelIndex |= 2;
                    continue;
                }

                if (tileMin.X >= cellMid.X)
                {
                    if (tileMax.Y <= cellMid.Y)
                    {
                        cellMin.X = cellMid.X;
                        cellMax.Y = cellMid.Y;
                        levelIndex |= 1;
                        continue;
                    }

                    if (tileMin.Y >= cellMid.Y)
                    {
                        cellMin = cellMid;
                        levelIndex |= 3;
                        continue;
                    }

                    IntersectTileWithCells(cells, levelOffset, tileMin, tileMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                    cellMin = cellMid;
                    levelIndex |= 3;
                    continue;
                }

                if (tileMax.Y <= cellMid.Y)
                {
                    IntersectTileWithCells(cells, levelOffset, tileMin, tileMax, cellMin, cellMid, currentLevel, levelIndex);
                    cellMin.X = cellMid.X;
                    cellMax.Y = cellMid.Y;
                    levelIndex |= 1;
                    continue;
                }

                if (tileMin.Y >= cellMid.Y)
                {
                    IntersectTileWithCells(cells, levelOffset, tileMin, tileMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                    cellMin = cellMid;
                    levelIndex |= 3;
                    continue;
                }

                IntersectTileWithCells(cells, levelOffset, tileMin, tileMax, cellMin, cellMid, currentLevel, levelIndex);
                IntersectTileWithCells(cells, levelOffset, tileMin, tileMax, new(cellMid.X, cellMin.Y), new(cellMax.X, cellMid.Y), currentLevel, levelIndex | 1);
                IntersectTileWithCells(cells, levelOffset, tileMin, tileMax, new(cellMin.X, cellMid.Y), new(cellMid.X, cellMax.Y), currentLevel, levelIndex | 2);
                cellMin = cellMid;
                levelIndex |= 3;
            }

            cells.Add(levelOffset + levelIndex);
        }
    }

    /// <summary>
    /// Gets the cell index for the specified x, y coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>The cell index.</returns>
    internal int GetCellIndex(float x, float y) => this.GetCellIndex(x, y, this.levels);

    /// <summary>
    /// Gets the cell index for the specified x, y coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="width">The width of the cell.</param>
    /// <param name="height">The height of the cell.</param>
    /// <returns>The cell index.</returns>
    internal int GetCellIndex(float x, float y, float width, float height) => this.GetCellIndex(new Vector2(x, y), new(width, height));

    /// <summary>
    /// Gets the cell index for the specified x, y coordinates.
    /// </summary>
    /// <param name="center">The center coordinate.</param>
    /// <param name="size">The size of the cell.</param>
    /// <returns>The cell index.</returns>
    internal int GetCellIndex(Vector2 center, Vector2 size)
    {
        return this.GetCellIndex(center.X, center.Y, GetLevelCore(size));

        int GetLevelCore(Vector2 required)
        {
            var current = this.maximum - this.minimum;

            var level = 0;
            while (VectorMath.LessThanAll(required, current))
            {
                level++;
                current *= 0.5F;
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

    private int GetCellIndex(float x, float y, int level) => this.sublevel is not 0
        ? LevelOffset[this.sublevel + level] + (this.sublevelIndex << (level * 2)) + this.GetLevelIndex(x, y, level)
        : LevelOffset[level] + this.GetLevelIndex(x, y, level);

    private int GetCellIndex(int levelIndex, int level) => this.sublevel is not 0
        ? levelIndex + (this.sublevelIndex << (level * 2)) + LevelOffset[this.sublevel + level]
        : levelIndex + LevelOffset[level];

    private int GetLevelIndex(int cellIndex, int level) => this.sublevel is not 0
        ? cellIndex - (this.sublevelIndex << (level * 2)) - LevelOffset[this.sublevel + level]
        : cellIndex - LevelOffset[level];

    private int GetLevelIndex(float x, float y, int level)
    {
        var cellMin = this.minimum;
        var cellMax = this.maximum;

        var levelIndex = default(int);

        while (level is not 0)
        {
            levelIndex <<= 2;

            var cellMid = (cellMin + cellMax) * 0.5F;

            if (x < cellMid.X)
            {
                cellMax.X = cellMid.X;
            }
            else
            {
                cellMin.X = cellMid.X;
                levelIndex |= 1;
            }

            if (y < cellMid.Y)
            {
                cellMax.Y = cellMid.Y;
            }
            else
            {
                cellMin.Y = cellMid.Y;
                levelIndex |= 2;
            }

            level--;
        }

        return levelIndex;
    }
}