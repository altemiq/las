// -----------------------------------------------------------------------
// <copyright file="ColorNearInfraredReader3.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

using Altemiq.IO.Las.Compression;

/// <summary>
/// The compressed reader for <see cref="Color"/> with near-infrared values, version 3.
/// </summary>
internal sealed class ColorNearInfraredReader3 : IContextReader
{
    private readonly Context[] contexts = new Context[4];

    private readonly LayeredValue valueRgb;

    private readonly LayeredValue valueNir;

    private readonly IEntropyDecoder decoder;

    private byte[] bytes = [];

    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorNearInfraredReader3"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="decompressSelective">The selective decompress value.</param>
    public ColorNearInfraredReader3(IEntropyDecoder decoder, DecompressSelections decompressSelective = DecompressSelections.All)
    {
        this.decoder = decoder;
        this.valueRgb = new(decompressSelective.HasFlag(DecompressSelections.RGB));
        this.valueNir = new(decompressSelective.HasFlag(DecompressSelections.NIR));
        this.contexts[0] = new(this.valueRgb, this.valueNir);
        this.contexts[1] = new(this.valueRgb, this.valueNir);
        this.contexts[2] = new(this.valueRgb, this.valueNir);
        this.contexts[3] = new(this.valueRgb, this.valueNir);
    }

    /// <inheritdoc/>
    public bool ChunkSizes()
    {
        this.valueRgb.ByteCount = this.decoder.GetStream().ReadUInt32LittleEndian();
        this.valueNir.ByteCount = this.decoder.GetStream().ReadUInt32LittleEndian();
        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item, ref uint context)
    {
        var stream = this.decoder.GetStream();

        var byteCount = this.valueRgb.GetByteCountIfRequested()
                        + this.valueNir.GetByteCountIfRequested();

        // make sure the buffer is sufficiently large
        if (byteCount > this.bytes.Length)
        {
            this.bytes = new byte[byteCount];
        }

        // load the requested bytes and init the corresponding instreams and decoders
        var index = this.valueRgb.InitializeIfRequested(stream, this.bytes);
        _ = this.valueNir.InitializeIfRequested(stream, this.bytes, (int)index);

        // mark the four scanner channel contexts as unused
        for (var c = 0; c < 4; c++)
        {
            this.contexts[c].Unused = true;
        }

        // set scanner channel as current context
        this.currentContext = context; // all other items use context set by POINT14 reader

        // create and init models and decompressors
        var startIndex = item.Length - 8;
        this.CreateAndInitModelsAndDecompressors(this.currentContext, item[startIndex..]);

        return true;
    }

    /// <inheritdoc/>
    public void Read(Span<byte> item, uint context)
    {
        // get last
        var processingContext = this.contexts[this.currentContext];

        // check for context switch
        if (this.currentContext != context)
        {
            this.currentContext = context; // all other items use context set by POINT14 reader
            if (processingContext.Unused)
            {
                Span<byte> byteArray = stackalloc byte[3 * sizeof(ushort)];
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(byteArray, processingContext.LastRed);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(byteArray[sizeof(ushort)..], processingContext.LastGreen);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(byteArray[(2 * sizeof(ushort))..], processingContext.LastBlue);
                this.CreateAndInitModelsAndDecompressors(this.currentContext, byteArray);
                processingContext = this.contexts[this.currentContext];
            }
        }

        // decompress
        if (this.valueRgb.Changed && processingContext.RgbRequested)
        {
            var sym = this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbByteUsedModel);
            if ((sym & 1) is not 0)
            {
                var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels0);
                var value = (ushort)(corr + (processingContext.LastRed & 0xFF)).Fold();
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, value);
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, (ushort)(processingContext.LastRed & 0xFF));
            }

            if ((sym & (1 << 1)) is not 0)
            {
                var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels1);
                var original = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
                var value = (ushort)((corr + (processingContext.LastRed >> 8)).Fold() << 8);
                original |= value;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, original);
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, (ushort)(processingContext.LastRed & 0xFF00));
            }

            if ((sym & (1 << 6)) is not 0)
            {
                var diff = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) - (processingContext.LastRed & 0x00FF);
                if ((sym & (1 << 2)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels2);
                    var value = (ushort)(corr + (diff + (processingContext.LastGreen & 0xFF)).Clamp()).Fold();
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], value);
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], (ushort)(processingContext.LastGreen & 0xFF));
                }

                if ((sym & (1 << 4)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels4);
                    diff = (diff + ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0x00FF) - (processingContext.LastGreen & 0x00FF))) / 2;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], (corr + (diff + (processingContext.LastBlue & 0xFF)).Clamp()).Fold());
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], (ushort)(processingContext.LastBlue & 0xFF));
                }

                diff = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) >> 8) - (processingContext.LastRed >> 8);
                if ((sym & (1 << 3)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels3);
                    var original = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
                    var value = (ushort)((corr + (diff + (processingContext.LastGreen >> 8)).Clamp()).Fold() << 8);
                    original |= value;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], original);
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], (ushort)(processingContext.LastGreen & 0xFF00));
                }

                if ((sym & (1 << 5)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels5);
                    diff = (diff + ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) >> 8) - (processingContext.LastGreen >> 8))) / 2;
                    var original = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
                    var value = (ushort)((corr + (diff + (processingContext.LastBlue >> 8)).Clamp()).Fold() << 8);
                    original |= value;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], original);
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], (ushort)(processingContext.LastBlue & 0xFF00));
                }
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item));
            }

            processingContext.LastRed = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
            processingContext.LastGreen = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
            processingContext.LastBlue = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
        }
        else
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, processingContext.LastRed);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], processingContext.LastGreen);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], processingContext.LastBlue);
        }

        if (this.valueNir.Changed && processingContext.NirRequested)
        {
            var sym = this.valueNir.Decoder.DecodeSymbol(processingContext.NirByteUsedModel);
            if ((sym & 1) is not 0)
            {
                var corr = (byte)this.valueNir.Decoder.DecodeSymbol(processingContext.NirDiffModels0);
                var value = (ushort)(corr + (processingContext.LastNir & 0xFF)).Fold();
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(3 * sizeof(ushort))..], value);
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(3 * sizeof(ushort))..], (ushort)(processingContext.LastNir & 0xFF));
            }

            if ((sym & (1 << 1)) is not 0)
            {
                var corr = (byte)this.valueNir.Decoder.DecodeSymbol(processingContext.NirDiffModels1);
                var original = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(3 * sizeof(ushort))..]);
                var value = (ushort)((corr + (processingContext.LastNir >> 8)).Fold() << 8);
                original |= value;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(3 * sizeof(ushort))..], original);
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(3 * sizeof(ushort))..], (ushort)(processingContext.LastNir & 0xFF00));
            }

            processingContext.LastNir = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(3 * sizeof(ushort))..]);
        }
        else
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(3 * sizeof(ushort))..], processingContext.LastNir);
        }
    }

    private void CreateAndInitModelsAndDecompressors(uint context, ReadOnlySpan<byte> item)
    {
        // first create all entropy models (if needed)
        var contextToInitialize = this.contexts[context];
        if (contextToInitialize.RgbRequested)
        {
            // then init entropy models
            _ = contextToInitialize.RgbByteUsedModel.Initialize();
            _ = contextToInitialize.RgbDiffModels0.Initialize();
            _ = contextToInitialize.RgbDiffModels1.Initialize();
            _ = contextToInitialize.RgbDiffModels2.Initialize();
            _ = contextToInitialize.RgbDiffModels3.Initialize();
            _ = contextToInitialize.RgbDiffModels4.Initialize();
            _ = contextToInitialize.RgbDiffModels5.Initialize();
        }

        if (contextToInitialize.NirRequested)
        {
            // then init entropy models
            _ = contextToInitialize.NirByteUsedModel.Initialize();
            _ = contextToInitialize.NirDiffModels0.Initialize();
            _ = contextToInitialize.NirDiffModels1.Initialize();
        }

        // init current context from item
        contextToInitialize.LastRed = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        contextToInitialize.LastGreen = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        contextToInitialize.LastBlue = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
        contextToInitialize.LastNir = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(3 * sizeof(ushort))..]);

        contextToInitialize.Unused = false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context
    {
        public readonly ISymbolModel? RgbByteUsedModel;

        public readonly ISymbolModel? RgbDiffModels0;
        public readonly ISymbolModel? RgbDiffModels1;
        public readonly ISymbolModel? RgbDiffModels2;
        public readonly ISymbolModel? RgbDiffModels3;
        public readonly ISymbolModel? RgbDiffModels4;
        public readonly ISymbolModel? RgbDiffModels5;

        public readonly ISymbolModel? NirByteUsedModel;
        public readonly ISymbolModel? NirDiffModels0;
        public readonly ISymbolModel? NirDiffModels1;

        public ushort LastRed;
        public ushort LastGreen;
        public ushort LastBlue;
        public ushort LastNir;

        public bool Unused;

        public Context(LayeredValue rgb, LayeredValue nir)
        {
            if (rgb.Requested)
            {
                this.RgbByteUsedModel = rgb.Decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);
                this.RgbDiffModels0 = rgb.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.RgbDiffModels1 = rgb.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.RgbDiffModels2 = rgb.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.RgbDiffModels3 = rgb.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.RgbDiffModels4 = rgb.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.RgbDiffModels5 = rgb.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
            }

            if (nir.Requested)
            {
                this.NirByteUsedModel = nir.Decoder.CreateSymbolModel(4);
                this.NirDiffModels0 = nir.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.NirDiffModels1 = nir.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
            }
        }

        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(RgbByteUsedModel), nameof(RgbDiffModels0), nameof(RgbDiffModels1), nameof(RgbDiffModels2), nameof(RgbDiffModels3), nameof(RgbDiffModels4), nameof(RgbDiffModels5))]
        public bool RgbRequested => this.RgbByteUsedModel is not null;

        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(NirByteUsedModel), nameof(NirDiffModels0), nameof(NirDiffModels1))]
        public bool NirRequested => this.NirByteUsedModel is not null;
    }
}