namespace Altemiq.IO.Las;

public class GpsTimeTests
{
    [Test]
    [Arguments(PointDataRecord.Id)]
#if LAS1_2_OR_GREATER
    [Arguments(ColorPointDataRecord.Id)]
#endif
    public async Task ReadOffsetFromInvalidPointDataFormat(byte pointDataFormatId)
    {
        var builder = new HeaderBlockBuilder(pointDataFormatId);
        var header = builder.HeaderBlock;
        await Assert.That(GpsTime.GetOffset(header)).IsDefault();
    }

#if LAS1_5_OR_GREATER
    [Test]
    [Arguments(1995, 500000000)]
    [Arguments(2005, 750000000)]
    [Arguments(2010, 1000000000)]
    [Arguments(2020, 1250000000)]
    [Arguments(2025, 1500000000)]
    [Arguments(2035, 1750000000)]
    [Arguments(2040, 2000000000)]
    public async Task GetRecommendedOffset(int year, int offset)
    {
        await Assert.That(GpsTime.GetOffset(new DateTime(year, 1, 1))).IsEqualTo(offset);
    }
#endif
}