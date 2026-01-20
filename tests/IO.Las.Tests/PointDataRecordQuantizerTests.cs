namespace Altemiq.IO.Las;

public class PointDataRecordQuantizerTests
{
    private readonly PointDataRecordQuantizer quantizer = new PointDataRecordQuantizer(
        new(0.01, 0.01, 0.01),
        new(123, 456, 789),
        0);
    
    [Test]
    [MethodDataSource(nameof(GetToDoubleXY))]
    public async Task ConvertToDoubleXY(int inputX, int inputY, double expectedX, double expectedY)
    {
        await Assert.That(this.quantizer.Get(inputX, inputY))
            .Member(p => p.X, x => x.IsEqualTo(expectedX)).And
            .Member(p => p.Y, y => y.IsEqualTo(expectedY));
    }
    
    [Test]
    [MethodDataSource(nameof(GetToIntXY))]
    public async Task ConvertToIntXY(double inputX, double inputY, int expectedX, int expectedY)
    {
        await Assert.That(this.quantizer.Get(inputX, inputY))
            .Member(p => p.X, x => x.IsEqualTo(expectedX)).And
            .Member(p => p.Y, y => y.IsEqualTo(expectedY));
    }
    
    [Test]
    public async Task ConvertToDoubleXYZ()
    {
        var (x, y, z) = this.quantizer.Get(321, 123, 456);
        await Assert.That(x).IsEqualTo(126.21);
        await Assert.That(y).IsEqualTo(457.23);
        await Assert.That(z).IsEqualTo(793.56);
    }

    public IEnumerable<Func<(int, int, double, double)>> GetToDoubleXY()
    {
        yield return () => (321, 123, 126.21, 457.23);
        yield return () => (-321, 123, 119.79, 457.23);
    }
    
    public IEnumerable<Func<(double, double, int, int)>> GetToIntXY()
    {
        yield return () => (126.21, 457.23, 321, 123);
        yield return () => (126.2199, 457.23, 322, 123);
        yield return () => (119.79, 457.23, -321, 123);
        yield return () => (119.7999, 457.23, -320, 123);
    }
}