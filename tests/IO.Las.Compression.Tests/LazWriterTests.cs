// -----------------------------------------------------------------------
// <copyright file="LazWriterTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

using System.Diagnostics.CodeAnalysis;

public class LazWriterTests
{
#if LAS1_4_OR_GREATER
    [Test]
    public async Task CompressedHeaderWithExtraBytes()
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 1),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 1,
            LegacyNumberOfPointRecords = 2,
        };

        builder.SetCompressed();

        GeoKeyDirectoryTag geoKeyDirectoryTag = new(
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 });
        ExtraBytes extraBytes =
        [
            new()
            {
                DataType = ExtraBytesDataType.Short,
                Options = ExtraBytesOptions.Scale | ExtraBytesOptions.Offset,
                Scale = 0.01,
                Offset = 250,
                Name = "height above ground",
                Description = "vertical point to TIN distance",
            },
        ];

        MemoryStream memoryStream = new();
        const double ExtraValue = 123.34;
        Span<byte> span = stackalloc byte[sizeof(short)];
        LazWriter lasWriter = new(memoryStream, true);
        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);
        _ = extraBytes.Write(span, 124.56);
        lasWriter.Write(
            new GpsPointDataRecord
            {
                X = default,
                Y = default,
                Z = default,
                Classification = Classification.HighVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            },
            span);

        _ = extraBytes.Write(span, ExtraValue);
        lasWriter.Write(
            new GpsPointDataRecord
            {
                X = 1,
                Y = 1,
                Z = 1,
                Classification = Classification.LowVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            },
            span);

        await lasWriter.DisposeAsync();

        memoryStream.Position = 0;
        LazReader lasReader = new(memoryStream);
        var outputHeader = lasReader.Header;
        _ = lasReader.ReadPointDataRecord();
        var (outputPoint, data) = lasReader.ReadPointDataRecord();
        var bytes = data.ToArray();

        await lasReader.DisposeAsync();

        await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);

        await Assert.That(outputPoint)
            .IsNotNull()
            .And.IsTypeOf<IPointDataRecord>()
            .And.Member(p => p.Classification, classification => classification.IsEqualTo(Classification.LowVegetation));

        await Assert.That(extraBytes.GetValue(0, bytes)).ValueIsTypeOf<double>().And.IsEqualTo(ExtraValue);
    }


    [Test]
    public async Task CompressedHeaderWithExtraBytesAsync()
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 1),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 1,
            LegacyNumberOfPointRecords = 2,
        };

        builder.SetCompressed();

        GeoKeyDirectoryTag geoKeyDirectoryTag = new(
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 });
        ExtraBytes extraBytes =
        [
            new()
            {
                DataType = ExtraBytesDataType.Short,
                Options = ExtraBytesOptions.Scale | ExtraBytesOptions.Offset,
                Scale = 0.01,
                Offset = 250,
                Name = "height above ground",
                Description = "vertical point to TIN distance",
            },
        ];

        MemoryStream memoryStream = new();
        const double ExtraValue = 123.34;
        Memory<byte> memory = new byte[sizeof(short)];
        LazWriter lasWriter = new(memoryStream, true);

        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);
        _ = extraBytes.Write(memory.Span, 124.56);
        await lasWriter.WriteAsync(
            new GpsPointDataRecord
            {
                X = default,
                Y = default,
                Z = default,
                Classification = Classification.HighVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            },
            memory);

        _ = extraBytes.Write(memory.Span, ExtraValue);
        await lasWriter.WriteAsync(
            new GpsPointDataRecord
            {
                X = 1,
                Y = 1,
                Z = 1,
                Classification = Classification.LowVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            },
            memory);

        await lasWriter.DisposeAsync();

        memoryStream.Position = 0;
        LazReader lasReader = new(memoryStream);

        var outputHeader = lasReader.Header;
        _ = await lasReader.ReadPointDataRecordAsync();
        var (outputPoint, bytes) = await lasReader.ReadPointDataRecordAsync();

        await lasReader.DisposeAsync();

        await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);

        await Assert.That(outputPoint)
            .IsNotNull()
            .And.IsTypeOf<IPointDataRecord>()
            .And.Member(p => p.Classification, classification => classification.IsEqualTo(Classification.LowVegetation));

        await Assert.That(extraBytes.GetValue(0, bytes.Span)).ValueIsTypeOf<double>().And.IsEqualTo(ExtraValue);
    }
#endif

    [Test]
    public async Task WriteHeaderWithMultipleCompressedTags()
    {
        List<VariableLengthRecord> records =
        [
#if LAS1_5_OR_GREATER
            new CompressedTag(new(ExtendedGpsPointDataRecord.Id, 0, Compressor.LayeredChunked, 3)),
            new CompressedTag(new(ExtendedGpsPointDataRecord.Id, 0, Compressor.LayeredChunked, 3)),
#elif LAS1_4_OR_GREATER
            new CompressedTag(new LasZip(PointDataRecord.Id, 0, Compressor.LayeredChunked)),
            new CompressedTag(new LasZip(PointDataRecord.Id, 0, Compressor.LayeredChunked)),
#else
            new CompressedTag(new LasZip(PointDataRecord.Id, Compressor.PointWise)),
            new CompressedTag(new LasZip(PointDataRecord.Id, Compressor.PointWise)),
#endif
        ];

        LazWriter writer = new(new MemoryStream());
        await Assert.That([SuppressMessage("ReSharper", "AccessToDisposedClosure")] () => writer.Write(HeaderBlock.Default, records)).ThrowsNothing();
        await writer.DisposeAsync();
        await Assert.That(records).HasSingleItem();
    }

    [Test]
    public async Task ExplodedStream()
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 1),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 1,
#if LAS1_4_OR_GREATER
            LegacyNumberOfPointRecords = 2,
#else
            NumberOfPointRecords = 2,
#endif
        };

        GeoKeyDirectoryTag geoKeyDirectoryTag = new(
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 });
#if LAS1_4_OR_GREATER
        ExtraBytes extraBytes =
        [
            new()
            {
                DataType = ExtraBytesDataType.Short,
                Options = ExtraBytesOptions.Scale | ExtraBytesOptions.Offset,
                Scale = 0.01,
                Offset = 250,
                Name = "height above ground",
                Description = "vertical point to TIN distance",
            },
        ];
#endif

        LasMultipleMemoryStream stream = new();

#if LAS1_4_OR_GREATER
        const double ExtraValue = 123.34;
        Span<byte> span = stackalloc byte[sizeof(ushort)];
#endif

        LazWriter lasWriter = new(stream, true);
#if LAS1_4_OR_GREATER
        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);

        extraBytes.Write(span, 124.56);
        lasWriter.Write(
            new GpsPointDataRecord
            {
                X = default,
                Y = default,
                Z = default,
                Classification = Classification.HighVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            },
            span);

        extraBytes.Write(span, ExtraValue);
        lasWriter.Write(
            new GpsPointDataRecord
            {
                X = 1,
                Y = 1,
                Z = 1,
                Classification = Classification.LowVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            },
            span);
#else
            lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag);

            lasWriter.Write(new GpsPointDataRecord
            {
                X = default,
                Y = default,
                Z = default,
                Classification = Classification.HighVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            });

            lasWriter.Write(new GpsPointDataRecord
            {
                X = 1,
                Y = 1,
                Z = 1,
                Classification = Classification.LowVegetation,
                EdgeOfFlightLine = default,
                GpsTime = default,
                NumberOfReturns = 1,
                ReturnNumber = 1,
                PointSourceId = default,
                ScanDirectionFlag = default,
                ScanAngleRank = default,
            });
#endif

        await lasWriter.DisposeAsync();

        stream.Reset();
        LazReader lasReader = new(stream);

        var outputHeader = lasReader.Header;
        _ = lasReader.ReadPointDataRecord();
        var (outputPoint, data) = lasReader.ReadPointDataRecord();
        var bytes = data.ToArray();

        await lasReader.DisposeAsync();

        await stream.DisposeAsync();

        await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);

        await Assert.That(outputPoint)
            .IsNotNull()
            .And.IsTypeOf<IPointDataRecord>()
            .And.Member(p => p.Classification, classification => classification.IsEqualTo(Classification.LowVegetation));

#if LAS1_4_OR_GREATER
        await Assert.That(extraBytes.GetValue(0, bytes)).ValueIsTypeOf<double>().And.IsEqualTo(ExtraValue);
#endif
    }

#if LAS1_4_OR_GREATER
    [Test]
    [Arguments(3000, 4, typeof(FixedLayeredChunkedReader))]
    [Arguments(CompressedTag.VariableChunkSize, 6, typeof(VariableLayeredChunkedReader))]
    public async Task LayeredChunked(int chunkSize, int numberChunks, Type expectedType)
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 4),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 6,
            NumberOfPointRecords = 10000,
        };

        builder.SetCompressed();

        GeoKeyDirectoryTag geoKeyDirectoryTag = new(
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 });

        CompressedTag compressedTag = new(builder.HeaderBlock, Enumerable.Empty<VariableLengthRecord>(), Compressor.LayeredChunked)
        {
            ChunkSize = chunkSize,
        };

        MemoryStream memoryStream = new();
        LazWriter lasWriter = new(memoryStream, true);
        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag, compressedTag);

        // write different length chunks
        WriteChunk(lasWriter, 1234);
        WriteChunk(lasWriter, 4321);
        WriteChunk(lasWriter, 505);
        WriteChunk(lasWriter, 1050);
        WriteChunk(lasWriter, 2500);
        WriteChunk(lasWriter, 390);

        await lasWriter.DisposeAsync();

        memoryStream.Position = 0;
        LazReader lasReader = new(memoryStream);
        var outputHeader = lasReader.Header;

        await Assert.That(lasReader.VariableLengthRecords).Contains(vlr => vlr is CompressedTag)
            .And.Member(
                records => records.OfType<CompressedTag>().Single(),
                record => record.IsNotNull().And.Member(
                        ct => ct.Compressor,
                        compressor => compressor.IsEqualTo(Compressor.LayeredChunked))
                    .And.IsAssignableTo<CompressedTag>());

        ulong count = default;
        while (lasReader.ReadPointDataRecord() is { PointDataRecord: not null })
        {
            count++;
        }

        await CheckPointReader(lasReader, expectedType, numberChunks);

        await lasReader.DisposeAsync();

        await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)6);

        await Assert.That(count).IsEqualTo(outputHeader.NumberOfPointRecords);

        static void WriteChunk(LazWriter writer, int size)
        {
            writer.Write(CreateRecords(size).Select(r => new LasPointMemory(r, default)), size);
        }

        static IEnumerable<IBasePointDataRecord> CreateRecords(int size)
        {
            Random random = new(size);

            for (var i = 0; i < size; i++)
            {
                yield return new ExtendedGpsPointDataRecord
                {
                    X = random.Next(),
                    Y = random.Next(),
                    Z = random.Next(),
                    Classification = (ExtendedClassification)random.Next(0, 22),
                    EdgeOfFlightLine = default,
                    GpsTime = default,
                    NumberOfReturns = 1,
                    ReturnNumber = 1,
                    PointSourceId = default,
                    ScanDirectionFlag = default,
                    ScanAngle = default,
                };
            }
        }

        static async Task CheckPointReader(LazReader reader, Type expectedType, int numberChunksValue)
        {
            var pointReader = await Assert.That(reader.GetType()
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Single(static fi => fi.FieldType == typeof(IPointReader))
                .GetValue(reader)).IsTypeOf(expectedType);

            await Assert.That((uint)typeof(ChunkedReader).GetField(nameof(numberChunksValue), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(pointReader)!).IsEqualTo((uint)numberChunksValue);
        }
    }

    [Test]
    [Arguments(3000, 4, typeof(FixedLayeredChunkedReader))]
    [Arguments(CompressedTag.VariableChunkSize, 6, typeof(VariableLayeredChunkedReader))]
    public async Task LayeredChunkedAsync(int chunkSize, int numberChunks, Type expectedType)
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 4),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 6,
            NumberOfPointRecords = 10000,
        };

        builder.SetCompressed();

        GeoKeyDirectoryTag geoKeyDirectoryTag = new(
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new GeoKeyEntry { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 });

        CompressedTag compressedTag = new(builder.HeaderBlock, Enumerable.Empty<VariableLengthRecord>(), Compressor.LayeredChunked)
        {
            ChunkSize = chunkSize,
        };

        MemoryStream memoryStream = new();
        LazWriter lasWriter = new(memoryStream, true);
        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag, compressedTag);

        // write different length chunks
        await WriteChunkAsync(lasWriter, 1234);
        await WriteChunkAsync(lasWriter, 4321);
        await WriteChunkAsync(lasWriter, 505);
        await WriteChunkAsync(lasWriter, 1050);
        await WriteChunkAsync(lasWriter, 2500);
        await WriteChunkAsync(lasWriter, 390);

        await lasWriter.DisposeAsync();

        memoryStream.Position = 0;
        LazReader lasReader = new(memoryStream);
        var outputHeader = lasReader.Header;

        await Assert.That(lasReader.VariableLengthRecords).Contains(vlr => vlr is CompressedTag)
            .And.Member(
                records => records.OfType<CompressedTag>().Single(),
                record => record.IsNotNull().And.Member(
                        tag => tag.Compressor,
                        compressor => compressor.IsEqualTo(Compressor.LayeredChunked))
                    .And.IsAssignableTo<CompressedTag>());

        int count = default;
        while (await lasReader.ReadPointDataRecordAsync() is { PointDataRecord: not null })
        {
            count++;
        }

        await CheckPointReader(lasReader, expectedType, numberChunks);

        await lasReader.DisposeAsync();

        await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)6);

        await Assert.That(count).IsEqualTo((int)outputHeader.NumberOfPointRecords);

        static ValueTask WriteChunkAsync(LazWriter writer, int size)
        {
            return writer.WriteAsync(CreateRecords(size).Select(r => new LasPointMemory(r, default)), size);
        }

        static IEnumerable<IBasePointDataRecord> CreateRecords(int size)
        {
            Random random = new(size);

            for (var i = 0; i < size; i++)
            {
                yield return new ExtendedGpsPointDataRecord
                {
                    X = random.Next(),
                    Y = random.Next(),
                    Z = random.Next(),
                    Classification = (ExtendedClassification)random.Next(0, 22),
                    EdgeOfFlightLine = default,
                    GpsTime = default,
                    NumberOfReturns = 1,
                    ReturnNumber = 1,
                    PointSourceId = default,
                    ScanDirectionFlag = default,
                    ScanAngle = default,
                };
            }
        }

        static async Task CheckPointReader(LazReader reader, Type expectedType, int numberChunksValue)
        {
            var pointReader = await Assert.That(reader.GetType()
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Single(static fi => fi.FieldType == typeof(IPointReader))
                .GetValue(reader)).IsTypeOf(expectedType);

            await Assert.That((uint)typeof(ChunkedReader).GetField(nameof(numberChunksValue), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(pointReader)!).IsEqualTo((uint)numberChunksValue);
        }
    }

    [Test]
    [MatrixDataSource]
    public async Task ExtendedVariableLengthRecords([Matrix(true, false)] bool exploded)
    {
        ExtendedVariableLengthRecord record = new UnknownExtendedVariableLengthRecord(
            new()
            {
                UserId = "mine",
                RecordId = 12345,
                Description = "test EVLR",
                RecordLengthAfterHeader = 0,
            },
            []);
        Stream stream = exploded
                ? new LasMultipleMemoryStream()
                : new MemoryStream();
        LazWriter writer = new(stream, leaveOpen: true);
        HeaderBlockBuilder headerBuilder = new();
        writer.Write(headerBuilder.HeaderBlock);
        writer.Write(record);
        await writer.DisposeAsync();

        stream.Position = 0;
        LazReader reader = new(stream);
        await Assert.That(reader.ExtendedVariableLengthRecords)
            .HasSingleItem()
            .ContainsOnly(e => ExtendedVariableLengthRecordComparer.Instance.Equals(e, record));
        await reader.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    [MatrixDataSource]
    public async Task SpecialExtendedVariableLengthRecords([Matrix(true, false)] bool exploded)
    {
        _ = VariableLengthRecordProcessor.Instance.TryRegisterCompression();
        ExtendedVariableLengthRecord record = new UnknownExtendedVariableLengthRecord(
            new()
            {
                UserId = "mine",
                RecordId = 12345,
                Description = "test EVLR",
                RecordLengthAfterHeader = 0,
            },
            []);
        Stream memoryStream = exploded
            ? new LasMultipleMemoryStream()
            : new MemoryStream();
        LazWriter writer = new(memoryStream, leaveOpen: true);
        HeaderBlockBuilder headerBuilder = new();
        headerBuilder.SetCompressed();
        writer.Write(headerBuilder.HeaderBlock);
        writer.Write(record, true);
        await writer.DisposeAsync();

        memoryStream.Position = 0;
        LazReader reader = new(memoryStream);
        await Assert.That(reader.ExtendedVariableLengthRecords).IsEmpty();

        await Assert.That(reader.VariableLengthRecords.OfType<CompressedTag>())
            .HasSingleItem()
            .And.Member(x => x.Single().NumOfSpecialEvlrs, x => x.IsEqualTo(1));

        await reader.DisposeAsync();
        await memoryStream.DisposeAsync();
    }

    [Test]
    public async Task NirPointWriting()
    {
        var quantizer = new PointDataRecordQuantizer();
        ExtendedGpsColorNearInfraredWaveformPointDataRecord first = new()
        {
            X = default,
            Y = default,
            Z = default,
            GpsTime = quantizer.GetGpsTime(DateTime.UtcNow),
            Color = default,
            ReturnNumber = 1,
            NumberOfReturns = 1,
            EdgeOfFlightLine = default,
            PointSourceId = default,
            ScanDirectionFlag = default,
            Classification = default,
            ScanAngle = default,
            NearInfrared = default,
            WavePacketDescriptorIndex = default,
            ByteOffsetToWaveformData = default,
            WaveformPacketSizeInBytes = default,
            ReturnPointWaveformLocation = default,
            ParametricDx = default,
            ParametricDy = default,
            ParametricDz = default,
        };
        ExtendedGpsColorNearInfraredWaveformPointDataRecord second = new()
        {
            X = 1,
            Y = 2,
            Z = 3,
            GpsTime = quantizer.GetGpsTime(DateTime.UtcNow),
            Color = Color.FromRgb(0, 0, ushort.MaxValue),
            ReturnNumber = 1,
            NumberOfReturns = 1,
            EdgeOfFlightLine = default,
            PointSourceId = default,
            ScanDirectionFlag = default,
            Classification = default,
            ScanAngle = default,
            NearInfrared = 14336,
            WavePacketDescriptorIndex = 1,
            ByteOffsetToWaveformData = 2,
            WaveformPacketSizeInBytes = 3,
            ReturnPointWaveformLocation = 4F,
            ParametricDx = 5F,
            ParametricDy = 6F,
            ParametricDz = 7F,
        };

        MemoryStream stream = new();
        LazWriter writer = new(stream, leaveOpen: true);
        var headerBuilder = HeaderBlockBuilder.FromPointType<ExtendedGpsColorNearInfraredWaveformPointDataRecord>();
        headerBuilder.NumberOfPointRecords = 2;
        headerBuilder.FileCreation = DateTime.UtcNow.Date;
        headerBuilder.GlobalEncoding = GlobalEncoding.StandardGpsTime;
        headerBuilder.SetCompressed();

        writer.Write(headerBuilder.HeaderBlock);

        writer.Write(first);
        writer.Write(second);
        await writer.DisposeAsync();

        stream.Position = 0;

        LazReader reader = new(stream);
        await Assert.That(reader.ReadPointDataRecord().PointDataRecord).IsEqualTo(first);
        await Assert.That(reader.ReadPointDataRecord().PointDataRecord).IsEqualTo(second);
        await reader.DisposeAsync();

        await stream.DisposeAsync();
    }

    [Test]
    public async Task ColorPointWriting()
    {
        var quantizer = new PointDataRecordQuantizer();
        ExtendedGpsColorPointDataRecord first = new()
        {
            X = default,
            Y = default,
            Z = default,
            GpsTime = quantizer.GetGpsTime(DateTime.UtcNow),
            ReturnNumber = 1,
            NumberOfReturns = 1,
            ScanDirectionFlag = default,
            Classification = default,
            ScanAngle = default,
            EdgeOfFlightLine = default,
            PointSourceId = default,
            Color = default,
        };
        ExtendedGpsColorPointDataRecord second = new()
        {
            X = 1,
            Y = 2,
            Z = 3,
            GpsTime = quantizer.GetGpsTime(DateTime.UtcNow),
            Color = Color.FromRgb(0, 0, ushort.MaxValue),
            ScannerChannel = 2,
            ReturnNumber = 1,
            NumberOfReturns = 2,
            Withheld = true,
            Intensity = 1,
            UserData = 1,
            PointSourceId = 1,
            ScanDirectionFlag = default,
            Classification = default,
            ScanAngle = default,
            EdgeOfFlightLine = default,
        };
        ExtendedGpsColorPointDataRecord third = new()
        {
            X = default,
            Y = default,
            Z = default,
            GpsTime = quantizer.GetGpsTime(DateTime.UtcNow.AddHours(1)),
            Color = Color.FromRgb(0, ushort.MaxValue, 0),
            ReturnNumber = 3,
            NumberOfReturns = 3,
            Overlap = true,
            Intensity = 2,
            UserData = 2,
            PointSourceId = 2,
            ScanAngle = 2,
            ScanDirectionFlag = true,
            EdgeOfFlightLine = default,
            Classification = default,
        };

        MemoryStream stream = new();
        LazWriter writer = new(stream, leaveOpen: true);
        var headerBuilder = HeaderBlockBuilder.FromPointType<ExtendedGpsColorPointDataRecord>();
        headerBuilder.NumberOfPointRecords = 3;
        headerBuilder.FileCreation = DateTime.UtcNow.Date;
        headerBuilder.GlobalEncoding = GlobalEncoding.StandardGpsTime;
        headerBuilder.SetCompressed();

        writer.Write(headerBuilder.HeaderBlock);

        writer.Write(first);
        writer.Write(second);
        writer.Write(third);
        await writer.DisposeAsync();

        stream.Position = 0;

        LazReader reader = new(stream);
        await Assert.That(reader.ReadPointDataRecord().PointDataRecord).IsEqualTo(first);
        await Assert.That(reader.ReadPointDataRecord().PointDataRecord).IsEqualTo(second);
        await Assert.That(reader.ReadPointDataRecord().PointDataRecord).IsEqualTo(third);
        await reader.DisposeAsync();

        await stream.DisposeAsync();
    }
#endif
}