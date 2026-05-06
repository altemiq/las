namespace Altemiq.IO.Las.Indexing;

public class LasIndexTests
{
    [Test]
    public async Task ReadLaxFile()
    {
        var stream = typeof(LasIndexTests).Assembly.GetManifestResourceStream(typeof(LasIndexTests), "fusa.lax")
                     ?? throw new System.Diagnostics.UnreachableException();
        await Assert.That(() => LasIndex.ReadFrom(stream)).ThrowsNothing();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task GetIndex()
    {
        var index = new LasIndex(new(0, 50, 0, 50, 50)) { { 1F, 1F, 5 } };
        await Assert.That(index.IndexOf(5)).IsEqualTo(1);
    }
}