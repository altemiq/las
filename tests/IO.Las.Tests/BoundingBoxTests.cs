namespace Altemiq.IO.Las;

public class BoundingBoxTests
{
    [Test]
    [MethodDataSource(nameof(IntersectData))]
    public async Task Intersect(BoundingBox first, BoundingBox second, BoundingBox expected)
    {
        await Assert.That(first.Intersect(second)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(IntersectsWithData))]
    public async Task IntersectsWith(BoundingBox first, BoundingBox second, bool expected)
    {
        await Assert.That(first.IntersectsWith(second)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(UnionData))]
    public async Task Union(BoundingBox first, BoundingBox second, BoundingBox expected)
    {
        await Assert.That(BoundingBox.Union(first, second)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(ContainsBoundingBoxData))]
    public async Task ContainsBoundingBox(BoundingBox first, BoundingBox second, bool expected)
    {
        await Assert.That(first.Contains(second)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(ContainsPointData))]
    public async Task ContainsPoint(BoundingBox first, double x, double y, double z, bool expected)
    {
        await Assert.That(first.Contains(x, y, z)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(InflateData))]
    public async Task Inflate(BoundingBox first, double x, double y, double z, BoundingBox expected)
    {
        await Assert.That(BoundingBox.Inflate(first, x, y, z)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(OffsetData))]
    public async Task Offset(BoundingBox first, double x, double y, double z, BoundingBox expected)
    {
        await Assert.That(first.Offset(x, y, z)).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(IsEmptyData))]
    public async Task IsEmpty(BoundingBox first, bool expected)
    {
        await Assert.That(first.IsEmpty).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(NormalizeData))]
    public async Task Normalize(BoundingBox first, BoundingBox expected)
    {
        await Assert.That(first.Normalize()).IsEqualTo(expected);
    }

    [Test]
    [MethodDataSource(nameof(NotEqualsData))]
    public async Task NotEquals(BoundingBox value)
    {
        await Assert.That(BoundingBox.Empty).IsNotEqualTo(value);
    }

    public IEnumerable<Func<(BoundingBox, BoundingBox, BoundingBox)>> IntersectData()
    {
        yield return () => (new(0, 0, 0, 2, 2, 2), new(1, 1, 1, 3, 3, 3), new(1, 1, 1, 2, 2, 2));
        yield return () => (new(1, 1, 1, 3, 3, 3), new(0, 0, 0, 2, 2, 2), new(1, 1, 1, 2, 2, 2));
        yield return () => (new(0, 0, 0, 1, 1, 1), new(2, 2, 2, 3, 3, 3), BoundingBox.Empty);
        yield return () => (new(2, 2, 2, 3, 3, 3), new(0, 0, 0, 1, 1, 1), BoundingBox.Empty);
    }

    public IEnumerable<Func<(BoundingBox, BoundingBox, bool)>> IntersectsWithData()
    {
        yield return () => (new(0, 0, 0, 2, 2, 2), new(1, 1, 1, 3, 3, 3), true);
        yield return () => (new(1, 1, 1, 3, 3, 3), new(0, 0, 0, 2, 2, 2), true);
        yield return () => (new(0, 0, 0, 1, 1, 1), new(2, 2, 2, 3, 3, 3), false);
        yield return () => (new(2, 2, 2, 3, 3, 3), new(0, 0, 0, 1, 1, 1), false);
    }

    public IEnumerable<Func<(BoundingBox, BoundingBox, BoundingBox)>> UnionData()
    {
        yield return () => (new(0, 0, 0, 2, 2, 2), new(1, 1, 1, 3, 3, 3), new(0, 0, 0, 3, 3, 3));
        yield return () => (new(1, 1, 1, 3, 3, 3), new(0, 0, 0, 2, 2, 2), new(0, 0, 0, 3, 3, 3));
        yield return () => (new(0, 0, 0, 1, 1, 1), new(2, 2, 2, 3, 3, 3), new(0, 0, 0, 3, 3, 3));
        yield return () => (new(2, 2, 2, 3, 3, 3), new(0, 0, 0, 1, 1, 1), new(0, 0, 0, 3, 3, 3));
    }

    public IEnumerable<Func<(BoundingBox, BoundingBox, bool)>> ContainsBoundingBoxData()
    {
        yield return () => (new(0, 0, 0, 2, 2, 2), new(0, 0, 0, 2, 2, 2), true);
        yield return () => (new(0, 0, 0, 2, 2, 2), new(0, 0, 0, 1, 1, 1), true);
        yield return () => (new(0, 0, 0, 2, 2, 2), new(1, 1, 1, 2, 2, 2), true);
        yield return () => (new(0, 0, 0, 3, 3, 3), new(1, 1, 1, 2, 2, 2), true);
        yield return () => (new(1, 1, 1, 2, 2, 2), new(0, 0, 0, 1, 1, 1), false);
        yield return () => (new(0, 0, 0, 1, 1, 1), new(1, 1, 1, 2, 2, 2), false);
        yield return () => (new(2, 2, 2, 3, 3, 3), new(0, 0, 0, 1, 1, 1), false);
        yield return () => (new(0, 0, 0, 1, 1, 1), new(2, 2, 2, 3, 3, 3), false);
    }

    public IEnumerable<Func<(BoundingBox, double, double, double, bool)>> ContainsPointData()
    {
        yield return () => (new(0, 0, 0, 2, 2, 2), 0, 0, 0, true);
        yield return () => (new(0, 0, 0, 2, 2, 2), 2, 2, 2, true);
        yield return () => (new(0, 0, 0, 2, 2, 2), 1, 1, 1, true);
        yield return () => (new(1, 1, 1, 2, 2, 2), 0, 0, 0, false);
        yield return () => (new(0, 0, 0, 1, 1, 1), 2, 2, 2, false);
    }

    public IEnumerable<Func<(BoundingBox, double, double, double, BoundingBox)>> InflateData()
    {
        yield return () => (new(1, 1, 1, 3, 3, 3), 1, 1, 1, new(0, 0, 0, 4, 4, 4));
        yield return () => (new(0, 0, 0, 4, 4, 4), -1, -1, -1, new(1, 1, 1, 3, 3, 3));
    }

    public IEnumerable<Func<(BoundingBox, double, double, double, BoundingBox)>> OffsetData()
    {
        yield return () => (new(1, 1, 1, 3, 3, 3), 1, 1, 1, new(2, 2, 2, 4, 4, 4));
        yield return () => (new(2, 2, 2, 4, 4, 4), -1, -1, -1, new(1, 1, 1, 3, 3, 3));
    }

    public IEnumerable<Func<(BoundingBox, bool)>> IsEmptyData()
    {
        yield return () => (new(1, 1, 1, 1, 3, 3), true);
        yield return () => (new(1, 1, 1, 3, 1, 3), true);
        yield return () => (new(1, 1, 1, 3, 3, 1), true);
        yield return () => (new(1, 1, 1, 0, 3, 3), true);
        yield return () => (new(1, 1, 1, 3, 0, 3), true);
        yield return () => (new(1, 1, 1, 3, 3, 0), true);
        yield return () => (new(2, 2, 2, 4, 4, 4), false);
    }

    public IEnumerable<Func<(BoundingBox, BoundingBox)>> NormalizeData()
    {
        yield return () => (new(1, 1, 1, 1, 3, 3), new(1, 1, 1, 1, 3, 3));
        yield return () => (new(1, 1, 1, 3, 1, 3), new(1, 1, 1, 3, 1, 3));
        yield return () => (new(1, 1, 1, 3, 3, 1), new(1, 1, 1, 3, 3, 1));
        yield return () => (new(1, 1, 1, 0, 3, 3), new(0, 1, 1, 1, 3, 3));
        yield return () => (new(1, 1, 1, 3, 0, 3), new(1, 0, 1, 3, 1, 3));
        yield return () => (new(1, 1, 1, 3, 3, 0), new(1, 1, 0, 3, 3, 1));
        yield return () => (new(2, 2, 2, 4, 4, 4), new(2, 2, 2, 4, 4, 4));
    }

    public IEnumerable<Func<BoundingBox>> NotEqualsData()
    {
        yield return () => new(1, 0, 0, 0, 0, 0);
        yield return () => new(0, 1, 0, 0, 0, 0);
        yield return () => new(0, 0, 1, 0, 0, 0);
        yield return () => new(0, 0, 0, 1, 0, 0);
        yield return () => new(0, 0, 0, 0, 1, 0);
        yield return () => new(0, 0, 0, 0, 0, 1);
    }
}