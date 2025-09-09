// -----------------------------------------------------------------------
// <copyright file="FixedLayeredChunkedWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The point-wise chunked point writer.
/// </summary>
/// <inheritdoc cref="ChunkedWriter" />
internal sealed class FixedLayeredChunkedWriter(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip) : ChunkedWriter(rawWriter, pointDataLength, pointDataFormatId, zip, zip.ChunkSize)
{
    /// <inheritdoc/>
    public override IEnumerable<uint> GetChunkTotals() => Accumulate(Enumerable.Range(1, this.TotalChunks));

    /// <inheritdoc/>
    protected override ChunkWriter Create(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip) => new LayeredChunkWriter(rawWriter, pointDataLength, pointDataFormatId, zip);

    private static IEnumerable<uint> Accumulate(IEnumerable<int> source)
    {
        uint start = 0;
        foreach (var value in source.Select(x => (uint)x))
        {
            yield return start + value;
            start += value;
        }
    }
}