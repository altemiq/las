namespace Altemiq.IO.Las.S3;

public class S3LasTests
{
    [Test]
    [MethodDataSource(nameof(GetUris))]
    public async Task TryParseUri(string uri, Amazon.RegionEndpoint region)
    {
        _ = await Assert.That(Amazon.S3.Util.AmazonS3Uri.TryParseAmazonS3Uri(uri, out var output)).IsTrue();
        _ = await Assert.That(output.Bucket).IsEqualTo(Data.BucketName);
        _ = await Assert.That(output.Key).IsEqualTo(Data.Path);
        _ = await Assert.That(output.Region).IsEqualTo(region);
    }

    public static IEnumerable<Func<(string Uri, Amazon.RegionEndpoint Region)>> GetUris()
    {
        yield return () => (Data.S3Scheme, null);
        yield return () => (Data.PathStyle, Amazon.RegionEndpoint.APSoutheast2);
        yield return () => (Data.PathStyleNoRegion, Amazon.RegionEndpoint.USEast1);
        yield return () => (Data.PathStyleDash, Amazon.RegionEndpoint.APSoutheast2);
        yield return () => (Data.VirtualHostedStyle, Amazon.RegionEndpoint.APSoutheast2);
    }
}