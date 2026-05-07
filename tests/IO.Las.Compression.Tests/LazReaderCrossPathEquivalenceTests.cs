// -----------------------------------------------------------------------
// <copyright file="LazReaderCrossPathEquivalenceTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// Tests asserting that the parsed <c>ReadPointDataRecord()</c> path and the
/// raw-bytes <c>ReadPointDataRecordData(Span&lt;byte&gt;)</c> path produce identical
/// parsed records for the same input. These guard the upcoming refactor that
/// eliminates the intermediate double-copy in the raw-bytes path: if the refactor
/// diverges the two paths, these tests will catch it.
/// </summary>
public class LazReaderCrossPathEquivalenceTests
{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [Test]
    [Arguments("fusa.laz")]
#if LAS1_2_OR_GREATER
    [Arguments("coloured.laz")]
#endif
#if LAS1_4_OR_GREATER
    [Arguments("point_7.laz")]
    [Arguments("fusa_height_7.laz")]
    [Arguments("fusa_height.laz")]
#endif
    public async Task ParsedAndSpanPathsAgree(string resource)
    {
        const int PointsToCompare = 64;

        // Path 1: parsed record via ReadPointDataRecord()
        await using var streamA = GetResource(resource);
        await using var readerA = new LazReader(streamA);
        var parsedRecords = new IBasePointDataRecord[PointsToCompare];
        var parsedExtras = new byte[PointsToCompare][];
        for (var i = 0; i < PointsToCompare; i++)
        {
            var (rec, extra) = readerA.ReadPointDataRecord();
            parsedRecords[i] = rec;
            parsedExtras[i] = extra.ToArray();
        }

        // Path 2: raw bytes via ReadPointDataRecordData(Span<byte>), then parse each point.
        await using var streamB = GetResource(resource);
        await using var readerB = new LazReader(streamB);
        var pointLength = readerB.PointDataLength;
        var buffer = new byte[pointLength * PointsToCompare];
        var read = readerB.ReadPointDataRecordData(buffer);
        await Assert.That(read).IsEqualTo(PointsToCompare);

        for (var i = 0; i < PointsToCompare; i++)
        {
            var (rec, extra) = readerB.Read(buffer.AsSpan(i * pointLength, pointLength));
            var extraArray = extra.ToArray();

            await Assert.That(rec).IsEqualTo(parsedRecords[i])
                .Because($"[{resource}] point {i}: parsed record from span path must match parsed record from ReadPointDataRecord path");

            await Assert.That(extraArray).IsEquivalentTo(parsedExtras[i])
                .Because($"[{resource}] point {i}: extra bytes from span path must match extra bytes from ReadPointDataRecord path");
        }
    }

    [Test]
    [Arguments("fusa.laz")]
#if LAS1_2_OR_GREATER
    [Arguments("coloured.laz")]
#endif
#if LAS1_4_OR_GREATER
    [Arguments("point_7.laz")]
    [Arguments("fusa_height_7.laz")]
#endif
    public async Task SpanPathProducesStableBytesAcrossReaders(string resource)
    {
        // Read first 64 points via span path, twice, using two independent readers.
        // Both must produce byte-identical output. This catches any per-call state bleed.
        const int PointsToCompare = 64;

        await using var streamA = GetResource(resource);
        await using var readerA = new LazReader(streamA);
        var pointLength = readerA.PointDataLength;

        var bufferA = new byte[pointLength * PointsToCompare];
        var readA = readerA.ReadPointDataRecordData(bufferA);
        await Assert.That(readA).IsEqualTo(PointsToCompare);

        await using var streamB = GetResource(resource);
        await using var readerB = new LazReader(streamB);

        var bufferB = new byte[pointLength * PointsToCompare];
        var readB = readerB.ReadPointDataRecordData(bufferB);
        await Assert.That(readB).IsEqualTo(PointsToCompare);

        await Assert.That(bufferA).IsEquivalentTo(bufferB);
    }
#endif

    private static Stream GetResource(string resource) =>
        typeof(LazReaderCrossPathEquivalenceTests).Assembly.GetManifestResourceStream(typeof(LazReaderTests), resource)
            ?? throw new System.Diagnostics.UnreachableException($"Failed to get resource '{resource}'");
}
