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