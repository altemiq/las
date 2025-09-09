// -----------------------------------------------------------------------
// <copyright file="WavePacketReader1.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed wave-packet reader, version 1.
/// </summary>
internal sealed class WavePacketReader1 : ISimpleReader
{
    private readonly IEntropyDecoder decoder;

    private readonly byte[] lastItem = new byte[28];

    private readonly ISymbolModel packetIndex;

    private readonly ISymbolModel[] offsetDiff = new ISymbolModel[4];

    private readonly IntegerDecompressor icOffsetDiff;

    private readonly IntegerDecompressor icPacketSize;

    private readonly IntegerDecompressor icReturnPoint;

    private readonly IntegerDecompressor icXyz;

    private int lastDiff32;

    private uint symLastOffsetDiff;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavePacketReader1"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    public WavePacketReader1(IEntropyDecoder decoder)
    {
        this.decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));

        // create models and integer compressors
        this.packetIndex = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.offsetDiff[0] = decoder.CreateSymbolModel(4);
        this.offsetDiff[1] = decoder.CreateSymbolModel(4);
        this.offsetDiff[2] = decoder.CreateSymbolModel(4);
        this.offsetDiff[3] = decoder.CreateSymbolModel(4);
        this.icOffsetDiff = new(decoder, 32);
        this.icPacketSize = new(decoder, 32);
        this.icReturnPoint = new(decoder, 32);
        this.icXyz = new(decoder, 32, 3);
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        // init state
        this.lastDiff32 = default;
        this.symLastOffsetDiff = default;

        _ = this.packetIndex.Initialize();
        foreach (var offsetModel in this.offsetDiff)
        {
            _ = offsetModel.Initialize();
        }

        this.icOffsetDiff.Initialize();
        this.icPacketSize.Initialize();
        this.icReturnPoint.Initialize();
        this.icXyz.Initialize();

        item.Slice(1, this.lastItem.Length).CopyTo(this.lastItem);

        return true;
    }

    /// <inheritdoc/>
    public void Read(Span<byte> item)
    {
        item[0] = (byte)this.decoder.DecodeSymbol(this.packetIndex);

        var lastWavePacket = new WavePacket13(this.lastItem);
        this.symLastOffsetDiff = this.decoder.DecodeSymbol(this.offsetDiff[this.symLastOffsetDiff]);

        var currentWavePacket = new WavePacket13(
            this.symLastOffsetDiff switch
            {
                0 => lastWavePacket.Offset,
                1 => lastWavePacket.Offset + lastWavePacket.PacketSize,
                2 => lastWavePacket.Offset + GetLastDiff(),
                _ => this.decoder.ReadUInt64(),
            },
            (uint)this.icPacketSize.Decompress((int)lastWavePacket.PacketSize),
            BitConverter.Int32BitsToSingle(this.icReturnPoint.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.ReturnPoint))),
            BitConverter.Int32BitsToSingle(this.icXyz.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.X))),
            BitConverter.Int32BitsToSingle(this.icXyz.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.Y), 1)),
            BitConverter.Int32BitsToSingle(this.icXyz.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.Z), 2)));

        currentWavePacket.WriteTo(item);

        item.Slice(1, this.lastItem.Length).CopyTo(this.lastItem);

        ulong GetLastDiff()
        {
            this.lastDiff32 = this.icOffsetDiff.Decompress(this.lastDiff32);
            return (ulong)this.lastDiff32;
        }
    }
}