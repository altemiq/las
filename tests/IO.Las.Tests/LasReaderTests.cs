namespace Altemiq.IO.Las;

public class LasReaderTests
{
    [Test]
    public async Task ReadLas()
    {
        await using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                              ?? throw new InvalidOperationException("Failed to get stream");
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords.Count).IsEqualTo(1);

        var point = reader.ReadPointDataRecord();
        var pointDataRecord = point.PointDataRecord;
        var data = point.ExtraBytes.ToArray();
        
        await Assert.That(pointDataRecord).IsTypeOf<GpsPointDataRecord>().Satisfies(p => p.X, x => x.IsNotDefault());
        await Assert.That(data).IsEmpty();
    }

    [Test]
    public async Task ReadLasWithFileSignature()
    {
        await using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                              ?? throw new InvalidOperationException("Failed to get stream");
        byte[] bytes = new byte[4];
        
        await Assert.That(await stream.ReadAsync(bytes)).IsEqualTo(bytes.Length);
        
        using LasReader reader = new(stream, System.Text.Encoding.UTF8.GetString(bytes));
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasCount().EqualToOne();

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
        await using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa_height.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).HasCount().EqualTo(2);

        double min = double.MaxValue;
        double max = double.MinValue;
        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();
        
        while (reader.ReadPointDataRecord() is { } point)
        {
            if (point.PointDataRecord is null)
            {
                break;
            }
            
            if (extraBytes[0].GetData(point.ExtraBytes) is double value)
            {
                if (value < min)
                {
                    min = value;
                }

                if (value > max)
                {
                    max = value;
                }
            }
        }

        _ = await Assert.That(min).IsBetween(-1.76 - 0.001, -1.76 + 0.001);
        _ = await Assert.That(max).IsBetween(19.72 - 0.001, 19.72 + 0.001);
    }
#endif
    
    [Test]
    public async Task ReadByPointIndex()
    {
        await using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new(1, 1));

        var quantizer = new PointDataRecordQuantizer(reader.Header);
        var fileCreation = reader.Header.FileCreation.GetValueOrDefault();
        await Assert.That(reader.ReadPointDataRecord(10).PointDataRecord)
            .IsNotNull()
            .Satisfies(p => p.X, x => x.IsEqualTo(27799961))
            .Satisfies(p => p.Y, y => y.IsEqualTo(612234368))
            .Satisfies(p => p.Z, z => z.IsEqualTo(6222))
            .IsAssignableTo<IGpsPointDataRecord>()
            .Satisfies(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));

        await Assert.That(reader.ReadPointDataRecord(277500).PointDataRecord)
            .IsNotNull()
            .Satisfies(p => p.X, x => x.IsEqualTo(27775097))
            .Satisfies(p => p.Y, y => y.IsEqualTo(612225071))
            .Satisfies(p => p.Z, z => z.IsEqualTo(4228))
            .IsAssignableTo<IGpsPointDataRecord>()
            .Satisfies(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));
    }
    
    [Test]
    public async Task ReadLasAsync()
    {
        await using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new Version(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasCount().EqualToOne();

        _ = await Assert.That(await reader.ReadPointDataRecordAsync())
            .Satisfies(p => p.PointDataRecord, p => p.IsNotNull().And.Satisfies(pt => pt.X, x => x.IsNotDefault()))
            .Satisfies(p => p.ExtraBytes, extraBytes => extraBytes.Satisfies(e => e.IsEmpty, empty => empty.IsTrue()));
    }

    [Test]
    public async Task ReadLasWithFileSignatureAsync()
    {
        await using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
        byte[] bytes = new byte[4];
        await Assert.That(stream.ReadAsync(bytes, 0, bytes.Length)).IsEqualTo(bytes.Length);
        using LasReader reader = new(stream, System.Text.Encoding.UTF8.GetString(bytes));
        await CheckHeader(reader.Header, new(1, 1));
        _ = await Assert.That(reader.VariableLengthRecords).HasCount().EqualToOne();

        var point = await reader.ReadPointDataRecordAsync();
        
        _ = await Assert.That(point.PointDataRecord).IsAssignableTo<IGpsPointDataRecord>();
        _ = await Assert.That(point.ExtraBytes.IsEmpty).IsTrue();
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task ReadWithExtraBytesAsync()
    {
        await using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa_height.las")
                                    ?? throw new InvalidOperationException("Failed to get stream");
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new Version(1, 4));
        _ = await Assert.That(reader.VariableLengthRecords).HasCount().EqualTo(2);

        var extraBytes = reader.VariableLengthRecords.OfType<ExtraBytes>().Single();

        double min = double.MaxValue;
        double max = double.MinValue;

        while (await reader.ReadPointDataRecordAsync() is { } point)
        {
            _ = await Assert.That(point.PointDataRecord).IsAssignableTo<IGpsPointDataRecord>();
            
            double value = await Assert.That(await extraBytes[0].GetDataAsync(point.ExtraBytes)).IsTypeOf<double>();
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
        using Stream stream = typeof(LasReaderTests).Assembly.GetManifestResourceStream(typeof(LasReaderTests), "fusa.las")
                              ?? throw new InvalidOperationException("Failed to get stream");
        using LasReader reader = new(stream);
        await CheckHeader(reader.Header, new Version(1, 1));

        var quantizer = new PointDataRecordQuantizer(reader.Header);
        var fileCreation = reader.Header.FileCreation.GetValueOrDefault();
        var point = await reader.ReadPointDataRecordAsync(10);
        await Assert.That(point.PointDataRecord)
            .IsNotNull()
            .Satisfies(p => p.X, x => x.IsEqualTo(27799961))
            .Satisfies(p => p.Y, y => y.IsEqualTo(612234368))
            .Satisfies(p => p.Z, z => z.IsEqualTo(6222))
            .IsTypeOf<GpsPointDataRecord>()
            .Satisfies(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));

        point = await reader.ReadPointDataRecordAsync(277500);
        await Assert.That(point.PointDataRecord)
            .Satisfies(p => p.X, x => x.IsEqualTo(27775097))
            .Satisfies(p => p.Y, y => y.IsEqualTo(612225071))
            .Satisfies(p => p.Z, z => z.IsEqualTo(4228))
            .IsAssignableTo<IGpsPointDataRecord>()
            .Satisfies(p => quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsBetween(fileCreation - TimeSpan.FromDays(7), fileCreation + TimeSpan.FromDays(7)));
    }
    
    private static async Task CheckHeader(HeaderBlock headerBlock, Version version)
    {
        _ = await Assert.That(headerBlock.FileSignature).IsEqualTo("LASF");
        _ = await Assert.That(headerBlock.FileSourceId).IsEqualTo((ushort)0);
#if LAS1_2_OR_GREATER
        _ = await Assert.That(headerBlock.GlobalEncoding).IsEqualTo(GlobalEncoding.None);
#endif
        _ = await Assert.That(headerBlock.ProjectId).IsEqualTo(Guid.Empty);
        _ = await Assert.That(headerBlock.Version).IsEqualTo(version);
        _ = await Assert.That(headerBlock.SystemIdentifier).IsEqualTo("LAStools (c) by rapidlasso GmbH");
        _ = await Assert.That(headerBlock.FileCreation).Satisfies(static x => x!.Value.Date, static x => x.IsEqualTo(new(2010, 2, 9)));
#if LAS1_4_OR_GREATER
        _ = await Assert.That(headerBlock.NumberOfPointRecords).IsEqualTo(277573UL);
        _ = await Assert.That(headerBlock.LegacyNumberOfPointsByReturn).IsEquivalentTo([263413U, 13879U, 281U, 0U, 0U]);
        _ = version < new Version(1, 4)
            ? await Assert.That(headerBlock.NumberOfPointsByReturn).IsEquivalentTo([263413UL, 13879UL, 281UL, 0UL, 0UL])
            : await Assert.That(headerBlock.NumberOfPointsByReturn).IsEquivalentTo([263413UL, 13879UL, 281UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL, 0UL]);
#else
        _ = await Assert.That(headerBlock.NumberOfPointRecords).IsEqualTo(277573U);
        _ = await Assert.That(headerBlock.NumberOfPointsByReturn).IsEquivalentTo([263413U, 13879U, 281U, 0U, 0U]);
#endif
        _ = await Assert.That(headerBlock.ScaleFactor).IsEqualTo(new(0.01, 0.01, 0.01));
        _ = await Assert.That(headerBlock.Offset).IsEqualTo(new(0.0, 0.0, 0.0));
        _ = await Assert.That(headerBlock.Min).IsEqualTo(new(277750.0, 6122250.0, 42.21));
        _ = await Assert.That(headerBlock.Max).IsEqualTo(new(277999.99, 6122499.99, 64.35));
    }
}