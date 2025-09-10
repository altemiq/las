namespace Altemiq.IO.Las;

public class LasMultipleStreamsTests
{
    [Test]
    public async Task Sorted()
    {
        SortedDictionary<string, Stream> dictionary = new(LasStreams.Comparer);
        await using BasicLasMultipleMemoryStreams stream = new(dictionary);
#if LAS1_3_OR_GREATER
        stream.SwitchTo(LasStreams.ExtendedVariableLengthRecord);
#endif
        stream.SwitchTo(LasStreams.VariableLengthRecord);
        stream.SwitchTo(LasStreams.PointData);
        stream.SwitchTo(LasStreams.Header);

        await Assert.That(dictionary.Keys).IsEquivalentTo(
        [
            LasStreams.Header,
            LasStreams.VariableLengthRecord,
            LasStreams.PointData,
#if LAS1_3_OR_GREATER
            LasStreams.ExtendedVariableLengthRecord,
#endif
        ]);
    }

    [Test]
    public async Task AddSameTwice()
    {
        SortedDictionary<string, Stream> dictionary = new(LasStreams.Comparer);
        await using BasicLasMultipleMemoryStreams stream = new(dictionary);
        _ = stream.SwitchTo(LasStreams.Header);
        _ = stream.SwitchTo(LasStreams.VariableLengthRecord);
        _ = stream.SwitchTo(LasStreams.PointData);
        _ = stream.SwitchTo(LasStreams.Header);
        _ = stream.SwitchTo(LasStreams.PointData);
        _ = stream.SwitchTo(LasStreams.Header);
        _ = stream.SwitchTo(LasStreams.VariableLengthRecord);
        _ = stream.SwitchTo(LasStreams.PointData);

        _ = await Assert.That(dictionary.Keys).IsEquivalentTo(
        [
            LasStreams.Header,
            LasStreams.VariableLengthRecord,
            LasStreams.PointData,
        ]);
    }

    private class BasicLasMultipleMemoryStreams(IDictionary<string, Stream> dictionary) : LasMultipleMemoryStream(dictionary);
    
}