// -----------------------------------------------------------------------
// <copyright file="GpsColorPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="GpsColorPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class GpsColorPointDataRecordReader(ArithmeticDecoder decoder, int extraBytes) : PointDataRecordReader<GpsColorPointDataRecord>(decoder, GpsColorPointDataRecord.Size + extraBytes, GpsColorPointDataRecord.Size)
{
    private readonly GpsTimeReader gpsTimeReader = new(decoder);

    private readonly ColorReader2 colorReader = new(decoder);

    private readonly ISimpleReader byteReader = extraBytes switch
    {
        0 => NullSimpleReader.Instance,
        _ => new ByteReader2(decoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
                                                                && this.gpsTimeReader.Initialize(item[PointDataRecord.Size..])
                                                                && this.colorReader.Initialize(item[GpsPointDataRecord.Size..])
                                                                && this.byteReader.Initialize(item[GpsColorPointDataRecord.Size..]);

    /// <inheritdoc/>
    protected override GpsColorPointDataRecord Read(ReadOnlySpan<byte> source) => GpsColorPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override void ProcessData(Span<byte> destination)
    {
        base.ProcessData(destination);
        this.gpsTimeReader.Read(destination[PointDataRecord.Size..]);
        this.colorReader.Read(destination[GpsPointDataRecord.Size..]);
        this.byteReader.Read(destination[GpsColorPointDataRecord.Size..]);
    }
}