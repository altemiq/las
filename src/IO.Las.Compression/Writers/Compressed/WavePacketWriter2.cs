// -----------------------------------------------------------------------
// <copyright file="WavePacketWriter2.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed writer for wave packet values, version 2.
/// </summary>
internal sealed class WavePacketWriter2 : ISimpleWriter
{
    private readonly IEntropyEncoder encoder;

    private readonly byte[] lastItem = new byte[28];

    private readonly ISymbolModel packetIndex;

    private readonly ISymbolModel[] offsetDiff = new ISymbolModel[6];

    private readonly IntegerCompressor icOffsetDiff;

    private readonly IntegerCompressor icPacketSize;

    private readonly IntegerCompressor icReturnPoint;

    private readonly IntegerCompressor icXyz;

    private int lastDiff32;

    private uint symLastOffsetDiff;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavePacketWriter2"/> class.
    /// </summary>
    /// <param name="encoder">The encoder.</param>
    public WavePacketWriter2(IEntropyEncoder encoder)
    {
        this.encoder = encoder;

        // create models and integer compressors
        this.packetIndex = encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
        for (var i = 0; i < this.offsetDiff.Length; i++)
        {
            this.offsetDiff[i] = encoder.CreateSymbolModel(4);
        }

        this.icOffsetDiff = new(this.encoder, 32);
        this.icPacketSize = new(this.encoder, 32);
        this.icReturnPoint = new(this.encoder, 32);
        this.icXyz = new(this.encoder, 32, 3);
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        // init state
        this.lastDiff32 = default;
        this.symLastOffsetDiff = default;

        // init models and integer compressors
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
    public void Write(Span<byte> item)
    {
        this.encoder.EncodeSymbol(this.packetIndex, item[0]);

        var currentWavePacket = new WavePacket13(item);
        var lastWavePacket = new WavePacket13(this.lastItem);

        // calculate the difference between the two offsets
        var currDiff64 = BitConverter.UInt64BitsToInt64Bits(currentWavePacket.Offset) - BitConverter.UInt64BitsToInt64Bits(lastWavePacket.Offset);

        // if the current difference can be represented with 32 bits
        if (currDiff64.IsInt32())
        {
            if (currDiff64 is 0)
            {
                // current difference is zero
                this.encoder.EncodeSymbol(this.offsetDiff[this.symLastOffsetDiff], 0);
                this.symLastOffsetDiff = 0;
            }
            else if (currDiff64 == lastWavePacket.PacketSize)
            {
                this.encoder.EncodeSymbol(this.offsetDiff[this.symLastOffsetDiff], 1);
                this.symLastOffsetDiff = 1;
            }
            else
            {
                this.encoder.EncodeSymbol(this.offsetDiff[this.symLastOffsetDiff], 2);
                this.symLastOffsetDiff = 2;
                var currDiff32 = (int)currDiff64;
                this.icOffsetDiff.Compress(this.lastDiff32, currDiff32);
                this.lastDiff32 = currDiff32;
            }
        }
        else
        {
            this.encoder.EncodeSymbol(this.offsetDiff[this.symLastOffsetDiff], 3);
            this.symLastOffsetDiff = 3;

            this.encoder.WriteInt64(currentWavePacket.Offset);
        }

        this.icPacketSize.Compress((int)lastWavePacket.PacketSize, (int)currentWavePacket.PacketSize);
        this.icReturnPoint.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.ReturnPoint), BitConverter.SingleToInt32Bits(currentWavePacket.ReturnPoint));
        this.icXyz.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.X), BitConverter.SingleToInt32Bits(currentWavePacket.X));
        this.icXyz.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.Y), BitConverter.SingleToInt32Bits(currentWavePacket.Y), 1);
        this.icXyz.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.Z), BitConverter.SingleToInt32Bits(currentWavePacket.Z), 2);

        item.Slice(1, this.lastItem.Length).CopyTo(this.lastItem);
    }

    /// <inheritdoc/>
    public ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.Write(item.Span);
        return default;
    }
}