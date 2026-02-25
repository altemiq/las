namespace Altemiq.IO.Las.Compression.Arrow;

public class LazReaderExtensionTests
{
    [Test]
    public async Task ToArrowBatches()
    {
        var reader = new Data.MockLazReader();

        var batches = reader.ToArrowBatches();

        await Assert.That(batches).Count().IsEqualTo(1);
    }
}