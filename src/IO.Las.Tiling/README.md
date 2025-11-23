## About

<!-- A description of the package and where one can find more documentation -->

Provides support for tiled LAS/LAZ files

## Key Features

<!-- The key features of this package -->

* Reads Tiling VLRs from LA(S/Z) files

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Tiling;

VariableLengthRecordProcessor.Instance.RegisterTiling();

LasReader reader = new("example.las");

var tiling = reader.VariableLengthRecords.OfType<Tiling>().First();
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.Tiling.Tiling`

## Additional Documentation

* [LAStools](https://github.com/LAStools/LAStools)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Tiling is released as open source under the [GNU Lesser General Public License v2.1 only license](https://licenses.nuget.org/LGPL-2.1-only). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).