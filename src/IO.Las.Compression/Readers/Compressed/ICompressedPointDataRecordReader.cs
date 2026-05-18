// -----------------------------------------------------------------------
// <copyright file="ICompressedPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed point data record reader.
/// </summary>
public interface ICompressedPointDataRecordReader : IPointDataRecordReader
{
    /// <summary>
    /// Reads the point data record data.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <returns>The number of bytes read.</returns>
    int Read(Span<byte> destination);

    /// <summary>
    /// Reads the point data record data.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of bytes read.</returns>
    ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default);
}