// -----------------------------------------------------------------------
// <copyright file="LasIndex.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// LAS index.
/// </summary>
public class LasIndex : IEnumerable<LasIndexCell>
{
    /// <summary>
    /// The default threshold.
    /// </summary>
    public const int DefaultThreshold = 1000;

    /// <summary>
    /// The default minimum points.
    /// </summary>
    public const int DefaultMinimumPoints = 100000;

    /// <summary>
    /// The default maximum intervals.
    /// </summary>
    public const int DefaultMaximumIntervals = -20;

    /// <summary>
    /// The default tile size.
    /// </summary>
    public const float DefaultTileSize = default;

    private const string Signature = "LASX";
    private readonly LasQuadTree spatial;
    private readonly LasInterval interval;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasIndex"/> class.
    /// </summary>
    /// <param name="spatial">The spatial quad-tree.</param>
    /// <param name="threshold">The interval threshold.</param>
    public LasIndex(LasQuadTree spatial, int threshold = 1000)
        : this(spatial, new LasInterval(threshold))
    {
    }

    private LasIndex(LasQuadTree spatial, LasInterval interval)
    {
        this.spatial = spatial;
        this.interval = interval;
    }

    /// <summary>
    /// Creates the index from the LAS reader   .
    /// </summary>
    /// <param name="reader">The LAS reader.</param>
    /// <param name="tileSize">The tile size.</param>
    /// <param name="maximumIntervals">The maximum interval.</param>
    /// <param name="minimumPoints">The minimum points.</param>
    /// <param name="threshold">The threshold.</param>
    /// <returns>The LAS index.</returns>
    public static LasIndex Create(ILasReader reader, float tileSize = DefaultTileSize, int maximumIntervals = DefaultMaximumIntervals, uint minimumPoints = DefaultMinimumPoints, int threshold = DefaultThreshold)
    {
        var header = reader.Header;
        if (tileSize is DefaultTileSize)
        {
#pragma warning disable IDE0055
            tileSize = (header.Max.X - header.Min.X, header.Max.Y - header.Min.Y) switch
            {
                (< 1000, < 1000) => 10F,
                (< 10000, < 10000) => 100F,
                (< 100000, < 100000) => 1000F,
                (< 1000000, < 1000000) => 10000F,
                _ => 100000F,
            };
#pragma warning restore IDE0055
        }

        var quadTree = new LasQuadTree(header.Min.X, header.Max.X, header.Min.Y, header.Max.Y, tileSize);
        var index = new LasIndex(quadTree, threshold);
        var quantizer = new PointDataRecordQuantizer(header);

        var current = default(uint);
        while (reader.ReadPointDataRecord() is { PointDataRecord: { } record })
        {
            _ = index.Add(quantizer.GetX(record.X), quantizer.GetY(record.Y), current++);
        }

        index.Complete(minimumPoints, maximumIntervals);
        return index;
    }

    /// <summary>
    /// Reads the index from the specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The LAS index.</returns>
    public static LasIndex ReadFrom(string path)
    {
        using var stream = File.OpenRead(path);
        return ReadFrom(stream);
    }

    /// <summary>
    /// Reads the index from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The LAS index.</returns>
    public static LasIndex ReadFrom(Stream stream)
    {
        if (stream is ICacheStream prepareStream)
        {
            // download the stream
            prepareStream.Cache(0, (int)stream.Length);
        }

        var length = (int)(stream.Length - stream.Position);
        var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(length);
        var bytesRead = stream.Read(bytes, 0, length);
        var index = ReadFrom(bytes.AsSpan(0, bytesRead));
        System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
        return index;
    }

    /// <summary>
    /// Reads the index from the specified reader.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The LAS index.</returns>
    public static LasIndex ReadFrom(ReadOnlySpan<byte> source)
    {
        var signature = System.Text.Encoding.UTF8.GetString(source[..4]);
        if (signature is not Signature)
        {
            ThrowInvalidSignature(signature, nameof(source));
        }

        // ignore 4..8
        return Create(LasQuadTree.ReadFrom(source[8..52]), LasInterval.ReadFrom(source[52..]));

        static LasIndex Create(LasQuadTree spatial, LasInterval interval)
        {
            ManageCells(interval, spatial);
            return new(spatial, interval);
        }

#if NET8_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1863:Use 'CompositeFormat'", Justification = "This is a formatted string for an exception.")]
#endif
        [System.Diagnostics.CodeAnalysis.DoesNotReturn]
        static void ThrowInvalidSignature(string signature, string paramName)
        {
            throw new ArgumentException(string.Format(Las.Properties.Resources.Culture, Las.Properties.Resources.InvalidSignature, Signature, signature), paramName);
        }
    }

    /// <summary>
    /// Reads the index from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LAS index.</returns>
    public static async ValueTask<LasIndex> ReadFromAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is IAsyncCacheStream asyncCacheStream)
        {
            await asyncCacheStream.CacheAsync(0, (int)stream.Length, cancellationToken).ConfigureAwait(false);
        }
        else if (stream is ICacheStream cacheStream)
        {
            // download the stream
            cacheStream.Cache(0, (int)stream.Length);
        }

        var length = (int)(stream.Length - stream.Position);
        var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(length);
        var bytesRead
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            = await stream.ReadAsync(bytes.AsMemory(0, length), cancellationToken).ConfigureAwait(false);
#else
            = await stream.ReadAsync(bytes, 0, length, cancellationToken).ConfigureAwait(false);
#endif

        var index = ReadFrom(bytes.AsSpan(0, bytesRead));
        System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
        return index;
    }

    /// <summary>
    /// Clones this into an empty instance.
    /// </summary>
    /// <returns>The empty instance.</returns>
    public LasIndex CloneEmpty() => new(this.spatial.CloneEmpty(), this.interval.CloneEmpty());

    /// <summary>
    /// Returns this instance as a read-only list of <see cref="LasIndexCell"/>.
    /// </summary>
    /// <returns>A read-only list of <see cref="LasIndexCell"/>.</returns>
    public IReadOnlyList<LasIndexCell> AsReadOnly() => new ReadOnlyLasIndex([.. this]);

    /// <summary>
    /// Adds the specified x, y point to the index.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="index">The index.</param>
    /// <returns><see langword="true"/> if the point was added; otherwise <see langword="false"/>.</returns>
    public bool Add(double x, double y, uint index)
    {
        var cell = this.spatial.GetCellIndex(x, y);
        return this.interval.Add(index, cell);
    }

    /// <summary>
    /// Adds the specified start and end at the specified index to the index.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <param name="start">The start point index.</param>
    /// <param name="end">The end point index.</param>
    /// <returns><see langword="true"/> if the cell was added; otherwise <see langword="false"/>.</returns>
    public bool Add(int cellIndex, uint start, uint end) => this.interval.Add(cellIndex, new(start, end));

    /// <summary>
    /// Complete's the index.
    /// </summary>
    /// <param name="minimumPoints">The minimum points.</param>
    /// <param name="maximumIntervals">The maximum intervals.</param>
    public void Complete(uint minimumPoints, int maximumIntervals)
    {
        if (minimumPoints is not 0)
        {
            var cellHashes = new[]
            {
                GetHashes(this.interval),
                new Dictionary<int, uint>(),
            };

            var firstHash = default(int);
            while (cellHashes[firstHash].Count > 0)
            {
                var secondHash = (firstHash + 1) % 2;
                cellHashes[secondHash].Clear();

                // coarsen if a coarser cell will still have fewer than the minimum points (and points in all subcells)
                var coarsened = false;

                using var enumerator = cellHashes[firstHash].GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var outer = enumerator.Current;
                    if (outer.Value is not 0
                        && this.spatial.Coarsen(outer.Key, out var coarserIndex, out var indices))
                    {
                        var full = default(uint);
                        var numberFilled = default(uint);
                        for (var i = 0U; i < indices.Length; i++)
                        {
                            KeyValuePair<int, uint> inner;
                            if (outer.Key == indices[i])
                            {
                                inner = outer;
                            }
                            else if (cellHashes[firstHash].TryGetValue(indices[i], out var temp))
                            {
                                inner = new(indices[i], temp);
                            }
                            else
                            {
                                continue;
                            }

                            full += inner.Value;
                            cellHashes[firstHash][inner.Key] = default;
                            numberFilled++;
                        }

                        if ((full < minimumPoints) && (numberFilled == indices.Length))
                        {
                            _ = this.interval.MergeCells(indices, coarserIndex);
                            coarsened = true;
                            cellHashes[secondHash][coarserIndex] = full;
                        }
                    }
                }

                if (!coarsened)
                {
                    break;
                }

                firstHash = (firstHash + 1) % 2;
            }

            ManageCells(this.interval, this.spatial);

            static IDictionary<int, uint> GetHashes(LasInterval interval)
            {
                var cellHash = new Dictionary<int, uint>();
                using var enumerator = interval.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    cellHash[current.Key] = current.Value.Full;
                }

                return cellHash;
            }
        }

        if (maximumIntervals < 0)
        {
            maximumIntervals = -maximumIntervals * this.interval.GetNumberOfCells();
        }

        if (maximumIntervals is not 0)
        {
            this.interval.MergeIntervals(maximumIntervals);
        }
    }

    /// <summary>
    /// Gets the index of the cell containing the point index.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    /// <returns>The index of the cell containing <paramref name="pointIndex"/>; otherwise -1.</returns>
    public int IndexOf(uint pointIndex)
    {
        // traverse the tree to find the index
        var i = this.interval.FirstOrDefault(i => Contains(i.Value, pointIndex));
        return i.Value is null ? -1 : i.Key;

        static bool Contains(LasIntervalStartCell cell, uint index)
        {
            LasIntervalCell? current = cell;
            while (current is not null)
            {
                if (current.Start >= index && current.End <= index)
                {
                    return true;
                }

                current = current.Next;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the index of the cell.
    /// </summary>
    /// <param name="cell">The cell to get the index for.</param>
    /// <returns>The cell index.</returns>
    public int IndexOf(LasIndexCell cell)
    {
        var width = cell.MaximumX - cell.MinimumX;
        var height = cell.MaximumY - cell.MinimumY;
        var x = cell.MinimumX + (width / 2);
        var y = cell.MinimumY + (height / 2);
        return this.GetCellIndex(x, y, width, height);
    }

    /// <summary>
    /// Gets the cell index.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="width">The cell width.</param>
    /// <param name="height">The cell height.</param>
    /// <returns>The cell index.</returns>
    public int GetCellIndex(double x, double y, double width, double height) => this.spatial.GetCellIndex(x, y, width, height);

    /// <summary>
    /// Gets all the ranges.
    /// </summary>
    /// <returns>The ranges.</returns>
    public IEnumerable<Range> All() => GetRanges(this.GetCells(this.spatial.AllCells()));

    /// <summary>
    /// Gets the ranges within the specified rectangle.
    /// </summary>
    /// <param name="minX">The minimum x-coordinate.</param>
    /// <param name="minY">The minimum y-coordinate.</param>
    /// <param name="maxX">The maximum x-coordinate.</param>
    /// <param name="maxY">The maximum y-coordinate.</param>
    /// <returns>The ranges.</returns>
    public IEnumerable<Range> WithinRectangle(double minX, double minY, double maxX, double maxY) => GetRanges(this.GetCells(this.spatial.CellsWithinRectangle(minX, minY, maxX, maxY)));

    /// <summary>
    /// Gets the ranges within the specified tile.
    /// </summary>
    /// <param name="left">The lower-left x-coordinate.</param>
    /// <param name="bottom">The lower-right y-coordinate.</param>
    /// <param name="size">The size of the tile.</param>
    /// <returns>The ranges.</returns>
    public IEnumerable<Range> WithinTile(float left, float bottom, float size) => GetRanges(this.GetCells(this.spatial.CellsWithinTile(left, bottom, size)));

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
        writer.Write(Signature.ToCharArray());
        writer.Write(0U); // version
        this.spatial.WriteTo(writer);
        this.interval.WriteTo(writer);
    }

    /// <inheritdoc/>
    public IEnumerator<LasIndexCell> GetEnumerator() => new Enumerator(this);

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is LasIndex index
        && this.spatial.Equals(index.spatial)
        && this.interval.Equals(index.interval);

    /// <inheritdoc/>
    public override int GetHashCode()
#if NETSTANDARD2_0_OR_GREATER || NET46_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        => HashCode.Combine(this.spatial, this.interval);
#else
        => (this.spatial, this.interval).GetHashCode();
#endif

    private static void ManageCells(LasInterval interval, LasQuadTree spatial)
    {
        using var enumerator = interval.GetEnumerator();
        while (enumerator.MoveNext())
        {
            _ = spatial.ManageCell(enumerator.Current.Key);
        }
    }

    private static IEnumerable<Range> GetRanges(LasIntervalStartCell? cell)
    {
        LasIntervalCell? current = cell;
        while (current is not null)
        {
            yield return new(Index.FromStart(current.Start), Index.FromStart(current.End));
            current = current.Next;
        }
    }

    private LasIntervalStartCell? GetCells(IList<int> cellIndexes)
    {
        var cellsToMerge = new List<LasIntervalStartCell>();

        foreach (var cellIndex in cellIndexes)
        {
            if (this.interval.TryGetCell(cellIndex, out var cell))
            {
                cellsToMerge.Add(cell);
            }
        }

        return this.interval.Merge(cellsToMerge);
    }

    private readonly struct Enumerator(LasIndex index) : IEnumerator<LasIndexCell>
    {
        private readonly IEnumerator<KeyValuePair<int, LasIntervalStartCell>> enumerator = index.interval.GetEnumerator();

        private readonly LasQuadTree quadTree = index.spatial;

        private readonly int hashCode = index.GetHashCode();

        public LasIndexCell Current
        {
            get
            {
                var current = this.enumerator.Current;
                var (minimumX, minimumY, maximumX, maximumY) = this.quadTree.GetBounds(current.Key);
                var ranges = GetRanges(current.Value);
                return new(minimumX, minimumY, maximumX, maximumY, ranges);
            }
        }

        object System.Collections.IEnumerator.Current => this.Current;

        public void Dispose() => this.enumerator.Dispose();

        public bool MoveNext() => this.enumerator.MoveNext();

        public void Reset() => this.enumerator.Reset();

        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj) => obj is Enumerator e && this.quadTree == e.quadTree;

        public override int GetHashCode() => this.hashCode;

        public override string ToString() => this.quadTree.ToString();
    }

    private readonly struct ReadOnlyLasIndex(LasIndexCell[] cells) : IReadOnlyList<LasIndexCell>
    {
        private readonly LasIndexCell[] cells = cells;

        public int Count => this.cells.Length;

        public LasIndexCell this[int index] => this.cells[index];

        public IEnumerator<LasIndexCell> GetEnumerator() => new ReadOnlyEnumerator(this.cells);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

        private struct ReadOnlyEnumerator(LasIndexCell[] cells) : IEnumerator<LasIndexCell>
        {
            private readonly LasIndexCell[] cells = cells;

            private int index = -1;

            readonly LasIndexCell IEnumerator<LasIndexCell>.Current => this.cells[this.index];

            readonly object System.Collections.IEnumerator.Current => this.cells[this.index];

            bool System.Collections.IEnumerator.MoveNext() => ++this.index < this.cells.Length;

            void System.Collections.IEnumerator.Reset() => this.index = -1;

            readonly void IDisposable.Dispose()
            {
            }
        }
    }
}