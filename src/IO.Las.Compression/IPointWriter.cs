// -----------------------------------------------------------------------
// <copyright file="IPointWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The point writer.
/// </summary>
internal interface IPointWriter
{
    /// <summary>
    /// Closes the point writer.
    /// </summary>
    /// <param name="stream">The stream.</param>
    void Close(Stream stream);

    /// <summary>
    /// Initializes the point writer.
    /// </summary>
    /// <param name="stream">The stream.</param>
    void Initialize(Stream stream);

    /// <summary>
    /// Writes the point.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="record">The point.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    void Write(Stream stream, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes);

    /// <summary>
    /// Writes the point asynchronously.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="record">The point.</param>
    /// <param name="extraBytes">The extra bytes.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    ValueTask WriteAsync(Stream stream, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default);
}