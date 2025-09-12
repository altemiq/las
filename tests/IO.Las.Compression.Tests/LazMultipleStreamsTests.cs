namespace Altemiq.IO.Las.Compression;

public class LazMultipleStreamsTests
{
    [Test]
    public async Task Sorted()
    {
        SortedDictionary<string, Stream> dictionary = new(LazStreams.Comparer);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using BasicLazMultipleMemoryStreams stream = new(dictionary);
#if LAS1_3_OR_GREATER
        stream.SwitchTo(LasStreams.ExtendedVariableLengthRecord);
#endif
        stream.SwitchTo(LasStreams.VariableLengthRecord);
        stream.SwitchTo(LasStreams.PointData);
#if LAS1_3_OR_GREATER
        stream.SwitchTo(LazStreams.SpecialExtendedVariableLengthRecord);
#endif
        stream.SwitchTo(LasStreams.Header);
        stream.SwitchTo(LazStreams.ChunkTable);
        stream.SwitchTo(LazStreams.FormatChunk(10));
        stream.SwitchTo(LazStreams.FormatChunk(1));
        stream.SwitchTo(LazStreams.FormatChunk(5));
        stream.SwitchTo(LazStreams.ChunkTablePosition);

        await Assert.That(dictionary.Keys).IsEquivalentTo(
            [
                LasStreams.Header,
                LasStreams.VariableLengthRecord,
                LasStreams.PointData,
                LazStreams.ChunkTablePosition,
                LazStreams.FormatChunk(1),
                LazStreams.FormatChunk(5),
                LazStreams.FormatChunk(10),
                LazStreams.ChunkTable,
#if LAS1_3_OR_GREATER
                LasStreams.ExtendedVariableLengthRecord,
                LazStreams.SpecialExtendedVariableLengthRecord,
#endif
            ]);
    }

    [Test]
    public async Task AddSameTwice()
    {
        SortedDictionary<string, Stream> dictionary = new(LazStreams.Comparer);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        await
#endif
        using BasicLazMultipleMemoryStreams stream = new(dictionary);
        _ = stream.SwitchTo(LasStreams.Header);
        _ = stream.SwitchTo(LasStreams.VariableLengthRecord);
        _ = stream.SwitchTo(LazStreams.FormatChunk(0));
        _ = stream.SwitchTo(LazStreams.FormatChunk(1));
        _ = stream.SwitchTo(LazStreams.FormatChunk(2));
        _ = stream.SwitchTo(LazStreams.FormatChunk(0));
        _ = stream.SwitchTo(LazStreams.FormatChunk(1));
        _ = stream.SwitchTo(LazStreams.FormatChunk(2));
        _ = stream.SwitchTo(LasStreams.Header);
        _ = stream.SwitchTo(LazStreams.FormatChunk(0));
        _ = stream.SwitchTo(LazStreams.FormatChunk(1));
        _ = stream.SwitchTo(LazStreams.FormatChunk(2));
        _ = stream.SwitchTo(LasStreams.Header);
        _ = stream.SwitchTo(LasStreams.VariableLengthRecord);

        _ = await Assert.That(dictionary.Keys).IsEquivalentTo(
            [
                LasStreams.Header,
                LasStreams.VariableLengthRecord,
                LazStreams.FormatChunk(0),
                LazStreams.FormatChunk(1),
                LazStreams.FormatChunk(2),
            ]);
    }

    private class BasicLazMultipleMemoryStreams(IDictionary<string, Stream> dictionary) : LazMultipleMemoryStream(dictionary);
}