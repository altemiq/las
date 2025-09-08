// -----------------------------------------------------------------------
// <copyright file="PointWiseChunkedReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.PointWiseChunked"/> <see cref="IPointReader"/>.
/// </summary>
/// <inheritdoc cref="ChunkedReader" />
internal sealed class PointWiseChunkedReader(Readers.IPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip) : ChunkedReader(new PointWiseChunkReader(rawReader, header, zip), zip.ChunkSize);