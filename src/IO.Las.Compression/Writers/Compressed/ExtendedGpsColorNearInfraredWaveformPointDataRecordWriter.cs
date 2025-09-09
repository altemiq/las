// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorNearInfraredWaveformPointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.IPointDataRecordWriter"/> for <see cref="ExtendedGpsColorPointDataRecord"/> instances.
/// </summary>
/// <param name="encoder">The encoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class ExtendedGpsColorNearInfraredWaveformPointDataRecordWriter(IEntropyEncoder encoder, int extraBytes) : ExtendedGpsPointDataRecordWriter<ExtendedGpsColorNearInfraredWaveformPointDataRecord>(encoder)
{
    private readonly ColorNearInfraredWriter3 colorNearInfraredWriter = new(encoder);

    private readonly WavePacketWriter3 wavePacketWriter = new(encoder);

    private readonly IContextWriter byteWriter = extraBytes switch
    {
        0 => NullContextWriter.Instance,
        _ => new ByteWriter3(encoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item, ref uint context) => base.Initialize(item, ref context)
        && this.colorNearInfraredWriter.Initialize(item[ExtendedGpsPointDataRecord.Size..], ref context)
        && this.wavePacketWriter.Initialize(item[ExtendedGpsColorNearInfraredPointDataRecord.Size..], ref context)
        && this.byteWriter.Initialize(item[ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size..], ref context);

    /// <inheritdoc/>
    public override bool ChunkSizes() => base.ChunkSizes()
        && this.colorNearInfraredWriter.ChunkSizes()
        && this.wavePacketWriter.ChunkSizes()
        && this.byteWriter.ChunkSizes();

    /// <inheritdoc/>
    public override bool ChunkBytes() => base.ChunkBytes()
        && this.colorNearInfraredWriter.ChunkBytes()
        && this.wavePacketWriter.ChunkBytes()
        && this.byteWriter.ChunkBytes();

    /// <inheritdoc/>
    public override void Write(Span<byte> item, ref uint context)
    {
        base.Write(item, ref context);
        this.colorNearInfraredWriter.Write(item[ExtendedGpsPointDataRecord.Size..], ref context);
        this.wavePacketWriter.Write(item[ExtendedGpsColorNearInfraredPointDataRecord.Size..], ref context);
        this.byteWriter.Write(item[ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size..], ref context);
    }

    /// <inheritdoc/>
    public override async ValueTask<uint> WriteAsync(Memory<byte> item, uint context, CancellationToken cancellationToken = default)
    {
        context = await base.WriteAsync(item, context, cancellationToken).ConfigureAwait(false);
        context = await this.colorNearInfraredWriter.WriteAsync(item[ExtendedGpsPointDataRecord.Size..], context, cancellationToken).ConfigureAwait(false);
        context = await this.wavePacketWriter.WriteAsync(item[ExtendedGpsColorNearInfraredPointDataRecord.Size..], context, cancellationToken).ConfigureAwait(false);
        return await this.byteWriter.WriteAsync(item[ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size..], context, cancellationToken).ConfigureAwait(false);
    }
}