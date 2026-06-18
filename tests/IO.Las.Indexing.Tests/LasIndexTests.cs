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

    [Test]
    public async Task CreateFromReader()
    {
        var stream = typeof(LasIndexTests).Assembly.GetManifestResourceStream(typeof(LasIndexTests), "fusa.lax")
                     ?? throw new System.Diagnostics.UnreachableException();
        var fromLax = await Assert.That(() => LasIndex.ReadFrom(stream)).IsTypeOf<LasIndex>();
        await stream.DisposeAsync();

        var reader = new LasReader(typeof(LasIndexTests).Assembly.GetManifestResourceStream(typeof(LasIndexTests), "fusa.las")
                                   ?? throw new System.Diagnostics.UnreachableException());
        //var rdr = LasIndex.Create(reader);
        var fromLas = await Assert.That(() => LasIndex.Create(reader)).IsTypeOf<LasIndex>();
        await reader.DisposeAsync();

        await Assert.That(fromLax).IsEquivalentTo(fromLas);
    }
}