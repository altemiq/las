## About

<!-- A description of the package and where one can find more documentation -->

Provides support for coordinate systems for the OGC variable length records in Altemiq.IO.Las

## Key Features

<!-- The key features of this package -->

* Support for proj.db reading of coordinate reference systems

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

Getting the WKT:
```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Geodesy;

// this will search for proj.db in standard places
using var context = new ProjContext();
context.Open();

var wkt = context.GetWkt(28355);

// write this to the file
var vlr = new new OgcCoordinateSystemWkt(wkt);
```

Converting GeoTIFF to WKT
```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Geodesy;

// geo-tiff vlrs
GeoKeyDirectoryTag geoKeyDirectoryTag =
[
    new() { Count = 1, KeyId = GeoKey.GTModelTypeGeoKey, ValueOffset = 1 },
    new() { Count = 1, KeyId = GeoKey.ProjectedCRSGeoKey, ValueOffset = 28355 },
    new() { Count = 1, KeyId = GeoKey.ProjLinearUnitsGeoKey, ValueOffset = 9001 },
    new() { Count = 1, KeyId = GeoKey.VerticalUnitsGeoKey, ValueOffset = 9001 },
];

IEnumerable<VariableLengthRecords> vlrs = [geoKeyDirectoryTag];

// convert this to WKT
var vlrsWithOgcWkt = vlrs.ToWkt();
```

Writing:
```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Geodesy;

HeaderBlockBuilder builder = new()
{
    SystemIdentifier = "Our System",
    GeneratingSoftware = "My.Software.exe",
    Version = new(1, 4),
    FileCreation = new DateTime(2010, 1, 1).AddDays(40),
    PointDataFormatId = 6,
};

using LasWriter writer = new("example.las");
writer.Write(builder.HeaderBlock, 28355);
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.Geodesy.ProjContext`

## Additional Documentation

* [OGC](https://www.ogc.org/)
* [PROJ](https://proj.org/)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Geodesy is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).