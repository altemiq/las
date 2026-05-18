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
internal sealed class PointDataRecordReader(ArithmeticDecoder decoder, int extraBytes) : PointDataRecordReader<PointDataRecord>(decoder, PointDataRecord.Size + extraBytes, PointDataRecord.Size)
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
    protected override void ProcessData(Span<byte> destination)
    {
        base.ProcessData(destination);
        this.byteReader.Read(destination[PointDataRecord.Size..]);
    }
}