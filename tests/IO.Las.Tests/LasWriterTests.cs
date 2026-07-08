// -----------------------------------------------------------------------
// <copyright file="LasWriterTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

public class LasWriterTests
{
    [Test]
    public async Task HeaderWithoutExtraBytes()
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 1),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 1,
        };

        GeoKeyDirectoryTag geoKeyDirectoryTag =
        [
            new() { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new() { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new() { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new() { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 },
        ];

        MemoryStream memoryStream = new();
        LasWriter lasWriter = new(memoryStream, true);
        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag);
        await lasWriter.DisposeAsync();

        memoryStream.Position = 0;
        LasReader lasReader = new(memoryStream);
        var outputHeader = lasReader.Header;
        await lasReader.DisposeAsync();

        _ = await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        _ = await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        _ = await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);
    }

#if LAS1_4_OR_GREATER
    [Test]
    public async Task HeaderWithExtraBytes()
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 1),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 1,
            LegacyNumberOfPointRecords = 1,
        };

        GeoKeyDirectoryTag geoKeyDirectoryTag =
        [
            new() { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new() { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new() { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new() { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 },
        ];
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
        LasWriter lasWriter = new(memoryStream, true);
        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);

        GpsPointDataRecord point = new()
        {
            X = 0,
            Y = 0,
            Z = 0,
            ReturnNumber = 0,
            NumberOfReturns = 0,
            Classification = Classification.LowVegetation,
            ScanDirectionFlag = false,
            EdgeOfFlightLine = false,
            ScanAngleRank = 0,
            PointSourceId = 0,
            GpsTime = 0,
        };

        _ = extraBytes.Write(span, ExtraValue);
        lasWriter.Write(point, span);
        await lasWriter.DisposeAsync();

        memoryStream.Position = 0;
        LasReader lasReader = new(memoryStream);
        var outputHeader = lasReader.Header;
        var (outputPoint, bytes) = lasReader.ReadPointDataRecord();
        var extra = bytes.ToArray();
        await lasReader.DisposeAsync();

        _ = await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        _ = await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        _ = await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);

        _ = await Assert.That(outputPoint)
            .IsNotNull()
            .And.IsTypeOf<IPointDataRecord>()
            .And.Member(static p => p.Classification, static classification => classification.IsEqualTo(Classification.LowVegetation));

        _ = await Assert.That(extraBytes.GetValue(0, extra)).ValueIsTypeOf<double>().And.IsEqualTo(ExtraValue);
    }

    [Test]
    public async Task HeaderWithExtraBytesAsync()
    {
        HeaderBlockBuilder builder = new()
        {
            SystemIdentifier = "LAS tests",
            GeneratingSoftware = "Las.Tests.exe",
            Version = new(1, 1),
            FileCreation = new DateTime(2010, 1, 1).AddDays(40),
            PointDataFormatId = 1,
            LegacyNumberOfPointRecords = 1,
        };

        GeoKeyDirectoryTag geoKeyDirectoryTag =
        [
            new() { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
            new() { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 32754 },
            new() { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
            new() { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 },
        ];
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
        var span = new byte[sizeof(short)];
        LasWriter lasWriter = new(memoryStream, true);
        lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);

        GpsPointDataRecord point = new()
        {
            X = 0,
            Y = 0,
            Z = 0,
            ReturnNumber = 0,
            NumberOfReturns = 0,
            Classification = Classification.LowVegetation,
            ScanDirectionFlag = false,
            EdgeOfFlightLine = false,
            ScanAngleRank = 0,
            PointSourceId = 0,
            GpsTime = 0,
        };

        _ = extraBytes.Write(span, ExtraValue);
        await lasWriter.WriteAsync(point, span);

        await lasWriter.DisposeAsync();

        memoryStream.Position = 0;
        LasReader lasReader = new(memoryStream);
        var outputHeader = lasReader.Header;
        var outputPoint = await lasReader.ReadPointDataRecordAsync();
        await lasReader.DisposeAsync();

        _ = await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        _ = await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        _ = await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);

        _ = await Assert.That(outputPoint.PointDataRecord)
            .IsNotNull()
            .And.IsTypeOf<IPointDataRecord>()
            .And.Member(static p => p.Classification, static classification => classification.IsEqualTo(Classification.LowVegetation));

        _ = await Assert.That(await extraBytes.GetValueAsync(0, outputPoint.ExtraBytes)).ValueIsTypeOf<double>().And.IsEqualTo(ExtraValue);
    }
#endif

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

        LasWriter lasWriter = new(stream, true);
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
        LasReader lasReader = new(stream);
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
            .And.Member(static p => p.Classification, static classification => classification.IsEqualTo(Classification.LowVegetation));

#if LAS1_4_OR_GREATER
        await Assert.That(extraBytes.GetValue(0, bytes)).ValueIsTypeOf<double>().And.IsEqualTo(ExtraValue);
#endif
    }

#if LAS1_4_OR_GREATER
    [Test]
    [MatrixDataSource]
    public async Task ExtendedVariableLengthRecords([Matrix(true, false)] bool exploded)
    {
        var record = new UnknownExtendedVariableLengthRecord(
            new()
            {
                UserId = "mine",
                RecordId = 12345,
                Description = "test EVLR",
                RecordLengthAfterHeader = 0,
            },
            [1, 2, 3, 4]);
        Stream memoryStream = exploded
            ? new LasMultipleMemoryStream()
            : new MemoryStream();
        LasWriter writer = new(memoryStream, leaveOpen: true);
        HeaderBlockBuilder headerBuilder = new();
        writer.Write(headerBuilder.HeaderBlock);
        writer.Write(record);
        await writer.DisposeAsync();

        memoryStream.Position = 0;
        LasReader reader = new(memoryStream);
        _ = await Assert.That(reader.ExtendedVariableLengthRecords).HasSingleItem()
            .And.Member(static x => x.Single(), x => x.IsEqualTo(record));

        await reader.DisposeAsync();
        await memoryStream.DisposeAsync();
    }
#endif
}