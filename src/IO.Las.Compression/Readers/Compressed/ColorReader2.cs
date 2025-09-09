// -----------------------------------------------------------------------
// <copyright file="ColorReader2.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The color reader.
/// </summary>
internal sealed class ColorReader2 : ISimpleReader
{
    private readonly IEntropyDecoder decoder;

    private readonly ISymbolModel byteUsedModel;

    private readonly ISymbolModel rgbDiffModels0;
    private readonly ISymbolModel rgbDiffModels1;
    private readonly ISymbolModel rgbDiffModels2;
    private readonly ISymbolModel rgbDiffModels3;
    private readonly ISymbolModel rgbDiffModels4;
    private readonly ISymbolModel rgbDiffModels5;

    private ushort lastRed;
    private ushort lastGreen;
    private ushort lastBlue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorReader2"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    public ColorReader2(IEntropyDecoder decoder)
    {
        this.decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));

        // create models and integer compressors
        this.byteUsedModel = this.decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
        this.rgbDiffModels0 = this.decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.rgbDiffModels1 = this.decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.rgbDiffModels2 = this.decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.rgbDiffModels3 = this.decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.rgbDiffModels4 = this.decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        this.rgbDiffModels5 = this.decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        _ = this.byteUsedModel.Initialize();
        _ = this.rgbDiffModels0.Initialize();
        _ = this.rgbDiffModels1.Initialize();
        _ = this.rgbDiffModels2.Initialize();
        _ = this.rgbDiffModels3.Initialize();
        _ = this.rgbDiffModels4.Initialize();
        _ = this.rgbDiffModels5.Initialize();

        // init last item
        this.lastRed = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        this.lastGreen = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        this.lastBlue = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);

        return true;
    }

    /// <inheritdoc/>
    public void Read(Span<byte> item)
    {
        var sym = this.decoder.DecodeSymbol(this.byteUsedModel);
        if ((sym & 1) is not 0)
        {
            var corr = (byte)this.decoder.DecodeSymbol(this.rgbDiffModels0);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, (corr + (this.lastRed & 0xFF)).Fold());
        }
        else
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, (ushort)(this.lastRed & 0xFF));
        }

        if ((sym & (1 << 1)) is not 0)
        {
            var corr = (byte)this.decoder.DecodeSymbol(this.rgbDiffModels1);
            var value = (ushort)(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) | (ushort)((corr + (this.lastRed >> 8)).Fold() << 8));
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, value);
        }
        else
        {
            var value = (ushort)(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) | (this.lastRed & 0xFF00));
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, value);
        }

        if ((sym & (1 << 6)) is not 0)
        {
            var diff = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) - (this.lastRed & 0x00FF);
            if ((sym & (1 << 2)) is not 0)
            {
                var corr = (byte)this.decoder.DecodeSymbol(this.rgbDiffModels2);
                ushort value = (corr + (diff + (this.lastGreen & 0xFF)).Clamp()).Fold();
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], value);
            }
            else
            {
                var value = (ushort)(this.lastGreen & 0xFF);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], value);
            }

            if ((sym & (1 << 4)) is not 0)
            {
                var corr = (byte)this.decoder.DecodeSymbol(this.rgbDiffModels4);
                diff = (diff + ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0x00FF) - (this.lastGreen & 0x00FF))) / 2;
                ushort value = (corr + (diff + (this.lastBlue & 0xFF)).Clamp()).Fold();
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], value);
            }
            else
            {
                var value = (ushort)(this.lastBlue & 0xFF);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], value);
            }

            diff = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) >> 8) - (this.lastRed >> 8);
            if ((sym & (1 << 3)) is not 0)
            {
                var corr = (byte)this.decoder.DecodeSymbol(this.rgbDiffModels3);
                var value = (ushort)(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) | (ushort)((corr + (diff + (this.lastGreen >> 8)).Clamp()).Fold() << 8));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], value);
            }
            else
            {
                var value = (ushort)(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) | (this.lastGreen & 0xFF00));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], value);
            }

            if ((sym & (1 << 5)) is not 0)
            {
                var corr = (byte)this.decoder.DecodeSymbol(this.rgbDiffModels5);
                diff = (diff + ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) >> 8) - (this.lastGreen >> 8))) / 2;
                var value = (ushort)(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) | ((corr + (diff + (this.lastBlue >> 8)).Clamp()).Fold() << 8));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], value);
            }
            else
            {
                var value = (ushort)(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) | (ushort)(this.lastBlue & 0xFF00));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], value);
            }
        }
        else
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item));
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item));
        }

        this.lastRed = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        this.lastGreen = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        this.lastBlue = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
    }
}