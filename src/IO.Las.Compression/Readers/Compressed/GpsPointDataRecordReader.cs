// -----------------------------------------------------------------------
// <copyright file="GpsPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="GpsPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class GpsPointDataRecordReader(IEntropyDecoder decoder, int extraBytes) : PointDataRecordReader<GpsPointDataRecord>(decoder, GpsPointDataRecord.Size + extraBytes, GpsPointDataRecord.Size)
{
    private readonly GpsTimeReader gpsTimeReader = new(decoder);

    private readonly ISimpleReader byteReader = extraBytes switch
    {
        0 => NullSimpleReader.Instance,
        _ => new ByteReader2(decoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.gpsTimeReader.Initialize(item[PointDataRecord.Size..])
        && this.byteReader.Initialize(item[GpsPointDataRecord.Size..]);

    /// <inheritdoc/>
    protected override GpsPointDataRecord Read(ReadOnlySpan<byte> source) => GpsPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override Span<byte> ProcessData()
    {
        var data = base.ProcessData();
        this.gpsTimeReader.Read(data[PointDataRecord.Size..]);
        this.byteReader.Read(data[GpsPointDataRecord.Size..]);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var data = await base.ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        this.gpsTimeReader.Read(data[PointDataRecord.Size..].Span);
        this.byteReader.Read(data[GpsPointDataRecord.Size..].Span);
        return data;
    }
}