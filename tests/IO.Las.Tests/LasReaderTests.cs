namespace Altemiq.IO.Las;

public class LasReaderTests
{

    [Test]
    public async Task ReadEnumerable()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LasReader reader = new(stream);

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;

        foreach (var point in reader)
        {
            pointDataRecord = point.PointDataRecord;
            data = point.ExtraBytes.ToArray();

            break;
        }

        _ = await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>().And.Member(static p => p.X, static x => x.IsNotDefault());
        _ = await Assert.That(data).IsEmpty();

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task ReadAsyncEnumerable()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                               ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LasReader reader = new(stream);

        await foreach (var (pointDataRecord, extraBytes) in reader)
        {
            _ = await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>().And.Member(static p => p.X, static x => x.IsNotDefault());
            _ = await Assert.That(extraBytes.ToArray()).IsEmpty();

            break;
        }

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task ReadLas()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                 ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords.Count).IsEqualTo(1);

        var (pointDataRecord, extraBytes) = reader.ReadPointDataRecord();

        _ = await Assert.That(extraBytes.ToArray()).IsEmpty();
        _ = await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>().And.Member(static p => p.X, static x => x.IsNotDefault());

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task ReadLasWithFileSignature()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                              ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        var bytes = new byte[4];

        _ = await Assert.That(await stream.ReadAsync(bytes)).IsEqualTo(bytes.Length);

        LasReader reader = new(stream, System.Text.Encoding.UTF8.GetString(bytes));
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasSingleItem();

        var (pointDataRecord, extraBytes) = reader.ReadPointDataRecord();

        _ = await Assert.That(extraBytes.ToArray()).IsEmpty();
        _ = await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>();

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytes()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa_height.las")
                     ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        var min = double.MaxValue;
        var max = double.MinValue;
        LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(2);

        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        while (reader.ReadPointDataRecord() is { PointDataRecord: not null } point)
        {
            if (extraBytes[0].GetValue(point.ExtraBytes) is not double value)
            {
                continue;
            }

            _ = Interlocked.MinExchange(ref min, value);
            _ = Interlocked.MaxExchange(ref max, value);
        }

        await reader.DisposeAsync();

        await stream.DisposeAsync();

        _ = await Assert.That(min).IsEqualTo(-1.76).Within(0.001);
        _ = await Assert.That(max).IsEqualTo(19.72).Within(0.001);
    }
#endif

    [Test]
    public async Task ReadByPointIndex()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));

        var quantizer = new PointDataRecordQuantizer(reader.Header);
        var fileCreation = reader.Header.FileCreation.GetValueOrDefault();
        _ = await Assert.That(reader.ReadPointDataRecord(10).PointDataRecord)
            .IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27799961))
            .And.Member(p => p.Y, y => y.IsEqualTo(612234368))
            .And.Member(p => p.Z, z => z.IsEqualTo(6222))
            .And.IsTypeOf<IGpsPointDataRecord>()
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsEqualTo(fileCreation).Within(TimeSpan.FromDays(7)));

        _ = await Assert.That(reader.ReadPointDataRecord(277500).PointDataRecord)
            .IsNotNull()
            .And.Member(p => p.X, x => x.IsEqualTo(27775097))
            .And.Member(p => p.Y, y => y.IsEqualTo(612225071))
            .And.Member(p => p.Z, z => z.IsEqualTo(4228))
            .And.IsTypeOf<IGpsPointDataRecord>()
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsEqualTo(fileCreation).Within(TimeSpan.FromDays(7)));

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task ReadLasAsync()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasSingleItem();

        _ = await Assert.That(await reader.ReadPointDataRecordAsync())
            .Member(static p => p.PointDataRecord, static p => p.IsNotNull().And.Member(static pt => pt.X, static x => x.IsNotDefault()))
            .And.Member(static p => p.ExtraBytes.IsEmpty, static empty => empty.IsTrue());

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task ReadLasWithFileSignatureAsync()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        var bytes = new byte[4];
        _ = await Assert.That(stream.ReadAsync(bytes, 0, bytes.Length)).IsEqualTo(bytes.Length);
        LasReader reader = new(stream, System.Text.Encoding.UTF8.GetString(bytes));
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasSingleItem();

        var (pointDataRecord, extraBytes) = await reader.ReadPointDataRecordAsync();

        await reader.DisposeAsync();
        await stream.DisposeAsync();

        _ = await Assert.That(pointDataRecord).IsAssignableTo<IGpsPointDataRecord>();
        _ = await Assert.That(extraBytes.IsEmpty).IsTrue();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytesAsync()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa_height.las")
                                    ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(2);

        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        var min = double.MaxValue;
        var max = double.MinValue;

        while (await reader.ReadPointDataRecordAsync() is { PointDataRecord: not null } point)
        {
            _ = await Assert.That(point.PointDataRecord).IsAssignableTo<IGpsPointDataRecord>();

            var value = await Assert.That(await extraBytes.GetValueAsync(0, point.ExtraBytes)).ValueIsTypeOf<double>();
            _ = Interlocked.MinExchange(ref min, value);
            _ = Interlocked.MaxExchange(ref max, value);
        }

        await reader.DisposeAsync();
        await stream.DisposeAsync();

        _ = await Assert.That(min).IsEqualTo(-1.76).Within(0.001);
        _ = await Assert.That(max).IsEqualTo(19.72).Within(0.001);
    }
#endif

    [Test]
    public async Task ReadByPointIndexAsync()
    {
        var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                               ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LasReader reader = new(stream);
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
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsEqualTo(fileCreation).Within(TimeSpan.FromDays(7)));

        point = await reader.ReadPointDataRecordAsync(277500);
        _ = await Assert.That(point.PointDataRecord)
            .Member(p => p.X, x => x.IsEqualTo(27775097))
            .And.Member(p => p.Y, y => y.IsEqualTo(612225071))
            .And.Member(p => p.Z, z => z.IsEqualTo(4228))
            .And.IsTypeOf<IGpsPointDataRecord>()
            .And.Member(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsEqualTo(fileCreation).Within(TimeSpan.FromDays(7)));

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [Test]
    public async Task ReadToSpan()
    {
        await using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                               ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        await using LasReader reader = new(stream);

        var span = new byte[1024];

        var pointsRead = await Assert.That(reader.ReadPointDataRecordData(span)).IsNotZero();

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;

        for (var i = 0; i < pointsRead; i++)
        {
            (pointDataRecord, var extraBytes) = reader.Read(span.AsSpan(i * reader.PointDataLength, reader.PointDataLength));
            data = extraBytes.ToArray();
        }

        _ = await Assert.That(pointDataRecord)
            .IsTypeOf<GpsPointDataRecord>().And
            .Member(static p => p.X, static x => x.IsNotDefault());
        _ = await Assert.That(data).IsEmpty();
    }

    [Test]
    public async Task ReadToMemory()
    {
        await using var stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                               ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        await using LasReader reader = new(stream);

        var span = new byte[1024];

        var pointsRead = await reader.ReadPointDataRecordDataAsync(span);
        _ = await Assert.That(pointsRead).IsNotZero();

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;

        for (var i = 0; i < pointsRead; i++)
        {
            (pointDataRecord, var extraBytes) = await reader.ReadAsync(span.AsMemory(i * reader.PointDataLength, reader.PointDataLength));
            data = extraBytes.ToArray();
        }

        _ = await Assert.That(pointDataRecord)
            .IsTypeOf<GpsPointDataRecord>().And
            .Member(static p => p.X, static x => x.IsNotDefault());
        _ = await Assert.That(data).IsEmpty();
    }
#endif

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