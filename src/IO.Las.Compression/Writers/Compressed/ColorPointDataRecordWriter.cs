// -----------------------------------------------------------------------
// <copyright file="ColorPointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.IPointDataRecordWriter"/> for <see cref="ColorPointDataRecord"/> instances.
/// </summary>
/// <param name="encoder">The encoder.</param>
/// <param name="extraBytes">The extra bytes.</param>
internal sealed class ColorPointDataRecordWriter(IEntropyEncoder encoder, int extraBytes) : PointDataRecordWriter<ColorPointDataRecord>(encoder)
{
    private readonly ColorWriter2 colorWriter = new(encoder);

    private readonly ISimpleWriter byteWriter = extraBytes switch
    {
        0 => NullSimpleWriter.Instance,
        _ => new ByteWriter2(encoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.colorWriter.Initialize(item[PointDataRecord.Size..])
        && this.byteWriter.Initialize(item[ColorPointDataRecord.Size..]);

    /// <inheritdoc/>
    public override void Write(Span<byte> item)
    {
        base.Write(item);
        this.colorWriter.Write(item[PointDataRecord.Size..]);
        this.byteWriter.Write(item[ColorPointDataRecord.Size..]);
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        await base.WriteAsync(item, cancellationToken).ConfigureAwait(false);
        await this.colorWriter.WriteAsync(item[PointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
        await this.byteWriter.WriteAsync(item[ColorPointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
    }
}