namespace Altemiq.IO.Las.Indexing;

public class LasIndexCellTests
{
    [Test]
    [MatrixDataSource]
    public async Task ContainsPoint([Matrix(0, 25)] double x, [Matrix(1000, 1025)] double y)
    {
        var cell = new LasIndexCell(new(0, 1000), new(100, 1100), []);
        _ = await Assert.That(cell.Contains(x, y)).IsTrue();
    }

    [Test]
    [MatrixDataSource]
    public async Task ContainsX([Matrix(0, 25, 50, 75)] double x)
    {
        var cell = new LasIndexCell(new(0, 0), new(100, 0), []);
        _ = await Assert.That(cell.ContainsX(x)).IsTrue();
    }

    [Test]
    [MatrixDataSource]
    public async Task ContainsY([Matrix(0, 25, 50, 75)] double y)
    {
        var cell = new LasIndexCell(new(0, 0), new(0, 100), []);
        _ = await Assert.That(cell.ContainsY(y)).IsTrue();
    }

    [Test]
    [Arguments(-1, 999)]
    [Arguments(-1, 1025)]
    [Arguments(25, 999)]
    [Arguments(100, 1100)]
    [Arguments(100, 1101)]
    [Arguments(101, 1100)]
    [Arguments(100, 1075)]
    [Arguments(75, 1100)]
    [Arguments(101, 1075)]
    [Arguments(75, 1101)]
    public async Task DoesNotContainPoint(double x, double y)
    {
        var cell = new LasIndexCell(new(0, 1000), new(100, 1100), []);
        _ = await Assert.That(cell.Contains(x, y)).IsFalse();
    }

    [Test]
    [MatrixDataSource]
    public async Task DoesNotContainX([Matrix(-1, 100, 101)] double x)
    {
        var cell = new LasIndexCell(new(0, float.MinValue), new(100, float.MaxValue), []);
        _ = await Assert.That(cell.ContainsX(x)).IsFalse();
    }

    [Test]
    [MatrixDataSource]
    public async Task DoesNotContainY([Matrix(-1, 100, 101)] double y)
    {
        var cell = new LasIndexCell(new(float.MinValue, 0), new(float.MaxValue, 100), []);
        _ = await Assert.That(cell.ContainsY(y)).IsFalse();
    }

    [Test]
    [Arguments(10U)]
    [Arguments(50U)]
    [Arguments(200U)]
    [Arguments(250U)]
    public async Task ContainsIndex(uint index)
    {
        var cell = new LasIndexCell(new(0, 0), new(100, 100), [new(10, 100), new(200, 300)]);
        _ = await Assert.That(cell.Contains(index)).IsTrue();
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
        _ = await Assert.That(cell.Contains(index)).IsFalse();
    }
}