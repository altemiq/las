namespace Altemiq.IO.Las;

public class PointConverterTests
{
    private static
#if !NETFRAMEWORK
        readonly
#endif
        PointDataRecord PointDataRecord = new()
    {
        X = Values.X,
        Y = Values.Y,
        Z = Values.Z,
        Intensity = Values.Intensity,
        ReturnNumber = Values.ReturnNumber,
        NumberOfReturns = Values.NumberOfReturns,
        Synthetic = Values.Synthetic,
        KeyPoint = Values.KeyPoint,
        Withheld = Values.Withheld,
        ScanAngleRank = Values.ScanAngleRank,
        ScanDirectionFlag = Values.ScanDirectionFlag,
        EdgeOfFlightLine = Values.EdgeOfFlightLine,
        Classification = Values.Classification,
        UserData = Values.UserData,
        PointSourceId = Values.PointSourceId,
    };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        GpsPointDataRecord GpsPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            ScanAngleRank = Values.ScanAngleRank,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Classification.MediumVegetation,
            UserData = Values.UserData,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        ColorPointDataRecord ColorPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            ScanAngleRank = Values.ScanAngleRank,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Classification.MediumVegetation,
            UserData = Values.UserData,
            PointSourceId = Values.PointSourceId,
            Color = Values.Color,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        GpsColorPointDataRecord GpsColorPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            ScanAngleRank = Values.ScanAngleRank,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.Classification,
            UserData = Values.UserData,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
            Color = Values.Color,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        GpsWaveformPointDataRecord GpsWaveformPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            ScanAngleRank = Values.ScanAngleRank,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.Classification,
            UserData = Values.UserData,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
            WavePacketDescriptorIndex = Values.WavePacketDescriptorIndex,
            ByteOffsetToWaveformData = Values.ByteOffsetToWaveformData,
            WaveformPacketSizeInBytes = Values.WaveformPacketSizeInBytes,
            ReturnPointWaveformLocation = Values.ReturnPointWaveformLocation,
            ParametricDx = Values.ParametricDx,
            ParametricDy = Values.ParametricDy,
            ParametricDz = Values.ParametricDz,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        GpsColorWaveformPointDataRecord GpsColorWaveformPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            ScanAngleRank = Values.ScanAngleRank,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.Classification,
            UserData = Values.UserData,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
            Color = Values.Color,
            WavePacketDescriptorIndex = Values.WavePacketDescriptorIndex,
            ByteOffsetToWaveformData = Values.ByteOffsetToWaveformData,
            WaveformPacketSizeInBytes = Values.WaveformPacketSizeInBytes,
            ReturnPointWaveformLocation = Values.ReturnPointWaveformLocation,
            ParametricDx = Values.ParametricDx,
            ParametricDy = Values.ParametricDy,
            ParametricDz = Values.ParametricDz,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsPointDataRecord ExtendedGpsPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            Overlap = Values.Overlap,
            ScannerChannel = Values.ScannerChannel,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.ExtendedClassification,
            UserData = Values.UserData,
            ScanAngle = Values.ScanAngle,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsColorPointDataRecord ExtendedGpsColorPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            Overlap = Values.Overlap,
            ScannerChannel = Values.ScannerChannel,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.ExtendedClassification,
            UserData = Values.UserData,
            ScanAngle = Values.ScanAngle,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
            Color = Values.Color,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsColorNearInfraredPointDataRecord ExtendedGpsColorNearInfraredPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            Overlap = Values.Overlap,
            ScannerChannel = Values.ScannerChannel,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.ExtendedClassification,
            UserData = Values.UserData,
            ScanAngle = Values.ScanAngle,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
            Color = Values.Color,
            NearInfrared = Values.NearInfrared,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsWaveformPointDataRecord ExtendedGpsWaveformPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            Overlap = Values.Overlap,
            ScannerChannel = Values.ScannerChannel,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.ExtendedClassification,
            UserData = Values.UserData,
            ScanAngle = Values.ScanAngle,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
            WavePacketDescriptorIndex = Values.WavePacketDescriptorIndex,
            ByteOffsetToWaveformData = Values.ByteOffsetToWaveformData,
            WaveformPacketSizeInBytes = Values.WaveformPacketSizeInBytes,
            ReturnPointWaveformLocation = Values.ReturnPointWaveformLocation,
            ParametricDx = Values.ParametricDx,
            ParametricDy = Values.ParametricDy,
            ParametricDz = Values.ParametricDz,
        };
    
    private static
#if !NETFRAMEWORK
        readonly
#endif
        ExtendedGpsColorNearInfraredWaveformPointDataRecord ExtendedGpsColorNearInfraredWaveformPointDataRecord = new()
        {
            X = Values.X,
            Y = Values.Y,
            Z = Values.Z,
            Intensity = Values.Intensity,
            ReturnNumber = Values.ReturnNumber,
            NumberOfReturns = Values.NumberOfReturns,
            Synthetic = Values.Synthetic,
            KeyPoint = Values.KeyPoint,
            Withheld = Values.Withheld,
            Overlap = Values.Overlap,
            ScannerChannel = Values.ScannerChannel,
            ScanDirectionFlag = Values.ScanDirectionFlag,
            EdgeOfFlightLine = Values.EdgeOfFlightLine,
            Classification = Values.ExtendedClassification,
            UserData = Values.UserData,
            ScanAngle = Values.ScanAngle,
            PointSourceId = Values.PointSourceId,
            GpsTime = Values.GpsTime,
            Color = Values.Color,
            NearInfrared = Values.NearInfrared,
            WavePacketDescriptorIndex = Values.WavePacketDescriptorIndex,
            ByteOffsetToWaveformData = Values.ByteOffsetToWaveformData,
            WaveformPacketSizeInBytes = Values.WaveformPacketSizeInBytes,
            ReturnPointWaveformLocation = Values.ReturnPointWaveformLocation,
            ParametricDx = Values.ParametricDx,
            ParametricDy = Values.ParametricDy,
            ParametricDz = Values.ParametricDz,
        };
    
    [Test]
    [MethodDataSource(nameof(ToExtendedData))]
    public async Task ToExtended(IBasePointDataRecord point, IExtendedPointDataRecord extendedPoint)
    {
        await Assert.That(PointConverter.ToExtended(point)).IsEqualTo(extendedPoint);
    }
    
    [Test]
    [MethodDataSource(nameof(ToSimpleData))]
    public async Task ToSimple(IBasePointDataRecord point, IPointDataRecord simplePoint)
    {
        await Assert.That(PointConverter.ToSimple(point)).IsEqualTo(simplePoint);
    }

    public static IEnumerable<Func<(IBasePointDataRecord point, IExtendedPointDataRecord extendedPoint)>> ToExtendedData()
    {
        yield return () => (PointDataRecord, ExtendedGpsPointDataRecord with { GpsTime = default });
        yield return () => (GpsPointDataRecord, ExtendedGpsPointDataRecord);
        yield return () => (ColorPointDataRecord, ExtendedGpsColorPointDataRecord with { GpsTime = default });
        yield return () => (GpsColorPointDataRecord, ExtendedGpsColorPointDataRecord);
        yield return () => (GpsWaveformPointDataRecord, ExtendedGpsWaveformPointDataRecord);
        yield return () => (GpsColorWaveformPointDataRecord, ExtendedGpsColorNearInfraredWaveformPointDataRecord with { NearInfrared = default });
    }
    
    public static IEnumerable<Func<(IBasePointDataRecord point, IPointDataRecord simplePoint)>> ToSimpleData()
    {
        yield return () => (ExtendedGpsPointDataRecord, GpsPointDataRecord);
        yield return () => (ExtendedGpsColorPointDataRecord, GpsColorPointDataRecord);
        yield return () => (ExtendedGpsColorNearInfraredPointDataRecord, GpsColorPointDataRecord);
        yield return () => (ExtendedGpsWaveformPointDataRecord, GpsWaveformPointDataRecord);
        yield return () => (ExtendedGpsColorNearInfraredWaveformPointDataRecord, GpsColorWaveformPointDataRecord);
    }
    
    private static class Values
    {
        public const int X = 5977566;
        public const int Y = 957396;
        public const int Z = -513145;
        public const ushort Intensity = 36022;
        public const byte ReturnNumber = 1;
        public const byte NumberOfReturns = 2;
        public const sbyte ScanAngleRank = 77;
        public const short ScanAngle = 12833;
        public const Classification Classification = Altemiq.IO.Las.Classification.MediumVegetation;
        public const ExtendedClassification ExtendedClassification = Altemiq.IO.Las.ExtendedClassification.MediumVegetation;
        public const bool Synthetic = false;
        public const bool KeyPoint = false;
        public const bool Withheld = false;
        public const bool ScanDirectionFlag = false;
        public const bool EdgeOfFlightLine = false;
        public const bool Overlap = false;
        public const byte UserData = 0;
        public const ushort PointSourceId = 0;
        public const double GpsTime = 181652401.20220396;
        public const byte ScannerChannel = 0;
        public const ushort NearInfrared = 16805;
        public const int WavePacketDescriptorIndex = 1;
        public const ulong ByteOffsetToWaveformData = 210883772UL;
        public const uint WaveformPacketSizeInBytes = 64U;
        public const float ReturnPointWaveformLocation = 15969F;
        public const float ParametricDx = -0.0000025741017F;
        public const float ParametricDy = 0.00000259652484F;
        public const float ParametricDz = 0.000149812418F;
        public static readonly Color Color = Color.FromRgb(25191, 42903, 16805);
    }
}