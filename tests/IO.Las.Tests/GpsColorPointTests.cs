namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class GpsColorPointTests
{
    private static readonly PointDataRecordQuantizer Quantizer = new();

    private static readonly byte[] Bytes =
    [
        0xC5, 0x0A, 0x00, 0x00, 0x9F, 0xFA, 0xFF, 0xFF, 0xC3, 0xE5, 0xFF, 0xFF, 0xAE, 0x01, 0x09, 0x01, 0xE3,
        0x00, 0x00, 0x00, 0x00, 0x68, 0x90, 0xA2, 0x64, 0x0B, 0x1E, 0x41, 0x90, 0xA2, 0x64, 0x0B, 0x1E, 0x41,
    ];

    private static
#if !NETFRAMEWORK
        readonly
#endif
        GpsColorPointDataRecord Point = new()
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
            GpsTime = 492249.15875399113,
            Color = Color.FromRgb(41616, 2916, 16670),
        };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await CheckPoint(MemoryMarshal.Read<GpsColorPointDataRecord>(Bytes));
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
        _ = Point.CopyTo(destination);
        _ = await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    private static async Task CheckPoint(GpsColorPointDataRecord record)
    {
        _ = await Assert.That(record)
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
            .And.Member(static p => p.PointSourceId, static pointSourceId => pointSourceId.IsDefault())
            .And.Member(static p => Quantizer.GetDateTime(p.GpsTime), static gpsTime => gpsTime.IsAfter(new(2010, 1, 1)))
            .And.Member(
                static p => p.Color,
                static color => color
                    .Member(static c => c.R, static r => r.IsNotDefault())
                    .And.Member(static c => c.G, static g => g.IsNotDefault())
                    .And.Member(static c => c.B, static b => b.IsNotDefault()));
    }
}