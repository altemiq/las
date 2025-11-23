## About

<!-- A description of the package and where one can find more documentation -->

Provides support for Cloud-Optimized Point Clouds for Altemiq.IO.Las.Compression

## Key Features

<!-- The key features of this package -->

* Support for COPC variable length records

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Cloud;

VariableLengthRecordProcessor.Instance.RegisterCloudOptimized();

using LazReader reader = new("example.copc.laz");

// read the info
var copcInfo = reader.VariableLengthRecords.OfType<CopcInfo>().First();

// get the hierarchy
var copcHierarchy = reader.ExtendedVariableLengthRecords.OfType<CopcHierarchy>().First();
var entry = copcHierarchy.Root.First();

// read the points from that entry
var points = reader.ReadPointDataRecords(entry);
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.Cloud.CopcInfo`
* `Altemiq.IO.Las.Cloud.CopcHierarchy`

## Additional Documentation

* [Cloud Optimized Point Cloud Specification](https://copc.io/)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Cloud is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).