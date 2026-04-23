// -----------------------------------------------------------------------
// <copyright file="ILasReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a LAS file reader.
/// </summary>
public interface ILasReader
{
    /// <summary>
    /// Gets the header.
    /// </summary>
    HeaderBlock Header { get; }

    /// <summary>
    /// Gets the variable length records.
    /// </summary>
    IReadOnlyList<VariableLengthRecord> VariableLengthRecords { get; }

#if LAS1_4_OR_GREATER
    /// <summary>
    /// Gets the extended variable length records.
    /// </summary>
    IReadOnlyList<ExtendedVariableLengthRecord> ExtendedVariableLengthRecords { get; }
#endif

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    /// <summary>
    /// Gets the point data length.
    /// </summary>
    ushort PointDataLength { get; }

    /// <summary>
    /// Reads the point data record data.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <returns>The number of points read.</returns>
    int ReadPointDataRecordData(Span<byte> buffer);

    /// <summary>
    /// Reads the point data record data asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to read into.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of points read.</returns>
    ValueTask<int> ReadPointDataRecordDataAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
#endif

    /// <summary>
    /// Reads the point data record.
    /// </summary>
    /// <returns>The next <see cref="LasPointSpan"/>.</returns>
    LasPointSpan ReadPointDataRecord();

    /// <summary>
    /// Reads the point data record at the specified index.
    /// </summary>
    /// <param name="index">The point data record index.</param>
    /// <returns>The <see cref="LasPointSpan"/> at <paramref name="index"/>.</returns>
    /// <exception cref="KeyNotFoundException">The specified index could not be found.</exception>
    LasPointSpan ReadPointDataRecord(ulong index);

    /// <summary>
    /// Reads the point data record asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The next <see cref="LasPointMemory"/>.</returns>
    ValueTask<LasPointMemory> ReadPointDataRecordAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the point data record asynchronously at the specified index.
    /// </summary>
    /// <param name="index">The point data record index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The <see cref="LasPointMemory"/> at <paramref name="index"/>.</returns>
    /// <exception cref="KeyNotFoundException">The specified index could not be found.</exception>
    ValueTask<LasPointMemory> ReadPointDataRecordAsync(ulong index, CancellationToken cancellationToken = default);
}