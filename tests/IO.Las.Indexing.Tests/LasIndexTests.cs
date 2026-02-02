namespace Altemiq.IO.Las.Indexing;

public class LasIndexTests
{
    [Test]
    public async Task ReadLaxFile()
    {
        var stream = typeof(LasIndexTests).Assembly.GetManifestResourceStream(typeof(LasIndexTests), "fusa.lax")
                     ?? throw new System.Diagnostics.UnreachableException();
        _ = await Assert.That(() => LasIndex.ReadFrom(stream)).ThrowsNothing();
    }

}