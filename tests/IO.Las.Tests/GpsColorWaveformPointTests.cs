using System.Runtime.InteropServices;

namespace Altemiq.IO.Las;

public class GpsColorWaveformPointTests
{
    private static readonly PointDataRecordQuantizer Quantizer = new();

    private static readonly byte[] Bytes =
    [
        0xC5, 0x0A, 0x00, 0x00, 0x9F, 0xFA, 0xFF, 0xFF, 0xC3, 0xE5, 0xFF, 0xFF, 0xAE, 0x01, 0x09, 0x01, 0xE3, 0x00, 0x00, 0x00, 0x00, 0x68, 0x90, 0xA2, 0x64, 0x0B, 0x1E, 0x41, 0x90, 0xA2, 0x64, 0x0B, 0x1E, 0x41, 0x01, 0xF0, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    private static readonly GpsColorWaveformPointDataRecord Point = new()
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
        WavePacketDescriptorIndex = 1,
        ByteOffsetToWaveformData = 240UL,
        WaveformPacketSizeInBytes = 240U,
        ReturnPointWaveformLocation = 0,
        ParametricDx = 0,
        ParametricDy = 0,
        ParametricDz = 0,
    };

    [Test]
    [LittleEndianOnly]
    public async Task FromMemory()
    {
        await CheckPoint(MemoryMarshal.Read<GpsColorWaveformPointDataRecord>(Bytes));
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

    private static async Task CheckPoint(GpsColorWaveformPointDataRecord record)
    {
        await Assert.That(record)
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
            .Satisfies(p => p.UserData, userData => userData.IsZero())
            .Satisfies(p => p.PointSourceId, pointSourceId => pointSourceId.IsZero())
            .Satisfies(p => Quantizer.GetDateTime(p.GpsTime), gpsTime => gpsTime.IsAfter(new(2010, 1, 1)))
            .Satisfies(
                p => p.Color,
                color => color
                    .Satisfies(c => c.R, r => r.IsNotZero())
                    .Satisfies(c => c.G, g => g.IsNotZero())
                    .Satisfies(c => c.B, b => b.IsNotZero()))
            .Satisfies(p => p.WavePacketDescriptorIndex, wavePacketDescriptorIndex => wavePacketDescriptorIndex.IsEqualTo((byte)1))
            .Satisfies(p => p.ByteOffsetToWaveformData, byteOffsetToWaveformData => byteOffsetToWaveformData.IsEqualTo(240UL))
            .Satisfies(p => p.WaveformPacketSizeInBytes, waveformPacketSizeInBytes => waveformPacketSizeInBytes.IsEqualTo(240U))
            .Satisfies(p => p.ReturnPointWaveformLocation, returnPointWaveformLocation => returnPointWaveformLocation.IsZero())
            .Satisfies(p => p.ParametricDx, parametricDx => parametricDx.IsZero())
            .Satisfies(p => p.ParametricDy, parametricDy => parametricDy.IsZero())
            .Satisfies(p => p.ParametricDz, parametricDz => parametricDz.IsZero());
    }
}