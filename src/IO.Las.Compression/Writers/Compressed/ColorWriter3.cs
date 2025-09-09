// -----------------------------------------------------------------------
// <copyright file="ColorWriter3.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed writer for <see cref="Color"/> values, version 3.
/// </summary>
internal sealed class ColorWriter3 : IContextWriter
{
    private readonly IEntropyEncoder encoder;

    private readonly Context[] contexts = new Context[4];

    private readonly LayeredValue valueRgb;

    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorWriter3"/> class.
    /// </summary>
    /// <param name="encoder">The encoder.</param>
    public ColorWriter3(IEntropyEncoder encoder)
    {
        this.encoder = encoder;
        this.valueRgb = new();
        this.contexts[0] = new(this.valueRgb.Encoder);
        this.contexts[1] = new(this.valueRgb.Encoder);
        this.contexts[2] = new(this.valueRgb.Encoder);
        this.contexts[3] = new(this.valueRgb.Encoder);
    }

    /// <inheritdoc/>
    public bool ChunkBytes()
    {
        var stream = this.encoder.GetStream();

        this.valueRgb.CopyToStreamIfChanged(stream);

        return true;
    }

    /// <inheritdoc/>
    public bool ChunkSizes()
    {
        var stream = this.encoder.GetStream();

        // finish the encoders
        this.valueRgb.EncoderDone();

        // output the sizes of all layer (i.e. number of bytes per layer)
        stream.WriteUInt32LittleEndian(this.valueRgb.GetByteCountIfChanged());

        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item, ref uint context)
    {
        this.valueRgb.Initialize();

        this.currentContext = context; // all other items use context set by POINT14 writer

        this.CreateAndInitModelsAndCompressors(this.currentContext, item);

        return true;
    }

    /// <inheritdoc/>
    public void Write(Span<byte> item, ref uint context)
    {
        var processingContext = this.contexts[this.currentContext];
        var lastItem = processingContext.LastItem;

        // check for context switch
        if (this.currentContext != context)
        {
            this.currentContext = context; // all other items use context set by POINT14 writer
            if (processingContext.Unused)
            {
                Span<byte> bytes = stackalloc byte[sizeof(ushort) * 3];
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes, lastItem[0]);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[sizeof(ushort)..], lastItem[1]);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes[(sizeof(ushort) + sizeof(ushort))..], lastItem[2]);
                this.CreateAndInitModelsAndCompressors(this.currentContext, bytes);
                processingContext = this.contexts[this.currentContext];
                lastItem = processingContext.LastItem;
            }
        }

        // compress
        var sym = ((lastItem[0] & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) ? 1U : 0U) << 0;
        sym |= ((lastItem[0] & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF00) ? 1U : 0U) << 1;
        sym |= ((lastItem[1] & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0x00FF) ? 1U : 0U) << 2;
        sym |= ((lastItem[1] & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF00) ? 1U : 0U) << 3;
        sym |= ((lastItem[2] & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0x00FF) ? 1U : 0U) << 4;
        sym |= ((lastItem[2] & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0xFF00) ? 1U : 0U) << 5;
        sym |= (((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0x00FF) ? 1U : 0U)
            | ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0x00FF) ? 1U : 0U)
            | ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF00) ? 1U : 0U)
            | ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF00) != (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0xFF00) ? 1U : 0U)) << 6;
        this.valueRgb.Encoder.EncodeSymbol(processingContext.ByteUsedModel, sym);

        var diffL = default(int);
        if ((sym & 1) is not 0)
        {
            diffL = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0xFF) - (lastItem[0] & 0xFF);
            this.valueRgb.Encoder.EncodeSymbol(processingContext.RgbDiffModels[0], diffL.Fold());
        }

        var diffH = default(int);
        if ((sym & (1 << 1)) is not 0)
        {
            diffH = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) >> 8) - (lastItem[0] >> 8);
            this.valueRgb.Encoder.EncodeSymbol(processingContext.RgbDiffModels[1], diffH.Fold());
        }

        if ((sym & (1 << 6)) is not 0)
        {
            if ((sym & (1 << 2)) is not 0)
            {
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF) - (diffL + (lastItem[1] & 0xFF).Clamp());
                this.valueRgb.Encoder.EncodeSymbol(processingContext.RgbDiffModels[2], corr.Fold());
            }

            if ((sym & (1 << 4)) is not 0)
            {
                diffL = (diffL + (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0xFF) - (lastItem[1] & 0xFF)) / 2;
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) & 0xFF) - (diffL + (lastItem[2] & 0xFF)).Clamp();
                this.valueRgb.Encoder.EncodeSymbol(processingContext.RgbDiffModels[4], corr.Fold());
            }

            if ((sym & (1 << 3)) is not 0)
            {
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) >> 8) - (diffH + (lastItem[1] >> 8)).Clamp();
                this.valueRgb.Encoder.EncodeSymbol(processingContext.RgbDiffModels[3], corr.Fold());
            }

            if ((sym & (1 << 5)) is not 0)
            {
                diffH = (diffH + (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) >> 8) - (lastItem[1] >> 8)) / 2;
                var corr = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]) >> 8) - (diffH + (lastItem[2] >> 8)).Clamp();
                this.valueRgb.Encoder.EncodeSymbol(processingContext.RgbDiffModels[5], corr.Fold());
            }
        }

        this.valueRgb.Changed |= sym is not 0;

        lastItem[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        lastItem[1] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        lastItem[2] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
    }

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0042:Do not use blocking calls in an async method", Justification = "This would cause recursion")]
    public ValueTask<uint> WriteAsync(Memory<byte> item, uint context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.Write(item.Span, ref context);
        return new(context);
    }

    private void CreateAndInitModelsAndCompressors(uint context, ReadOnlySpan<byte> item)
    {
        var contextToInitialize = this.contexts[context];

        // then init entropy models
        _ = contextToInitialize.ByteUsedModel.Initialize();
        _ = contextToInitialize.RgbDiffModels[0].Initialize();
        _ = contextToInitialize.RgbDiffModels[1].Initialize();
        _ = contextToInitialize.RgbDiffModels[2].Initialize();
        _ = contextToInitialize.RgbDiffModels[3].Initialize();
        _ = contextToInitialize.RgbDiffModels[4].Initialize();
        _ = contextToInitialize.RgbDiffModels[5].Initialize();

        // init current context from item
        contextToInitialize.LastItem[0] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        contextToInitialize.LastItem[1] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        contextToInitialize.LastItem[2] = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);

        contextToInitialize.Unused = false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context(IEntropyEncoder encoder)
    {
        public readonly ushort[] LastItem = new ushort[3];

        public readonly ISymbolModel ByteUsedModel = encoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);

        public readonly ISymbolModel[] RgbDiffModels =
        [
            encoder.CreateSymbolModel(ArithmeticCoder.ModelCount),
            encoder.CreateSymbolModel(ArithmeticCoder.ModelCount),
            encoder.CreateSymbolModel(ArithmeticCoder.ModelCount),
            encoder.CreateSymbolModel(ArithmeticCoder.ModelCount),
            encoder.CreateSymbolModel(ArithmeticCoder.ModelCount),
            encoder.CreateSymbolModel(ArithmeticCoder.ModelCount),
        ];

        public bool Unused = true;
    }
}