namespace Altemiq.IO.Las.Compression;

public sealed class ExtendedVariableLengthRecordComparer : IEqualityComparer<ExtendedVariableLengthRecord>
{
    public static readonly ExtendedVariableLengthRecordComparer Instance = new();

    public bool Equals(ExtendedVariableLengthRecord x, ExtendedVariableLengthRecord y)
    {
        return (x, y) switch
        {
            (null, not null) or (not null, null) => false,
            (null, null) => true,
            _ => ExtendedVariableLengthRecordHeaderComparer.Instance.Equals(x.Header, y.Header),
        };
    }

    public int GetHashCode(ExtendedVariableLengthRecord obj)
    {
        return obj.GetHashCode();
    }
}

public sealed class ExtendedVariableLengthRecordHeaderComparer : IEqualityComparer<ExtendedVariableLengthRecordHeader>
{
    public static readonly ExtendedVariableLengthRecordHeaderComparer Instance = new();

    public bool Equals(ExtendedVariableLengthRecordHeader x, ExtendedVariableLengthRecordHeader y)
    {
        return x.UserId.Equals(y.UserId) && x.Description.Equals(y.Description) && x.RecordId == y.RecordId;
    }

    public int GetHashCode(ExtendedVariableLengthRecordHeader obj)
    {
        return obj.GetHashCode();
    }
}