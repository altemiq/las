namespace Altemiq.IO.Las;

using System.Runtime.InteropServices;

public class ExtendedGpsColorNearInfraredWaveformPointTests
{
    private static readonly PointDataRecordQuantizer Quantizer = new();

    private static readonly byte[] Bytes =
    [
        0xDE, 0x35, 0x5B, 0x00, 0xD4, 0x9B, 0x0E, 0x00, 0x87, 0x2B, 0xF8, 0xFF, 0xB6, 0x8C, 0x21, 0x00, 0x04, 0x00, 0x72, 0x78, 0x00, 0x00, 0x47, 0x87, 0x67, 0x62, 0x97, 0xA7, 0xA5, 0x41, 0x67, 0x62, 0x97, 0xA7, 0xA5, 0x41, 0xA5, 0x41, 0x01, 0xBC, 0xD4, 0x91, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x84, 0x79, 0x46, 0xBB, 0xBE, 0x2C, 0xB6, 0xF5, 0x3F, 0x2E, 0x36, 0xF7, 0x16, 0x1D, 0x39,
    ];

    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsColorNearInfraredWaveformPointDataRecord Point = new()
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
            NearInfrared = 16805,
            WavePacketDescriptorIndex = 1,
            ByteOffsetToWaveformData = 210883772UL,
            WaveformPacketSizeInBytes = 64U,
            ReturnPointWaveformLocation = 15969F,
            ParametricDx = -0.0000025741017F,
            ParametricDy = 0.00000259652484F,
            ParametricDz = 0.000149812418F,
        };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await CheckPoint(MemoryMarshal.Read<ExtendedGpsColorNearInfraredWaveformPointDataRecord>(Bytes));
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

    private static async Task CheckPoint(ExtendedGpsColorNearInfraredWaveformPointDataRecord record)
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
            .And.Member(p => Quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsAfter(new(2017, 1, 1)).And.IsBefore(new(2017, 12, 31)))
            .And.Member(
                p => p.Color,
                color => color
                    .Member(c => c.R, r => r.IsEqualTo((ushort)25191))
                    .And.Member(c => c.G, g => g.IsEqualTo((ushort)42903))
                    .And.Member(c => c.B, b => b.IsEqualTo((ushort)16805)))
            .And.Member(p => p.WavePacketDescriptorIndex, wavePacketDescriptorIndex => wavePacketDescriptorIndex.IsEqualTo((byte)1))
            .And.Member(p => p.ByteOffsetToWaveformData, byteOffsetToWaveformData => byteOffsetToWaveformData.IsEqualTo(210883772UL))
            .And.Member(p => p.WaveformPacketSizeInBytes, waveformPacketSizeInBytes => waveformPacketSizeInBytes.IsEqualTo(64U))
            .And.Member(p => p.ReturnPointWaveformLocation, returnPointWaveformLocation => returnPointWaveformLocation.IsEqualTo(15969F))
            .And.Member(p => p.ParametricDx, parametricDx => parametricDx.IsEqualTo(-0.0000025741017F))
            .And.Member(p => p.ParametricDy, parametricDy => parametricDy.IsEqualTo(0.00000259652484F))
            .And.Member(p => p.ParametricDz, parametricDz => parametricDz.IsEqualTo(0.000149812418F));
    }
}