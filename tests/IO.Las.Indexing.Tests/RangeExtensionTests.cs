namespace Altemiq.IO.Las.Indexing;

public class RangeExtensionTests
{
    [Test]
    public async Task GetIndexesWithMaxValue()
    {
        var indexes = new Range(new(uint.MaxValue - 1), new(uint.MaxValue));
        _ = await Assert.That(indexes.GetIndexes()).Count().IsEqualTo(2);
    }
}