namespace Altemiq.IO.Las.Indexing;

public class LasIntervalTests
{
    [Test]
    public async Task AddFirstPointCreatesCell()
    {
        var interval = new LasInterval();
        var created = interval.Add(pointIndex: 10, cellIndex: 1);

        await Assert.That(created).IsTrue();
        await Assert.That(interval.GetNumberOfCells()).IsEqualTo(1);
        await Assert.That(interval.TryGetCell(1, out var cell)).IsTrue();
        await Assert.That(cell!.Start).IsEqualTo(10U);
        await Assert.That(cell.End).IsEqualTo(10U);
        await Assert.That(cell.Full).IsEqualTo(1U);
        await Assert.That(cell.Total).IsEqualTo(1U);
        await Assert.That(cell.Next).IsNull();
    }

    [Test]
    public async Task AddConsecutivePointsExtendsExistingInterval()
    {
        var interval = new LasInterval(threshold: 1000);
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);
        _ = interval.Add(12, 1);

        _ = interval.TryGetCell(1, out var cell);
        await Assert.That(cell!.Start).IsEqualTo(10U);
        await Assert.That(cell.End).IsEqualTo(12U);
        await Assert.That(cell.Full).IsEqualTo(3U);
        await Assert.That(cell.Total).IsEqualTo(3U);
        await Assert.That(cell.Next).IsNull();
    }

    [Test]
    public async Task AddPointsWithGapLargerThanThresholdCreatesNewInterval()
    {
        var interval = new LasInterval(threshold: 10);
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);

        // gap of 100 points exceeds threshold of 10 -> should create a new tail interval
        var createdNew = interval.Add(111, 1);
        await Assert.That(createdNew).IsTrue();

        _ = interval.TryGetCell(1, out var cell);
        await Assert.That(cell!.Start).IsEqualTo(10U);
        await Assert.That(cell.End).IsEqualTo(11U);
        await Assert.That(cell.Next).IsNotNull();
        await Assert.That(cell.Next!.Start).IsEqualTo(111U);
        await Assert.That(cell.Next.End).IsEqualTo(111U);
        await Assert.That(cell.Full).IsEqualTo(3U);

        // Total counts per-interval spans: (10..11)+(111..111) = 2+1 = 3
        await Assert.That(cell.Total).IsEqualTo(3U);
    }

    [Test]
    public async Task AddPointsToDifferentCells()
    {
        var interval = new LasInterval();
        _ = interval.Add(10, 1);
        _ = interval.Add(20, 2);
        _ = interval.Add(30, 3);

        await Assert.That(interval.GetNumberOfCells()).IsEqualTo(3);
        await Assert.That(interval.TryGetCell(1, out _)).IsTrue();
        await Assert.That(interval.TryGetCell(2, out _)).IsTrue();
        await Assert.That(interval.TryGetCell(3, out _)).IsTrue();
        await Assert.That(interval.TryGetCell(4, out _)).IsFalse();
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

        await Assert.That(roundTripped.Equals(original)).IsTrue();
        await Assert.That(roundTripped.GetNumberOfCells()).IsEqualTo(original.GetNumberOfCells());
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

        await Assert.That(secondBytes).IsEquivalentTo(firstBytes);
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

        // the chain must still be head(10..11) -> tail(50..?) -- the tail must have been found and extended,
        // not overwritten. That both proves the rehydrate path and that the head is untouched.
        await Assert.That(cell!.Start).IsEqualTo(10U);
        await Assert.That(cell.End).IsEqualTo(11U);
        await Assert.That(cell.Next).IsNotNull();
        await Assert.That(cell.Next!.Start).IsEqualTo(50U);
        await Assert.That(cell.Next.End).IsEqualTo(52U);
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

        await Assert.That(first.Equals(second)).IsFalse();
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

        await Assert.That(built.Equals(roundTripped)).IsTrue();
        await Assert.That(built.GetHashCode()).IsEqualTo(roundTripped.GetHashCode());
    }

    [Test]
    public async Task CloneEmptyProducesEmptyInstance()
    {
        var interval = new LasInterval(threshold: 42);
        _ = interval.Add(1, 1);
        _ = interval.Add(2, 2);

        var clone = interval.CloneEmpty();
        await Assert.That(clone.GetNumberOfCells()).IsEqualTo(0);
    }

    [Test]
    public async Task MergeEmptyCollectionReturnsNull()
    {
        var interval = new LasInterval();
        var result = interval.Merge([]);
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task MergeSingleCellReturnsCellDirectly()
    {
        var interval = new LasInterval(threshold: 5);
        _ = interval.Add(10, 1);
        _ = interval.Add(11, 1);

        _ = interval.TryGetCell(1, out var cell);

        var merged = interval.Merge([cell!]);
        await Assert.That(merged).IsSameReferenceAs(cell);
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

        var merged = interval.Merge([cell1!, cell2!]);
        await Assert.That(merged).IsNotNull();

        // merged chain: 10..11 then 100..101 (gap > threshold so two intervals remain)
        await Assert.That(merged!.Start).IsEqualTo(10U);
        await Assert.That(merged.End).IsEqualTo(11U);
        await Assert.That(merged.Next).IsNotNull();
        await Assert.That(merged.Next!.Start).IsEqualTo(100U);
        await Assert.That(merged.Next.End).IsEqualTo(101U);
        await Assert.That(merged.Full).IsEqualTo(4U); // 2 + 2 input points
    }

    [Test]
    public async Task MergeCellsSingleIndexMovesToNewKey()
    {
        var interval = new LasInterval();
        _ = interval.Add(10, 5);

        var ok = interval.MergeCells([5], 42);
        await Assert.That(ok).IsTrue();
        await Assert.That(interval.TryGetCell(5, out _)).IsFalse();
        await Assert.That(interval.TryGetCell(42, out _)).IsTrue();
    }

    [Test]
    public async Task MergeCellsMultipleIndicesCombinesAtNewKey()
    {
        var interval = new LasInterval(threshold: 5);
        _ = interval.Add(10, 1);
        _ = interval.Add(100, 2);

        var ok = interval.MergeCells([1, 2], 99);
        await Assert.That(ok).IsTrue();
        await Assert.That(interval.TryGetCell(1, out _)).IsFalse();
        await Assert.That(interval.TryGetCell(2, out _)).IsFalse();
        await Assert.That(interval.TryGetCell(99, out var merged)).IsTrue();
        await Assert.That(merged!.Full).IsEqualTo(2U);
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
        var intervalsBefore = CountIntervals(cellBefore!);
        await Assert.That(intervalsBefore).IsEqualTo(3);

        // cap to 2 intervals total (1 cell * 2). Should collapse the smallest gap (38 between 11..50)
        interval.MergeIntervals(maximumIntervals: 2);

        _ = interval.TryGetCell(1, out var cellAfter);
        var intervalsAfter = CountIntervals(cellAfter!);
        await Assert.That(intervalsAfter).IsEqualTo(2);

        // first interval should be 10..51 after the smaller gap was eaten
        await Assert.That(cellAfter!.Start).IsEqualTo(10U);
        await Assert.That(cellAfter.End).IsEqualTo(51U);
        await Assert.That(cellAfter.Next).IsNotNull();
        await Assert.That(cellAfter.Next!.Start).IsEqualTo(250U);
        await Assert.That(cellAfter.Next.End).IsEqualTo(251U);

        static int CountIntervals(LasIntervalCell cell)
        {
            var count = 0;
            var current = cell;
            while (current is not null)
            {
                count++;
                current = current.Next;
            }

            return count;
        }
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
        await Assert.That(interval.GetNumberOfCells()).IsEqualTo(originalCells);
    }
}
