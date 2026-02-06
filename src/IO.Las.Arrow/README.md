## About

<!-- A description of the package and where one can find more documentation -->

Provides support for reading and writing Point Clouds to and from the Apache Arrow format.

## Key Features

<!-- The key features of this package -->

* Writing a LAS file to arrow record batches
* Reader arrow record batches as a LAS file

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

```csharp
using Altemiq.IO.Las;

using LazReader reader = new("example.las");

// get the batches
var batches = reader.ToArrowBatches();

// read the batches as a LAS file
ArrowLasReader arrowReader = new(batches);
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.ArrowLasReader`
* `Altemiq.IO.Las.LasReaderExtensions`

## Additional Documentation

* [Apache Arrow](https://arrow.apache.org/)
* [Apache Arrow .NET](https://github.com/apache/arrow-dotnet)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Arrow is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).