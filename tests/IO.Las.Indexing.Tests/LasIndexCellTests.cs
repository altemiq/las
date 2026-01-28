using System.Numerics;

namespace Altemiq.IO.Las.Indexing;

public class LasIndexCellTests
{
    [Test]
    [Arguments(0, 0)]
    [Arguments(25, 0)]
    [Arguments(0, 25)]
    [Arguments(25, 25)]
    public async Task ContainsPoint(double x, double y)
    {
        var cell = new LasIndexCell(new(0, 0), new(100, 100), []);
        await Assert.That(cell.Contains(x, y)).IsTrue();
    }
    
    [Test]
    [Arguments(-1, -1)]
    [Arguments(-1, 25)]
    [Arguments(25, -1)]
    [Arguments(100, 100)]
    [Arguments(100, 101)]
    [Arguments(101, 100)]
    [Arguments(100, 75)]
    [Arguments(75, 100)]
    [Arguments(101, 75)]
    [Arguments(75, 101)]
    public async Task DoesNotContainsPoint(double x, double y)
    {
        var cell = new LasIndexCell(new(0, 0), new(100, 100), []);
        await Assert.That(cell.Contains(x, y)).IsFalse();
    }

    [Test]
    [Arguments(10U)]
    [Arguments(50U)]
    [Arguments(200U)]
    [Arguments(250U)]
    public async Task ContainsIndex(uint index)
    {
        var cell = new LasIndexCell(new(0, 0), new(100, 100), [new(10, 100), new(200, 300)]);
        await Assert.That(cell.Contains(index)).IsTrue();
    }
    
    [Test]
    [Arguments(5U)]
    [Arguments(100U)]
    [Arguments(150U)]
    [Arguments(300U)]
    [Arguments(350U)]
    public async Task DoesNotContainsIndex(uint index)
    {
        var cell = new LasIndexCell(new(0, 0), new(100, 100), [new(10, 100), new(200, 300)]);
        await Assert.That(cell.Contains(index)).IsFalse();
    }
}