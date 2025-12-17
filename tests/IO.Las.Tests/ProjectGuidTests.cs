namespace Altemiq.IO.Las;

public class ProjectGuidTests
{
    private const string ProjectGuid1 = "00112233-4455-6677-8899-AABBCCDDEEFF";
    private const string ProjectGuid2 = "00112233-4455-6677-4D79-50726F6A3031";

    [Test]
    [Arguments(ProjectGuid1, new byte[] { 0x33, 0x22, 0x11, 0x00, 0x55, 0x44, 0x77, 0x66, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF })]
    [Arguments(ProjectGuid2, new byte[] { 0x33, 0x22, 0x11, 0x00, 0x55, 0x44, 0x77, 0x66, 0x4D, 0x79, 0x50, 0x72, 0x6F, 0x6A, 0x30, 0x31 })]
    public async Task HaveTheCorrectBytes(string id, byte[] bytes)
    {
        _ = await Assert.That(Guid.Parse(id).ToByteArray()).IsEquivalentTo(bytes);
    }

    [Test]
    [Arguments(ProjectGuid1, "guid1.bin")]
    [Arguments(ProjectGuid2, "guid2.bin")]
    public async Task ReadTheBytesCorrectly(string id, string resource)
    {
#if NET
        await
#endif
        using var stream = typeof(ProjectGuidTests).Assembly.GetManifestResourceStream(typeof(ProjectGuidTests), resource)
                              ?? throw new InvalidOperationException("Failed to get stream");
        var length = (int)stream.Length;
        var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(length);
#if NET
        _ = await Assert.That(await stream.ReadAsync(bytes.AsMemory(0, length))).IsEqualTo(length);
#else
        _ = await Assert.That(await stream.ReadAsync(bytes, 0, length)).IsEqualTo(length);
#endif
        _ = await Assert.That(bytes).Count().IsEqualTo(16);

        _ = await Assert.That(new Guid(bytes)).IsEqualTo(Guid.Parse(id));
        System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
    }

#if LAS1_2_OR_GREATER
    [Test]
    [Arguments(ProjectGuid1, "guid1.las")]
    [Arguments(ProjectGuid2, "guid2.las")]
    public async Task ReadTheHeaderCorrectly(string id, string resource)
    {
#if NET
        await
#endif
        using var stream = typeof(ProjectGuidTests).Assembly.GetManifestResourceStream(typeof(ProjectGuidTests), resource)
                              ?? throw new InvalidOperationException("Failed to get stream");
        HeaderBlockReader headerReader = new(stream);
        var headerBlock = headerReader.GetHeaderBlock();
        _ = await Assert.That(headerBlock.ProjectId).IsEqualTo(Guid.Parse(id));
    }
#endif
}