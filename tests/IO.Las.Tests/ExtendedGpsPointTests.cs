namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class ExtendedGpsPointTests
{
    private static readonly PointDataRecordQuantizer Quantizer = new();

    private static readonly byte[] Bytes =
    [
        0xDE, 0x35, 0x5B, 0x00, 0xD4, 0x9B, 0x0E, 0x00, 0x87, 0x2B, 0xF8, 0xFF, 0xB6, 0x8C, 0x21, 0x00, 0x04, 0x00, 0x72, 0x78, 0x00, 0x00, 0x47, 0x87, 0x67, 0x62, 0x97, 0xA7, 0xA5, 0x41,
    ];

    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsPointDataRecord Point = new()
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
        };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await CheckPoint(MemoryMarshal.Read<ExtendedGpsPointDataRecord>(Bytes));
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

    private static async Task CheckPoint(ExtendedGpsPointDataRecord record)
    {
        _ = await Assert.That(record)
            .Member(p => p.X, x => x.IsEqualTo(5977566))
            .And.Member(p => p.Y, y => y.IsEqualTo(957396))
            .And.Member(p => p.Z, z => z.IsEqualTo(-513145))
            .And.Member(p => p.Intensity, intensity => intensity.IsEqualTo((ushort)36022))
            .And.Member(p => p.ReturnNumber, returnNumber => returnNumber.IsEqualTo((byte)1))
            .And.Member(p => p.NumberOfReturns, numberOfReturns => numberOfReturns.IsEqualTo((byte)2))
            .And.Member(p => p.Synthetic, synthetic => synthetic.IsFalse())
            .And.Member(p => p.KeyPoint, keyPoint => keyPoint.IsFalse())
            .And.Member(p => p.Withheld, withheld => withheld.IsFalse())
            .And.Member(p => p.Overlap, overlap => overlap.IsFalse())
            .And.Member(p => p.ScannerChannel, scannerChannel => scannerChannel.IsDefault())
            .And.Member(p => p.ScanDirectionFlag, scanDirectionFlag => scanDirectionFlag.IsFalse())
            .And.Member(p => p.EdgeOfFlightLine, edgeOfFlightLine => edgeOfFlightLine.IsFalse())
            .And.Member(p => p.Classification, classification => classification.IsEqualTo(ExtendedClassification.MediumVegetation))
            .And.Member(p => p.UserData, userData => userData.IsDefault())
            .And.Member(p => p.ScanAngle, scanAngle => scanAngle.IsEqualTo((short)30834))
            .And.Member(p => p.PointSourceId, pointSourceId => pointSourceId.IsDefault())
            .And.Member(p => Quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsAfter(new(2017, 1, 1)).And.IsBefore(new(2017, 12, 31)));
    }
}