// -----------------------------------------------------------------------
// <copyright file="VariableLayeredChunkedWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The layered chunked point writer.
/// </summary>
/// <inheritdoc cref="ChunkedWriter" />
internal sealed class VariableLayeredChunkedWriter(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip) : ChunkedWriter(rawWriter, pointDataLength, pointDataFormatId, zip, LasZip.VariableChunkSize)
{
    private readonly List<uint> chunkTotals = [];

    /// <inheritdoc/>
    public override IEnumerable<uint> GetChunkTotals()
    {
        foreach (var chunkTotal in this.chunkTotals)
        {
            yield return chunkTotal;
        }

        // do any of the other ones
        foreach (var writer in this.GetWriters())
        {
            yield return writer.Count;
        }
    }

    /// <summary>
    /// Writes the points as a chunk.
    /// </summary>
    /// <param name="stream">The writer.</param>
    /// <param name="points">The points to write.</param>
    /// <param name="count">The number of points.</param>
    /// <exception cref="InvalidOperationException">Chunk is already being written.</exception>
    /// <exception cref="ArgumentException"><paramref name="points"/> does not have <paramref name="count"/> items.</exception>
    public void Write(Stream stream, IEnumerable<LasPointMemory> points, int count)
    {
        if (this.GetWriter(DefaultChunkKey, stream) is not { Count: 0 } chunkWriter)
        {
            ThrowAlreadyInChunkException();
            return;
        }

        using var enumerator = points.GetEnumerator();
        for (var i = 0; i < count; i++)
        {
            if (!enumerator.MoveNext())
            {
                ThrowLessPointsThanCount(nameof(points), count, i);
            }

            var current = enumerator.Current;
            this.Write(stream, current.PointDataRecord!, current.ExtraBytes.Span);
        }

        this.FinalizeChunk(stream, chunkWriter);
    }

    /// <summary>
    /// Writes the points as a chunk.
    /// </summary>
    /// <param name="stream">The writer.</param>
    /// <param name="points">The points to write.</param>
    /// <exception cref="InvalidOperationException">Chunk is already being written.</exception>
    public void Write(Stream stream, IEnumerable<LasPointMemory> points)
    {
        if (this.GetWriter(DefaultChunkKey, stream) is not { Count: 0 } chunkWriter)
        {
            ThrowAlreadyInChunkException();
            return;
        }

        foreach (var point in points)
        {
            this.Write(stream, point.PointDataRecord!, point.ExtraBytes.Span);
        }

        this.FinalizeChunk(stream, chunkWriter);
    }

    /// <summary>
    /// Writes the points as a chunk asynchronously.
    /// </summary>
    /// <param name="stream">The writer.</param>
    /// <param name="points">The points to write.</param>
    /// <param name="count">The number of points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">Chunk is already being written.</exception>
    /// <exception cref="ArgumentException"><paramref name="points"/> does not have <paramref name="count"/> items.</exception>
    public ValueTask WriteAsync(Stream stream, IEnumerable<LasPointMemory> points, int count, CancellationToken cancellationToken = default)
    {
        if (this.GetWriter(DefaultChunkKey, stream) is { Count: 0 } chunkWriter)
        {
            return WriteAsyncCore(stream, points, count, cancellationToken);
        }

        ThrowAlreadyInChunkException();
        return default;

        async ValueTask WriteAsyncCore(Stream outputStream, IEnumerable<LasPointMemory> pointsToWrite, int pointCount, CancellationToken token)
        {
            using var enumerator = pointsToWrite.GetEnumerator();
            for (var i = 0; i < pointCount; i++)
            {
                if (!enumerator.MoveNext())
                {
                    ThrowLessPointsThanCount(nameof(pointsToWrite), pointCount, i);
                }

                var current = enumerator.Current;
                await this.WriteAsync(outputStream, current.PointDataRecord!, current.ExtraBytes, token).ConfigureAwait(false);
            }

            this.FinalizeChunk(outputStream, chunkWriter);
        }
    }

    /// <summary>
    /// Writes the points as a chunk asynchronously.
    /// </summary>
    /// <param name="stream">The writer.</param>
    /// <param name="points">The points to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">Chunk is already being written.</exception>
    public ValueTask WriteAsync(Stream stream, IEnumerable<LasPointMemory> points, CancellationToken cancellationToken = default)
    {
        if (this.GetWriter(DefaultChunkKey, stream) is { Count: 0 } chunkWriter)
        {
            return WriteAsyncCore(stream, points, cancellationToken);
        }

        ThrowAlreadyInChunkException();
        return default;

        async ValueTask WriteAsyncCore(Stream outputStream, IEnumerable<LasPointMemory> pointsToWrite, CancellationToken token)
        {
            foreach (var point in pointsToWrite)
            {
                await this.WriteAsync(outputStream, point.PointDataRecord!, point.ExtraBytes, token).ConfigureAwait(false);
            }

            this.FinalizeChunk(outputStream, chunkWriter);
        }
    }

#if NETSTANDARD2_0_OR_GREATER || NET461_OR_GREATER || NETCOREAPP3_0_OR_GREATER
    /// <summary>
    /// Writes the points as a chunk asynchronously.
    /// </summary>
    /// <param name="stream">The writer.</param>
    /// <param name="points">The points to write.</param>
    /// <param name="count">The number of points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">Chunk is already being written.</exception>
    /// <exception cref="ArgumentException"><paramref name="points"/> does not have <paramref name="count"/> items.</exception>
    public ValueTask WriteAsync(Stream stream, IAsyncEnumerable<LasPointMemory> points, int count, CancellationToken cancellationToken = default)
    {
        if (this.GetWriter(DefaultChunkKey, stream) is { Count: 0 } chunkWriter)
        {
            return WriteAsyncCore(stream, points, count, cancellationToken);
        }

        ThrowAlreadyInChunkException();
        return default;

        async ValueTask WriteAsyncCore(Stream outputStream, IAsyncEnumerable<LasPointMemory> pointsToWrite, int pointCount, CancellationToken token)
        {
            var enumerator = pointsToWrite.GetAsyncEnumerator(token);
            for (var i = 0; i < pointCount; i++)
            {
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    ThrowLessPointsThanCount(nameof(pointsToWrite), pointCount, i);
                }

                var current = enumerator.Current;
                await this.WriteAsync(outputStream, current.PointDataRecord!, current.ExtraBytes, token).ConfigureAwait(false);
            }

            this.FinalizeChunk(outputStream, chunkWriter);
        }
    }

    /// <summary>
    /// Writes the points as a chunk asynchronously.
    /// </summary>
    /// <param name="stream">The writer.</param>
    /// <param name="points">The points to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous task.</returns>
    /// <exception cref="InvalidOperationException">Chunk is already being written.</exception>
    public ValueTask WriteAsync(Stream stream, IAsyncEnumerable<LasPointMemory> points, CancellationToken cancellationToken = default)
    {
        return this.GetWriter(DefaultChunkKey, stream) is { Count: 0 } chunkWriter
            ? WriteAsyncCore(stream, points, cancellationToken)
            : throw new InvalidOperationException(Compression.Properties.Resources.CannotWriteChunkWhenAlreadyWritingAChunk);

        async ValueTask WriteAsyncCore(Stream outputStream, IAsyncEnumerable<LasPointMemory> pointsToWrite, CancellationToken token)
        {
            await foreach (var point in pointsToWrite.WithCancellation(token).ConfigureAwait(false))
            {
                await this.WriteAsync(outputStream, point.PointDataRecord!, point.ExtraBytes, token).ConfigureAwait(false);
            }

            this.FinalizeChunk(outputStream, chunkWriter);
        }
    }
#endif

    /// <inheritdoc/>
    protected override void AddChunkToTableIfCompleted(Stream stream, ChunkWriterWrapper? chunkWriter) => this.AddChunkToTableIfCompleted(stream, chunkWriter, cw => this.chunkTotals.Add(cw.Count));

    /// <inheritdoc/>
    protected override void CompressValue(IntegerCompressor integerCompressor, int index)
    {
        integerCompressor.Compress(index is 0 ? 0 : (int)this.chunkTotals[index - 1], this.chunkTotals[index]);
        base.CompressValue(integerCompressor, index);
    }

    /// <inheritdoc/>
    protected override ChunkWriter Create(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip) => new LayeredChunkWriter(rawWriter, pointDataLength, pointDataFormatId, zip);

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void ThrowAlreadyInChunkException() => throw new InvalidOperationException(Compression.Properties.Resources.CannotWriteChunkWhenAlreadyWritingAChunk);

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    private static void ThrowLessPointsThanCount(string name, int count, int i) => throw new ArgumentException(string.Format(Compression.Properties.Resources.Culture, Compression.Properties.Resources.FailedToReadItems, count, name, i - 1), name);
}