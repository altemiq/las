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
            .Member(static p => p.X, static x => x.IsEqualTo(1))
            .And.Member(static p => p.Y, static y => y.IsEqualTo(2))
            .And.Member(static p => p.Z, static z => z.IsEqualTo(3))
            .And.Member(static p => p.Intensity, static intensity => intensity.IsEqualTo((ushort)4))
            .And.Member(static p => p.ReturnNumber, static returnNumber => returnNumber.IsEqualTo((byte)5))
            .And.Member(static p => p.NumberOfReturns, static numberOfReturns => numberOfReturns.IsEqualTo((byte)6))
            .And.Member(static p => p.ScanDirectionFlag, static scanDirectionFlag => scanDirectionFlag.IsTrue())
            .And.Member(static p => p.EdgeOfFlightLine, static edgeOfFlightLine => edgeOfFlightLine.IsTrue())
            .And.Member(static p => p.Classification, static classification => classification.IsEqualTo(Classification.LowPoint))
            .And.Member(static p => p.Synthetic, static synthetic => synthetic.IsTrue())
            .And.Member(static p => p.KeyPoint, static keyPoint => keyPoint.IsTrue())
            .And.Member(static p => p.Withheld, static withheld => withheld.IsTrue())
            .And.Member(static p => p.ScanAngleRank, static scanAngleRank => scanAngleRank.IsEqualTo((sbyte)-8))
            .And.Member(static p => p.UserData, static userData => userData.IsEqualTo((byte)9))
            .And.Member(static p => p.PointSourceId, static pointSourceId => pointSourceId.IsEqualTo((ushort)10));
    }

    private static async Task CheckPoint(PointDataRecord point)
    {
        _ = await Assert.That(point)
            .Member(static p => p.X, static x => x.IsEqualTo(2757))
            .And.Member(static p => p.Y, static y => y.IsEqualTo(-1377))
            .And.Member(static p => p.Z, static z => z.IsEqualTo(-6717))
            .And.Member(static p => p.Intensity, static intensity => intensity.IsEqualTo((ushort)430))
            .And.Member(static p => p.ReturnNumber, static returnNumber => returnNumber.IsEqualTo((byte)1))
            .And.Member(static p => p.NumberOfReturns, static numberOfReturns => numberOfReturns.IsEqualTo((byte)1))
            .And.Member(static p => p.ScanDirectionFlag, static scanDirectionFlag => scanDirectionFlag.IsFalse())
            .And.Member(static p => p.EdgeOfFlightLine, static edgeOfFlightLine => edgeOfFlightLine.IsFalse())
            .And.Member(static p => p.Classification, static classification => classification.IsEqualTo(Classification.Unclassified))
            .And.Member(static p => p.Synthetic, static synthetic => synthetic.IsFalse())
            .And.Member(static p => p.KeyPoint, static keyPoint => keyPoint.IsFalse())
            .And.Member(static p => p.Withheld, static withheld => withheld.IsFalse())
            .And.Member(static p => p.ScanAngleRank, static scanAngleRank => scanAngleRank.IsEqualTo((sbyte)-29))
            .And.Member(static p => p.UserData, static userData => userData.IsDefault())
            .And.Member(static p => p.PointSourceId, static pointSourceId => pointSourceId.IsDefault());
    }
}