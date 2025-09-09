// -----------------------------------------------------------------------
// <copyright file="LayeredChunkWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.LayeredChunked"/> <see cref="IPointWriter"/>.
/// </summary>
internal sealed class LayeredChunkWriter : ChunkWriter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredChunkWriter"/> class.
    /// </summary>
    /// <inheritdoc cref="PointWiseWriter" />
    public LayeredChunkWriter(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip)
        : base(rawWriter, pointDataLength, pointDataFormatId, zip)
    {
        if (zip.Compressor is not Compressor.LayeredChunked)
        {
            throw new NotSupportedException();
        }
    }

    /// <inheritdoc/>
    public override void Close(Stream stream)
    {
        // write how many points are in the chunk
        stream.WriteUInt32LittleEndian(this.Count);

        // write all layers
        if (this.Writer is Writers.Compressed.IContext context)
        {
            _ = context.ChunkSizes();
            _ = context.ChunkBytes();
        }

        base.Close(stream);
    }
}