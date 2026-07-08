namespace Altemiq.IO.Las;

public class PointDataRecordQuantizerTests
{
    private readonly PointDataRecordQuantizer quantizer = new(
        new(0.01, 0.01, 0.01),
        new(123, 456, 789),
        0);

    [Test]
    [MethodDataSource(nameof(GetToDoubleXY))]
    public async Task ConvertToDoubleXY(int inputX, int inputY, double expectedX, double expectedY)
    {
        _ = await Assert.That(quantizer.Get(inputX, inputY)).IsEqualTo(new(expectedX, expectedY));
    }

    [Test]
    [MethodDataSource(nameof(GetToIntXY))]
    public async Task ConvertToIntXY(double inputX, double inputY, int expectedX, int expectedY)
    {
        _ = await Assert.That(quantizer.Get(inputX, inputY))
            .Member(p => p.X, x => x.IsEqualTo(expectedX)).And
            .Member(p => p.Y, y => y.IsEqualTo(expectedY));
    }

    [Test]
    public async Task ConvertToDoubleXYZ()
    {
        _ = await Assert.That(quantizer.Get(321, 123, 456)).IsEqualTo(new(126.21, 457.23, 793.56));
    }

    [Test]
    public async Task ConvertToIntXYZ()
    {
        _ = await Assert.That(quantizer.Get(126.21, 457.23, 793.56))
            .Member(static p => p.X, static x => x.IsEqualTo(321)).And
            .Member(static p => p.Y, static y => y.IsEqualTo(123)).And
            .Member(static p => p.Z, static z => z.IsEqualTo(456));
    }

#if NETCOREAPP3_0_OR_GREATER
    [Test]
    public async Task Quantize3D()
    {
        int[] x = [321, -321, 0, 100, 200, -200, 50, 10];
        int[] y = [123, 123, 456, 0, 100, -100, 50, 10];
        int[] z = [456, 456, 789, 1000, 0, -1000, 50, 10];
        var results = new Vector3D[x.Length];

        quantizer.Quantize(x, y, z, results);

        _ = await Assert.That(results[0]).IsEqualTo(new(126.21, 457.23, 793.56));
        _ = await Assert.That(results[1]).IsEqualTo(new(119.79, 457.23, 793.56));
        _ = await Assert.That(results[2]).IsEqualTo(new(123.00, 460.56, 796.89));
        _ = await Assert.That(results[3]).IsEqualTo(new(124.00, 456.00, 799.00));
        _ = await Assert.That(results[4]).IsEqualTo(new(125.00, 457.00, 789.00));
        _ = await Assert.That(results[5]).IsEqualTo(new(121.00, 455.00, 779.00));
        _ = await Assert.That(results[6]).IsEqualTo(new(123.50, 456.50, 789.50));
        _ = await Assert.That(results[7]).IsEqualTo(new(123.10, 456.10, 789.10));
    }

    [Test]
    public async Task Quantize3F()
    {
        int[] x = [321, -321, 0, 100, 200, -200, 50, 10, 321];
        int[] y = [123, 123, 456, 0, 100, -100, 50, 10, 123];
        int[] z = [456, 456, 789, 1000, 0, -1000, 50, 10, 456];
        var results = new System.Numerics.Vector3[x.Length];

        quantizer.Quantize(x, y, z, results);

        _ = await Assert.That(results[0]).IsEqualTo(new(126.21F, 457.23F, 793.56F));
        _ = await Assert.That(results[1]).IsEqualTo(new(119.79F, 457.23F, 793.56F));
        _ = await Assert.That(results[2]).IsEqualTo(new(123.00F, 460.56F, 796.89F));
        _ = await Assert.That(results[3]).IsEqualTo(new(124.00F, 456.00F, 799.00F));
        _ = await Assert.That(results[4]).IsEqualTo(new(125.00F, 457.00F, 789.00F));
        _ = await Assert.That(results[5]).IsEqualTo(new(121.00F, 455.00F, 779.00F));
        _ = await Assert.That(results[6]).IsEqualTo(new(123.50F, 456.50F, 789.50F));
        _ = await Assert.That(results[7]).IsEqualTo(new(123.10F, 456.10F, 789.10F));
        _ = await Assert.That(results[8]).IsEqualTo(new(126.21F, 457.23F, 793.56F));
    }

    [Test]
    public async Task Quantize2D()
    {
        int[] x = [321, -321, 0, 100, 200];
        int[] y = [123, 123, 456, 0, 100];
        var results = new Vector2D[x.Length];

        quantizer.Quantize(x, y, results);

        _ = await Assert.That(results[0]).IsEqualTo(new(126.21, 457.23));
        _ = await Assert.That(results[1]).IsEqualTo(new(119.79, 457.23));
        _ = await Assert.That(results[2]).IsEqualTo(new(123.00, 460.56));
        _ = await Assert.That(results[3]).IsEqualTo(new(124.00, 456.00));
        _ = await Assert.That(results[4]).IsEqualTo(new(125.00, 457.00));
    }

    [Test]
    public async Task Quantize2F()
    {
        int[] x = [321, -321, 0, 100, 200, 321, -321, 0, 100];
        int[] y = [123, 123, 456, 0, 100, 123, 123, 456, 0];
        var results = new System.Numerics.Vector2[x.Length];

        quantizer.Quantize(x, y, results);

        _ = await Assert.That(results[0]).IsEqualTo(new(126.21F, 457.23F));
        _ = await Assert.That(results[1]).IsEqualTo(new(119.79F, 457.23F));
        _ = await Assert.That(results[2]).IsEqualTo(new(123.00F, 460.56F));
        _ = await Assert.That(results[3]).IsEqualTo(new(124.00F, 456.00F));
        _ = await Assert.That(results[4]).IsEqualTo(new(125.00F, 457.00F));
        _ = await Assert.That(results[5]).IsEqualTo(new(126.21F, 457.23F));
        _ = await Assert.That(results[6]).IsEqualTo(new(119.79F, 457.23F));
        _ = await Assert.That(results[7]).IsEqualTo(new(123.00F, 460.56F));
        _ = await Assert.That(results[8]).IsEqualTo(new(124.00F, 456.00F));
    }

    [Test]
    public async Task QuantizeEmptyD()
    {
        int[] x = [];
        int[] y = [];
        int[] z = [];
        Vector3D[] results = [];

        quantizer.Quantize(x, y, z, results);

        _ = await Assert.That(results.Length).IsEqualTo(0);
    }

    [Test]
    public async Task QuantizeEmptyF()
    {
        int[] x = [];
        int[] y = [];
        int[] z = [];
        System.Numerics.Vector3[] results = [];

        quantizer.Quantize(x, y, z, results);

        _ = await Assert.That(results.Length).IsEqualTo(0);
    }
#endif

    public IEnumerable<Func<(int, int, double, double)>> GetToDoubleXY()
    {
        yield return static () => (321, 123, 126.21, 457.23);
        yield return static () => (-321, 123, 119.79, 457.23);
    }

    public IEnumerable<Func<(double, double, int, int)>> GetToIntXY()
    {
        yield return static () => (126.21, 457.23, 321, 123);
        yield return static () => (126.2199, 457.23, 322, 123);
        yield return static () => (119.79, 457.23, -321, 123);
        yield return static () => (119.7999, 457.23, -320, 123);
    }
}