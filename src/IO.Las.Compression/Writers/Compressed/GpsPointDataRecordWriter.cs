// -----------------------------------------------------------------------
// <copyright file="GpsPointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.IPointDataRecordWriter"/> for <see cref="GpsPointDataRecord"/> instances.
/// </summary>
/// <param name="encoder">The encoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class GpsPointDataRecordWriter(IEntropyEncoder encoder, int extraBytes) : PointDataRecordWriter<GpsPointDataRecord>(encoder)
{
    private readonly GpsTimeWriter gpsTimeWriter = new(encoder);

    private readonly ISimpleWriter byteWriter = extraBytes switch
    {
        0 => NullSimpleWriter.Instance,
        _ => new ByteWriter2(encoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.gpsTimeWriter.Initialize(item[PointDataRecord.Size..])
        && this.byteWriter.Initialize(item[GpsPointDataRecord.Size..]);

    /// <inheritdoc/>
    public override void Write(Span<byte> item)
    {
        base.Write(item);
        this.gpsTimeWriter.Write(item[PointDataRecord.Size..]);
        this.byteWriter.Write(item[GpsPointDataRecord.Size..]);
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        await base.WriteAsync(item, cancellationToken).ConfigureAwait(false);
        await this.gpsTimeWriter.WriteAsync(item[PointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
        await this.byteWriter.WriteAsync(item[GpsPointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
    }
}