// -----------------------------------------------------------------------
// <copyright file="GpsColorWaveformPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="GpsColorWaveformPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class GpsColorWaveformPointDataRecordReader(Compression.ArithmeticDecoder decoder, int extraBytes) : PointDataRecordReader<GpsColorWaveformPointDataRecord>(decoder, GpsColorPointDataRecord.Size + extraBytes, GpsColorPointDataRecord.Size)
{
    private readonly GpsTimeReader gpsTimeReader = new(decoder);

    private readonly ColorReader2 colorReader = new(decoder);

    private readonly WavePacketReader1 waveformReader = new(decoder);

    private readonly ISimpleReader byteReader = extraBytes switch
    {
        0 => NullSimpleReader.Instance,
        _ => new ByteReader2(decoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.gpsTimeReader.Initialize(item[PointDataRecord.Size..])
        && this.colorReader.Initialize(item[GpsPointDataRecord.Size..])
        && this.waveformReader.Initialize(item[GpsColorPointDataRecord.Size..])
        && this.byteReader.Initialize(item[GpsColorWaveformPointDataRecord.Size..]);

    /// <inheritdoc/>
    protected override GpsColorWaveformPointDataRecord Read(ReadOnlySpan<byte> source) => GpsColorWaveformPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override Span<byte> ProcessData()
    {
        var data = base.ProcessData();
        this.gpsTimeReader.Read(data[PointDataRecord.Size..]);
        this.colorReader.Read(data[GpsPointDataRecord.Size..]);
        this.waveformReader.Read(data[GpsColorPointDataRecord.Size..]);
        this.byteReader.Read(data[GpsColorWaveformPointDataRecord.Size..]);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var data = await base.ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        this.gpsTimeReader.Read(data[PointDataRecord.Size..].Span);
        this.colorReader.Read(data[GpsPointDataRecord.Size..].Span);
        this.waveformReader.Read(data[GpsColorPointDataRecord.Size..].Span);
        this.byteReader.Read(data[GpsColorWaveformPointDataRecord.Size..].Span);
        return data;
    }
}