namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class ColorPointTests
{
    private static readonly byte[] Bytes =
    [
        0xC5, 0x0A, 0x00, 0x00, 0x9F, 0xFA, 0xFF, 0xFF, 0xC3, 0xE5, 0xFF, 0xFF, 0xAE, 0x01, 0x09, 0x01, 0xE3, 0x00, 0x00, 0x00, 0x00, 0x68, 0x90, 0xA2, 0x64, 0x0B,
    ];

    private static
#if !NETFRAMEWORK
        readonly
#endif
        ColorPointDataRecord Point = new()
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
            Color = Color.FromRgb(26624, 41616, 2916),
        };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await CheckPoint(MemoryMarshal.Read<ColorPointDataRecord>(Bytes));
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
    public async Task CopyTo()
    {
        var destination = new byte[Bytes.Length];
        Point.CopyTo(destination);
        await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    private static async Task CheckPoint(ColorPointDataRecord record)
    {
        _ = await Assert.That(record)
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
            .And.Member(p => p.PointSourceId, pointSourceId => pointSourceId.IsDefault())
            .And.Member(
                p => p.Color,
                color => color
                    .Member(c => c.R, r => r.IsNotDefault())
                    .And.Member(c => c.G, g => g.IsNotDefault())
                    .And.Member(c => c.B, b => b.IsNotDefault()));
    }
}