## About

<!-- A description of the package and where one can find more documentation -->

Provides support for LAZ compression for Altemiq.IO.Las

## Key Features

<!-- The key features of this package -->

* Support for LAZ compression in reading/writing
* Layered reading/writing
* Chunked reading/writing

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

Reading:
```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Cloud;

VariableLengthRecordProcessor.Instance.RegisterCompression();

using LazReader reader = new("example.laz");

while (await reader.ReadPointDataRecordAsync() is { PointDataRecord: not null } point)
{
    if (point.PointDataRecord is IGpsPointDataRecord gps)
    {
        // do something with the GPS information
    }
    
    // get the extra bytes
    var value = await extraBytes.GetValueAsync(0, point.ExtraBytes);
    if (value is double doubleValue)
    {
        // do something with the extra bytes.
    }
}
```

Writing:
```csharp
using Altemiq.IO.Las;

HeaderBlockBuilder builder = new()
{
    SystemIdentifier = "Our System",
    GeneratingSoftware = "My.Software.exe",
    Version = new(1, 4),
    FileCreation = new DateTime(2010, 1, 1).AddDays(40),
    PointDataFormatId = 6,
    NumberOfPointRecords = 1,
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

Span<byte> span = stackalloc byte[sizeof(short)];
using LazWriter writer = new("example.las");
writer.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);

ExtendedGpsPointDataRecord point = new()
{
    X = 0,
    Y = 0,
    Z = 0,
    ReturnNumber = 0,
    NumberOfReturns = 0,
    Classification = ExtendedClassification.LowVegetation,
    EdgeOfFlightLine = default,
    GpsTime = default,
    NumberOfReturns = 1,
    ReturnNumber = 1,
    PointSourceId = default,
    ScanDirectionFlag = default,
    ScanAngle = default,
};

_ = extraBytes.Write(span, 123.34);
writer.Write(point, span);
```

Support for LAS/LAZ
```csharp
using Altemiq.IO.Las;

// this will read the header, and return either a 
// LasReader, or LazReader depending on whether the
// file is compressed or now
var reader = LazReader.Create("input.laz");
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.LazReader`
* `Altemiq.IO.Las.LazWriter`
* `Altemiq.IO.Las.CompressedTag`

## Additional Documentation

* [LAZ Specification](https://downloads.rapidlasso.de/doc/LAZ_Specification_1.4_R1.pdf)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Compressions is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).