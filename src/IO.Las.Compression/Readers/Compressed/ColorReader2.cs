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
    private readonly ArithmeticDecoder decoder;

    private readonly ArithmeticSymbolModel byteUsedModel;

    private readonly ArithmeticSymbolModel rgbDiffModels0;
    private readonly ArithmeticSymbolModel rgbDiffModels1;
    private readonly ArithmeticSymbolModel rgbDiffModels2;
    private readonly ArithmeticSymbolModel rgbDiffModels3;
    private readonly ArithmeticSymbolModel rgbDiffModels4;
    private readonly ArithmeticSymbolModel rgbDiffModels5;

    private ushort lastRed;
    private ushort lastGreen;
    private ushort lastBlue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorReader2"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    public ColorReader2(ArithmeticDecoder decoder)
    {
        ArgumentNullException.ThrowIfNull(decoder);
        this.decoder = decoder;

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

        // Snapshot last values once; work in locals (red/green/blue as two halves each)
        // and emit a single LE write per component at the end. This avoids the
        // ~20 span read/write round-trips the previous version did on the 6-byte
        // destination buffer.
        var lastRedLo = this.lastRed & 0xFF;
        var lastRedHi = (this.lastRed >> 8) & 0xFF;
        var lastGreenLo = this.lastGreen & 0xFF;
        var lastGreenHi = (this.lastGreen >> 8) & 0xFF;
        var lastBlueLo = this.lastBlue & 0xFF;
        var lastBlueHi = (this.lastBlue >> 8) & 0xFF;

        // Red low byte
        var redLo = (sym & 1) is not 0
            ? ((byte)this.decoder.DecodeSymbol(this.rgbDiffModels0) + lastRedLo).Fold()
            : lastRedLo;

        // Red high byte
        var redHi = (sym & (1 << 1)) is not 0
            ? ((byte)this.decoder.DecodeSymbol(this.rgbDiffModels1) + lastRedHi).Fold()
            : lastRedHi;

        int greenLo;
        int greenHi;
        int blueLo;
        int blueHi;
        if ((sym & (1 << 6)) is not 0)
        {
            // Low-byte green
            var diff = redLo - lastRedLo;
            greenLo = (sym & (1 << 2)) is not 0
                ? ((byte)this.decoder.DecodeSymbol(this.rgbDiffModels2) + (diff + lastGreenLo).Clamp()).Fold()
                : lastGreenLo;

            // Low-byte blue; diff halves into the green/red average
            var blueDiffLo = (diff + (greenLo - lastGreenLo)) / 2;
            blueLo = (sym & (1 << 4)) is not 0
                ? ((byte)this.decoder.DecodeSymbol(this.rgbDiffModels4) + (blueDiffLo + lastBlueLo).Clamp()).Fold()
                : lastBlueLo;

            // High-byte green
            diff = redHi - lastRedHi;
            greenHi = (sym & (1 << 3)) is not 0
                ? ((byte)this.decoder.DecodeSymbol(this.rgbDiffModels3) + (diff + lastGreenHi).Clamp()).Fold()
                : lastGreenHi;

            // High-byte blue
            var blueDiffHi = (diff + (greenHi - lastGreenHi)) / 2;
            blueHi = (sym & (1 << 5)) is not 0
                ? ((byte)this.decoder.DecodeSymbol(this.rgbDiffModels5) + (blueDiffHi + lastBlueHi).Clamp()).Fold()
                : lastBlueHi;
        }
        else
        {
            // When bit 6 is unset, green and blue both take the red value.
            greenLo = redLo;
            greenHi = redHi;
            blueLo = redLo;
            blueHi = redHi;
        }

        var red = (ushort)(redLo | (redHi << 8));
        var green = (ushort)(greenLo | (greenHi << 8));
        var blue = (ushort)(blueLo | (blueHi << 8));

        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, red);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], green);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], blue);

        this.lastRed = red;
        this.lastGreen = green;
        this.lastBlue = blue;
    }
}