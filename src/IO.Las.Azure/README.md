## About

<!-- A description of the package and where one can find more documentation -->

Provides support for reading LAS/LAZ files through Azure Blob Storage

## Key Features

<!-- The key features of this package -->

* Read LAS(Z) files directly from an Azure Blob Storage URL
* Ability to open all S3 URL schemes including
  * S3
  * Virtual Hosted-Style
  * Path-Style

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.Azure;

# example URIs
# HTTP: https://example-account.blob.core.windows.net/example-container/example-path/example.las

LasReader reader = new(BlobLas.OpenRead(url));
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.Azure.BlobLas`
* `Altemiq.IO.Las.Azure.BlobChunkedStream`

## Additional Documentation

* [Azure Blob Storage](https://azure.microsoft.com/en-us/products/storage/blobs/)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.Azure is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).