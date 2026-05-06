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
    private const uint SignatureValue = ((uint)'V' << 24) | ((uint)'S' << 16) | ((uint)'A' << 8) | 'L';

    private readonly Dictionary<int, LasIntervalStartCell> cellsDictionary;

    private readonly IntervalArena arena;

    private readonly int threshold = DefaultThreshold;

    private uint numberIntervals;

    private int lastIndex = int.MinValue;

    private LasIntervalStartCell? lastCell;

    /// <summary>
    /// Initializes a new instance of the <see cref="LasInterval"/> class.
    /// </summary>
    /// <param name="threshold">The threshold.</param>
    public LasInterval(int threshold = DefaultThreshold)
        : this([], new()) => this.threshold = threshold;

    private LasInterval(Dictionary<int, LasIntervalStartCell> cells, IntervalArena arena)
    {
        this.cellsDictionary = cells;
        this.arena = arena;
    }

    /// <summary>
    /// Gets the arena backing this interval's tail cells.
    /// </summary>
    public IntervalArena Arena => this.arena;

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

        // the on-disk format does not expose a total interval count; seed the arena with a reasonable
        // starting capacity (one tail per cell is typical) and let it grow as needed
        var tailArena = new IntervalArena((int)numberOfCells);

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
                startCell = new() { Full = full };
            }
            else
            {
                // consume the first interval off the span so we can initialize the start cell directly via its ctor
                var firstStart = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);
                var firstEnd = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);

                startCell = new(firstStart, firstEnd) { Full = full };
                intervals--;
            }

            incoming.Add(cellIndex, startCell);

            // allocate tail cells directly into the arena, linking as we go
            var previous = IntervalArena.NullIndex;
            while (intervals is not 0)
            {
                var tailStart = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);
                var tailEnd = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(source[index..]);
                index += sizeof(uint);

                startCell.Total += tailEnd - tailStart + 1;

                var tailIndex = tailArena.Allocate(tailStart, tailEnd);
                if (previous is IntervalArena.NullIndex)
                {
                    startCell.FirstTail = tailIndex;
                }
                else
                {
                    tailArena[previous].Next = tailIndex;
                }

                previous = tailIndex;
                intervals--;
            }

            numberOfCells--;
        }

        return new(incoming, tailArena);

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

        if (!this.lastCell.Add(pointIndex, this.threshold, this.arena))
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
    /// Enumerates the intervals contained within the specified <paramref name="startCell"/> as
    /// <c>(Start, End)</c> pairs. Intended for use by consumers that want to walk a chain without
    /// touching the arena directly.
    /// </summary>
    /// <param name="startCell">The start cell.</param>
    /// <returns>The intervals in the chain.</returns>
    public IEnumerable<(uint Start, uint End)> EnumerateIntervals(LasIntervalStartCell startCell)
    {
        if (startCell.HasInlineInterval)
        {
            yield return (startCell.Start, startCell.End);
        }

        var cursor = startCell.FirstTail;
        while (cursor is not IntervalArena.NullIndex)
        {
            var cell = this.arena[cursor];
            yield return (cell.Start, cell.End);
            cursor = cell.Next;
        }
    }

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
                    mergedCells = Merge(cellsToMerge, this.threshold, this.arena, ref this.numberIntervals);
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

        // build a flat list of (gap, donor) tuples sorted by gap where the donor identifies the cell
        // whose tail-end will be extended if we decide to eliminate the gap. When the gap lives
        // between the inline head and the first tail, the donor is the start cell itself (held as
        // a reference). Other-wise the donor is an arena tail index.
        //
        // Entries can be invalidated when the referenced tail cell is swallowed by an earlier merge;
        // we detect this by tombstoning the swallowed cell in the arena (Start == 1, End == 0).
        var sortedGaps = new SortedList<uint, GapEntry>(new DuplicateKeyComparer<uint>());
        foreach (var startCell in this.cellsDictionary.Values)
        {
            if (startCell.FirstTail is IntervalArena.NullIndex)
            {
                continue;
            }

            // gap between inline head and first tail
            sortedGaps.Add(
                this.arena[startCell.FirstTail].Start - startCell.End - 1,
                GapEntry.ForHead(startCell));

            // gaps between consecutive tails
            var cursor = startCell.FirstTail;
            while (this.arena[cursor].Next is not IntervalArena.NullIndex)
            {
                var next = this.arena[cursor].Next;
                sortedGaps.Add(
                    this.arena[next].Start - this.arena[cursor].End - 1,
                    GapEntry.ForTail(cursor));
                cursor = next;
            }
        }

        // maybe nothing to do
        if (sortedGaps.Count <= maximumIntervals)
        {
            return;
        }

        var size = sortedGaps.Count;

        while (size > maximumIntervals)
        {
            var element = sortedGaps.First();
            var entry = element.Value;
            sortedGaps.RemoveAt(0);

            // tomb-stoned tail? decrement the running counter and move on.
            if (entry.Kind is GapEntry.OwnerKind.Tail && IsTombstone(this.arena[entry.TailIndex]))
            {
                this.numberIntervals--;
                continue;
            }

            if (!this.TryCollapseGap(entry, out var swallowedTail))
            {
                continue;
            }

            // mark the swallowed tail as a tombstone so any later sortedGaps entry keyed on it is
            // skipped when it is popped, mirroring the original class-based behavior where the
            // consumed object was mutated in place with `Start = 1, End = 0`.
            if (swallowedTail is not IntervalArena.NullIndex)
            {
                this.arena[swallowedTail].Start = 1U;
                this.arena[swallowedTail].End = 0U;
            }

            // collapse succeeded; determine whether there's a new gap to re-insert
            if (this.TryGetPostCollapseGap(entry, out var newGap))
            {
                sortedGaps.Add(newGap, entry);
            }
            else
            {
                this.numberIntervals--;
            }

            size--;
        }

        // subtract any tombstone entries still pending in `sortedGaps` that we did not consume.
        var pendingSentinels = default(uint);
#pragma warning disable S3267 // plain foreach avoids the LINQ iterator on a hot path
        foreach (var entry in sortedGaps.Values)
        {
            if (entry.Kind is GapEntry.OwnerKind.Tail && IsTombstone(this.arena[entry.TailIndex]))
            {
                pendingSentinels++;
            }
        }
#pragma warning restore S3267

        this.numberIntervals -= pendingSentinels;

        // update totals
        foreach (var startCell in this.cellsDictionary.Values)
        {
            var total = startCell.HasInlineInterval ? startCell.End - startCell.Start + 1 : 0U;
            var cursor = startCell.FirstTail;
            while (cursor is not IntervalArena.NullIndex)
            {
                ref var cell = ref this.arena[cursor];
                total += cell.End - cell.Start + 1;
                cursor = cell.Next;
            }

            startCell.Total = total;
        }

        static bool IsTombstone(LasIntervalCell cell) => cell is { Start: 1U, End: 0U };
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
        foreach (var (key, startCell) in this.cellsDictionary)
        {
            var numberOfPoints = startCell.Full;

            // write index of cell
            writer.Write(key);

            if (canSeek)
            {
                // reserve 4 bytes for the interval count, write the payload, then seek back and patch.
                // this folds the previous count-then-write double-walk into a single linked-list traversal.
                var countPosition = baseStream.Position;
                writer.Write(0U);
                writer.Write(numberOfPoints);

                var numberOfIntervals = default(uint);
                if (startCell.HasInlineInterval)
                {
                    writer.Write(startCell.Start);
                    writer.Write(startCell.End);
                    numberOfIntervals++;
                }

                var cursor = startCell.FirstTail;
                while (cursor is not IntervalArena.NullIndex)
                {
                    ref var cell = ref this.arena[cursor];
                    writer.Write(cell.Start);
                    writer.Write(cell.End);
                    numberOfIntervals++;
                    cursor = cell.Next;
                }

                var endPosition = baseStream.Position;
                baseStream.Position = countPosition;
                writer.Write(numberOfIntervals);
                baseStream.Position = endPosition;
            }
            else
            {
                // non-seekable stream: fall back to the two-pass approach
                var numberOfIntervals = startCell.HasInlineInterval ? 1U : 0U;
                var cursor = startCell.FirstTail;
                while (cursor is not IntervalArena.NullIndex)
                {
                    numberOfIntervals++;
                    cursor = this.arena[cursor].Next;
                }

                writer.Write(numberOfIntervals);
                writer.Write(numberOfPoints);

                if (startCell.HasInlineInterval)
                {
                    writer.Write(startCell.Start);
                    writer.Write(startCell.End);
                }

                cursor = startCell.FirstTail;
                while (cursor is not IntervalArena.NullIndex)
                {
                    ref var cell = ref this.arena[cursor];
                    writer.Write(cell.Start);
                    writer.Write(cell.End);
                    cursor = cell.Next;
                }
            }
        }
    }

    /// <summary>
    /// Merges the specified cells.
    /// </summary>
    /// <param name="cells">The cells to merge.</param>
    /// <returns>The merged cell; or <see langword="null"/> if <paramref name="cells"/> is <see langword="null"/> or empty.</returns>
    public LasIntervalStartCell? Merge(ICollection<LasIntervalStartCell> cells)
    {
        var ignore = default(uint);
        return Merge(cells, this.threshold, this.arena, ref ignore);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not LasInterval interval)
        {
            return false;
        }

        if (this.cellsDictionary.Count != interval.cellsDictionary.Count)
        {
            return false;
        }

        using var firstEnumerator = this.cellsDictionary.OrderBy(kvp => kvp.Key).GetEnumerator();
        using var secondEnumerator = interval.cellsDictionary.OrderBy(kvp => kvp.Key).GetEnumerator();

        while (firstEnumerator.MoveNext())
        {
            if (!secondEnumerator.MoveNext())
            {
                return false;
            }

            var first = firstEnumerator.Current;
            var second = secondEnumerator.Current;

            if (first.Key != second.Key)
            {
                return false;
            }

            if (!this.CheckCell(first.Value, interval, second.Value))
            {
                return false;
            }
        }

        return !secondEnumerator.MoveNext();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Hashes only the cell count (the cheapest structural invariant that <see cref="Equals(object?)"/>
    /// also checks). Including the dictionary reference identity or `threshold` here would violate the
    /// Equals/GetHashCode contract, since neither is compared by <see cref="Equals(object?)"/>.
    /// </remarks>
    public override int GetHashCode() => this.cellsDictionary.Count;

    /// <summary>
    /// Gets the cell at the specified index.
    /// </summary>
    /// <param name="cellIndex">The cell index.</param>
    /// <param name="cell">When this method returns, the value associated with <paramref name="cellIndex"/>, if the cell index is found; otherwise <see langword="null" />. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the interval contains the cell index; otherwise <see langword=""/>.</returns>
    public bool TryGetCell(int cellIndex, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out LasIntervalStartCell? cell) => this.cellsDictionary.TryGetValue(cellIndex, out cell);

    private static LasIntervalStartCell? Merge(ICollection<LasIntervalStartCell> cells, int threshold, IntervalArena arena, ref uint numberOfIntervals)
    {
        return cells switch
        {
            null or { Count: 0 } => null,
            IList<LasIntervalStartCell> { Count: 1 } cellList => cellList[0],
            { Count: 1 } => cells.First(),
            { Count: > 1 } => MergeImpl(cells, threshold, arena, ref numberOfIntervals),
            _ => throw new System.Diagnostics.UnreachableException(),
        };

        static LasIntervalStartCell MergeImpl(IEnumerable<LasIntervalStartCell> cells, int threshold, IntervalArena arena, ref uint numberOfIntervals)
        {
            var merged = new LasIntervalStartCell();

            // flatten every interval from every cell into (Start, End) pairs, then sort. Using a flat
            // list avoids the per-insert O(log n) rebalancing of a sorted tree and tolerates duplicate
            // Start values (which can occur with overlapping intervals produced by coarsening).
            var flattened = new List<(uint Start, uint End)>();
            foreach (var cell in cells)
            {
                merged.Full += cell.Full;
                if (cell.HasInlineInterval)
                {
                    flattened.Add((cell.Start, cell.End));
                }

                var cursor = cell.FirstTail;
                while (cursor is not IntervalArena.NullIndex)
                {
                    ref var tail = ref arena[cursor];
                    flattened.Add((tail.Start, tail.End));
                    cursor = tail.Next;
                }
            }

            if (flattened is { Count: 0 })
            {
                return merged;
            }

            flattened.Sort(static (a, b) => a.Start.CompareTo(b.Start));

            // initialize merged with the first interval as the inline head
            var first = flattened[0];
            merged.Start = first.Start;
            merged.End = first.End;
            merged.HasInlineInterval = true;
            merged.Total = first.End - first.Start + 1;

            // merge intervals, appending new tail cells into the shared arena
            var previousTail = IntervalArena.NullIndex;
            var tailEnd = merged.End;
            for (var i = 1; i < flattened.Count; i++)
            {
                var current = flattened[i];
                var diff = (int)(current.Start - tailEnd);
                if (diff > threshold)
                {
                    var newIndex = arena.Allocate(current.Start, current.End);
                    if (previousTail is IntervalArena.NullIndex)
                    {
                        merged.FirstTail = newIndex;
                    }
                    else
                    {
                        arena[previousTail].Next = newIndex;
                    }

                    previousTail = newIndex;
                    tailEnd = current.End;
                    merged.Total += current.End - current.Start + 1;
                }
                else
                {
                    var extension = (int)(current.End - tailEnd);
                    if (extension > 0)
                    {
                        if (previousTail is IntervalArena.NullIndex)
                        {
                            merged.End = current.End;
                        }
                        else
                        {
                            arena[previousTail].End = current.End;
                        }

                        tailEnd = current.End;
                        merged.Total += (uint)extension;
                    }

                    numberOfIntervals--;
                }
            }

            if (previousTail is not IntervalArena.NullIndex)
            {
                merged.SetLastTail(previousTail);
            }

            return merged;
        }
    }

    private bool CheckCell(LasIntervalStartCell firstHead, LasInterval other, LasIntervalStartCell secondHead)
    {
        if (firstHead.HasInlineInterval != secondHead.HasInlineInterval ||
            firstHead.Start != secondHead.Start ||
            firstHead.End != secondHead.End)
        {
            return false;
        }

        var firstCursor = firstHead.FirstTail;
        var secondCursor = secondHead.FirstTail;
        while (firstCursor is not IntervalArena.NullIndex && secondCursor is not IntervalArena.NullIndex)
        {
            ref var f = ref this.arena[firstCursor];
            ref var s = ref other.arena[secondCursor];
            if (f.Start != s.Start || f.End != s.End)
            {
                return false;
            }

            firstCursor = f.Next;
            secondCursor = s.Next;
        }

        return firstCursor is IntervalArena.NullIndex && secondCursor is IntervalArena.NullIndex;
    }

    private bool TryCollapseGap(GapEntry entry, out int swallowedTail)
    {
        // the gap lives between a donor cell and the first tail it is linked to. after collapse,
        // the donor's end is extended to cover the next cell's end, and the next cell is removed
        // from the chain. The swallowed tail's arena index is returned so the caller can tombstone it.
        if (entry.Kind is GapEntry.OwnerKind.Head)
        {
            var head = entry.Head!;
            if (head.FirstTail is IntervalArena.NullIndex)
            {
                swallowedTail = IntervalArena.NullIndex;
                return false;
            }

            swallowedTail = head.FirstTail;
            ref var firstTail = ref this.arena[head.FirstTail];
            head.End = firstTail.End;
            head.FirstTail = firstTail.Next;
            return true;
        }

        ref var donor = ref this.arena[entry.TailIndex];
        if (donor.Next is IntervalArena.NullIndex)
        {
            swallowedTail = IntervalArena.NullIndex;
            return false;
        }

        swallowedTail = donor.Next;
        ref var successor = ref this.arena[donor.Next];
        donor.End = successor.End;
        donor.Next = successor.Next;
        return true;
    }

    private bool TryGetPostCollapseGap(GapEntry entry, out uint gap)
    {
        if (entry.Kind is GapEntry.OwnerKind.Head)
        {
            var head = entry.Head!;
            if (head.FirstTail is IntervalArena.NullIndex)
            {
                gap = default;
                return false;
            }

            gap = this.arena[head.FirstTail].Start - head.End - 1;
            return true;
        }

        ref var donor = ref this.arena[entry.TailIndex];
        if (donor.Next is IntervalArena.NullIndex)
        {
            gap = default;
            return false;
        }

        gap = this.arena[donor.Next].Start - donor.End - 1;
        return true;
    }

    /// <summary>
    /// Identifies a gap between two consecutive intervals; either between the inline head and its
    /// first tail, or between two tail cells in the arena.
    /// </summary>
    private sealed class GapEntry
    {
        private GapEntry(OwnerKind kind, LasIntervalStartCell? head, int tailIndex)
        {
            this.Kind = kind;
            this.Head = head;
            this.TailIndex = tailIndex;
        }

        public enum OwnerKind : byte
        {
            Head,
            Tail,
        }

        public OwnerKind Kind { get; }

        public LasIntervalStartCell? Head { get; }

        public int TailIndex { get; }

        public static GapEntry ForHead(LasIntervalStartCell head) => new(OwnerKind.Head, head, IntervalArena.NullIndex);

        public static GapEntry ForTail(int tailIndex) => new(OwnerKind.Tail, default, tailIndex);
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