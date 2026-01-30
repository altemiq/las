namespace Altemiq.IO.Las.S3;

public static class Data
{
    public const string BucketName = "bucket-name-with-dashes-12345";
    public const string Path = "first-path/second-path/third-path/file.ext";

    private const string Host = "amazonaws.com";

    private static string Region { get; } = Amazon.RegionEndpoint.APSoutheast2.SystemName;

    public static string S3Scheme { get; } = $"{Uri.UriSchemeS3}://{BucketName}/{Path}";
    public static string PathStyleNoRegion { get; } = $"https://{Uri.UriSchemeS3}.{Host}/{BucketName}/{Path}";
    public static string PathStyle { get; } = $"https://{Uri.UriSchemeS3}.{Region}.{Host}/{BucketName}/{Path}";
    public static string PathStyleDash { get; } = $"https://{Uri.UriSchemeS3}-{Region}.{Host}/{BucketName}/{Path}";
    public static string VirtualHostedStyle { get; } = $"https://{BucketName}.{Uri.UriSchemeS3}.{Region}.{Host}/{Path}";
}