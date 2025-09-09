// -----------------------------------------------------------------------
// <copyright file="PointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="PointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class PointDataRecordReader(IEntropyDecoder decoder, int extraBytes) : PointDataRecordReader<PointDataRecord>(decoder, PointDataRecord.Size + extraBytes, PointDataRecord.Size)
{
    private readonly ISimpleReader byteReader = extraBytes switch
    {
        0 => NullSimpleReader.Instance,
        _ => new ByteReader2(decoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.byteReader.Initialize(item[PointDataRecord.Size..]);

    /// <inheritdoc/>
    protected override PointDataRecord Read(ReadOnlySpan<byte> source) => PointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override Span<byte> ProcessData()
    {
        var data = base.ProcessData();
        this.byteReader.Read(data[PointDataRecord.Size..]);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var data = await base.ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        this.byteReader.Read(data[PointDataRecord.Size..].Span);
        return data;
    }
}