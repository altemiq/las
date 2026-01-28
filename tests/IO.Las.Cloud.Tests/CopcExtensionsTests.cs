namespace Altemiq.IO.Las.Cloud;

public class CopcExtensionsTests
{
    [Test]
    public async Task ToBoundingBox()
    {
        var key = new CopcHierarchy.VoxelKey(1, 0, 1, 1);
        var min = new Vector3D(0, 0, 0);
        var max = new Vector3D(100, 100, 100);
        var boundingBox = key.ToBoundingBox(min, max);
        await Assert.That(boundingBox)
            .IsEqualTo(new BoundingBox(0, 50, 50, 50, 100, 100)).And
            .Satisfies(
                bb => new BoundingBox(min, max).Contains(bb),
                static contains => contains.IsTrue());
    }

    [Test]
    [MethodDataSource(nameof(GetParentData))]
    public async Task GetParent(CopcHierarchy.VoxelKey key, CopcHierarchy.VoxelKey expected)
    {
        await Assert.That(key.Parent()).IsEqualTo(expected);
    }

    public IEnumerable<Func<(CopcHierarchy.VoxelKey Key, CopcHierarchy.VoxelKey Expected)>> GetParentData()
    {
        yield return () => (new(1, 0, 1, 1), new(0, 0, 0, 0));
        yield return () => (new(1, 2, 1, 1), new(0, 1, 0, 0));
        yield return () => (new(1, 1, 2, 1), new(0, 0, 1, 0));
        yield return () => (new(1, 1, 1, 2), new(0, 0, 0, 1));
    }
}