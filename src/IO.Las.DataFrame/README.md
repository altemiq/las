## About

<!-- A description of the package and where one can find more documentation -->

Provides support for reading and writing Point Clouds to and from Polars DataFrames.

## Key Features

<!-- The key features of this package -->

* Read a LAS file into a DataFrame
* Read a LAS file into a LazyFrame

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

### Data frame loading

```csharp
using Altemiq.IO.Las;

using LazReader reader = new("example.las");

// read the data frame
var data = Polars.CSharp.DataFrame.ReadLas(reader);

// write the data frame as a LAS file
data.WriteTo(arrowReader =>
{
    // use the arrow LAS reader
});
```

### Lazy frame loading

```csharp
using Altemiq.IO.Las;

using LazReader reader = new("example.las");

// read the lazy frame
var data = Polars.CSharp.LazyFrame.ScanLas(reader);

// sink the lazy frame as a LAS file
data.SinkTo(arrowReader =>
{
    // use the arrow LAS reader
});
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.DataFrameExtensions`
* `Altemiq.IO.Las.LazyFrameExtensions`
* `Altemiq.IO.Las.SchemaExtensions`

## Additional Documentation

* [Polars](https://pola.rs/)
* [Polars.NET](https://github.com/ErrorLSC/Polars.NET)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.DataFrame is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).