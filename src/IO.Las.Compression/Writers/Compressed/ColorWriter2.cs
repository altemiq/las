// -----------------------------------------------------------------------
// <copyright file="ColorWriter2.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed writer for <see cref="Color"/> values, version 2.
/// </summary>
/// <param name="encoder">The encoder.</param>
internal sealed class ColorWriter2(IEntropyEncoder encoder) : ISimpleWriter
{
    private readonly ISymbolModel byteUsedModel = encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
    private readonly ISymbolModel rgbDiffModels0 = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    private readonly ISymbolModel rgbDiffModels1 = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    private readonly ISymbolModel rgbDiffModels2 = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    private readonly ISymbolModel rgbDiffModels3 = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    private readonly ISymbolModel rgbDiffModels4 = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
    private readonly ISymbolModel rgbDiffModels5 = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);

    private readonly ushort[] lastItem = new ushort[3];

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        // init models and integer compressors
        _ = this.byteUsedModel.Initialize();
        _ = this.rgbDiffModels0.Initialize();
        _ = this.rgbDiffModels1.Initialize();
        _ = this.rgbDiffModels2.Initialize();
        _ = this.rgbDiffModels3.Initialize();
        _ = this.rgbDiffModels4.Initialize();
        _ = this.rgbDiffModels5.Initialize();

        this.lastItem[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        this.lastItem[1] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        this.lastItem[2] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);

        return true;
    }

    /// <inheritdoc/>
    public void Write(Span<byte> item)
    {
        var sym = ((this.lastItem[0] & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) ? 1U : 0U) << 0;
        sym |= ((this.lastItem[0] & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF00) ? 1U : 0U) << 1;
        sym |= ((this.lastItem[1] & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0x00FF) ? 1U : 0U) << 2;
        sym |= ((this.lastItem[1] & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF00) ? 1U : 0U) << 3;
        sym |= ((this.lastItem[2] & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0x00FF) ? 1U : 0U) << 4;
        sym |= ((this.lastItem[2] & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0xFF00) ? 1U : 0U) << 5;
        sym |= (((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0x00FF) ? 1U : 0U)
            | ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0x00FF) ? 1U : 0U)
            | ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF00) ? 1U : 0U)
            | ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0xFF00) ? 1U : 0U)) << 6;
        encoder.EncodeSymbol(this.byteUsedModel, sym);

        var diffL = default(int);
        if ((sym & 1) is not 0)
        {
            diffL = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF) - (this.lastItem[0] & 0xFF);
            encoder.EncodeSymbol(this.rgbDiffModels0, diffL.Fold());
        }

        var diffH = default(int);
        if ((sym & (1 << 1)) is not 0)
        {
            diffH = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) >> 8) - (this.lastItem[0] >> 8);
            encoder.EncodeSymbol(this.rgbDiffModels1, diffH.Fold());
        }

        if ((sym & (1 << 6)) is not 0)
        {
            if ((sym & (1 << 2)) is not 0)
            {
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF) - (diffL + (this.lastItem[1] & 0xFF).Clamp());
                encoder.EncodeSymbol(this.rgbDiffModels2, corr.Fold());
            }

            if ((sym & (1 << 4)) is not 0)
            {
                diffL = (diffL + (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF) - (this.lastItem[1] & 0xFF)) / 2;
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0xFF) - (diffL + (this.lastItem[2] & 0xFF)).Clamp();
                encoder.EncodeSymbol(this.rgbDiffModels4, corr.Fold());
            }

            if ((sym & (1 << 3)) is not 0)
            {
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) >> 8) - (diffH + (this.lastItem[1] >> 8)).Clamp();
                encoder.EncodeSymbol(this.rgbDiffModels3, corr.Fold());
            }

            if ((sym & (1 << 5)) is not 0)
            {
                diffH = (diffH + (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) >> 8) - (this.lastItem[1] >> 8)) / 2;
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) >> 8) - (diffH + (this.lastItem[2] >> 8)).Clamp();
                encoder.EncodeSymbol(this.rgbDiffModels5, corr.Fold());
            }
        }

        this.lastItem[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        this.lastItem[1] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        this.lastItem[2] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
    }

    /// <inheritdoc/>
    public ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.Write(item.Span);
        return default;
    }
}