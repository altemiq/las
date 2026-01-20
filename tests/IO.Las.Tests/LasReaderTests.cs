namespace Altemiq.IO.Las;

public class LasReaderTests
{
    [Test]
    public async Task ReadEnumerable()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                               ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;

        foreach (var point in reader)
        {
            pointDataRecord = point.PointDataRecord;
            data = point.ExtraBytes.ToArray();

            break;
        }

        await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>().And.Member(p => p.X, x => x.IsNotDefault());
        await Assert.That(data).IsEmpty();
    }

    [Test]
    public async Task ReadAsyncEnumerable()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                               ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);

        await foreach (var point in reader)
        {
            var pointDataRecord = point.PointDataRecord;
            var data = point.ExtraBytes.ToArray();

            await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>().And.Member(p => p.X, x => x.IsNotDefault());
            await Assert.That(data).IsEmpty();

            break;
        }

    }

    [Test]
    public async Task ReadLas()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                              ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords.Count).IsEqualTo(1);

        var point = reader.ReadPointDataRecord();
        var pointDataRecord = point.PointDataRecord;
        var data = point.ExtraBytes.ToArray();

        await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>().And.Member(p => p.X, x => x.IsNotDefault());
        await Assert.That(data).IsEmpty();
    }

    [Test]
    public async Task ReadLasWithFileSignature()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                              ?? throw new InvalidOperationException("Failed to get stream");
        var bytes = new byte[4];

        await Assert.That(await stream.ReadAsync(bytes)).IsEqualTo(bytes.Length);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream, System.Text.Encoding.UTF8.GetString(bytes));
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasSingleItem();

        var point = reader.ReadPointDataRecord();
        var pointDataRecord = point.PointDataRecord;
        var data = point.ExtraBytes.ToArray();

        _ = await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>();
        _ = await Assert.That(data).IsEmpty();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytes()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa_height.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(2);

        var min = double.MaxValue;
        var max = double.MinValue;
        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        while (reader.ReadPointDataRecord() is { PointDataRecord: not null } point)
        {
            if (extraBytes[0].GetValue(point.ExtraBytes) is not double value)
            {
                continue;
            }

            if (value < min)
            {
                min = value;
            }

            if (value > max)
            {
                max = value;
            }
        }

        _ = await Assert.That(min).IsBetween(-1.76 - 0.001, -1.76 + 0.001);
        _ = await Assert.That(max).IsBetween(19.72 - 0.001, 19.72 + 0.001);
    }
#endif

    [Test]
    public async Task ReadByPointIndex()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));

        var quantizer = new PointDataRecordQuantizer(reader.Header);
        var fileCreation = reader.Header.FileCreation.GetValueOrDefault();
        await Assert.That(reader.ReadPointDataRecord(10).PointDataRecord)
            .IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27799961))
            .And.Member(p => p.Y, y => y.IsEqualTo(612234368))
            .And.Member(p => p.Z, z => z.IsEqualTo(6222))
            .And.IsTypeOf<IGpsPointDataRecord>()
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));

        await Assert.That(reader.ReadPointDataRecord(277500).PointDataRecord)
            .IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27775097))
            .And.Member(p => p.Y, y => y.IsEqualTo(612225071))
            .And.Member(p => p.Z, z => z.IsEqualTo(4228))
            .And.IsTypeOf<IGpsPointDataRecord>()
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));
    }

    [Test]
    public async Task ReadLasAsync()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasSingleItem();

        _ = await Assert.That(await reader.ReadPointDataRecordAsync())
            .Member(p => p.PointDataRecord, p => p.IsNotNull().And.Member(pt => pt.X, x => x.IsNotDefault()))
            .And.Member(p => p.ExtraBytes.IsEmpty, empty => empty.IsTrue());
    }

    [Test]
    public async Task ReadLasWithFileSignatureAsync()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
        var bytes = new byte[4];
        await Assert.That(stream.ReadAsync(bytes, 0, bytes.Length)).IsEqualTo(bytes.Length);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream, System.Text.Encoding.UTF8.GetString(bytes));
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasSingleItem();

        var point = await reader.ReadPointDataRecordAsync();

        _ = await Assert.That(point.PointDataRecord).IsAssignableTo<IGpsPointDataRecord>();
        _ = await Assert.That(point.ExtraBytes.IsEmpty).IsTrue();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytesAsync()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa_height.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(2);

        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        var min = double.MaxValue;
        var max = double.MinValue;

        while (await reader.ReadPointDataRecordAsync() is { PointDataRecord: not null } point)
        {
            _ = await Assert.That(point.PointDataRecord).IsAssignableTo<IGpsPointDataRecord>();

            var value = await Assert.That(await extraBytes.GetValueAsync(0, point.ExtraBytes)).IsTypeOf<double>();
            if (value < min)
            {
                min = value;
            }

            if (value > max)
            {
                max = value;
            }
        }

        _ = await Assert.That(min).IsBetween(-1.76 - 0.001, -1.76 + 0.001);
        _ = await Assert.That(max).IsBetween(19.72 - 0.001, 19.72 + 0.001);
    }
#endif

    [Test]
    public async Task ReadByPointIndexAsync()
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                               ?? throw new InvalidOperationException("Failed to get stream");
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));

        var quantizer = new PointDataRecordQuantizer(reader.Header);
        var fileCreation = reader.Header.FileCreation.GetValueOrDefault();
        var point = await reader.ReadPointDataRecordAsync(10);
        _ = await Assert.That(point.PointDataRecord)
            .IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27799961))
            .And.Member(p => p.Y, y => y.IsEqualTo(612234368))
            .And.Member(p => p.Z, z => z.IsEqualTo(6222))
            .And.IsTypeOf<GpsPointDataRecord>()
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));

        point = await reader.ReadPointDataRecordAsync(277500);
        _ = await Assert.That(point.PointDataRecord)
            .Member(p => p.X, x => x.IsEqualTo(27775097))
            .And.Member(p => p.Y, y => y.IsEqualTo(612225071))
            .And.Member(p => p.Z, z => z.IsEqualTo(4228))
            .And.IsTypeOf<IGpsPointDataRecord>()
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));
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