// -----------------------------------------------------------------------
// <copyright file="VariableLayeredChunkedReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.LayeredChunked"/> <see cref="IPointReader"/>.
/// </summary>
/// <inheritdoc cref="ChunkedReader" />
internal sealed class VariableLayeredChunkedReader(Readers.IPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip, int pointDataLength) : ChunkedReader(new LayeredChunkReader(rawReader, header, zip, pointDataLength), LasZip.VariableChunkSize)
{
    private uint[] chunkTotals = [];

    /// <inheritdoc/>
    protected override ulong GetFirstPoint(int index) => this.chunkTotals[index];

    /// <inheritdoc/>
    protected override void ReadChunkTable(Stream stream, long chunksStart, ref long[] chunkStarts, ref uint numberChunks, ref uint tabledChunks) => ReadChunkTable(
        stream,
        chunksStart,
        this.Reader.Decoder,
        ref chunkStarts,
        ref numberChunks,
        ref tabledChunks,
        out this.chunkTotals);

    /// <inheritdoc/>
    protected override uint GetChunkSize(uint chunk) => this.chunkTotals[chunk + 1] - this.chunkTotals[chunk];

    /// <inheritdoc/>
    protected override (uint Chunk, uint Delta) GetDelta(ulong target)
    {
        var targetChunk = SearchChunkTable((uint)target, 0, (uint)(this.chunkTotals.Length - 1));
        var delta = (uint)(target - this.chunkTotals[targetChunk]);
        return (targetChunk, delta);

        uint SearchChunkTable(uint index, uint lower, uint upper)
        {
            if (lower + 1 == upper)
            {
                return lower;
            }

            var mid = (lower + upper) / 2;
            return index >= this.chunkTotals[mid]
                ? SearchChunkTable(index, mid, upper)
                : SearchChunkTable(index, lower, mid);
        }
    }
}