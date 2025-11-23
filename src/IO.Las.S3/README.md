## About

<!-- A description of the package and where one can find more documentation -->

Provides support for reading LAS/LAZ files through Amazon S3

## Key Features

<!-- The key features of this package -->

* Read LAS(Z) files directly from an Amazon S3 URL
* Ability to open all S3 URL schemes including
  * S3
  * Virtual Hosted-Style
  * Path-Style

## How to Use

<!-- A compelling example on how to use this package with code, as well as any specific guidelines for when to use the package -->

```csharp
using Altemiq.IO.Las;
using Altemiq.IO.Las.S3;

# example URIs
# S3: s3://example-bucket/example-path/example.las
# Virtual Hosted-Style: https://example-bucket.s3.amazonaws.com/example-path/example.las
# Path-Style: https://s3.amazonaws.com/example-bucket/example-path/example.las
var url = new Uri("s3://example-bucket/example-path/example.las");

LasReader reader = new(S3Las.OpenRead(url));
```

## Main Types

<!-- The main types provided in this library -->

The main types provided by this library are:

* `Altemiq.IO.Las.S3.S3Las`
* `Altemiq.IO.Las.S3.S3ChunkedStream`
* `Altemiq.IO.Las.S3.S3UriUtility`

## Additional Documentation

* [Amazon S3](https://aws.amazon.com/s3/)
* [AWS SDK for .NET](https://github.com/aws/aws-sdk-net)

## Feedback & Contributing

<!-- How to provide feedback on this package and contribute to it -->

Altemiq.IO.Las.S3 is released as open source under the [MIT license](https://licenses.nuget.org/MIT). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/altemiq/las).