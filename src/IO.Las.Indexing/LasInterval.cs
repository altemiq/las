// -----------------------------------------------------------------------
// <copyright file="LasInterval.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Indexing;

/// <summary>
/// The LAS interval.
/// </summary>
internal sealed class LasInterval : IEnumerable<KeyValuePair<int, LasIntervalStartCell>>
{
    private const int DefaultThreshold = 1000;
    private const string Signature = "LASV";
    private const uint SignatureValue = ((uint)'V' << 24) | ((uint)'S' << 16) | ((uint)'A' << 8) | (uint)'L';

    private readonly Dictionary<int, LasIntervalStartCell> cellsDictionary;

    private readonly int threshold = DefaultThreshold;

    private uint numberIntervals;

    private int lastIndex = int.MinValue;

    private LasIntervalStartCell? lastCell;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasInterval"/> class.
    /// </summary>
    /// <param name="threshold">The threshold.</param>
    public LasInterval(int threshold = DefaultThreshold)
        : this([]) => this.threshold = threshold;

    private LasInterval(Dictionary<int, LasIntervalStartCell> cells) => this.cellsDictionary = cells;

    /// <summary>
    /// Reads the interval from the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The LAS interval.</returns>
    public static LasInterval ReadFrom(Stream stream)
    {
        var length = (int)(stream.Length - stream.Position);
        var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(length);
        try
        {
            _ = stream.ReadAtLeast(bytes.AsSpan(0, length), length);
            return ReadFrom(bytes.AsSpan(0, length));
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    /// <summary>
    /// Reads the interval from the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The LAS interval.</returns>
    public static LasInterval ReadFrom(ReadOnlySpan<byte> source)
    {
        if (System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[..4]) is not SignatureValue)
        {
            ThrowInvalidSignature(System.Text.Encoding.UTF8.GetString(source[..4]), nameof(source));
        }

        // ignore 4..8
        var numberOfCells = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[8..12]);

        // pre-size the dictionary to avoid rehashing; `numberOfCells` is the exact final count
        var incoming = new Dictionary<int, LasIntervalStartCell>((int)numberOfCells);
        var index = 12;
        while (numberOfCells is not 0)
        {
            var cellIndex = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[index..]);
            index += sizeof(int);

            var intervals = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
            index += sizeof(uint);
            var full = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
            index += sizeof(uint);

            LasIntervalStartCell startCell;
            if (intervals is 0)
            {
                startCell = new LasIntervalStartCell { Full = full };
            }
            else
            {
                // consume the first interval off the span so we can initialize the start cell directly via its ctor
                var firstStart = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);
                var firstEnd = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);

                startCell = new LasIntervalStartCell(firstStart, firstEnd) { Full = full };
                intervals--;
            }

            incoming.Add(cellIndex, startCell);

            LasIntervalCell tail = startCell;
            while (intervals is not 0)
            {
                var next = new LasIntervalCell
                {
                    Start = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]),
                };
                index += sizeof(uint);
                next.End = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);

                startCell.Total += next.End - next.Start + 1;
                tail.Next = next;
                tail = next;
                intervals--;
            }

            numberOfCells--;
        }

        return new(incoming);

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
    /// Clones this into an empty instance.
    /// </summary>
    /// <returns>The empty instance.</returns>
    public LasInterval CloneEmpty() => new(this.threshold);

    /// <summary>
    /// Adds the point index, to the cell index.
    /// </summary>
    /// <param name="pointIndex">The point index.</param>
    /// <param name="cellIndex">The cell index.</param>
    /// <returns><see langword="true"/> if successful; otherwise <see langword="false" />.</returns>
    public bool Add(uint pointIndex, int cellIndex)
    {
        if (this.lastCell is null || this.lastIndex != cellIndex)
        {
            this.lastIndex = cellIndex;
            if (!this.cellsDictionary.TryGetValue(cellIndex, out var cell))
            {
                this.lastCell = new(pointIndex);
                this.cellsDictionary.Add(cellIndex, this.lastCell);
                this.numberIntervals++;
                return true;
            }

            this.lastCell = cell;
        }

        if (!this.lastCell.Add(pointIndex, this.threshold))
        {
            return false;
        }

        this.numberIntervals++;
        return true;
    }

    /// <summary>
    /// Adds the cell at the specified cell index.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <param name="cell">The cell to add.</param>
    /// <returns><see langword="true"/> if successful; otherwise <see langword="false" />.</returns>
    public bool Add(int cellIndex, LasIntervalStartCell cell) => this.cellsDictionary.TryAdd(cellIndex, cell);

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<int, LasIntervalStartCell>> GetEnumerator() => this.cellsDictionary.GetEnumerator();

    /// <inheritdoc/>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Gets the number of cells.
    /// </summary>
    /// <returns>The number of cells.</returns>
    public int GetNumberOfCells() => this.cellsDictionary.Count;

    /// <summary>
    /// Merge the cells.
    /// </summary>
    /// <param name="indices">The indices.</param>
    /// <param name="index">The new index.</param>
    /// <returns><see langword="true"/> if successful; otherwise <see langword="false" />.</returns>
    public bool MergeCells(int[] indices, int index)
    {
        if (indices is { Length: 1 })
        {
            if (!this.cellsDictionary.TryGetValue(indices[0], out var cell))
            {
                return false;
            }

            this.cellsDictionary.Add(index, cell);
            _ = this.cellsDictionary.Remove(indices[0]);
        }
        else
        {
            LasIntervalStartCell? mergedCells = default;
            var cellsToMerge = new List<LasIntervalStartCell>();
            foreach (var idx in indices)
            {
                _ = AddCellToMergeCellSet(cellsToMerge, idx, erase: true);
            }

            if (!MergeCore(ref mergedCells, cellsToMerge))
            {
                return false;
            }

            if (mergedCells is not null)
            {
                this.cellsDictionary.Add(index, mergedCells);
            }
        }

        return true;

        bool MergeCore(ref LasIntervalStartCell? mergedCells, ICollection<LasIntervalStartCell> cellsToMerge)
        {
            // discard any previous merge result
            mergedCells = default;

            switch (cellsToMerge)
            {
                // are there cells to merge
                case { Count: 0 }:
                    return false;

                // is there just one cell
                case IList<LasIntervalStartCell> { Count: 1 } cellsToMergeList:
                    // simply use this cell as the merge cell
                    mergedCells = cellsToMergeList[0];
                    break;

                case { Count: 1 }:
                    // simply use this cell as the merge cell
                    mergedCells = cellsToMerge.First();
                    break;

                default:
                    mergedCells = Merge(cellsToMerge, this.threshold, ref this.numberIntervals);
                    break;
            }

            return true;
        }

        bool AddCellToMergeCellSet(List<LasIntervalStartCell> cellsToMerge, int cellIndex, bool erase)
        {
            if (!this.cellsDictionary.TryGetValue(cellIndex, out var element))
            {
                return false;
            }

            cellsToMerge.Add(element);

            if (erase)
            {
                _ = this.cellsDictionary.Remove(cellIndex);
            }

            return true;
        }
    }

    /// <summary>
    /// Merges the intervals.
    /// </summary>
    /// <param name="maximumIntervals">The maximum intervals.</param>
    public void MergeIntervals(int maximumIntervals)
    {
        // each cell has minimum one interval
        if (maximumIntervals < this.cellsDictionary.Count)
        {
            maximumIntervals = default;
        }
        else
        {
            maximumIntervals -= this.cellsDictionary.Count;
        }

        // order intervals by smallest gap
        var sortedCells = new SortedList<uint, LasIntervalCell>(new DuplicateKeyComparer<uint>());
        foreach (var startCell in this.cellsDictionary.Values)
        {
            LasIntervalCell cell = startCell;
            while (cell.Next is { } next)
            {
                sortedCells.Add(next.Start - cell.End - 1, cell);
                cell = next;
            }
        }

        // maybe nothing to do
        if (sortedCells.Count <= maximumIntervals)
        {
            return;
        }

        var size = sortedCells.Count;

        while (size > maximumIntervals)
        {
            var element = sortedCells.First();
            var cell = element.Value;
            sortedCells.RemoveAt(0);
            if (cell is { Start: 1, End: 0 })
            {
                // signals that the cell is to be deleted
                this.numberIntervals--;
            }
            else if (cell.Next is { } cellToDelete)
            {
                cell.End = cellToDelete.End;
                cell.Next = cellToDelete.Next;
                if (cell.Next is not null)
                {
                    sortedCells.Add(cell.Next.Start - cell.End - 1, cell);

                    // signal that the cell is to be deleted
                    cellToDelete.Start = 1;
                    cellToDelete.End = default;
                }
                else
                {
                    this.numberIntervals--;
                }

                size--;
            }
        }

        // subtract any sentinel cells still pending in `sortedCells` (Start == 1, End == 0) that we did not
        // consume in the main loop, and recompute `Total` on every start cell in a single final sweep.
        var pendingSentinels = default(uint);
#pragma warning disable S3267 // plain foreach avoids the LINQ iterator on a hot path
        foreach (var cell in sortedCells.Values)
        {
            if (cell is { Start: 1, End: 0 })
            {
                pendingSentinels++;
            }
        }
#pragma warning restore S3267

        this.numberIntervals -= pendingSentinels;

        // update totals
        foreach (var startCell in this.cellsDictionary.Values)
        {
            startCell.Total = default;
            LasIntervalCell? cell = startCell;
            while (cell is not null)
            {
                startCell.Total += cell.End - cell.Start + 1;
                cell = cell.Next;
            }
        }
    }

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
        // write the 4-char ASCII signature as a little-endian uint to avoid the `char[4]` allocation from `ToCharArray`
        writer.Write(SignatureValue);
        writer.Write(0U); // version

        // number of cells
        writer.Write((uint)this.cellsDictionary.Count);

        var baseStream = writer.BaseStream;
        var canSeek = baseStream.CanSeek;

        // loop over all cells
        foreach (var item in this.cellsDictionary)
        {
            var numberOfPoints = item.Value.Full;

            // write index of cell
            writer.Write(item.Key);

            if (canSeek)
            {
                // reserve 4 bytes for the interval count, write the payload, then seek back and patch.
                // this folds the previous count-then-write double-walk into a single linked-list traversal.
                var countPosition = baseStream.Position;
                writer.Write(0U);
                writer.Write(numberOfPoints);

                var numberOfIntervals = default(uint);
                LasIntervalCell? cell = item.Value;
                while (cell is not null)
                {
                    writer.Write(cell.Start);
                    writer.Write(cell.End);
                    numberOfIntervals++;
                    cell = cell.Next;
                }

                var endPosition = baseStream.Position;
                baseStream.Position = countPosition;
                writer.Write(numberOfIntervals);
                baseStream.Position = endPosition;
            }
            else
            {
                // non-seekable stream: fall back to the two-pass approach
                var numberOfIntervals = default(uint);
                LasIntervalCell? cell = item.Value;
                while (cell is not null)
                {
                    numberOfIntervals++;
                    cell = cell.Next;
                }

                writer.Write(numberOfIntervals);
                writer.Write(numberOfPoints);

                cell = item.Value;
                while (cell is not null)
                {
                    writer.Write(cell.Start);
                    writer.Write(cell.End);
                    cell = cell.Next;
                }
            }
        }
    }

    /// <summary>
    /// Merges the specified cells.
    /// </summary>
    /// <param name="cells">The cells to merge.</param>
    /// <returns>The mered cell; or <see langword="null"/> if <paramref name="cells"/> is <see langword="null"/> or empty.</returns>
    public LasIntervalStartCell? Merge(ICollection<LasIntervalStartCell> cells)
    {
        var ignore = default(uint);
        return Merge(cells, this.threshold, ref ignore);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not LasInterval interval)
        {
            return false;
        }

        using var firstEnumerator = this.cellsDictionary.OrderBy(kvp => kvp.Key).GetEnumerator();
        using var secondEnumerator = interval.cellsDictionary.OrderBy(kvp => kvp.Key).GetEnumerator();

        while (firstEnumerator.MoveNext())
        {
            if (secondEnumerator.MoveNext())
            {
                var first = firstEnumerator.Current;
                var second = secondEnumerator.Current;

                if (first.Key != second.Key)
                {
                    return false;
                }

                if (!CheckCell(first.Value, second.Value))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            static bool CheckCell(LasIntervalCell? first, LasIntervalCell? second)
            {
                while (true)
                {
                    if (first is null)
                    {
                        return second is null;
                    }

                    if (second is null || first.Start != second.Start || first.End != second.End)
                    {
                        return false;
                    }

                    first = first.Next;
                    second = second.Next;
                }
            }
        }

        return !secondEnumerator.MoveNext();
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.cellsDictionary, this.threshold);

    /// <summary>
    /// Gets the cell at the specified index.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <param name="cell">When this method returns, the value associated with <paramref name="cellIndex"/>, if the cell index is found; otherwise <see langword="null" />. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the interval contains the cell index; otherwise <see langword=""/>.</returns>
    public bool TryGetCell(int cellIndex, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LasIntervalStartCell? cell) => this.cellsDictionary.TryGetValue(cellIndex, out cell);

    private static LasIntervalStartCell? Merge(ICollection<LasIntervalStartCell> cells, int threshold, ref uint numberOfIntervals)
    {
        return cells switch
        {
            null or { Count: 0 } => null,
            IList<LasIntervalStartCell> { Count: 1 } cellList => cellList[0],
            { Count: 1 } => cells.First(),
            { Count: > 1 } => MergeImpl(cells, threshold, ref numberOfIntervals),
            _ => throw new System.Diagnostics.UnreachableException(),
        };

        static LasIntervalStartCell MergeImpl(IEnumerable<LasIntervalStartCell> cells, int threshold, ref uint numberOfIntervals)
        {
            var merged = new LasIntervalStartCell();

            // collect every cell in the chain into a flat list and sort by Start; using a flat list avoids the
            // exception thrown by `SortedDictionary` on duplicate `Start` keys (which can occur with overlapping
            // intervals produced by coarsening) and removes per-insert O(log n) rebalancing.
            var sortedCells = new List<LasIntervalCell>();
            foreach (var cell in cells)
            {
                merged.Full += cell.Full;
                LasIntervalCell? next = cell;
                while (next is not null)
                {
                    sortedCells.Add(next);
                    next = next.Next;
                }
            }

            if (sortedCells is { Count: 0 })
            {
                return merged;
            }

            sortedCells.Sort(static (a, b) => a.Start.CompareTo(b.Start));

            // initialize merged with first interval
            var current = sortedCells[0];
            merged.Start = current.Start;
            merged.End = current.End;
            merged.Total = current.End - current.Start + 1;

            // merge intervals
            LasIntervalCell last = merged;
            for (var i = 1; i < sortedCells.Count; i++)
            {
                current = sortedCells[i];
                var diff = (int)(current.Start - last.End);
                if (diff > threshold)
                {
                    last.Next = new(current);
                    last = last.Next;
                    merged.Total += current.End - current.Start + 1;
                }
                else
                {
                    diff = (int)(current.End - last.End);
                    if (diff > 0)
                    {
                        last.End = current.End;
                        merged.Total += (uint)diff;
                    }

                    numberOfIntervals--;
                }
            }

            return merged;
        }
    }

    private sealed class DuplicateKeyComparer<TKey> : IComparer<TKey>
        where TKey : struct, IComparable
    {
        public int Compare(TKey x, TKey y) => x.CompareTo(y) switch
        {
            // Handle equality as being lesser. Note: this will break Remove(key) or
            0 => -1,
            var i => i,
        };
    }
}