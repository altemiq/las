// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsWaveformPointDataRecordWriter4.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.IPointDataRecordWriter"/> for <see cref="ExtendedGpsWaveformPointDataRecord"/> instances.
/// </summary>
/// <param name="encoder">The encoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class ExtendedGpsWaveformPointDataRecordWriter4(IEntropyEncoder encoder, int extraBytes) : ExtendedGpsPointDataRecordWriter4<ExtendedGpsWaveformPointDataRecord>(encoder)
{
    private readonly WavePacketWriter4 wavePacketWriter = new(encoder);

    private readonly IContextWriter byteWriter = extraBytes switch
    {
        0 => NullContextWriter.Instance,
        _ => new ByteWriter4(encoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item, ref uint context) => base.Initialize(item, ref context)
        && this.wavePacketWriter.Initialize(item[ExtendedGpsPointDataRecord.Size..], ref context)
        && this.byteWriter.Initialize(item[ExtendedGpsWaveformPointDataRecord.Size..], ref context);

    /// <inheritdoc/>
    public override bool ChunkSizes() => base.ChunkSizes()
        && this.wavePacketWriter.ChunkSizes()
        && this.byteWriter.ChunkSizes();

    /// <inheritdoc/>
    public override bool ChunkBytes() => base.ChunkBytes()
        && this.wavePacketWriter.ChunkBytes()
        && this.byteWriter.ChunkBytes();

    /// <inheritdoc/>
    public override void Write(Span<byte> item, ref uint context)
    {
        base.Write(item, ref context);
        this.wavePacketWriter.Write(item[ExtendedGpsPointDataRecord.Size..], ref context);
        this.byteWriter.Write(item[ExtendedGpsWaveformPointDataRecord.Size..], ref context);
    }

    /// <inheritdoc/>
    public override async ValueTask<uint> WriteAsync(Memory<byte> item, uint context, CancellationToken cancellationToken = default)
    {
        context = await base.WriteAsync(item, context, cancellationToken).ConfigureAwait(false);
        context = await this.wavePacketWriter.WriteAsync(item[ExtendedGpsPointDataRecord.Size..], context, cancellationToken).ConfigureAwait(false);
        return await this.byteWriter.WriteAsync(item[ExtendedGpsWaveformPointDataRecord.Size..], context, cancellationToken).ConfigureAwait(false);
    }
}