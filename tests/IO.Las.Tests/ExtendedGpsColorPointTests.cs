namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class ExtendedGpsColorPointTests
{
    private static readonly PointDataRecordQuantizer Quantizer = new();

    private static readonly byte[] Bytes =
    [
        0xDE, 0x35, 0x5B, 0x00, 0xD4, 0x9B, 0x0E, 0x00, 0x87, 0x2B, 0xF8, 0xFF, 0xB6, 0x8C, 0x21, 0x00, 0x04, 0x00, 0x72, 0x78, 0x00, 0x00, 0x47, 0x87, 0x67, 0x62, 0x97, 0xA7, 0xA5, 0x41, 0x67, 0x62, 0x97, 0xA7, 0xA5, 0x41,
    ];

    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsColorPointDataRecord Point = new()
        {
            X = 5977566,
            Y = 957396,
            Z = -513145,
            Intensity = 36022,
            ReturnNumber = 1,
            NumberOfReturns = 2,
            Synthetic = false,
            KeyPoint = false,
            Withheld = false,
            Overlap = false,
            ScannerChannel = 0,
            ScanDirectionFlag = false,
            EdgeOfFlightLine = false,
            Classification = ExtendedClassification.MediumVegetation,
            UserData = 0,
            ScanAngle = 30834,
            PointSourceId = 0,
            GpsTime = 181652401.20220396,
            Color = Color.FromRgb(25191, 42903, 16805),
        };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await CheckPoint(MemoryMarshal.Read<ExtendedGpsColorPointDataRecord>(Bytes));
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

    private static async Task CheckPoint(ExtendedGpsColorPointDataRecord record)
    {
        _ = await Assert.That(record)
            .Member(static p => p.X, static x => x.IsEqualTo(5977566))
            .And.Member(static p => p.Y, static y => y.IsEqualTo(957396))
            .And.Member(static p => p.Z, static z => z.IsEqualTo(-513145))
            .And.Member(static p => p.Intensity, static intensity => intensity.IsEqualTo((ushort)36022))
            .And.Member(static p => p.ReturnNumber, static returnNumber => returnNumber.IsEqualTo((byte)1))
            .And.Member(static p => p.NumberOfReturns, static numberOfReturns => numberOfReturns.IsEqualTo((byte)2))
            .And.Member(static p => p.Synthetic, static synthetic => synthetic.IsFalse())
            .And.Member(static p => p.KeyPoint, static keyPoint => keyPoint.IsFalse())
            .And.Member(static p => p.Withheld, static withheld => withheld.IsFalse())
            .And.Member(static p => p.Overlap, static overlap => overlap.IsFalse())
            .And.Member(static p => p.ScannerChannel, static scannerChannel => scannerChannel.IsDefault())
            .And.Member(static p => p.ScanDirectionFlag, static scanDirectionFlag => scanDirectionFlag.IsFalse())
            .And.Member(static p => p.EdgeOfFlightLine, static edgeOfFlightLine => edgeOfFlightLine.IsFalse())
            .And.Member(static p => p.Classification, static classification => classification.IsEqualTo(ExtendedClassification.MediumVegetation))
            .And.Member(static p => p.UserData, static userData => userData.IsDefault())
            .And.Member(static p => p.ScanAngle, static scanAngle => scanAngle.IsEqualTo((short)30834))
            .And.Member(static p => p.PointSourceId, static pointSourceId => pointSourceId.IsDefault())
            .And.Member(static p => Quantizer.GetDateTime(p.GpsTime), static gpsTime => gpsTime.IsAfter(new(2017, 1, 1)).And.IsBefore(new(2017, 12, 31)))
            .And.Member(
                static p => p.Color,
                static color => color
                    .Member(static c => c.R, static r => r.IsNotDefault())
                    .And.Member(static c => c.G, static g => g.IsNotDefault())
                    .And.Member(static c => c.B, static b => b.IsNotDefault()));
    }
}