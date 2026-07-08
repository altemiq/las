namespace Altemiq.IO.Las.S3;

public class AmazonS3UriExtensionsTests
{
    [Test]
    public async Task ToS3()
    {
        _ = await Assert.That(new Amazon.S3.Util.AmazonS3Uri(Data.S3UriData.S3Scheme).ToS3Style()).IsEqualTo(new(Data.S3UriData.S3Scheme));
    }

    [Test]
    [MethodDataSource(nameof(GetPathStyleData))]
    public async Task ToPathStyle(string input, string expected)
    {
        _ = await Assert.That(new Amazon.S3.Util.AmazonS3Uri(input).ToPathStyle()).IsEqualTo(new(expected));
    }

    [Test]
    public async Task ToVirtualHostedStyle()
    {
        _ = await Assert.That(new Amazon.S3.Util.AmazonS3Uri(Data.S3UriData.PathStyle).ToVirtualHostStyle()).IsEqualTo(new(Data.S3UriData.VirtualHostedStyle));
    }

    public static IEnumerable<Func<(string Input, string Expected)>> GetPathStyleData()
    {
        yield return static () => (Data.S3UriData.S3Scheme, Data.S3UriData.PathStyleNoRegion);
        yield return static () => (Data.S3UriData.PathStyle, Data.S3UriData.PathStyle);
    }
}