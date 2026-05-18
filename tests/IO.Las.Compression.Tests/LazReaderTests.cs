// -----------------------------------------------------------------------
// <copyright file="LazReaderTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

public class LazReaderTests
{
#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadEnumerable()
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa_height_7.laz")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LazReader reader = new(stream);

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;
        var count = 0;

        foreach (var point in reader.ReadChunk(0))
        {
            pointDataRecord = point.PointDataRecord;
            data = point.ExtraBytes.ToArray();
            count++;
        }

        await reader.DisposeAsync();

        await Assert.That(count).IsEqualTo(50000);
        await Assert.That(pointDataRecord).IsTypeOf<ExtendedGpsColorPointDataRecord>().And.Member(p => p.X, x => x.IsNotDefault());
        await Assert.That(data).IsNotEmpty();

        await stream.DisposeAsync();
    }

    [Test]
    public async Task ReadAsyncEnumerable()
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa_height_7.laz")
                               ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LazReader reader = new(stream);

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;
        var count = 0;

        await foreach (var point in reader.ReadChunkAsync(0).WithCancellation(CancellationToken.None))
        {
            pointDataRecord = point.PointDataRecord;
            data = point.ExtraBytes.ToArray();
            count++;
        }

        await reader.DisposeAsync();

        await Assert.That(count).IsEqualTo(50000);
        await Assert.That(pointDataRecord).IsTypeOf<ExtendedGpsColorPointDataRecord>().And.Member(p => p.X, x => x.IsNotDefault());
        await Assert.That(data).IsNotEmpty();

        await stream.DisposeAsync();
    }
#endif

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
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), file)
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        LazReader reader = new(stream);
        await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(vlrCount);

        var count = 10;
        while (--count > 0)
        {
            await TestPoint(reader);
        }

        await reader.DisposeAsync();

        async Task TestPoint(LasReader pointReader)
        {
            var (point, data) = pointReader.ReadPointDataRecord();
            var extraBytes = data.ToArray();
            await Assert.That(point).IsNotNull().And.IsTypeOf(expectedType);
#if LAS1_4_OR_GREATER
            _ = hasExtraBytes ? await Assert.That(extraBytes).IsNotEmpty() : await Assert.That(extraBytes).IsEmpty();
#endif
        }

        await stream.DisposeAsync();
    }
#endif

    [Test]
    [Arguments("fusa.laz")]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz")]
#endif
    public async Task Create(string resource)
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), resource)
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        var reader = LazReader.Create(stream);
        await Assert.That(reader)
            .IsNotNull()
            .And.IsTypeOf<LazReader>()
            .And.Member(static x => x.IsCompressed, x => x.IsTrue());

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytes()
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa_height.laz")
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        var min = double.MaxValue;
        var max = double.MinValue;

        LazReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(3);
        var extraBytesTag = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        while (true)
        {
            var record = reader.ReadPointDataRecord();
            if (record.PointDataRecord is not { } point)
            {
                break;
            }

            var extraBytes = record.ExtraBytes.ToArray();

            await Assert.That(point).IsTypeOf<GpsPointDataRecord>();
            await Assert.That(extraBytes).IsNotEmpty();

            var value = await Assert.That(extraBytesTag[0].GetValue(extraBytes.AsSpan())).ValueIsTypeOf<double>();
            Interlocked.MinExchange(ref min, value);
            Interlocked.MaxExchange(ref max, value);
        }

        await reader.DisposeAsync();

        await stream.DisposeAsync();

        await Assert.That(min).IsEqualTo(-1.76).Within(0.001);
        await Assert.That(max).IsEqualTo(19.72).Within(0.001);
    }
#endif

    [Test]
    [Arguments("fusa.laz", 1, 1)]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz", 1, 4)]
#endif
    public async Task ReadByPointIndex(string resource, int major, int minor)
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), resource)
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        LazReader reader = new(stream);
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

        await reader.DisposeAsync();
        await stream.DisposeAsync();
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
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), file)
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        LazReader reader = new(stream);
        await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(vlrCount);

        await TestPointAsync(reader, expectedType, hasExtraBytes);
        await TestPointAsync(reader, expectedType, hasExtraBytes);

        await reader.DisposeAsync();
        await stream.DisposeAsync();

        static async Task TestPointAsync(LasReader pointReader, Type expectedType, bool hasExtraBytes)
        {
            var point = await pointReader.ReadPointDataRecordAsync();
            await Assert.That(point.PointDataRecord)
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
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa_height.laz")
                     ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        var min = double.MaxValue;
        var max = double.MinValue;

        LazReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        await Assert.That(reader.VariableLengthRecords).Count().IsEqualTo(3);
        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        while (await reader.ReadPointDataRecordAsync() is { PointDataRecord: not null } point)
        {
            await Assert.That(point.PointDataRecord).IsTypeOf<GpsPointDataRecord>();
            await Assert.That(point.ExtraBytes.IsEmpty).IsFalse();
            var value = await Assert.That(await extraBytes.GetValueAsync(0, point.ExtraBytes)).ValueIsTypeOf<double>();
            Interlocked.MinExchange(ref min, value);
            Interlocked.MaxExchange(ref max, value);
        }

        await reader.DisposeAsync();
        await stream.DisposeAsync();

        await Assert.That(min).IsEqualTo(-1.76).Within(0.001);
        await Assert.That(max).IsEqualTo(19.72).Within(0.001);
    }
#endif

    [Test]
    [Arguments("fusa.laz", 1, 1)]
#if LAS1_4_OR_GREATER
    [Arguments("fusa_height.laz", 1, 4)]
#endif
    public async Task ReadByPointIndexAsync(string resource, int major, int minor)
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), resource)
                           ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");

        LazReader reader = new(stream);
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

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [Test]
    public async Task ReadToSpan()
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa.laz")
                               ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LazReader reader = new(stream);

        var span = new byte[1024];

        var pointsRead = await Assert.That(reader.ReadPointDataRecordData(span)).IsNotZero();

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;

        for (var i = 0; i < pointsRead; i++)
        {
            (pointDataRecord, var extraBytes) = reader.Read(span.AsSpan(i * reader.PointDataLength, reader.PointDataLength));
            data = extraBytes.ToArray();
        }

        await Assert.That(pointDataRecord)
            .IsTypeOf<GpsPointDataRecord>().And
            .Member(p => p.X, x => x.IsNotDefault());
        await Assert.That(data).IsEmpty();

        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task ReadToMemory()
    {
        var stream = typeof(LazReaderTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), "fusa.laz")
                               ?? throw new System.Diagnostics.UnreachableException("Failed to get stream");
        LazReader reader = new(stream);

        var span = new byte[1024];

        var pointsRead = await reader.ReadPointDataRecordDataAsync(span);
        await Assert.That(pointsRead).IsNotZero();

        IBasePointDataRecord pointDataRecord = default;
        byte[] data = default;

        for (var i = 0; i < pointsRead; i++)
        {
            (pointDataRecord, var extraBytes) = await reader.ReadAsync(span.AsMemory(i * reader.PointDataLength, reader.PointDataLength));
            data = extraBytes.ToArray();
        }

        await reader.DisposeAsync();
        await stream.DisposeAsync();

        await Assert.That(pointDataRecord)
            .IsTypeOf<GpsPointDataRecord>().And
            .Member(p => p.X, x => x.IsNotDefault());
        await Assert.That(data).IsEmpty();
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
            .And.Member(static headerBlock => headerBlock.SystemIdentifier, static systemIdentifier => systemIdentifier.Contains("LAStools").And.Contains("rapidlasso"))
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