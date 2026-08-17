namespace Altemiq.IO.Las.Compression.Arrow.Data;

internal sealed class MockLazReader : ILasReader, ILazReader
{
    private readonly MockPointDataRecordReader reader = new();
    private readonly ChunkedReader chunkedReader;

    public MockLazReader()
    {
        var builder = new HeaderBlockBuilder(GpsPointDataRecord.Id)
        {
#if LAS1_4_OR_GREATER
            LegacyNumberOfPointRecords = 2,
#endif
            NumberOfPointRecords = 2,
        };
        var header = builder.HeaderBlock;
        Header = header;
        chunkedReader = ChunkedReader.CreateMock(reader, in header, 50000);
    }

    public HeaderBlock Header { get; }
    public IReadOnlyList<VariableLengthRecord> VariableLengthRecords { get; } = [];

#if LAS1_4_OR_GREATER
    public IReadOnlyList<ExtendedVariableLengthRecord> ExtendedVariableLengthRecords { get; } = [];
#endif

    public bool IsCompressed => true;

    public bool IsChunked => true;

    public ushort PointDataLength => GpsPointDataRecord.Size;

    public int ReadPointDataRecordData(Span<byte> buffer)
    {
        return reader.Read(buffer);
    }

    public ValueTask<int> ReadPointDataRecordDataAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return reader.ReadAsync(buffer, cancellationToken);
    }

    public LasPointSpan ReadPointDataRecord()
    {
        return reader.Read([]);
    }

    public LasPointSpan ReadPointDataRecord(ulong index)
    {
        throw new NotSupportedException();
    }

    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(CancellationToken cancellationToken = default)
    {
        return reader.ReadAsync(ReadOnlyMemory<byte>.Empty, cancellationToken);
    }

    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(ulong index, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk()
    {
        return new(chunkedReader, this);
    }

    public ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk(int chunk)
    {
        throw new NotSupportedException();
    }

    public ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync()
    {
        throw new NotSupportedException();
    }

    public ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync(int chunk)
    {
        throw new NotSupportedException();
    }

    public bool MoveToChunk(int index)
    {
        throw new NotSupportedException();
    }

    public bool MoveToChunk(long chunkStart)
    {
        throw new NotSupportedException();
    }

    public ValueTask<bool> MoveToChunkAsync(int index, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public ValueTask<bool> MoveToChunkAsync(long chunkStart, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }


    private sealed class MockPointDataRecordReader : Readers.Compressed.ICompressedPointDataRecordReader
    {
        private int count;

        private static readonly GpsPointDataRecord First = new()
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

        private static readonly GpsPointDataRecord Second = new()
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

        public LasPointSpan Read(ReadOnlySpan<byte> source)
        {
            count++;
            return count switch
            {
                1 => new(First, []),
                2 => new(Second, []),
                _ => default,
            };
        }

        public ValueTask<LasPointMemory> ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        {
            count++;
            return count switch
            {
                1 => new(new LasPointMemory(First, ReadOnlyMemory<byte>.Empty)),
                2 => new(new LasPointMemory(Second, ReadOnlyMemory<byte>.Empty)),
                _ => default,
            };
        }

        public int Read(Span<byte> buffer)
        {
            _ = First.CopyTo(buffer);
            _ = Second.CopyTo(buffer[GpsPointDataRecord.Size..]);
            return GpsPointDataRecord.Size * 2;
        }

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _ = First.CopyTo(buffer.Span);
            _ = Second.CopyTo(buffer[GpsPointDataRecord.Size..].Span);
            return new(GpsPointDataRecord.Size * 2);
        }
    }

}