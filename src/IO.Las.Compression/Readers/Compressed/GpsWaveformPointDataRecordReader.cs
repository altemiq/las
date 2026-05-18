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
internal sealed class GpsWaveformPointDataRecordReader(ArithmeticDecoder decoder, int extraBytes) : PointDataRecordReader<GpsWaveformPointDataRecord>(decoder, GpsWaveformPointDataRecord.Size + extraBytes, GpsWaveformPointDataRecord.Size)
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
    protected override void ProcessData(Span<byte> destination)
    {
        base.ProcessData(destination);
        this.gpsTimeReader.Read(destination[PointDataRecord.Size..]);
        this.waveformReader.Read(destination[GpsPointDataRecord.Size..]);
        this.byteReader.Read(destination[GpsWaveformPointDataRecord.Size..]);
    }
}