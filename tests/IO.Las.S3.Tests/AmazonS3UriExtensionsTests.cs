namespace Altemiq.IO.Las.S3;

public class AmazonS3UriExtensionsTests
{
    [Test]
    public async Task ToS3()
    {
        await Assert.That(new Amazon.S3.Util.AmazonS3Uri(Data.S3Scheme).ToS3Style()).IsEqualTo(new Uri(Data.S3Scheme));
    }

    [Test]
    [MethodDataSource(nameof(GetPathStyleData))]
    public async Task ToPathStyle(string input, string expected)
    {
        await Assert.That(new Amazon.S3.Util.AmazonS3Uri(input).ToPathStyle()).IsEqualTo(new Uri(expected));
    }

    [Test]
    public async Task ToVirtualHostedStyle()
    {
        await Assert.That(new Amazon.S3.Util.AmazonS3Uri(Data.PathStyle).ToVirtualHostStyle()).IsEqualTo(new Uri(Data.VirtualHostedStyle));
    }

    public static IEnumerable<Func<(string Input, string Expected)>> GetPathStyleData()
    {
        yield return () => (Data.S3Scheme, Data.PathStyleNoRegion);
        yield return () => (Data.PathStyle, Data.PathStyle);
    }
}