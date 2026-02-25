## About

<!-- A description of the package and where one can find more documentation -->

Provides support for reading compressed chunked Point Clouds to Polars DataFrames.

## Key Features

<!-- The key features of this package -->

* Read a chunked LAZ file into a DataFrame
* Read a chunked LAZ file into a LazyFrame

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

### Data frame loading

```csharp
using Altemiq.IO.Las;

using LazReader reader = new("example.laz");

// read the data frame
var data = Polars.CSharp.DataFrame.ReadLaz(reader);
```

### Lazy frame loading

```csharp
using Altemiq.IO.Las;

using LazReader reader = new("example.laz");

// read the lazy frame
var data = Polars.CSharp.LazyFrame.ScanLaz(reader);
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.DataFrameExtensions`
* `Altemiq.IO.Las.LazyFrameExtensions`

## Additional Documentation

* [Polars](https://pola.rs/)
* [Polars.NET](https://github.com/ErrorLSC/Polars.NET)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.DataFrame is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).