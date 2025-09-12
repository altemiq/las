// -----------------------------------------------------------------------
// <copyright file="PointDataRecordReader{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers;

/// <summary>
/// The point data record reader.
/// </summary>
/// <typeparam name="T">The type of point.</typeparam>
/// <param name="pointDataLength">The point data length.</param>
internal abstract class PointDataRecordReader<T>(int pointDataLength) : IPointDataRecordReader
    where T : IBasePointDataRecord
{
    /// <inheritdoc/>
    LasPointSpan IPointDataRecordReader.Read(ReadOnlySpan<byte> source) => new(this.Read(source[..pointDataLength]), source[pointDataLength..]);

    /// <inheritdoc/>
    async ValueTask<LasPointMemory> IPointDataRecordReader.ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken) => new(await this.ReadAsync(source[..pointDataLength], cancellationToken).ConfigureAwait(false), source[pointDataLength..]);

    /// <inheritdoc cref="IPointDataRecordReader.Read"/>
    protected abstract T Read(ReadOnlySpan<byte> source);

    /// <inheritdoc cref="IPointDataRecordReader.ReadAsync"/>
    protected virtual ValueTask<T> ReadAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return new(this.Read(source.Span));
    }
}