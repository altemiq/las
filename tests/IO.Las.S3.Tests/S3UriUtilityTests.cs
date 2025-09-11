namespace Altemiq.IO.Las.S3;

public class S3UriUtilityTests
{
    private const string BucketName = "bucket-name-with-dashes-12345";
    private const string Path = "first-path/second-path/third-path/file.ext";
    private const string Region = "ap-southeast-2";
    private const string Scheme = "s3";
    private const string Host = "amazonaws.com";
    
    private const string S3Scheme = $"{Scheme}://{BucketName}/{Path}";
    private const string PathStyleNoRegion = $"https://{Scheme}.{Host}/{BucketName}/{Path}";
    private const string PathStyle = $"https://{Scheme}.{Region}.{Host}/{BucketName}/{Path}";
    private const string PathStyleDash = $"https://{Scheme}-{Region}.{Host}/{BucketName}/{Path}";
    private const string VirtualHostedStyle = $"https://{BucketName}.{Scheme}.{Region}.{Host}/{Path}";

    [Test]
    [MethodDataSource(nameof(GetUris))]
    public async Task TransformUri(string url)
    {
        _ = await Assert.That(S3UriUtility.TransformUri(url)).IsNotNull().And.IsEqualTo(S3Scheme);
    }

    [Test]
    [MethodDataSource(nameof(GetUris))]
    public async Task TryTransformUri(string url)
    {
        _ = await Assert.That(S3UriUtility.TryTransformUri(url, out string output)).IsTrue();
        _ = await Assert.That(output).IsNotNull().And.IsEqualTo(S3Scheme);
    }

    public static IEnumerable<string> GetUris()
    {
        yield return S3Scheme;
        yield return PathStyle;
        yield return PathStyleNoRegion;
        yield return PathStyleDash;
        yield return VirtualHostedStyle;
    }
}