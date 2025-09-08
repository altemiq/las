// -----------------------------------------------------------------------
// <copyright file="IPointReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The point reader.
/// </summary>
internal interface IPointReader
{
    /// <summary>
    /// Closes the point reader.
    /// </summary>
    /// <param name="stream">The stream.</param>
    void Close(Stream stream);

    /// <summary>
    /// Initializes the point reader.
    /// </summary>
    /// <param name="stream">The stream.</param>
    void Initialize(Stream stream);

    /// <summary>
    /// Reads the point.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <returns>The point.</returns>
    LasPointSpan Read(Stream stream, int pointDataLength);

    /// <summary>
    /// Reads the point asynchronously.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The point.</returns>
    ValueTask<LasPointMemory> ReadAsync(Stream stream, int pointDataLength, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves to the specified target point.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <param name="current">The current point index.</param>
    /// <param name="target">The target point index.</param>
    /// <returns><see langword="true"/> if the reader is now at <paramref name="target"/>; otherwise <see langword="false"/>.</returns>
    bool MoveToPoint(Stream stream, int pointDataLength, ulong current, ulong target);

    /// <summary>
    /// Moves to the specified target point asynchronously.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="pointDataLength">The point data length.</param>
    /// <param name="current">The current point index.</param>
    /// <param name="target">The target point index.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the reader is now at <paramref name="target"/>; otherwise <see langword="false"/>.</returns>
    ValueTask<bool> MoveToPointAsync(Stream stream, int pointDataLength, ulong current, ulong target, CancellationToken cancellationToken = default);
}