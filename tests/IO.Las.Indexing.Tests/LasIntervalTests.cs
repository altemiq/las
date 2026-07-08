namespace Altemiq.IO.Las.Indexing;

public class LasIntervalTests
{
    [Test]
    public async Task AddFirstPointCreatesCell()
    {
        var interval = new LasInterval();
        var created = interval.Add(pointIndex: 10, cellIndex: 1);

        _ = await Assert.That(created).IsTrue();
        _ = await Assert.That(interval.GetNumberOfCells()).IsEqualTo(1);
        _ = await Assert.That(interval.TryGetCell(1, out var cell)).IsTrue();

        var intervals = GetIntervals(interval, cell);
        _ = await Assert.That(intervals).IsEquivalentTo([(10U, 10U)]);
        _ = await Assert.That(cell.Full).IsEqualTo(1U);
        _ = await Assert.That(cell.Total).IsEqualTo(1U);
    }

    [Test]
    public async Task AddConsecutivePointsExtendsExistingInterval()
    {
        var interval = new LasInterval(threshold: 1000);
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);
        _ = interval.Add(12, 1);

        _ = interval.TryGetCell(1, out var cell);
        var intervals = GetIntervals(interval, cell);
        _ = await Assert.That(intervals).IsEquivalentTo([(10U, 12U)]);
        _ = await Assert.That(cell.Full).IsEqualTo(3U);
        _ = await Assert.That(cell.Total).IsEqualTo(3U);
    }

    [Test]
    public async Task AddPointsWithGapLargerThanThresholdCreatesNewInterval()
    {
        var interval = new LasInterval(threshold: 10);
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);

        // gap of 100 points exceeds threshold of 10 -> should create a new tail interval
        var createdNew = interval.Add(111, 1);
        _ = await Assert.That(createdNew).IsTrue();

        _ = interval.TryGetCell(1, out var cell);
        var intervals = GetIntervals(interval, cell);
        _ = await Assert.That(intervals).IsEquivalentTo([(10U, 11U), (111U, 111U)]);
        _ = await Assert.That(cell.Full).IsEqualTo(3U);

        // Total counts per-interval spans: (10..11)+(111..111) = 2+1 = 3
        _ = await Assert.That(cell.Total).IsEqualTo(3U);
    }

    [Test]
    public async Task AddPointsToDifferentCells()
    {
        var interval = new LasInterval();
        _ = interval.Add(10, 1);
        _ = interval.Add(20, 2);
        _ = interval.Add(30, 3);

        _ = await Assert.That(interval.GetNumberOfCells()).IsEqualTo(3);
        _ = await Assert.That(interval.TryGetCell(1, out _)).IsTrue();
        _ = await Assert.That(interval.TryGetCell(2, out _)).IsTrue();
        _ = await Assert.That(interval.TryGetCell(3, out _)).IsTrue();
        _ = await Assert.That(interval.TryGetCell(4, out _)).IsFalse();
    }

    [Test]
    public async Task WriteToReadFromRoundTrip()
    {
        var original = new LasInterval(threshold: 5);
        _ = original.Add(10, 1);
        _ = original.Add(11, 1);
        _ = original.Add(50, 1);   // gap > threshold -> new interval
        _ = original.Add(51, 1);
        _ = original.Add(100, 2);
        _ = original.Add(200, 2);  // gap > threshold -> new interval
        _ = original.Add(1000, 3);

        using var stream = new MemoryStream();
        original.WriteTo(stream);
        stream.Position = 0;

        var roundTripped = LasInterval.ReadFrom(stream);

        _ = await Assert.That(roundTripped.Equals(original)).IsTrue();
        _ = await Assert.That(roundTripped.GetNumberOfCells()).IsEqualTo(original.GetNumberOfCells());
    }

    [Test]
    public async Task WriteToIsByteStableAcrossReadWriteCycle()
    {
        var original = new LasInterval(threshold: 20);
        _ = original.Add(1, 100);
        _ = original.Add(2, 100);
        _ = original.Add(3, 100);
        _ = original.Add(50, 100);  // new interval
        _ = original.Add(51, 100);
        _ = original.Add(60, 200);
        _ = original.Add(150, 200); // new interval

        using var firstStream = new MemoryStream();
        original.WriteTo(firstStream);
        var firstBytes = firstStream.ToArray();

        firstStream.Position = 0;
        var reloaded = LasInterval.ReadFrom(firstStream);

        using var secondStream = new MemoryStream();
        reloaded.WriteTo(secondStream);
        var secondBytes = secondStream.ToArray();

        _ = await Assert.That(secondBytes).IsEquivalentTo(firstBytes);
    }

    [Test]
    public async Task AddAfterReadFromAppendsToExistingChain()
    {
        // verify the rehydrate-`last`-after-read behaviour exposed by LasIntervalStartCell.Add:
        // after deserialization, the internal `last` pointer is null, but subsequent Add calls
        // must still extend the correct tail rather than overwriting the chain.
        var original = new LasInterval(threshold: 5);
        _ = original.Add(10, 1);
        _ = original.Add(11, 1);
        _ = original.Add(50, 1); // creates a tail (gap 38 > 5)
        _ = original.Add(51, 1);

        using var stream = new MemoryStream();
        original.WriteTo(stream);
        stream.Position = 0;
        var reloaded = LasInterval.ReadFrom(stream);

        // read-back defaults to DefaultThreshold = 1000, so a small gap is absorbed into the existing tail
        _ = reloaded.Add(52, 1);

        _ = reloaded.TryGetCell(1, out var cell);
        var intervals = GetIntervals(reloaded, cell);

        // the chain must still be head(10..11) -> tail(50..52) -- the tail must have been found and extended,
        // not overwritten. That both proves the rehydrate path and that the head is untouched.
        _ = await Assert.That(intervals).IsEquivalentTo([(10U, 11U), (50U, 52U)]);
    }

    [Test]
    public async Task EqualsReturnsFalseForDivergentChains()
    {
        var first = new LasInterval(threshold: 5);
        _ = first.Add(10, 1);
        _ = first.Add(11, 1);

        var second = new LasInterval(threshold: 5);
        _ = second.Add(10, 1);
        _ = second.Add(12, 1);

        _ = await Assert.That(first.Equals(second)).IsFalse();
    }

    [Test]
    public async Task EqualsReturnsTrueForIdenticalChainsBuiltDifferently()
    {
        var built = new LasInterval(threshold: 5);
        _ = built.Add(10, 1);
        _ = built.Add(11, 1);
        _ = built.Add(50, 1);
        _ = built.Add(51, 1);

        // round-trip through bytes to get a second instance with identical content
        using var stream = new MemoryStream();
        built.WriteTo(stream);
        stream.Position = 0;
        var roundTripped = LasInterval.ReadFrom(stream);

        _ = await Assert.That(built.Equals(roundTripped)).IsTrue();
        _ = await Assert.That(built.GetHashCode()).IsEqualTo(roundTripped.GetHashCode());
    }

    [Test]
    public async Task CloneEmptyProducesEmptyInstance()
    {
        var interval = new LasInterval(threshold: 42);
        _ = interval.Add(1, 1);
        _ = interval.Add(2, 2);

        var clone = interval.CloneEmpty();
        _ = await Assert.That(clone.GetNumberOfCells()).IsEqualTo(0);
    }

    [Test]
    public async Task MergeEmptyCollectionReturnsNull()
    {
        var interval = new LasInterval();
        var result = interval.Merge([]);
        _ = await Assert.That(result).IsNull();
    }

    [Test]
    public async Task MergeSingleCellReturnsCellDirectly()
    {
        var interval = new LasInterval(threshold: 5);
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);

        _ = interval.TryGetCell(1, out var cell);

        var merged = interval.Merge([cell]);
        _ = await Assert.That(merged).IsSameReferenceAs(cell);
    }

    [Test]
    public async Task MergeTwoCellsCombinesIntervalsInOrder()
    {
        var interval = new LasInterval(threshold: 5);
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);
        _ = interval.Add(100, 2);
        _ = interval.Add(101, 2);

        _ = interval.TryGetCell(1, out var cell1);
        _ = interval.TryGetCell(2, out var cell2);

        var merged = interval.Merge([cell1, cell2]);
        _ = await Assert.That(merged).IsNotNull();

        // merged chain: 10..11 then 100..101 (gap > threshold so two intervals remain)
        var intervals = GetIntervals(interval, merged);
        _ = await Assert.That(intervals).IsEquivalentTo([(10U, 11U), (100U, 101U)]);
        _ = await Assert.That(merged.Full).IsEqualTo(4U); // 2 + 2 input points
    }

    [Test]
    public async Task MergeCellsSingleIndexMovesToNewKey()
    {
        var interval = new LasInterval();
        _ = interval.Add(10, 5);

        var ok = interval.MergeCells([5], 42);
        _ = await Assert.That(ok).IsTrue();
        _ = await Assert.That(interval.TryGetCell(5, out _)).IsFalse();
        _ = await Assert.That(interval.TryGetCell(42, out _)).IsTrue();
    }

    [Test]
    public async Task MergeCellsMultipleIndicesCombinesAtNewKey()
    {
        var interval = new LasInterval(threshold: 5);
        _ = interval.Add(10, 1);
        _ = interval.Add(100, 2);

        var ok = interval.MergeCells([1, 2], 99);
        _ = await Assert.That(ok).IsTrue();
        _ = await Assert.That(interval.TryGetCell(1, out _)).IsFalse();
        _ = await Assert.That(interval.TryGetCell(2, out _)).IsFalse();
        _ = await Assert.That(interval.TryGetCell(99, out var merged)).IsTrue();
        _ = await Assert.That(merged.Full).IsEqualTo(2U);
    }

    [Test]
    public async Task MergeIntervalsCollapsesSmallestGapsFirst()
    {
        var interval = new LasInterval(threshold: 5);
        // build: 10..11, gap 38, 50..51, gap 198, 250..251 (three intervals)
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);
        _ = interval.Add(50, 1);
        _ = interval.Add(51, 1);
        _ = interval.Add(250, 1);
        _ = interval.Add(251, 1);

        _ = interval.TryGetCell(1, out var cellBefore);
        _ = await Assert.That(GetIntervals(interval, cellBefore).Count).IsEqualTo(3);

        // cap to 2 intervals total (1 cell * 2). Should collapse the smallest gap (38 between 11..50)
        interval.MergeIntervals(maximumIntervals: 2);

        _ = interval.TryGetCell(1, out var cellAfter);
        var intervals = GetIntervals(interval, cellAfter);

        // first interval should be 10..51 after the smaller gap was eaten; second stays 250..251
        _ = await Assert.That(intervals).IsEquivalentTo([(10U, 51U), (250U, 251U)]);
    }

    [Test]
    public async Task MergeIntervalsDoesNotShrinkBelowCellCount()
    {
        var interval = new LasInterval(threshold: 5);
        _ = interval.Add(10, 1);
        _ = interval.Add(50, 1);   // second interval in cell 1
        _ = interval.Add(100, 2);

        var originalCells = interval.GetNumberOfCells();
        interval.MergeIntervals(maximumIntervals: 0);

        // cell count should not change
        _ = await Assert.That(interval.GetNumberOfCells()).IsEqualTo(originalCells);
    }

    // helper: flatten a start cell's chain into a list of (Start, End) tuples.
    // tests use this to avoid depending on internal chain-walking details (arena indices, Next
    // pointers), making the tests robust to representation changes.
    private static List<(uint Start, uint End)> GetIntervals(LasInterval owner, LasIntervalStartCell cell)
    {
        return cell is null
            ? []
            : [.. owner.EnumerateIntervals(cell).Select(static t => (t.Start, t.End))];
    }
}