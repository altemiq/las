// -----------------------------------------------------------------------
// <copyright file="FixedLayeredChunkedReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Compressor.LayeredChunked"/> <see cref="IPointReader"/>.
/// </summary>
/// <inheritdoc cref="ChunkedReader" />
internal sealed class FixedLayeredChunkedReader(Readers.IPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip, int pointDataLength) : ChunkedReader(new LayeredChunkReader(rawReader, header, zip, pointDataLength), zip.ChunkSize);