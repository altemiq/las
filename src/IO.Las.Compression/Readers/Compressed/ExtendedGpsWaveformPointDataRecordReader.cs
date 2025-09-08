// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="ExtendedGpsWaveformPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The extra bytes.</param>
/// <param name="decompressSelective">The selected items to decompress.</param>
internal sealed class ExtendedGpsWaveformPointDataRecordReader(Compression.IEntropyDecoder decoder, int extraBytes, Compression.DecompressSelections decompressSelective = Compression.DecompressSelections.All) : ExtendedGpsPointDataRecordReader<ExtendedGpsWaveformPointDataRecord>(decoder, ExtendedGpsWaveformPointDataRecord.Size + extraBytes, ExtendedGpsWaveformPointDataRecord.Size, decompressSelective), IDisposable
{
    private readonly WavePacketReader3 waveformReader = new(decoder, decompressSelective);

    private readonly IContextReader byteReader = extraBytes switch
    {
        0 => NullContextReader.Instance,
        _ => new ByteReader3(decoder, (uint)extraBytes, decompressSelective),
    };

    private bool disposed;

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item, ref uint context) => base.Initialize(item, ref context)
        && this.waveformReader.Initialize(item[ExtendedGpsPointDataRecord.Size..], ref context)
        && this.byteReader.Initialize(item[ExtendedGpsWaveformPointDataRecord.Size..], ref context);

    /// <inheritdoc/>
    public override bool ChunkSizes() => base.ChunkSizes()
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
    protected override ExtendedGpsWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsWaveformPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override byte[] ProcessData()
    {
        var context = default(uint);
        var data = this.ProcessData(ref context);
        Span<byte> span = data;
        this.waveformReader.Read(span[ExtendedGpsPointDataRecord.Size..], context);
        this.byteReader.Read(span[ExtendedGpsWaveformPointDataRecord.Size..], context);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var context = default(uint);
        var data = await this.ProcessDataAsync(ref context, cancellationToken).ConfigureAwait(false);
        var span = data.Span;
        this.waveformReader.Read(span[ExtendedGpsPointDataRecord.Size..], context);
        this.byteReader.Read(span[ExtendedGpsWaveformPointDataRecord.Size..], context);
        return data;
    }
}