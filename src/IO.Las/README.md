## About

<!-- A description of the package and where one can find more documentation -->

Provides high-performance and low-allocating types that read and write LAS points to streams.

## Key Features

<!-- The key features of this package -->

* Supports LAS versions 1.1-1.5
* Support for ExtraBytes
* WKT support for OGC variable length records

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

Reading:
```csharp
using Altemiq.IO.Las;

using LasReader reader = new("example.las");

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

Span<byte> span = stackalloc byte[sizeof(short)];
using LasWriter writer = new("example.las");
writer.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);

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

_ = extraBytes.Write(span, 123.34);
writer.Write(point, span);
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.PointDataRecord`
* `Altemiq.IO.Las.LasReader`
* `Altemiq.IO.Las.LasWriter`
* `Altemiq.IO.Las.HeaderBlockBuilder`
* `Altemiq.IO.Las.HeaderBlock`
* `Altemiq.IO.Las.VariableLengthRecord`
* `Altemiq.IO.Las.ExtendedVariableLengthRecord` (for LAS 1.4+)

## Additional Documentation

* [ASPRS LAS Specification](https://github.com/ASPRSorg/LAS)
* [LAS Working Group](https://community.asprs.org/wg-las/home)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).