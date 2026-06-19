using System.Runtime.CompilerServices;

namespace Altemiq.IO.Las.Compression.Arrow.Data;

internal sealed class MockLazReader : ILasReader, ILazReader
{
    private readonly MockPointDataRecordReader reader = new();
    private readonly MockChunkedReader chunkedReader;

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
        this.Header = header;
        this.chunkedReader = new(this.reader, in header, 50000);
    }

    public HeaderBlock Header { get; }
    public IReadOnlyList<VariableLengthRecord> VariableLengthRecords { get; } = [];

#if LAS1_4_OR_GREATER
    public IReadOnlyList<ExtendedVariableLengthRecord> ExtendedVariableLengthRecords { get; } = [];
#endif

    public bool IsCompressed => true;

    public bool IsChunked => true;

    public ushort PointDataLength => GpsPointDataRecord.Size;

    public int ReadPointDataRecordData(Span<byte> buffer) => this.reader.Read(buffer);

    public ValueTask<int> ReadPointDataRecordDataAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => this.reader.ReadAsync(buffer, cancellationToken);

    public LasPointSpan ReadPointDataRecord() => this.reader.Read([]);

    public LasPointSpan ReadPointDataRecord(ulong index) => throw new NotImplementedException();

    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(CancellationToken cancellationToken = default) => this.reader.ReadAsync(ReadOnlyMemory<byte>.Empty, cancellationToken);

    public ValueTask<LasPointMemory> ReadPointDataRecordAsync(ulong index, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk() => new(this.chunkedReader, this);

    public ChunkedReader.ChunkedLasPointSpanEnumerable ReadChunk(int chunk) => throw new NotImplementedException();

    public ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync() => throw new NotImplementedException();

    public ChunkedReader.ChunkedLasPointMemoryEnumerable ReadChunkAsync(int chunk) => throw new NotImplementedException();

    public bool MoveToChunk(int index) => throw new NotImplementedException();

    public bool MoveToChunk(long chunkStart) => throw new NotImplementedException();

    public ValueTask<bool> MoveToChunkAsync(int index, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public ValueTask<bool> MoveToChunkAsync(long chunkStart, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    private sealed class MockChunkedReader : ChunkedReader
    {
        public MockChunkedReader(Readers.Compressed.ICompressedPointDataRecordReader reader, in HeaderBlock headerBlock, uint chunkSize) : base(
            new MockChunkReader(
                reader,
                headerBlock,
#if LAS1_4_OR_GREATER
                new(GpsPointDataRecord.Id, 0, Compressor.PointWiseChunked), GpsPointDataRecord.Size),
#else
                new(GpsPointDataRecord.Id, Compressor.PointWiseChunked), GpsPointDataRecord.Size),
#endif
                chunkSize)
        {

#if NET8_0_OR_GREATER
            ref uint chunkCount = ref ChunkCountField(this);
            chunkCount = 0;
#else
            SetChunkCountField(this, 0U);
#endif
        }

#if NET8_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "chunkCount")]
        private static extern ref uint ChunkCountField(ChunkedReader reader);
#else
        private static void SetChunkCountField(ChunkedReader reader, uint chunkCount) =>
            typeof(ChunkedReader)
                .GetField("chunkCount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(reader, chunkCount);
#endif
    }

    private sealed class MockChunkReader : ChunkReader
    {
        public MockChunkReader(Readers.Compressed.ICompressedPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip, int pointDataLength)
            : base(rawReader, in header, zip, pointDataLength)
        {
#if NET8_0_OR_GREATER
            ref Readers.Compressed.ICompressedPointDataRecordReader reader = ref ReaderField(this);
            reader = rawReader;
#else
            SetReaderField(this, rawReader);
#endif
        }

#if NET8_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "reader")]
        private static extern ref Readers.Compressed.ICompressedPointDataRecordReader ReaderField(PointWiseReader reader);
#else
        private static void SetReaderField(PointWiseReader reader, Readers.Compressed.ICompressedPointDataRecordReader rawReader) =>
            typeof(PointWiseReader)
                .GetField("reader", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(reader, rawReader);
#endif
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
            this.count++;
            return this.count switch
            {
                1 => new(First, []),
                2 => new(Second, []),
                _ => default,
            };
        }

        public ValueTask<LasPointMemory> ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
        {
            this.count++;
            return this.count switch
            {
                1 => new(new LasPointMemory(First, ReadOnlyMemory<byte>.Empty)),
                2 => new(new LasPointMemory(Second, ReadOnlyMemory<byte>.Empty)),
                _ => default,
            };
        }

        public int Read(Span<byte> buffer)
        {
            First.CopyTo(buffer);
            Second.CopyTo(buffer[GpsPointDataRecord.Size..]);
            return GpsPointDataRecord.Size * 2;
        }

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            First.CopyTo(buffer.Span);
            Second.CopyTo(buffer[GpsPointDataRecord.Size..].Span);
            return new(GpsPointDataRecord.Size * 2);
        }
    }
}