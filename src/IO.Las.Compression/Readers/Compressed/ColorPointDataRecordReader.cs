// -----------------------------------------------------------------------
// <copyright file="ColorPointDataRecordReader.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed <see cref="Readers.IPointDataRecordReader"/> for <see cref="ColorPointDataRecord"/> instances.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="extraBytes">The number of extra bytes.</param>
internal sealed class ColorPointDataRecordReader(IEntropyDecoder decoder, int extraBytes) : PointDataRecordReader<ColorPointDataRecord>(decoder, ColorPointDataRecord.Size + extraBytes, ColorPointDataRecord.Size)
{
    private readonly ColorReader2 colorReader = new(decoder);

    private readonly ISimpleReader byteReader = extraBytes switch
    {
        0 => NullSimpleReader.Instance,
        _ => new ByteReader2(decoder, (uint)extraBytes),
    };

    /// <inheritdoc/>
    public override bool Initialize(ReadOnlySpan<byte> item) => base.Initialize(item)
        && this.colorReader.Initialize(item[PointDataRecord.Size..])
        && this.byteReader.Initialize(item[ColorPointDataRecord.Size..]);

    /// <inheritdoc/>
    protected override ColorPointDataRecord Read(ReadOnlySpan<byte> source) => ColorPointDataRecord.Create(source);

    /// <inheritdoc/>
    protected override Span<byte> ProcessData()
    {
        var data = base.ProcessData();
        this.colorReader.Read(data[PointDataRecord.Size..]);
        this.byteReader.Read(data[ColorPointDataRecord.Size..]);
        return data;
    }

    /// <inheritdoc/>
    protected override async ValueTask<Memory<byte>> ProcessDataAsync(CancellationToken cancellationToken = default)
    {
        var data = await base.ProcessDataAsync(cancellationToken).ConfigureAwait(false);
        this.colorReader.Read(data[PointDataRecord.Size..].Span);
        this.byteReader.Read(data[ColorPointDataRecord.Size..].Span);
        return data;
    }
}