## About

<!-- A description of the package and where one can find more documentation -->

Provides support for reading LAS/LAZ files through HTTP(S)

## Key Features

<!-- The key features of this package -->

* Read LAS(Z) files directly from an HTTP(S) URL

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Http;

var url = new Uri("https://example.com/example.las");

LasReader reader = new(HttpLas.OpenRead(url));
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.Http.HttpLas`
* `Altemiq.IO.Las.Http.HttpChunkedStream`

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Http is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).