// -----------------------------------------------------------------------
// <copyright file="PointWiseChunkReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.PointWiseChunked"/> chunk reader.
/// </summary>
internal sealed class PointWiseChunkReader : ChunkReader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointWiseChunkReader"/> class.
    /// </summary>
    /// <inheritdoc cref="ChunkReader"/>
    public PointWiseChunkReader(Readers.IPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip)
        : base(rawReader, header, zip)
    {
        if (zip.Compressor is Compressor.PointWise or Compressor.None)
        {
            throw new NotSupportedException();
        }
    }
}