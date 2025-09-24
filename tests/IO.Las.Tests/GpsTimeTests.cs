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
}