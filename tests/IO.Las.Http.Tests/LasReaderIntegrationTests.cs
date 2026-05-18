namespace Altemiq.IO.Las.Http;

public class LasReaderIntegrationTests
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    [Arguments("/las/fusa.las", true)]
    [Arguments("/las/asuf.las", false)]
    public async Task LasExists(string path, bool expected)
    {
        await Assert.That(() => HttpLas.Exists(path, this.WebApplicationFactory.CreateClient())).IsEqualTo(expected);
    }

    [Test]
    [Arguments("/las/fusa.las", true)]
    [Arguments("/las/asuf.las", false)]
    public async Task LasExistsAsync(string path, bool expected)
    {
        await Assert.That(async () => await HttpLas.ExistsAsync(path, this.WebApplicationFactory.CreateClient())).IsEqualTo(expected);
    }

    [Test]
    public async Task ReadLasAsync()
    {
        var stream = await HttpLas.OpenReadAsync("/las/fusa.las", this.WebApplicationFactory.CreateClient());
        LasReader reader = new(stream);

        await CheckHeader(reader.Header, new(1, 1));
        await Assert.That(reader.VariableLengthRecords).HasSingleItem();

        await Assert.That(await reader.ReadPointDataRecordAsync())
            .Member(p => p.PointDataRecord, p => p.IsNotNull().And.Member(pt => pt.X, x => x.IsNotDefault()))
            .And.Member(p => p.ExtraBytes.IsEmpty, empty => empty.IsTrue());

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    private static async Task CheckHeader(HeaderBlock headerBlock, Version expectedVersion)
    {
        await Assert.That(headerBlock)
            .Member(static headerBlock => headerBlock.FileSignature, static fileSignature => fileSignature.IsEqualTo("LASF"))
            .And.Member(static headerBlock => headerBlock.FileSourceId, static fileSourceId => fileSourceId.IsDefault())
#if LAS1_2_OR_GREATER
            .And.Member(static headerBlock => headerBlock.GlobalEncoding, static globalEncoding => globalEncoding.IsEqualTo(GlobalEncoding.None))
#endif
            .And.Member(static headerBlock => headerBlock.ProjectId, static projectId => projectId.IsEqualTo(Guid.Empty))
            .And.Member(static headerBlock => headerBlock.Version, version => version.IsEqualTo(expectedVersion))
            .And.Member(static headerBlock => headerBlock.SystemIdentifier, static systemIdentifier => systemIdentifier.IsEqualTo("LAStools (c) by rapidlasso GmbH"))
            .And.Member(static headerBlock => headerBlock.FileCreation.GetValueOrDefault(), static fileCreation => fileCreation.IsEqualTo(new(2010, 2, 9)))
#if LAS1_4_OR_GREATER
            .And.Member(static headerBlock => headerBlock.NumberOfPointRecords, static numberOfPointRecords => numberOfPointRecords.IsEqualTo(277573UL))
            .And.Member(static headerBlock => headerBlock.LegacyNumberOfPointsByReturn, static legacyNumberOfPointsByReturn => legacyNumberOfPointsByReturn.IsEquivalentTo([263413U, 13879U, 281U, 0U, 0U]))
            .And.Member(static headerBlock => headerBlock.NumberOfPointsByReturn, numberOfPointsByReturn =>
                expectedVersion < new Version(1, 4)
                    ? numberOfPointsByReturn.IsEquivalentTo([263413UL, 13879UL, 281UL, 0UL, 0UL])
                    : numberOfPointsByReturn.IsEquivalentTo([263413UL, 13879UL, 281UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL]))
#else
            .And.Member(static headerBlock => headerBlock.NumberOfPointRecords, numberOfPointRecords => numberOfPointRecords.IsEqualTo(277573U))
            .And.Member(static headerBlock => headerBlock.NumberOfPointsByReturn, numberOfPointsByReturn => numberOfPointsByReturn.IsEquivalentTo([263413U, 13879U, 281U, 0U, 0U]))
#endif
            .And.Member(static headerBlock => headerBlock.ScaleFactor, scaleFactor => scaleFactor.IsEqualTo(new(0.01, 0.01, 0.01)))
            .And.Member(static headerBlock => headerBlock.Offset, offset => offset.IsDefault())
            .And.Member(static headerBlock => headerBlock.Min, min => min.IsEqualTo(new(277750.0, 6122250.0, 42.21)))
            .And.Member(static headerBlock => headerBlock.Max, max => max.IsEqualTo(new(277999.99, 6122499.99, 64.35)));
    }
}