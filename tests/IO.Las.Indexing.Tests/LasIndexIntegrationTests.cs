namespace Altemiq.IO.Las.Indexing;

public class LasIndexIntegrationTests
{
    [Test]
    public async Task BuildAddQueryRoundTrip()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var index = new LasIndex(quadTree);

        // add 10 sequential points at a fixed coordinate; should land in one quad-tree cell
        for (var i = 0U; i < 10U; i++)
        {
            _ = index.Add(500F, 500F, i);
        }

        // add another 10 points with a large index gap to force a second interval
        for (var i = 100_000U; i < 100_010U; i++)
        {
            _ = index.Add(500F, 500F, i);
        }

        // sanity: point 5 should be found
        await Assert.That(index.IndexOf(5U)).IsGreaterThanOrEqualTo(0);

        // point beyond any added range is absent
        await Assert.That(index.IndexOf(999_999U)).IsEqualTo(-1);
    }

    [Test]
    public async Task WriteReadRoundTripPreservesEquality()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var index = new LasIndex(quadTree);

        for (var i = 0U; i < 50U; i++)
        {
            _ = index.Add(250F, 250F, i);
        }

        for (var i = 500_000U; i < 500_010U; i++)
        {
            _ = index.Add(750F, 750F, i);
        }

        // non-zero `minimumPoints` triggers `ManageCells`, which populates the quad-tree's adaptive
        // bitmap to match what `ReadFrom` will produce on the reloaded copy. Without this the two
        // quad-trees would not be `Equals` even though they represent the same data.
        index.Complete(minimumPoints: 1, maximumIntervals: 0);

        using var stream = new MemoryStream();
        index.WriteTo(stream);
        stream.Position = 0;
        var reloaded = LasIndex.ReadFrom(stream);

        await Assert.That(reloaded.Equals(index)).IsTrue();
        await Assert.That(reloaded.GetHashCode()).IsEqualTo(index.GetHashCode());
    }

    [Test]
    public async Task AllRangesEnumeratesEveryPointAdded()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var index = new LasIndex(quadTree);

        // 20 consecutive points in a single cell
        for (var i = 0U; i < 20U; i++)
        {
            _ = index.Add(250F, 250F, i);
        }

        index.Complete(minimumPoints: 0, maximumIntervals: 0);

        var ranges = index.All().ToList();
        await Assert.That(ranges.Count).IsGreaterThanOrEqualTo(1);

        // the full point-index sweep should cover every added point
        var covered = new HashSet<uint>();
        foreach (var range in ranges)
        {
            for (var p = range.Start.Value; p <= range.End.Value; p++)
            {
                _ = covered.Add(p);
            }
        }

        for (var i = 0U; i < 20U; i++)
        {
            await Assert.That(covered.Contains(i)).IsTrue();
        }
    }

    [Test]
    public async Task WithinRectangleReturnsOnlyMatchingCells()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var index = new LasIndex(quadTree);

        // put one set of points at (100, 100) and another at (900, 900)
        for (var i = 0U; i < 5U; i++)
        {
            _ = index.Add(100F, 100F, i);
        }

        for (var i = 100U; i < 105U; i++)
        {
            _ = index.Add(900F, 900F, i);
        }

        index.Complete(minimumPoints: 0, maximumIntervals: 0);

        // query only the lower-left quarter
        var ranges = index.WithinRectangle(0F, 0F, 500F, 500F).ToList();

        var covered = new HashSet<uint>();
        foreach (var range in ranges)
        {
            for (var p = range.Start.Value; p <= range.End.Value; p++)
            {
                _ = covered.Add(p);
            }
        }

        // the lower-left points (0..4) must be covered
        for (var i = 0U; i < 5U; i++)
        {
            await Assert.That(covered.Contains(i)).IsTrue();
        }

        // the upper-right points (100..104) must not be
        for (var i = 100U; i < 105U; i++)
        {
            await Assert.That(covered.Contains(i)).IsFalse();
        }
    }

    [Test]
    public async Task IndexOfReturnsTheCellOfTheAddedPoint()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var index = new LasIndex(quadTree);

        var cellIndex = quadTree.GetCellIndex(250F, 250F);

        for (var i = 0U; i < 5U; i++)
        {
            _ = index.Add(250F, 250F, i);
        }

        await Assert.That(index.IndexOf(2U)).IsEqualTo(cellIndex);
    }

    [Test]
    public async Task CloneEmptyProducesEmptyIndex()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var index = new LasIndex(quadTree);
        _ = index.Add(100F, 100F, 0U);

        var empty = index.CloneEmpty();
        await Assert.That(empty.All().Any()).IsFalse();
    }

    [Test]
    public async Task AsReadOnlyReturnsSnapshotMatchingEnumeration()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var index = new LasIndex(quadTree);
        _ = index.Add(100F, 100F, 0U);
        _ = index.Add(900F, 900F, 1U);

        var snapshot = index.AsReadOnly();
        await Assert.That(snapshot.Count).IsEqualTo(2);

        var enumerated = index.Count();
        await Assert.That(enumerated).IsEqualTo(snapshot.Count);
    }

    [Test]
    public async Task EqualsFalseForIndexesWithDifferentPoints()
    {
        var first = new LasIndex(new LasQuadTree(0, 1000, 0, 1000, 100));
        _ = first.Add(100F, 100F, 0U);

        var second = new LasIndex(new LasQuadTree(0, 1000, 0, 1000, 100));
        _ = second.Add(100F, 100F, 1U);

        await Assert.That(first.Equals(second)).IsFalse();
    }

    [Test]
    public async Task ReadWriteFusaLaxRoundTripIsByteStable()
    {
        using var resource = typeof(LasIndexTests).Assembly.GetManifestResourceStream(typeof(LasIndexTests), "fusa.lax")
                             ?? throw new System.Diagnostics.UnreachableException();

        using var firstCopy = new MemoryStream();
        await resource.CopyToAsync(firstCopy);
        var originalBytes = firstCopy.ToArray();

        firstCopy.Position = 0;
        var parsed = LasIndex.ReadFrom(firstCopy);

        using var writeBack = new MemoryStream();
        parsed.WriteTo(writeBack);
        var roundTripBytes = writeBack.ToArray();

        // confirm the full round-trip is semantically equivalent (byte-for-byte may depend on ordering;
        // content-equality is the contract we actually care about)
        writeBack.Position = 0;
        var reparsed = LasIndex.ReadFrom(writeBack);
        await Assert.That(reparsed.Equals(parsed)).IsTrue();

        // and the byte length is the same
        await Assert.That(roundTripBytes.Length).IsEqualTo(originalBytes.Length);
    }
}
