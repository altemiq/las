// -----------------------------------------------------------------------
// <copyright file="LazReaderTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

public class LazReaderTests
{
#if LAS1_2_OR_GREATER
    [Test]
#if LAS1_4_OR_GREATER
    [Arguments("coloured.laz", typeof(GpsColorPointDataRecord), 5, false)]
    [Arguments("point_7.laz", typeof(ExtendedGpsColorPointDataRecord), 5, false)]
    [Arguments("fusa_height_7.laz", typeof(ExtendedGpsColorPointDataRecord), 3, true)]
    public async Task ReadLaz(string file, Type expectedType, int vlrCount, bool hasExtraBytes)
#else
    [Arguments("coloured.laz", typeof(GpsColorPointDataRecord), 5)]
    public async Task ReadLaz(string file, Type expectedType, int vlrCount)
#endif
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), file)
            ?? throw new InvalidOperationException("Failed to get stream");
        using LazReader reader = new(stream);
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(vlrCount);

        var count = 10;
        while (--count > 0)
        {
            await TestPoint(reader);
        }

        async Task TestPoint(LasReader pointReader)
        {
            var record = pointReader.ReadPointDataRecord();
            var point = record.PointDataRecord;
            var extraBytes = record.ExtraBytes.ToArray();
            _ = await Assert.That(point).IsNotNull().And.IsTypeOf(expectedType);
#if LAS1_4_OR_GREATER
            _ = hasExtraBytes ? await Assert.That(extraBytes).IsNotEmpty() : await Assert.That(extraBytes).IsEmpty();
#endif
        }
    }
#endif

    [Test]
    [Arguments("fusa.laz")]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz")]
#endif
    public async Task Create(string resource)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), resource)
            ?? throw new InvalidOperationException("Failed to get stream");

        var reader = LazReader.Create(stream);
        _ = await Assert.That(reader)
            .IsNotNull()
            .And.IsTypeOf<LazReader>()
            .And.Member(static x => x.IsCompressed, x => x.IsTrue());
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytes()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa_height.laz")
            ?? throw new InvalidOperationException("Failed to get stream");
        using LazReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(3);
        var extraBytesTag = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        var min = double.MaxValue;
        var max = double.MinValue;

        while (true)
        {
            var record = reader.ReadPointDataRecord();
            if (record.PointDataRecord is not { } point)
            {
                break;
            }

            var extraBytes = record.ExtraBytes.ToArray();

            _ = await Assert.That(point).IsTypeOf<GpsPointDataRecord>();
            _ = await Assert.That(extraBytes).IsNotEmpty();

            var value = await Assert.That(extraBytesTag[0].GetValue(extraBytes.AsSpan())).IsTypeOf<double>();
            min = Math.Min(value, min);
            max = Math.Max(value, max);
        }

        _ = await Assert.That(min).IsBetween(-1.76 - 0.001, -1.76 + 0.001);
        _ = await Assert.That(max).IsBetween(19.72 - 0.001, 19.72 + 0.001);
    }
#endif

    [Test]
    [Arguments("fusa.laz", 1, 1)]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz", 1, 4)]
#endif
    public async Task ReadByPointIndex(string resource, int major, int minor)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), resource)
            ?? throw new InvalidOperationException("Failed to get stream");
        using LazReader reader = new(stream);
        await CheckHeader(reader.Header, new(major, minor));

        var point = reader.ReadPointDataRecord(10).PointDataRecord!;
        await Assert.That(point).IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27799961))
            .And.Member(p => p.Y, y => y.IsEqualTo(612234368))
            .And.Member(p => p.Z, z => z.IsEqualTo(6222));

        point = reader.ReadPointDataRecord(277500).PointDataRecord!;
        await Assert.That(point).IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27775097))
            .And.Member(p => p.Y, y => y.IsEqualTo(612225071))
            .And.Member(p => p.Z, z => z.IsEqualTo(4228));
    }


#if LAS1_2_OR_GREATER
    [Test]
    [Arguments("coloured.laz", typeof(GpsColorPointDataRecord), 5, false)]
#if LAS1_4_OR_GREATER
    [Arguments("point_7.laz", typeof(ExtendedGpsColorPointDataRecord), 5, false)]
    [Arguments("fusa_height_7.laz", typeof(ExtendedGpsColorPointDataRecord), 3, true)]
#endif
    public async Task ReadLazAsync(string file, Type expectedType, int vlrCount, bool hasExtraBytes)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), file)
            ?? throw new InvalidOperationException("Failed to get stream");
        using LazReader reader = new(stream);
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(vlrCount);

        await TestPointAsync(reader, expectedType, hasExtraBytes);
        await TestPointAsync(reader, expectedType, hasExtraBytes);

        static async Task TestPointAsync(LasReader pointReader, Type expectedType, bool hasExtraBytes)
        {
            var point = await pointReader.ReadPointDataRecordAsync();
            _ = await Assert.That(point.PointDataRecord)
                .IsNotNull()
                .And.IsTypeOf(expectedType);
            _ = hasExtraBytes ? await Assert.That(point.ExtraBytes.IsEmpty).IsFalse() : await Assert.That(point.ExtraBytes.IsEmpty).IsTrue();
        }
    }
#endif

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytesAsync()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa_height.laz")
            ?? throw new InvalidOperationException("Failed to get stream");
        using LazReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(3);
        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        var min = double.MaxValue;
        var max = double.MinValue;

        while (await reader.ReadPointDataRecordAsync() is { PointDataRecord: not null } point)
        {
            _ = await Assert.That(point.PointDataRecord).IsTypeOf<GpsPointDataRecord>();
            _ = await Assert.That(point.ExtraBytes.IsEmpty).IsFalse();
            var value = await Assert.That(await extraBytes.GetValueAsync(0, point.ExtraBytes)).IsTypeOf<double>();
            min = Math.Min(value, min);
            max = Math.Max(value, max);
        }

        _ = await Assert.That(min).IsBetween(-1.76 - 0.001, -1.76 + 0.001);
        _ = await Assert.That(max).IsBetween(19.72 - 0.001, 19.72 + 0.001);
    }
#endif

    [Test]
    [Arguments("fusa.laz", 1, 1)]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz", 1, 4)]
#endif
    public async Task ReadByPointIndexAsync(string resource, int major, int minor)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), resource)
            ?? throw new InvalidOperationException("Failed to get stream");
        using LazReader reader = new(stream);
        await CheckHeader(reader.Header, new(major, minor));

        var record = await reader.ReadPointDataRecordAsync(10);
        await Assert.That(record.PointDataRecord).IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27799961))
            .And.Member(p => p.Y, y => y.IsEqualTo(612234368))
            .And.Member(p => p.Z, z => z.IsEqualTo(6222));

        record = await reader.ReadPointDataRecordAsync(277500);
        await Assert.That(record.PointDataRecord).IsNotNull()
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
            .And.Member(static headerBlock => headerBlock.SystemIdentifier, static systemIdentifier => systemIdentifier.IsEqualTo("LAStools (c) by rapidlasso GmbH"))
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