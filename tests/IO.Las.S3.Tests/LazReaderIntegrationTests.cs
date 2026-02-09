namespace Altemiq.IO.Las.S3;

public class LazReaderIntegrationTests
{
    [ClassDataSource<Data.S3ClientDataClass>(Shared = SharedType.PerTestSession)]
    public required Data.S3ClientDataClass S3ClientData { get; init; }
    
    [Test]
    [Arguments("lidar", "laz/fusa.laz", true)]
    [Arguments("lidar", "laz/asuf.laz", false)]
    public async Task LazExists(string bucketName, string blobName, bool expected)
    {
        await Assert.That(S3Las.Exists(bucketName, blobName, this.S3ClientData.S3Client)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("lidar", "laz/fusa.laz", true)]
    [Arguments("lidar", "laz/asuf.laz", false)]
    public async Task LazExistsAsync(string bucketName, string key, bool expected)
    {
        await Assert.That(async () => await S3Las.ExistsAsync(bucketName, key, this.S3ClientData.S3Client)).IsEqualTo(expected);
    }
    
    [Test]
    public async Task ReadLaz()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
            using var stream = await S3Las.OpenReadAsync("lidar", "laz/fusa.laz", this.S3ClientData.S3Client);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
            using LazReader reader = new(stream);

        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(2);

        await Assert.That(reader.ReadPointDataRecord(10).PointDataRecord).IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27799961))
            .And.Member(p => p.Y, y => y.IsEqualTo(612234368))
            .And.Member(p => p.Z, z => z.IsEqualTo(6222));

        await Assert.That(reader.ReadPointDataRecord(277500).PointDataRecord).IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27775097))
            .And.Member(p => p.Y, y => y.IsEqualTo(612225071))
            .And.Member(p => p.Z, z => z.IsEqualTo(4228));
    }

    private static async Task CheckHeader(HeaderBlock headerBlock, Version expectedVersion)
    {
        _ = await Assert.That(headerBlock)
            .Member(static headerBlock => headerBlock.FileSignature, static fileSignature => fileSignature.IsEqualTo("LASF"))
            .And.Member(static headerBlock => headerBlock.FileSourceId, static fileSourceId => fileSourceId.IsDefault())
#if LAS1_2_OR_GREATER
            .And.Member(static headerBlock => headerBlock.GlobalEncoding, static globalEncoding => globalEncoding.IsEqualTo(GlobalEncoding.None))
#endif
            .And.Member(static headerBlock => headerBlock.ProjectId, static projectId => projectId.IsEqualTo(Guid.Empty))
            .And.Member(static headerBlock => headerBlock.Version, version => version.IsEqualTo(expectedVersion))
            .And.Member(static headerBlock => headerBlock.SystemIdentifier, static systemIdentifier => systemIdentifier.Contains("LAStools").And.Contains("rapidlasso"))
            .And.Member(static headerBlock => headerBlock.FileCreation.GetValueOrDefault(), static fileCreation => fileCreation.IsEqualTo(new DateTime(2010, 2, 9)))
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
            .And.Member(static headerBlock => headerBlock.ScaleFactor, scaleFactor => scaleFactor.IsEqualTo(new Vector3D(0.01, 0.01, 0.01)))
            .And.Member(static headerBlock => headerBlock.Offset, offset => offset.IsDefault())
            .And.Member(static headerBlock => headerBlock.Min, min => min.IsEqualTo(new Vector3D(277750.0, 6122250.0, 42.21)))
            .And.Member(static headerBlock => headerBlock.Max, max => max.IsEqualTo(new Vector3D(277999.99, 6122499.99, 64.35)));
    }
}