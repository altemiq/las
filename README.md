# LAS .NET Library

[![Build Status](https://github.com/altemiq/las/actions/workflows/build.yml/badge.svg)](https://github.com/altemiq/las/actions/workflows/build.yml)

The LAS .NET library is a comprehensive, high-performance library for reading and writing ASPRS LAS LiDAR point cloud files. It supports LAS versions 1.1-1.5 and provides a modular package structure with extensions for various formats and cloud platforms.

## Features

- **High Performance**: Low-allocation types for efficient point cloud processing
- **Multiple LAS Versions**: Full support for LAS 1.1 through 1.5
- **Compression**: Support for LAZ compression
- **Cloud Integration**: Read/write from Azure Blob Storage, Amazon S3, and HTTP(S) endpoints
- **Arrow Integration**: Read/write to Apache Arrow format
- **DataFrame Support**: Integration with Polars DataFrames
- **Geodesy Support**: OGC WKT coordinate system support via PROJ
- **Indexing**: Spatial indexing for fast point access
- **Tiling Support**: Read tiling VLRs from LAS files
- **AOT Compatible**: Ready for Native AOT compilation

## Packages

| Package | Version | Description |
|---------|---------|-------------|
| **Altemiq.IO.Las** | [![Altemiq.IO.Las](https://img.shields.io/nuget/v/Altemiq.IO.Las)](https://www.nuget.org/packages/Altemiq.IO.Las) | Core library for reading/writing LAS files |
| **Altemiq.IO.Las.Compression** | [![Altemiq.IO.Las.Compression](https://img.shields.io/nuget/v/Altemiq.IO.Las.Compression)](https://www.nuget.org/packages/Altemiq.IO.Las.Compression) | LAZ compression support |
| **Altemiq.IO.Las.Cloud** | [![Altemiq.IO.Las.Cloud](https://img.shields.io/nuget/v/Altemiq.IO.Las.Cloud)](https://www.nuget.org/packages/Altemiq.IO.Las.Cloud) | Cloud-Optimized Point Clouds (COPC) |
| **Altemiq.IO.Las.Azure** | [![Altemiq.IO.Las.Azure](https://img.shields.io/nuget/v/Altemiq.IO.Las.Azure)](https://www.nuget.org/packages/Altemiq.IO.Las.Azure) | Azure Blob Storage support |
| **Altemiq.IO.Las.S3** | [![Altemiq.IO.Las.S3](https://img.shields.io/nuget/v/Altemiq.IO.Las.S3)](https://www.nuget.org/packages/Altemiq.IO.Las.S3) | Amazon S3 support |
| **Altemiq.IO.Las.Http** | [![Altemiq.IO.Las.Http](https://img.shields.io/nuget/v/Altemiq.IO.Las.Http)](https://www.nuget.org/packages/Altemiq.IO.Las.Http) | HTTP(S) support |
| **Altemiq.IO.Las.Indexing** | [![Altemiq.IO.Las.Indexing](https://img.shields.io/nuget/v/Altemiq.IO.Las.Indexing)](https://www.nuget.org/packages/Altemiq.IO.Las.Indexing) | Spatial indexing |
| **Altemiq.IO.Las.Tiling** | [![Altemiq.IO.Las.Tiling](https://img.shields.io/nuget/v/Altemiq.IO.Las.Tiling)](https://www.nuget.org/packages/Altemiq.IO.Las.Tiling) | Tiling VLR support |
| **Altemiq.IO.Las.Arrow** | [![Altemiq.IO.Las.Arrow](https://img.shields.io/nuget/v/Altemiq.IO.Las.Arrow)](https://www.nuget.org/packages/Altemiq.IO.Las.Arrow) | Apache Arrow integration |
| **Altemiq.IO.Las.Compression.Arrow** | [![Altemiq.IO.Las.Compression.Arrow](https://img.shields.io/nuget/v/Altemiq.IO.Las.Compression.Arrow)](https://www.nuget.org/packages/vIO.Las.Compression.Arrow) | Compressed Arrow integration |
| **Altemiq.IO.Las.DataFrame** | [![Altemiq.IO.Las.DataFrame](https://img.shields.io/nuget/v/Altemiq.IO.Las.DataFrame)](https://www.nuget.org/packages/Altemiq.IO.Las.DataFrame) | Polars DataFrame integration |
| **Altemiq.IO.Las.Compression.DataFrame** | [![Altemiq.IO.Las.Compression.DataFrame](https://img.shields.io/nuget/v/Altemiq.IO.Las.Compression.DataFrame)](https://www.nuget.org/packages/Altemiq.IO.Las.Compression.DataFrame) | Compressed DataFrame integration |
| **Altemiq.IO.Las.Geodesy** | [![Altemiq.IO.Las.Geodesy](https://img.shields.io/nuget/v/Altemiq.IO.Las.Geodesy)](https://www.nuget.org/packages/Altemiq.IO.Las.Geodesy) | OGC WKT coordinate systems |

## Command-Line Tool

The `las` command-line tool is available as a Native AOT-compiled executable and container. It provides a comprehensive interface for working with LAS files.

### Installation

#### .NET Tool

```bash
dotnet tool install --global Las
```

#### Container

```bash
docker run ghcr.io/altemiq/io/las
```

### Usage

```bash
# Show help
las --help

# Show file info
las info input.las

# Create spatial index
las index input.las

# Verify COPC file
las copc verify input.copc.laz

# Convert to LAS
las to las --input input.las --output output.las

# Convert to LAZ
las to laz --input input.las --output output.laz

# Convert to COPC (Cloud Optimized Point Cloud)
las to copc --input input.las --output output.copc.laz

# Sort points
las to sorted --input input.las --output output.las

# Split file into parts
las to exploded --input input.las --output output/
```

## Basic Usage

### Reading LAS Files

```csharp
using Altemiq.IO.Las;

using LasReader reader = new("example.las");

while (await reader.ReadPointDataRecordAsync() is { PointDataRecord: not null } point)
{
    Console.WriteLine($"Point: {point.X}, {point.Y}, {point.Z}");
}
```

### Writing LAS Files

```csharp
using Altemiq.IO.Las;

HeaderBlockBuilder builder = new()
{
    SystemIdentifier = "My System",
    GeneratingSoftware = "My.Software.exe",
    Version = new(1, 1),
    PointDataFormatId = 1,
    LegacyNumberOfPointRecords = 1,
};

using LasWriter writer = new("example.las");
writer.Write(builder.HeaderBlock, geoKeyDirectoryTag, extraBytes);
```

### Reading Compressed LAZ Files

```csharp
using Altemiq.IO.Las;

using LazReader reader = new("example.laz");

while (await reader.ReadPointDataRecordAsync() is { PointDataRecord: not null } point)
{
    Console.WriteLine($"Point: {point.X}, {point.Y}, {point.Z}");
}
```

### Cloud Integration

```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.S3;

var url = new Uri("s3://bucket/path/file.las");
LasReader reader = new(S3Las.OpenRead(url));
```

### Arrow Integration

```csharp
using Altemiq.IO.Las;

using LasReader reader = new("example.las");
var batches = reader.ToArrowBatches();

ArrowLasReader arrowReader = new(batches);
```

### DataFrame Integration

```csharp
using Altemiq.IO.Las;

using LasReader reader = new("example.las");
var data = Polars.CSharp.DataFrame.ReadLas(reader);
```

## Building

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Package
dotnet pack
```

## Requirements

- .NET 10.0 (latest)
- .NET 9.0
- .NET 8.0
- .NET 7.0
- .NET Standard 2.1
- .NET Standard 2.0

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [ASPRS](https://www.asprs.org/) for the LAS specification
- [LAStools](https://rapidlasso.com/lastools/) for LAZ compression library
- [Apache Arrow](https://arrow.apache.org/) for the columnar format
- [PROJ](https://proj.org/) for coordinate system transformations

## Resources

- [ASPRS LAS Specification](https://github.com/ASPRSorg/LAS)
- [LAZ Specification](https://downloads.rapidlasso.de/doc/LAZ_Specification_1.4_R1.pdf)
- [Cloud Optimized Point Cloud](https://copc.io/)
- [GitHub Repository](https://github.com/altemiq/las)
