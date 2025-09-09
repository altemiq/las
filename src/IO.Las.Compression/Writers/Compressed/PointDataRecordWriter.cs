// -----------------------------------------------------------------------
// <copyright file="PointDataRecordWriter.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.PointDataRecordWriter{T}"/> for <see cref="PointDataRecord"/> intances.
/// </summary>
/// <param name="encoder">The encoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class PointDataRecordWriter(IEntropyEncoder encoder, int extraBytes) : PointDataRecordWriter<PointDataRecord>(encoder)
{
    private readonly ISimpleWriter byteWriter = extraBytes switch
    {
        0 => NullSimpleWriter.Instance,
        _ => new ByteWriter2(encoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.byteWriter.Initialize(item[PointDataRecord.Size..]);

    /// <inheritdoc/>
    public override void Write(Span<byte> item)
    {
        base.Write(item);
        this.byteWriter.Write(item[PointDataRecord.Size..]);
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        await base.WriteAsync(item, cancellationToken).ConfigureAwait(false);
        await this.byteWriter.WriteAsync(item[PointDataRecord.Size..], cancellationToken).ConfigureAwait(false);
    }
}