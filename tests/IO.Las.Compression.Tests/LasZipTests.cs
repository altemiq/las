namespace Altemiq.IO.Las.Compression;

public class LasZipTests
{
#if LAS1_4_OR_GREATER
    [Test]
    [MatrixDataSource]
    public async Task InvalidCompressor([Matrix(Compressor.PointWise, Compressor.PointWiseChunked)] Compressor compressor)
    {
        _ = await Assert.That(() => _ = new LasZip(ExtendedGpsPointDataRecord.Id, 0, compressor, LasZip.GetValidVersion(ExtendedGpsPointDataRecord.Id, HeaderBlock.DefaultVersion))).Throws<ArgumentException>();
    }

    [Test]
    public async Task DowngradeCompressor()
    {
        LasZip lasZip = new(GpsPointDataRecord.Id, 0, Compressor.LayeredChunked);
        _ = await Assert.That(lasZip)
            .Member(static lasZip => lasZip.Compressor, static compressor => compressor.IsEqualTo(Compressor.PointWiseChunked))
            .And.Member(static lasZip => lasZip.ChunkSize, static chunkSize => chunkSize.IsEqualTo(50000U));
    }

    [Test]
    [Arguments(GpsPointDataRecord.Id, LasItemType.Byte)]
    [Arguments(ExtendedGpsPointDataRecord.Id, LasItemType.Byte14)]
    public async Task AddExtraBytes(byte pointDataFormatId, LasItemType lasItemType)
    {
        var version = LasZip.GetValidVersion(pointDataFormatId, HeaderBlock.DefaultVersion);
        LasZip lasZip = new(pointDataFormatId, 2, Compressor.None, version);
        _ = await Assert.That(lasZip.Items).Contains(new LasItem { Type = lasItemType, Version = version, Size = 2 });
    }

    [Test]
    public async Task InvalidCompressorValue()
    {
        _ = await Assert.That(() => new LasZip(PointDataRecord.Id, 0, (Compressor)ushort.MaxValue).Validate(PointDataRecord.Size)).Throws<InvalidDataException>();
    }
#else
    [Test]
    public async Task InvalidCompressorValue()
    {
        _ = await Assert.That(() => new LasZip(PointDataRecord.Id, (Compressor)ushort.MaxValue).Validate(PointDataRecord.Size)).Throws<InvalidDataException>();
    }
#endif
}