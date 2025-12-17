// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorPointDataRecordWriter3.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed <see cref="Writers.IPointDataRecordWriter"/> for <see cref="ExtendedGpsColorPointDataRecord"/> instances.
/// </summary>
/// <param name="encoder">The encoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class ExtendedGpsColorPointDataRecordWriter3(IEntropyEncoder encoder, int extraBytes) : ExtendedGpsPointDataRecordWriter3<ExtendedGpsColorPointDataRecord>(encoder)
{
    private readonly ColorWriter3 colorWriter = new(encoder);

    private readonly IContextWriter byteWriter = extraBytes switch
    {
        0 => NullContextWriter.Instance,
        _ => new ByteWriter3(encoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item, ref uint context) => base.Initialize(item, ref context)
        && this.colorWriter.Initialize(item[ExtendedGpsPointDataRecord.Size..], ref context)
        && this.byteWriter.Initialize(item[ExtendedGpsColorPointDataRecord.Size..], ref context);

    /// <inheritdoc/>
    public override bool ChunkSizes() => base.ChunkSizes()
        && this.colorWriter.ChunkSizes()
        && this.byteWriter.ChunkSizes();

    /// <inheritdoc/>
    public override bool ChunkBytes() => base.ChunkBytes()
        && this.colorWriter.ChunkBytes()
        && this.byteWriter.ChunkBytes();

    /// <inheritdoc/>
    public override void Write(Span<byte> item, ref uint context)
    {
        base.Write(item, ref context);
        this.colorWriter.Write(item[ExtendedGpsPointDataRecord.Size..], ref context);
        this.byteWriter.Write(item[ExtendedGpsColorPointDataRecord.Size..], ref context);
    }

    /// <inheritdoc/>
    public override async ValueTask<uint> WriteAsync(Memory<byte> item, uint context, CancellationToken cancellationToken = default)
    {
        context = await base.WriteAsync(item, context, cancellationToken).ConfigureAwait(false);
        context = await this.colorWriter.WriteAsync(item[ExtendedGpsPointDataRecord.Size..], context, cancellationToken).ConfigureAwait(false);
        return await this.byteWriter.WriteAsync(item[ExtendedGpsColorPointDataRecord.Size..], context, cancellationToken).ConfigureAwait(false);
    }
}