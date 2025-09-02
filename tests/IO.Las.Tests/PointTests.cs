using Microsoft.VisualBasic;

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
        Point.WriteLittleEndian(destination);
        await Assert.That(destination).IsEquivalentTo(Bytes);
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

        await Assert.That(point)
            .Satisfies(p => p.X, x => x.IsEqualTo(1))
            .Satisfies(p => p.Y, y => y.IsEqualTo(2))
            .Satisfies(p => p.Z, z => z.IsEqualTo(3))
            .Satisfies(p => p.Intensity, intensity => intensity.IsEqualTo((ushort)4))
            .Satisfies(p => p.ReturnNumber, returnNumber => returnNumber.IsEqualTo((byte)5))
            .Satisfies(p => p.NumberOfReturns, numberOfReturns => numberOfReturns.IsEqualTo((byte)6))
            .Satisfies(p => p.ScanDirectionFlag, scanDirectionFlag => scanDirectionFlag.IsTrue())
            .Satisfies(p => p.EdgeOfFlightLine, edgeOfFlightLine => edgeOfFlightLine.IsTrue())
            .Satisfies(p => p.Classification, classification => classification.IsEqualTo(Classification.LowPoint))
            .Satisfies(p => p.Synthetic, synthetic => synthetic.IsTrue())
            .Satisfies(p => p.KeyPoint, keyPoint => keyPoint.IsTrue())
            .Satisfies(p => p.Withheld, withheld => withheld.IsTrue())
            .Satisfies(p => p.ScanAngleRank, scanAngleRank => scanAngleRank.IsEqualTo((sbyte)-8))
            .Satisfies(p => p.UserData, userData => userData.IsEqualTo((byte)9))
            .Satisfies(p => p.PointSourceId, pointSourceId => pointSourceId.IsEqualTo((ushort)10));
    }

    private static async Task CheckPoint(PointDataRecord point)
    {
        await Assert.That(point)
            .Satisfies(p => p.X, x => x.IsEqualTo(2757))
            .Satisfies(p => p.Y, y => y.IsEqualTo(-1377))
            .Satisfies(p => p.Z, z => z.IsEqualTo(-6717))
            .Satisfies(p => p.Intensity, intensity => intensity.IsEqualTo((ushort)430))
            .Satisfies(p => p.ReturnNumber, returnNumber => returnNumber.IsEqualTo((byte)1))
            .Satisfies(p => p.NumberOfReturns, numberOfReturns => numberOfReturns.IsEqualTo((byte)1))
            .Satisfies(p => p.ScanDirectionFlag, scanDirectionFlag => scanDirectionFlag.IsFalse())
            .Satisfies(p => p.EdgeOfFlightLine, edgeOfFlightLine => edgeOfFlightLine.IsFalse())
            .Satisfies(p => p.Classification, classification => classification.IsEqualTo(Classification.Unclassified))
            .Satisfies(p => p.Synthetic, synthetic => synthetic.IsFalse())
            .Satisfies(p => p.KeyPoint, keyPoint => keyPoint.IsFalse())
            .Satisfies(p => p.Withheld, withheld => withheld.IsFalse())
            .Satisfies(p => p.ScanAngleRank, scanAngleRank => scanAngleRank.IsEqualTo((sbyte)-29))
            .Satisfies(p => p.UserData, userData => userData.IsDefault())
            .Satisfies(p => p.PointSourceId, pointSourceId => pointSourceId.IsDefault());
    }
}