namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class PointTests
{
    private static readonly byte[] Bytes =
    [
        0xC5, 0x0A, 0x00, 0x00, 0x9F, 0xFA, 0xFF, 0xFF, 0xC3, 0xE5, 0xFF, 0xFF, 0xAE, 0x01, 0x09, 0x01, 0xE3, 0x00, 0x00, 0x00,
    ];

    private static
#if !NETFRAMEWORK
        readonly
#endif
        PointDataRecord Point = new()
        {
            X = 2757,
            Y = -1377,
            Z = -6717,
            Intensity = 430,
            ReturnNumber = 1,
            NumberOfReturns = 1,
            ScanDirectionFlag = false,
            EdgeOfFlightLine = false,
            Classification = Classification.Unclassified,
            Synthetic = false,
            KeyPoint = false,
            Withheld = false,
            ScanAngleRank = -29,
            UserData = 0,
            PointSourceId = 0,
        };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await CheckPoint(MemoryMarshal.Read<PointDataRecord>(Bytes));
    }

    [Test]
    public async Task FromBytes()
    {
        await CheckPoint(new(Bytes));
    }

    [Test]
    [LittleEndianOnly]
    public async Task ToMemory()
    {
        var destination = new byte[Bytes.Length];
#if NETFRAMEWORK
        MemoryMarshal.Write(destination, ref Point);
#else
        MemoryMarshal.Write(destination, in Point);
#endif
        await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    [Test]
    public async Task ToBytes()
    {
        var destination = new byte[Bytes.Length];
        _ = Point.WriteLittleEndian(destination);
        _ = await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    [Test]
    public async Task SettingBitProperties()
    {
        var point = new PointDataRecord
        {
            X = 1,
            Y = 2,
            Z = 3,
            Intensity = 4,
            ReturnNumber = 5,
            NumberOfReturns = 6,
            ScanDirectionFlag = true,
            EdgeOfFlightLine = true,
            Classification = Classification.LowPoint,
            Synthetic = true,
            KeyPoint = true,
            Withheld = true,
            ScanAngleRank = -8,
            UserData = 9,
            PointSourceId = 10,
        };

        _ = await Assert.That(point)
            .Member(p => p.X, x => x.IsEqualTo(1))
            .And.Member(p => p.Y, y => y.IsEqualTo(2))
            .And.Member(p => p.Z, z => z.IsEqualTo(3))
            .And.Member(p => p.Intensity, intensity => intensity.IsEqualTo((ushort)4))
            .And.Member(p => p.ReturnNumber, returnNumber => returnNumber.IsEqualTo((byte)5))
            .And.Member(p => p.NumberOfReturns, numberOfReturns => numberOfReturns.IsEqualTo((byte)6))
            .And.Member(p => p.ScanDirectionFlag, scanDirectionFlag => scanDirectionFlag.IsTrue())
            .And.Member(p => p.EdgeOfFlightLine, edgeOfFlightLine => edgeOfFlightLine.IsTrue())
            .And.Member(p => p.Classification, classification => classification.IsEqualTo(Classification.LowPoint))
            .And.Member(p => p.Synthetic, synthetic => synthetic.IsTrue())
            .And.Member(p => p.KeyPoint, keyPoint => keyPoint.IsTrue())
            .And.Member(p => p.Withheld, withheld => withheld.IsTrue())
            .And.Member(p => p.ScanAngleRank, scanAngleRank => scanAngleRank.IsEqualTo((sbyte)-8))
            .And.Member(p => p.UserData, userData => userData.IsEqualTo((byte)9))
            .And.Member(p => p.PointSourceId, pointSourceId => pointSourceId.IsEqualTo((ushort)10));
    }

    private static async Task CheckPoint(PointDataRecord point)
    {
        _ = await Assert.That(point)
            .Member(p => p.X, x => x.IsEqualTo(2757))
            .And.Member(p => p.Y, y => y.IsEqualTo(-1377))
            .And.Member(p => p.Z, z => z.IsEqualTo(-6717))
            .And.Member(p => p.Intensity, intensity => intensity.IsEqualTo((ushort)430))
            .And.Member(p => p.ReturnNumber, returnNumber => returnNumber.IsEqualTo((byte)1))
            .And.Member(p => p.NumberOfReturns, numberOfReturns => numberOfReturns.IsEqualTo((byte)1))
            .And.Member(p => p.ScanDirectionFlag, scanDirectionFlag => scanDirectionFlag.IsFalse())
            .And.Member(p => p.EdgeOfFlightLine, edgeOfFlightLine => edgeOfFlightLine.IsFalse())
            .And.Member(p => p.Classification, classification => classification.IsEqualTo(Classification.Unclassified))
            .And.Member(p => p.Synthetic, synthetic => synthetic.IsFalse())
            .And.Member(p => p.KeyPoint, keyPoint => keyPoint.IsFalse())
            .And.Member(p => p.Withheld, withheld => withheld.IsFalse())
            .And.Member(p => p.ScanAngleRank, scanAngleRank => scanAngleRank.IsEqualTo((sbyte)-29))
            .And.Member(p => p.UserData, userData => userData.IsDefault())
            .And.Member(p => p.PointSourceId, pointSourceId => pointSourceId.IsDefault());
    }
}