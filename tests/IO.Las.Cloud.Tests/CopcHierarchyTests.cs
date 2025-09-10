namespace Altemiq.IO.Las.Cloud;

public class CopcHierarchyTests
{
    [Test]
    public async Task CreateFromEntries()
    {
        CopcHierarchy.Entry first = new(new(1, 2, 3, 4), 5, 6, 7);
        CopcHierarchy.Entry second = new(new(7, 6, 5, 4), 3, 2, 1);
        CopcHierarchy.Entry[] entries =
        [
            first,
            second,
        ];

        CopcInfo info = new();

        CopcHierarchy hierarchy = new(entries, info);

        _ = await Assert.That(info.RootHierOffset).IsEqualTo((ulong)CopcHierarchy.HeaderSize);
        _ = await Assert.That(info.RootHierSize).IsEqualTo(64UL);

        _ = await Assert.That(hierarchy.Root).IsNotNull();
        _ = await Assert.That(hierarchy.Root).IsEquivalentTo([first, second]);
    }

    [Test]
    public async Task WriteToMemory()
    {
        CopcHierarchy.Entry first = new(new(1, 2, 3, 4), 5, 6, 7);
        CopcHierarchy.Entry second = new(new(7, 6, 5, 4), 3, 2, 1);

        CopcInfo info = new();
        CopcHierarchy hierarchy = new([first, second], info);

        Span<byte> data = stackalloc byte[(int)hierarchy.Size()];

        hierarchy.CopyTo(data);

        var recordHeader = ExtendedVariableLengthRecordHeader.Read(data);
        var recordData = data[ExtendedVariableLengthRecordHeader.Size..];
        hierarchy = new(recordHeader, info, default, recordData);

        _ = await Assert.That(hierarchy.Root).IsNotNull();
        _ = await Assert.That(hierarchy.Root).IsEquivalentTo([first, second]);
    }

    [Test]
    public async Task WriteEntryToMemory()
    {
        CopcHierarchy.Entry input = new(new(1, 2, 3, 4), 5, 6, 7);
        byte[] bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(1024);
        input.CopyTo(bytes.AsSpan(10));

        CopcHierarchy.Entry output = new(bytes.AsSpan(10));

        _ = await Assert.That(output).IsEqualTo(input);
        System.Buffers.ArrayPool<byte>.Shared.Return(bytes);
    }
}