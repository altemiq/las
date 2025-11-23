## About

<!-- A description of the package and where one can find more documentation -->

Provides support for indexing LAS/LAZ files

## Key Features

<!-- The key features of this package -->

* Spatially indexes files to allow quick access

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

Reading:
```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Indexing;

VariableLengthRecordProcessor.Instance.RegisterIndexing();

LasReader reader = new("example.las");

// get the index
var index = LasIndex.ReadFrom("example.lax");

var points = reader.ReadPointDataRecords(index, new BoundingBox(-1, -1, -1, 1, 1, 1));
```

Creating:
```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Indexing;

VariableLengthRecordProcessor.Instance.RegisterIndexing();

LasReader reader = new("example.las");

// get the index
var index = LasIndex.Create(reader);

// write the index to file, normally Path.ChangeExtension("example.las", ".lax")
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.Indexing.LasIndex`

## Additional Documentation

* [LAStools](https://github.com/LAStools/LAStools)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Indexing is released as open source under the [GNU Lesser General Public License v2.1 only license](https://licenses.nuget.org/LGPL-2.1-only). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).