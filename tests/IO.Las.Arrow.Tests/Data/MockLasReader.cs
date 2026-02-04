namespace Altemiq.IO.Las.Arrow.Data;

internal sealed class MockLasReader : ILasReader
{
    private int count;

    public static readonly GpsPointDataRecord First = new()
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
        Withheld = true,
        ScanAngleRank = -29,
        UserData = 0,
        PointSourceId = 0,
        GpsTime = 492249.15875399113,
    };

    public static readonly GpsPointDataRecord Second = new()
    {
        X = 2758,
        Y = -1378,
        Z = -6718,
        Intensity = 428,
        ReturnNumber = 1,
        NumberOfReturns = 1,
        ScanDirectionFlag = false,
        EdgeOfFlightLine = true,
        Classification = Classification.LowVegetation,
        Synthetic = false,
        KeyPoint = false,
        Withheld = false,
        ScanAngleRank = 29,
        UserData = 1,
        PointSourceId = 0,
        GpsTime = 492249.15875399213,
    };

    public MockLasReader()
    {
        var builder = new HeaderBlockBuilder(GpsPointDataRecord.Id)
        {
            LegacyNumberOfPointRecords = 2,
            NumberOfPointRecords = 2,
        };
        this.Header = builder.HeaderBlock;
    }

    public HeaderBlock Header { get; }
    public IReadOnlyList<VariableLengthRecord> VariableLengthRecords { get; } = [];
    public IReadOnlyList<ExtendedVariableLengthRecord> ExtendedVariableLengthRecords { get; } = [];

    public LasPointSpan ReadPointDataRecord()
    {
        this.count++;
        return this.count switch
        {
            1 => new(First, []),
            2 => new(Second, []),
            _ => default,
        };
    }

    public LasPointSpan ReadPointDataRecord(ulong index)
    {
        switch (index)
        {
            case 0:
                this.count = 1;
                return new(First, []);
            case 1:
                this.count = 2;
                return new(Second, []);
            default:
                return default;
        }
    }

    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(ulong index, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}