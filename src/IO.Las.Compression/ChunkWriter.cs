// -----------------------------------------------------------------------
// <copyright file="ChunkWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The chunk <see cref="IPointWriter"/>.
/// </summary>
/// <inheritdoc cref="PointWiseWriter" />
internal class ChunkWriter(Writers.IPointDataRecordWriter rawWriter, int pointDataLength, byte pointDataFormatId, LasZip zip) : PointWiseWriter(rawWriter, pointDataLength, pointDataFormatId, zip)
{
    /// <summary>
    /// Gets the count.
    /// </summary>
    public uint Count { get; private set; }

    /// <inheritdoc/>
    public sealed override void Initialize(Stream stream)
    {
        base.Initialize(stream);
        this.Count = default;
    }

    /// <inheritdoc/>
    public sealed override void Write(Stream stream, IBasePointDataRecord record, ReadOnlySpan<byte> extraBytes)
    {
        this.Count++;
        base.Write(stream, record, extraBytes);
    }

    /// <inheritdoc/>
    public sealed override ValueTask WriteAsync(Stream stream, IBasePointDataRecord record, ReadOnlyMemory<byte> extraBytes, CancellationToken cancellationToken = default)
    {
        this.Count++;
        return base.WriteAsync(stream, record, extraBytes, cancellationToken);
    }
}