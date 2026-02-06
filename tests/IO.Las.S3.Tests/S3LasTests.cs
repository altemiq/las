namespace Altemiq.IO.Las.S3;

public class S3LasTests
{
    [Test]
    [MethodDataSource(nameof(GetUris))]
    public async Task TryParseUri(string uri, Amazon.RegionEndpoint region)
    {
        _ = await Assert.That(Amazon.S3.Util.AmazonS3Uri.TryParseAmazonS3Uri(uri, out var output)).IsTrue();
        _ = await Assert.That(output.Bucket).IsEqualTo(Data.S3UriData.BucketName);
        _ = await Assert.That(output.Key).IsEqualTo(Data.S3UriData.Path);
        _ = await Assert.That(output.Region).IsEqualTo(region);
    }

    public static IEnumerable<Func<(string Uri, Amazon.RegionEndpoint Region)>> GetUris()
    {
        yield return () => (Data.S3UriData.S3Scheme, null);
        yield return () => (Data.S3UriData.PathStyle, Amazon.RegionEndpoint.APSoutheast2);
        yield return () => (Data.S3UriData.PathStyleNoRegion, Amazon.RegionEndpoint.USEast1);
        yield return () => (Data.S3UriData.PathStyleDash, Amazon.RegionEndpoint.APSoutheast2);
        yield return () => (Data.S3UriData.VirtualHostedStyle, Amazon.RegionEndpoint.APSoutheast2);
    }
}