namespace Altemiq.IO.Las;

public class ColorTests
{
    [Test]
    public async Task SetValues()
    {
        var color = Color.FromRgb(12345, 54321, 12457);
        _ = await Assert.That(color.R).IsEqualTo((ushort)12345);
        _ = await Assert.That(color.G).IsEqualTo((ushort)54321);
        _ = await Assert.That(color.B).IsEqualTo((ushort)12457);
    }

    [Test]
    public async Task FromKnown()
    {
        var color = Color.Lime;
        _ = await Assert.That(color.R).IsEqualTo(ushort.MinValue);
        _ = await Assert.That(color.G).IsEqualTo(ushort.MaxValue);
        _ = await Assert.That(color.B).IsEqualTo(ushort.MinValue);
    }

    [Test]
    [Arguments(ushort.MaxValue)]
    [Arguments(ushort.MinValue)]
    public async Task TestRange(ushort value)
    {
        var color = Color.FromRgb(value, value, value);
        _ = await Assert.That(color.R).IsEqualTo(value);
        _ = await Assert.That(color.G).IsEqualTo(value);
        _ = await Assert.That(color.B).IsEqualTo(value);
    }

    [Test]
    public async Task RoundTrip()
    {
        var color = Color.FromRgb(12345, 54321, 12457);
        var next = Color.FromRgb(color.R, color.G, color.B);
        _ = await Assert.That(color).IsEqualTo(next);
    }

    [Test]
    public async Task ConvertToFromSystemColor()
    {
        var fuchsia = (Color)System.Drawing.Color.Fuchsia;
        System.Drawing.Color next = fuchsia;
        _ = await Assert.That(System.Drawing.Color.Fuchsia).IsEqualTo(next, ColorComparer.Instance);
    }

    [Test]
    public async Task KnownColors()
    {
        var color = (System.Drawing.Color)Color.Blue;
        _ = await Assert.That(color).IsEqualTo(System.Drawing.Color.Blue, ColorComparer.Instance);
    }

    [Test]
    public async Task HSL()
    {
        var systemColor = System.Drawing.Color.Fuchsia;
        var color = Color.Fuchsia;

        _ = await Assert.That(systemColor.GetHue()).IsEqualTo(color.GetHue());
        _ = await Assert.That(systemColor.GetSaturation()).IsEqualTo(color.GetSaturation());
        _ = await Assert.That(systemColor.GetBrightness()).IsEqualTo(color.GetBrightness());
    }

    private class ColorComparer : IEqualityComparer<System.Drawing.Color>
    {
        public static readonly IEqualityComparer<System.Drawing.Color> Instance = new ColorComparer();

        bool IEqualityComparer<System.Drawing.Color>.Equals(System.Drawing.Color x, System.Drawing.Color y)
        {
            return x.R == y.R && x.B == y.B && x.G == y.G;
        }

        int IEqualityComparer<System.Drawing.Color>.GetHashCode(System.Drawing.Color obj)
        {
            return HashCode.Combine(obj.R, obj.G, obj.B);
        }
    }
}