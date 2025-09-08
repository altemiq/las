// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorNearInfraredWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="ExtendedGpsColorNearInfraredWaveformPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The extra bytes.</param>
/// <param name="decompressSelective">The selected items to decompress.</param>
internal sealed class ExtendedGpsColorNearInfraredWaveformPointDataRecordReader(Compression.IEntropyDecoder decoder, int extraBytes, Compression.DecompressSelections decompressSelective = Compression.DecompressSelections.All) : ExtendedGpsPointDataRecordReader<ExtendedGpsColorNearInfraredWaveformPointDataRecord>(decoder, ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size + extraBytes, ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size, decompressSelective), IDisposable
{
    private readonly ColorNearInfraredReader3 colorNearInfraredReader = new(decoder, decompressSelective);

    private readonly WavePacketReader3 waveformReader = new(decoder, decompressSelective);

    private readonly IContextReader byteReader = extraBytes switch
    {
        0 => NullContextReader.Instance,
        _ => new ByteReader3(decoder, (uint)extraBytes, decompressSelective),
    };

    private bool disposed;

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item, ref uint context) => base.Initialize(item, ref context)
        && this.colorNearInfraredReader.Initialize(item[ExtendedGpsPointDataRecord.Size..], ref context)
        && this.waveformReader.Initialize(item[ExtendedGpsColorNearInfraredPointDataRecord.Size..], ref context)
        && this.byteReader.Initialize(item[ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size..], ref context);

    /// <inheritdoc/>
    public override bool ChunkSizes() => base.ChunkSizes()
        && this.colorNearInfraredReader.ChunkSizes()
        && this.waveformReader.ChunkSizes()
        && this.byteReader.ChunkSizes();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.disposed)
        {
            if (this.byteReader is IDisposable disposableByteReader)
            {
                disposableByteReader.Dispose();
            }

            this.disposed = true;
        }
    }

    /// <inheritdoc/>
    protected override ExtendedGpsColorNearInfraredWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsColorNearInfraredWaveformPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override byte[] ProcessData()
    {
        var context = default(uint);
        var data = this.ProcessData(ref context);
        Span<byte> span = data;
        this.colorNearInfraredReader.Read(span[ExtendedGpsPointDataRecord.Size..], context);
        this.waveformReader.Read(span[ExtendedGpsColorNearInfraredPointDataRecord.Size..], context);
        this.byteReader.Read(span[ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size..], context);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var context = default(uint);
        var data = await this.ProcessDataAsync(ref context, cancellationToken).ConfigureAwait(false);
        var span = data.Span;
        this.colorNearInfraredReader.Read(span[ExtendedGpsPointDataRecord.Size..], context);
        this.waveformReader.Read(span[ExtendedGpsColorNearInfraredPointDataRecord.Size..], context);
        this.byteReader.Read(span[ExtendedGpsColorNearInfraredWaveformPointDataRecord.Size..], context);
        return data;
    }
}