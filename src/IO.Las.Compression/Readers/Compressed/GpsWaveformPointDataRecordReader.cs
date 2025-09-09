// -----------------------------------------------------------------------
// <copyright file="GpsWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="GpsWaveformPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class GpsWaveformPointDataRecordReader(IEntropyDecoder decoder, int extraBytes) : PointDataRecordReader<GpsWaveformPointDataRecord>(decoder, GpsWaveformPointDataRecord.Size + extraBytes, GpsWaveformPointDataRecord.Size)
{
    private readonly GpsTimeReader gpsTimeReader = new(decoder);

    private readonly WavePacketReader1 waveformReader = new(decoder);

    private readonly ISimpleReader byteReader = extraBytes switch
    {
        0 => NullSimpleReader.Instance,
        _ => new ByteReader2(decoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.gpsTimeReader.Initialize(item[PointDataRecord.Size..])
        && this.waveformReader.Initialize(item[GpsPointDataRecord.Size..])
        && this.byteReader.Initialize(item[GpsWaveformPointDataRecord.Size..]);

    /// <inheritdoc/>
    protected override GpsWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => GpsWaveformPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override Span<byte> ProcessData()
    {
        var data = base.ProcessData();
        this.gpsTimeReader.Read(data[PointDataRecord.Size..]);
        this.waveformReader.Read(data[GpsPointDataRecord.Size..]);
        this.byteReader.Read(data[GpsWaveformPointDataRecord.Size..]);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var data = await base.ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        this.gpsTimeReader.Read(data[PointDataRecord.Size..].Span);
        this.waveformReader.Read(data[GpsPointDataRecord.Size..].Span);
        this.byteReader.Read(data[GpsWaveformPointDataRecord.Size..].Span);
        return data;
    }
}