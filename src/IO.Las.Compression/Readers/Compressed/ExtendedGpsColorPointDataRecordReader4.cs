// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsColorPointDataRecordReader4.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="ExtendedGpsColorPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The extra bytes.</param>
/// <param name="decompressSelective">The selected items to decompress.</param>
internal sealed class ExtendedGpsColorPointDataRecordReader4(IEntropyDecoder decoder, int extraBytes, DecompressSelections decompressSelective = DecompressSelections.All) : ExtendedGpsPointDataRecordReader4<ExtendedGpsColorPointDataRecord>(decoder, ExtendedGpsColorPointDataRecord.Size + extraBytes, ExtendedGpsColorPointDataRecord.Size, decompressSelective), IDisposable
{
    private readonly ColorReader4 colorReader = new(decoder, decompressSelective);

    private readonly IContextReader byteReader = extraBytes switch
    {
        0 => NullContextReader.Instance,
        _ => new ByteReader4(decoder, (uint)extraBytes, decompressSelective),
    };

    private bool disposed;

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item, ref uint context) => base.Initialize(item, ref context)
        && this.colorReader.Initialize(item[ExtendedGpsPointDataRecord.Size..], ref context)
        && this.byteReader.Initialize(item[ExtendedGpsColorPointDataRecord.Size..], ref context);

    /// <inheritdoc/>
    public override bool ChunkSizes() => base.ChunkSizes()
        && this.colorReader.ChunkSizes()
        && this.byteReader.ChunkSizes();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        if (this.byteReader is IDisposable disposableByteReader)
        {
            disposableByteReader.Dispose();
        }

        this.disposed = true;
    }

    /// <inheritdoc/>
    protected override ExtendedGpsColorPointDataRecord Read(ReadOnlySpan<byte> source) => ExtendedGpsColorPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override byte[] ProcessData()
    {
        var context = default(uint);
        var data = this.ProcessData(ref context);
        Span<byte> span = data;
        this.colorReader.Read(span[ExtendedGpsPointDataRecord.Size..], context);
        this.byteReader.Read(span[ExtendedGpsColorPointDataRecord.Size..], context);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var context = default(uint);
        var data = await this.ProcessDataAsync(ref context, cancellationToken).ConfigureAwait(false);
        var span = data.Span;
        this.colorReader.Read(span[ExtendedGpsPointDataRecord.Size..], context);
        this.byteReader.Read(span[ExtendedGpsColorPointDataRecord.Size..], context);
        return data;
    }
}