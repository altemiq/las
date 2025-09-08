// -----------------------------------------------------------------------
// <copyright file="IPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers;

/// <summary>
/// The <see cref="IBasePointDataRecord"/> reader.
/// </summary>
public interface IPointDataRecordReader
{
    /// <summary>
    /// Reads the point data record.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The point data record.</returns>
    LasPointSpan Read(ReadOnlySpan<byte> source);

    /// <summary>
    /// Reads the point data record asynchronously.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The point data record.</returns>
    ValueTask<LasPointMemory> ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default);
}