// -----------------------------------------------------------------------
// <copyright file="ChunkReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The chunk <see cref="IPointReader"/>.
/// </summary>
/// <param name="rawReader">The raw reader.</param>
/// <param name="header">The header block.</param>
/// <param name="zip">The zip information.</param>
internal class ChunkReader(Readers.IPointDataRecordReader rawReader, in HeaderBlock header, LasZip zip) : PointWiseReader(rawReader, header, zip, default);