namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class ExtendedVariableLengthRecordHeaderTests
{
    private static readonly byte[] Bytes =
    [
        0x00, 0x00, 0x63, 0x6F, 0x70, 0x63, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE8, 0x03, 0xC0, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x45, 0x50, 0x54, 0x20, 0x48, 0x69, 0x65, 0x72, 0x61, 0x72,
        0x63, 0x68, 0x79, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    ];

    private static readonly ExtendedVariableLengthRecordHeader Header = new()
    {
        UserId = "copc",
        RecordId = 1000,
        RecordLengthAfterHeader = 8896,
        Description = "EPT Hierarchy",
    };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await Assert.That(Marshal.PtrToStructure<ExtendedVariableLengthRecordHeader>(Marshal.UnsafeAddrOfPinnedArrayElement(Bytes, 0)))
            .IsNotDefault()
            .Satisfies(h => h.UserId, userId => userId.IsEqualTo("copc"))
            .Satisfies(h => h.RecordId, recordId => recordId.IsEqualTo((ushort)1000))
            .Satisfies(h => h.Description, description => description.IsEqualTo("EPT Hierarchy"));
    }

    [Test]
    public async Task FromBytes()
    {
        await Assert.That(new ExtendedVariableLengthRecordHeader(Bytes))
            .IsNotDefault()
            .Satisfies(h => h.UserId, userId => userId.IsEqualTo("copc"))
            .Satisfies(h => h.RecordId, recordId => recordId.IsEqualTo((ushort)1000))
            .Satisfies(h => h.Description, description => description.IsEqualTo("EPT Hierarchy"));
    }

    [Test]
    [LittleEndianOnly]
    public async Task ToMemory()
    {
        byte[] destination = new byte[Bytes.Length];
        Marshal.StructureToPtr(Header, Marshal.UnsafeAddrOfPinnedArrayElement(destination, 0), fDeleteOld: false);
        Array.Copy(Bytes, destination, 2);
        await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    [Test]
    public async Task ToBytes()
    {
        byte[] destination = new byte[Bytes.Length];
        Header.WriteLittleEndian(destination);
        Array.Copy(Bytes, destination, 2);
        await Assert.That(destination).IsEquivalentTo(Bytes);
    }
}