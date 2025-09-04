namespace Altemiq.IO.Las.Indexing;

public class LasIndexTests
{
    [Test]
    public async Task ReadLaxFile()
    {
        Stream stream = typeof(LasIndexTests).Assembly.GetManifestResourceStream(typeof(LasIndexTests), "fusa.lax") ?? throw new InvalidOperationException();
        _ = await Assert.That(() => LasIndex.ReadFrom(stream)).ThrowsNothing();
    }

}