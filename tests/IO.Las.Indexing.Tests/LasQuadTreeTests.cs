using System.Numerics;

namespace Altemiq.IO.Las.Indexing;

public class LasQuadTreeTests
{
    [Test]
    public async Task GetCell()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        await Assert.That(quadTree.GetCellIndex(25, 50)).IsEqualTo(100);
    }
    
    [Test]
    public async Task GetBounds()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        var (minimum, maximum) = quadTree.GetBounds(1, 1);
        await Assert.That(minimum).IsEqualTo(new Vector2(500, -300));
        await Assert.That(maximum).IsEqualTo(new Vector2(1300, 500));
    }
    
    [Test]
    public async Task WithinRectangle()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        await Assert.That(quadTree.CellsWithinRectangle(0, 100, 0, 100)).IsEquivalentTo([99]);
    }
    
    [Test]
    public async Task WithinTile()
    {
        var quadTree = new LasQuadTree(0, 1000, 0, 1000, 100);
        await Assert.That(quadTree.CellsWithinTile(0, 0, 100)).IsEquivalentTo([100]);
    }
}