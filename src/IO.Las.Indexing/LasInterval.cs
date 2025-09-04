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

    private readonly Dictionary<int, LasIntervalStartCell> cellsDictionary;

    private readonly int threshold = DefaultThreshold;

    private uint numberIntervals;

    private int lastIndex = int.MinValue;

    private LasIntervalStartCell? lastCell;

    private bool mergedCellsTemporary;

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
        var bytesRead = stream.Read(bytes, 0, length);
        var interval = ReadFrom(bytes.AsSpan(0, bytesRead));
        System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
        return interval;
    }

    /// <summary>
    /// Reads the interval from the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The LAS interval.</returns>
    public static LasInterval ReadFrom(ReadOnlySpan<byte> source)
    {
        var signature = System.Text.Encoding.UTF8.GetString(source[..4]);
        if (signature is not Signature)
        {
            ThrowInvalidSignature(signature, nameof(source));
        }

        // ignore 4..8
        var numberOfCells = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[8..12]);

        var incoming = new Dictionary<int, LasIntervalStartCell>();
        var index = 12;
        while (numberOfCells is not 0)
        {
            var cellIndex = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(source[index..]);
            index += sizeof(int);
            var startCell = new LasIntervalStartCell();
            incoming.Add(cellIndex, startCell);
            LasIntervalCell cell = startCell;

            var intervals = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
            index += sizeof(uint);
            startCell.Full = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
            index += sizeof(uint);
            startCell.Total = default;

            while (intervals is not 0)
            {
                cell.Start = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);
                cell.End = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);

                startCell.Total += cell.End - cell.Start + 1;
                intervals--;
                if (intervals is 0)
                {
                    continue;
                }

                cell.Next = new();
                cell = cell.Next;
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
    public bool Add(int cellIndex, LasIntervalStartCell cell)
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        => this.cellsDictionary.TryAdd(cellIndex, cell);
#else
    {
        try
        {
            this.cellsDictionary.Add(cellIndex, cell);
        }
        catch
        {
            return false;
        }

        return true;
    }
#endif

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

        bool MergeCore(ref LasIntervalStartCell? mergedCells, IList<LasIntervalStartCell> cellsToMerge)
        {
            // maybe delete temporary merge cells from the previous merge
            if (mergedCells is not null)
            {
                if (this.mergedCellsTemporary)
                {
                    var next = mergedCells.Next;
                    while (next is not null)
                    {
                        next = next.Next;
                    }
                }

                mergedCells = default;
            }

            // are there cells to merge
            if (cellsToMerge is { Count: 0 })
            {
                return false;
            }

            // is there just one cell
            if (cellsToMerge is { Count: 1 })
            {
                this.mergedCellsTemporary = false;

                // simply use this cell as the merge cell
                mergedCells = cellsToMerge[0];
            }
            else
            {
                this.mergedCellsTemporary = true;
                mergedCells = Merge(cellsToMerge, this.threshold, ref this.numberIntervals);
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
        foreach (var startCell in this.cellsDictionary.Select(static item => item.Value))
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

        foreach (var startCell in sortedCells
            .Select(static item => item.Value)
            .Where(static cell => cell is { Start: 1, End: 0 }))
        {
            this.numberIntervals--;
        }

        // update totals
        foreach (var startCell in this.cellsDictionary.Select(static item => item.Value))
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
        writer.Write(Signature.ToCharArray());
        writer.Write(0U); // version

        // number of cells
        writer.Write((uint)this.cellsDictionary.Count);

        // loop over all cells
        foreach (var item in this.cellsDictionary)
        {
            // count number of intervals and points in cell
            var numberOfIntervals = default(uint);
            var numberOfPoints = item.Value.Full;

            LasIntervalCell? cell = item.Value;
            while (cell is not null)
            {
                numberOfIntervals++;
                cell = cell.Next;
            }

            // write index of cell
            writer.Write(item.Key);
            writer.Write(numberOfIntervals);
            writer.Write(numberOfPoints);

            // write intervals
            cell = item.Value;
            while (cell is not null)
            {
                writer.Write(cell.Start);
                writer.Write(cell.End);
                cell = cell.Next;
            }
        }
    }

    /// <summary>
    /// Merges the specified cells.
    /// </summary>
    /// <param name="cells">The cells to merge.</param>
    /// <returns>The mered cell; or <see langword="null"/> if <paramref name="cells"/> is <see langword="null"/> or empty.</returns>
    public LasIntervalStartCell? Merge(IList<LasIntervalStartCell> cells)
    {
        var ignore = default(uint);
        return Merge(cells, this.threshold, ref ignore);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is LasInterval interval)
        {
            var firstEnumerator = this.cellsDictionary.GetEnumerator();
            var secondEnumerator = interval.cellsDictionary.GetEnumerator();

            if (firstEnumerator.MoveNext())
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
            }
            else if (secondEnumerator.MoveNext())
            {
                return false;
            }

            return true;
        }

        return false;

        static bool CheckCell(LasIntervalCell? first, LasIntervalCell? second)
        {
            return first is null
                ? second is null
                : second is not null && first.Start == second.Start && first.End == second.End && CheckCell(first.Next, second.Next);
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
#if NETSTANDARD2_0_OR_GREATER || NET46_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        => HashCode.Combine(this.cellsDictionary, this.threshold);
#else
        => (this.cells, this.threshold).GetHashCode();
#endif

    /// <summary>
    /// Gets the cell at the specified index.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <param name="cell">When this method returns, the value associated with <paramref name="cellIndex"/>, if the cell index is found; otherwise <see langword="null" />. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the interval contains the cell index; otherwise <see langword=""/>.</returns>
    public bool TryGetCell(int cellIndex, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LasIntervalStartCell? cell) => this.cellsDictionary.TryGetValue(cellIndex, out cell);

    private static LasIntervalStartCell? Merge(IList<LasIntervalStartCell> cells, int threshold, ref uint numberOfIntervals)
    {
        return cells switch
        {
            null or { Count: 0 } => null,
            { Count: 1 } c => c[0],
            { Count: > 1 } c => MergeImpl(c, threshold, ref numberOfIntervals),
        };

        static LasIntervalStartCell MergeImpl(IList<LasIntervalStartCell> cells, int threshold, ref uint numberOfIntervals)
        {
            var merged = new LasIntervalStartCell();
            var sortedCells = new SortedDictionary<uint, LasIntervalCell>();
            foreach (var cell in cells)
            {
                merged.Full += cell.Full;
                LasIntervalCell? next = cell;
                while (next is not null)
                {
                    sortedCells.Add(next.Start, next);
                    next = next.Next;
                }
            }

            if (sortedCells is { Count: 0 })
            {
                return merged;
            }

            // initialize merged with first interval
            using var enumerator = sortedCells.GetEnumerator();

            // we know we have at least one.
            _ = enumerator.MoveNext();

            var current = enumerator.Current.Value;
            merged.Start = current.Start;
            merged.End = current.End;
            merged.Total = current.End - current.Start + 1;

            // merge intervals
            LasIntervalCell last = merged;
            while (enumerator.MoveNext())
            {
                current = enumerator.Current.Value;
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
            { } i => i,
        };
    }
}