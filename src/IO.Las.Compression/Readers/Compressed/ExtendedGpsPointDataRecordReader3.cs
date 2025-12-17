// -----------------------------------------------------------------------
// <copyright file="ExtendedGpsPointDataRecordReader3.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="ExtendedGpsColorPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
/// <param name="decompressSelective">The selective decompress value.</param>
internal sealed class ExtendedGpsPointDataRecordReader3(IEntropyDecoder decoder, int extraBytes, DecompressSelections decompressSelective = DecompressSelections.All) : ExtendedGpsPointDataRecordReader3<ExtendedGpsPointDataRecord>(decoder, ExtendedGpsPointDataRecord.Size + extraBytes, ExtendedGpsPointDataRecord.Size, decompressSelective), IDisposable
{
    private readonly IContextReader byteReader = extraBytes switch
    {
        0 => NullContextReader.Instance,
        _ => new ByteReader3(decoder, (uint)extraBytes, decompressSelective),
    };

    private bool disposed;

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item, ref uint context) => base.Initialize(item, ref context)
        && this.byteReader.Initialize(item[ExtendedGpsPointDataRecord.Size..], ref context);

    /// <inheritdoc/>
    public override bool ChunkSizes() => base.ChunkSizes()
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
    protected override ExtendedGpsPointDataRecord Read(ReadOnlySpan<byte> source) => new(source);

    /// <inheritdoc/>
    protected override byte[] ProcessData()
    {
        var context = default(uint);
        var data = this.ProcessData(ref context);
        Span<byte> span = data;
        this.byteReader.Read(span[ExtendedGpsPointDataRecord.Size..], context);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var context = default(uint);
        var data = await this.ProcessDataAsync(ref context, cancellationToken).ConfigureAwait(false);
        this.byteReader.Read(data[ExtendedGpsPointDataRecord.Size..].Span, context);
        return data;
    }
}