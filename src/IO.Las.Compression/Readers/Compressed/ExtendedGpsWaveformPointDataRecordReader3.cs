// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsWaveformPointDataRecordReader3.cs" company="Altemiq">
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
internal sealed class ExtendedGpsWaveformPointDataRecordReader3(IEntropyDecoder decoder, int extraBytes, DecompressSelections decompressSelective = DecompressSelections.All) : ExtendedGpsPointDataRecordReader3<ExtendedGpsWaveformPointDataRecord>(decoder, ExtendedGpsWaveformPointDataRecord.Size + extraBytes, ExtendedGpsWaveformPointDataRecord.Size, decompressSelective), IDisposable
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
        if (this.disposed)
        {
            return;
        }

        if (this.byteReader is IDisposable disposableByteReader)
        {
            disposableByteReader.Dispose();
        }

        this.disposed = true;
    }

    /// <inheritdoc/>
    protected override ExtendedGpsWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsWaveformPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override void ProcessData(Span<byte> destination)
    {
        var context = default(uint);
        this.ProcessData(ref context, destination);
        this.waveformReader.Read(destination[ExtendedGpsPointDataRecord.Size..], context);
        this.byteReader.Read(destination[ExtendedGpsWaveformPointDataRecord.Size..], context);
    }
}