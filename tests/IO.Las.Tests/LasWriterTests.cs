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
            new() { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 }
        ];

        MemoryStream memoryStream = new();
        using (LasWriter lasWriter = new(memoryStream, true))
        {
            lasWriter.Write(builder.HeaderBlock, geoKeyDirectoryTag);
        }

        memoryStream.Position = 0;
        HeaderBlock outputHeader;
        using (LasReader lasReader = new(memoryStream))
        {
            outputHeader = lasReader.Header;
        }

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
                Description = "vertical point to TIN distance"
            }
        ];

        MemoryStream memoryStream = new();
        const double ExtraValue = 123.34;
        Span<byte> span = stackalloc byte[sizeof(short)];
        using (LasWriter lasWriter = new(memoryStream, true))
        {
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
                GpsTime = 0
            };

            extraBytes.Write(span, ExtraValue);
            lasWriter.Write(point, span);
        }

        memoryStream.Position = 0;
        HeaderBlock outputHeader;
        IBasePointDataRecord outputPoint;
        byte[] extra;
        using (LasReader lasReader = new(memoryStream))
        {
            outputHeader = lasReader.Header;
            var point = lasReader.ReadPointDataRecord();
            outputPoint = point.PointDataRecord;
            extra = point.ExtraBytes.ToArray();
        }

        _ = await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        _ = await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        _ = await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);

        _ = await Assert.That(outputPoint)
            .IsNotNull()
            .IsAssignableTo<IPointDataRecord>()
            .Satisfies(p => p.Classification, classification => classification.IsEqualTo(Classification.LowVegetation));

        _ = await Assert.That(extraBytes.GetValue(0, extra)).IsEqualTo(ExtraValue);
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
                Description = "vertical point to TIN distance"
            }
        ];

        MemoryStream memoryStream = new();
        const double ExtraValue = 123.34;
        var span = new byte[sizeof(short)];
        using (LasWriter lasWriter = new(memoryStream, true))
        {
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
                GpsTime = 0
            };

            extraBytes.Write(span, ExtraValue);
            await lasWriter.WriteAsync(point, span);
        }

        memoryStream.Position = 0;
        HeaderBlock outputHeader;
        LasPointMemory outputPoint;
        using (LasReader lasReader = new(memoryStream))
        {
            outputHeader = lasReader.Header;
            outputPoint = await lasReader.ReadPointDataRecordAsync();
        }

        _ = await Assert.That(outputHeader.SystemIdentifier).IsEqualTo("LAS tests");
        _ = await Assert.That(outputHeader.GeneratingSoftware).IsEqualTo("Las.Tests.exe");
        _ = await Assert.That(outputHeader.PointDataFormatId).IsEqualTo((byte)1);

        _ = await Assert.That(outputPoint.PointDataRecord)
            .IsNotNull()
            .IsAssignableTo<IPointDataRecord>()
            .Satisfies(p => p.Classification, classification => classification.IsEqualTo(Classification.LowVegetation));

        _ = await Assert.That(await extraBytes.GetValueAsync(0, outputPoint.ExtraBytes)).IsEqualTo(ExtraValue);
    }
#endif
}