// -----------------------------------------------------------------------
// <copyright file="PointWiseChunkWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.PointWiseChunked"/> <see cref="IPointWriter"/>.
/// </summary>
internal sealed class PointWiseChunkWriter : ChunkWriter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointWiseChunkWriter"/> class.
    /// </summary>
    /// <inheritdoc cref="PointWiseWriter" />
    public PointWiseChunkWriter(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip)
        : base(rawWriter, pointDataLength, pointDataFormatId, zip)
    {
        if (zip.Compressor is Compressor.PointWise or Compressor.None)
        {
            throw new NotSupportedException();
        }
    }

    /// <inheritdoc/>
    public override void Close(Stream stream) => this.Encoder.Done();
}