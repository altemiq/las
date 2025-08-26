using System.Runtime.InteropServices;

namespace Altemiq.IO.Las;

public class ExtendedGpsColorPointTests
{
    private static readonly PointDataRecordQuantizer Quantizer = new();

    private static readonly byte[] Bytes =
    [
        0xDE, 0x35, 0x5B, 0x00, 0xD4, 0x9B, 0x0E, 0x00, 0x87, 0x2B, 0xF8, 0xFF, 0xB6, 0x8C, 0x21, 0x00, 0x04, 0x00, 0x72, 0x78, 0x00, 0x00, 0x47, 0x87, 0x67, 0x62, 0x97, 0xA7, 0xA5, 0x41, 0x67, 0x62, 0x97, 0xA7, 0xA5, 0x41,
    ];

    private static readonly ExtendedGpsColorPointDataRecord Point = new()
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
        MemoryMarshal.Write(destination, in Point);
        await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    [Test]
    public async Task ToBytes()
    {
        var destination = new byte[Bytes.Length];
        Point.WriteLittleEndian(destination);
        await Assert.That(destination).IsEquivalentTo(Bytes);
    }

    private static async Task CheckPoint(ExtendedGpsColorPointDataRecord record)
    {
        await Assert.That(record)
            .Satisfies(p => p.X, x => x.IsEqualTo(5977566))
            .Satisfies(p => p.Y, y => y.IsEqualTo(957396))
            .Satisfies(p => p.Z, z => z.IsEqualTo(-513145))
            .Satisfies(p => p.Intensity, intensity => intensity.IsEqualTo((ushort)36022))
            .Satisfies(p => p.ReturnNumber, returnNumber => returnNumber.IsEqualTo((byte)1))
            .Satisfies(p => p.NumberOfReturns, numberOfReturns => numberOfReturns.IsEqualTo((byte)2))
            .Satisfies(p => p.Synthetic, synthetic => synthetic.IsFalse())
            .Satisfies(p => p.KeyPoint, keyPoint => keyPoint.IsFalse())
            .Satisfies(p => p.Withheld, withheld => withheld.IsFalse())
            .Satisfies(p => p.Overlap, overlap => overlap.IsFalse())
            .Satisfies(p => p.ScannerChannel, scannerChannel => scannerChannel.IsZero())
            .Satisfies(p => p.ScanDirectionFlag, scanDirectionFlag => scanDirectionFlag.IsFalse())
            .Satisfies(p => p.EdgeOfFlightLine, edgeOfFlightLine => edgeOfFlightLine.IsFalse())
            .Satisfies(p => p.Classification, classification => classification.IsEqualTo(ExtendedClassification.MediumVegetation))
            .Satisfies(p => p.UserData, userData => userData.IsZero())
            .Satisfies(p => p.ScanAngle, scanAngle => scanAngle.IsEqualTo((short)30834))
            .Satisfies(p => p.PointSourceId, pointSourceId => pointSourceId.IsZero())
            .Satisfies(p => Quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsAfter(new(2017, 1, 1)).IsBefore(new(2017, 12, 31)))
            .Satisfies(
                p => p.Color,
                color => color
                    .Satisfies(c => c.R, r => r.IsNotZero())
                    .Satisfies(c => c.G, g => g.IsNotZero())
                    .Satisfies(c => c.B, b => b.IsNotZero()));
    }
}