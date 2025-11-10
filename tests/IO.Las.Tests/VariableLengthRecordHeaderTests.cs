namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class VariableLengthRecordHeaderTests
{
    private static readonly byte[] Bytes =
    [
        0xBB, 0xAA, 0x4C, 0x41, 0x53, 0x46, 0x5F, 0x50, 0x72, 0x6F, 0x6A, 0x65, 0x63, 0x74, 0x69, 0x6F, 0x6E, 0x00, 0xAF, 0x87, 0x28, 0x00, 0x62, 0x79, 0x20, 0x4C, 0x41, 0x53, 0x74, 0x6F, 0x6F, 0x6C, 0x73, 0x20, 0x6F, 0x66, 0x20, 0x4D,
        0x61, 0x72, 0x74, 0x69, 0x6E, 0x20, 0x49, 0x73, 0x65, 0x6E, 0x62, 0x75, 0x72, 0x67, 0x00, 0x00
    ];

    private static readonly VariableLengthRecordHeader Header = new()
    {
        UserId = VariableLengthRecordHeader.ProjectionUserId,
        RecordId = 34735,
        RecordLengthAfterHeader = 40,
        Description = "by LAStools of Martin Isenburg",
    };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        _ = await Assert.That(Marshal.PtrToStructure<VariableLengthRecordHeader>(Marshal.UnsafeAddrOfPinnedArrayElement(Bytes, 0)))
            .IsNotDefault()
            .And.Member(h => h.UserId, userId => userId.IsEqualTo(VariableLengthRecordHeader.ProjectionUserId))
            .And.Member(h => h.RecordId, recordId => recordId.IsEqualTo((ushort)34735))
            .And.Member(h => h.Description, description => description.IsEqualTo("by LAStools of Martin Isenburg"));
    }

    [Test]
    public async Task FromBytes()
    {
        _ = await Assert.That(new VariableLengthRecordHeader(Bytes))
            .IsNotDefault()
            .And.Member(h => h.UserId, userId => userId.IsEqualTo(VariableLengthRecordHeader.ProjectionUserId))
            .And.Member(h => h.RecordId, recordId => recordId.IsEqualTo((ushort)34735))
            .And.Member(h => h.Description, description => description.IsEqualTo("by LAStools of Martin Isenburg"));
    }

    [Test]
    [LittleEndianOnly]
    public async Task ToMemory()
    {
        var destination = new byte[Bytes.Length];
        Marshal.StructureToPtr(Header, Marshal.UnsafeAddrOfPinnedArrayElement(destination, 0), fDeleteOld: false);
        Array.Copy(Bytes, destination, 2);
        _ = await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    [Test]
    public async Task ToBytes()
    {
        var destination = new byte[Bytes.Length];
        Header.WriteLittleEndian(destination);
        Array.Copy(Bytes, destination, 2);
        _ = await Assert.That(destination).IsEquivalentTo(Bytes);
    }
}