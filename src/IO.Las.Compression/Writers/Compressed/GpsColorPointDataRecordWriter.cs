// -----------------------------------------------------------------------
// <copyright file="GpsColorPointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.IPointDataRecordWriter"/> for <see cref="GpsColorPointDataRecord"/> instances.
/// </summary>
/// <param name="encoder">The encoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class GpsColorPointDataRecordWriter(IEntropyEncoder encoder, int extraBytes) : PointDataRecordWriter<GpsColorPointDataRecord>(encoder)
{
    private readonly GpsTimeWriter gpsTimeWriter = new(encoder);

    private readonly ColorWriter2 colorWriter = new(encoder);

    private readonly ISimpleWriter byteWriter = extraBytes switch
    {
        0 => NullSimpleWriter.Instance,
        _ => new ByteWriter2(encoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.gpsTimeWriter.Initialize(item[PointDataRecord.Size..])
        && this.colorWriter.Initialize(item[GpsPointDataRecord.Size..])
        && this.byteWriter.Initialize(item[GpsColorPointDataRecord.Size..]);

    /// <inheritdoc/>
    public override void Write(Span<byte> item)
    {
        base.Write(item);
        this.gpsTimeWriter.Write(item[PointDataRecord.Size..]);
        this.colorWriter.Write(item[GpsPointDataRecord.Size..]);
        this.byteWriter.Write(item[GpsColorPointDataRecord.Size..]);
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        await base.WriteAsync(item, cancellationToken).ConfigureAwait(false);
        await this.gpsTimeWriter.WriteAsync(item[PointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
        await this.colorWriter.WriteAsync(item[GpsPointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
        await this.byteWriter.WriteAsync(item[GpsColorPointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
    }
}