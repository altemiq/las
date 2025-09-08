// -----------------------------------------------------------------------
// <copyright file="ColorReader3.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

using Altemiq.IO.Las.Compression;

/// <summary>
/// The compressed reader for <see cref="Color"/> values, version 3.
/// </summary>
/// <param name="decoder">The decoder.</param>
/// <param name="decompressSelective">The selective decompress value.</param>
internal sealed class ColorReader3(IEntropyDecoder decoder, DecompressSelections decompressSelective = DecompressSelections.All) : IContextReader
{
    private readonly Context[] contexts = [new(decoder), new(decoder), new(decoder), new(decoder)];

    private readonly LayeredValue valueRgb = new(decompressSelective.HasFlag(DecompressSelections.RGB));

    private byte[] bytes = [];

    private uint currentContext;

    /// <inheritdoc/>
    public bool ChunkSizes()
    {
        this.valueRgb.ByteCount = decoder.GetStream().ReadUInt32LittleEndian();
        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item, ref uint context)
    {
        var stream = decoder.GetStream();

        // make sure the buffer is sufficiently large
        var byteCount = this.valueRgb.GetByteCountIfRequested();
        if (byteCount > this.bytes.Length)
        {
            this.bytes = new byte[byteCount];
        }

        // load the requested bytes and init the corresponding instreams and decoders
        _ = this.valueRgb.InitializeIfRequested(stream, this.bytes);

        // mark the four scanner channel contexts as unused
        for (var c = 0; c < 4; c++)
        {
            this.contexts[c].Unused = true;
        }

        // set scanner channel as current context
        this.currentContext = context; // all other items use context set by POINT14 reader

        // create and init models and decompressors
        this.CreateAndInitModelsAndDecompressors(this.currentContext, item);

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
                Span<byte> byteArray = stackalloc byte[sizeof(ushort) * 3];
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(byteArray, processingContext.LastItem0);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(byteArray[sizeof(ushort)..], processingContext.LastItem1);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(byteArray[(2 * sizeof(ushort))..], processingContext.LastItem2);
                this.CreateAndInitModelsAndDecompressors(this.currentContext, byteArray);
                processingContext = this.contexts[this.currentContext];
            }
        }

        // decompress
        if (this.valueRgb.Changed && !processingContext.Unused)
        {
            var sym = this.valueRgb.Decoder.DecodeSymbol(processingContext.ByteUsedModel);
            if ((sym & 1) is not 0)
            {
                var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels0);
                var value = (ushort)(corr + (processingContext.LastItem0 & 0xFF)).Fold();
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, value);
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, (ushort)(processingContext.LastItem0 & 0xFF));
            }

            if ((sym & (1 << 1)) is not 0)
            {
                var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels1);
                var original = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
                var value = (ushort)((corr + (processingContext.LastItem0 >> 8)).Fold() << 8);
                original |= value;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, original);
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, (ushort)(processingContext.LastItem0 & 0xFF00));
            }

            if ((sym & (1 << 6)) is not 0)
            {
                var diff = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) & 0x00FF) - (processingContext.LastItem0 & 0x00FF);
                if ((sym & (1 << 2)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels2);
                    var value = (ushort)(corr + (diff + (processingContext.LastItem1 & 0xFF)).Clamp()).Fold();
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], value);
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], (ushort)(processingContext.LastItem1 & 0xFF));
                }

                if ((sym & (1 << 4)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels4);
                    diff = (diff + ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) & 0x00FF) - (processingContext.LastItem1 & 0x00FF))) / 2;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], (corr + (diff + (processingContext.LastItem2 & 0xFF)).Clamp()).Fold());
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], (ushort)(processingContext.LastItem2 & 0xFF));
                }

                diff = (System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item) >> 8) - (processingContext.LastItem0 >> 8);
                if ((sym & (1 << 3)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels3);
                    var original = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
                    var value = (ushort)((corr + (diff + (processingContext.LastItem1 >> 8)).Clamp()).Fold() << 8);
                    original |= value;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], original);
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], (ushort)(processingContext.LastItem1 & 0xFF00));
                }

                if ((sym & (1 << 5)) is not 0)
                {
                    var corr = (byte)this.valueRgb.Decoder.DecodeSymbol(processingContext.RgbDiffModels5);
                    diff = (diff + ((System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]) >> 8) - (processingContext.LastItem1 >> 8))) / 2;
                    var original = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
                    var value = (ushort)((corr + (diff + (processingContext.LastItem2 >> 8)).Clamp()).Fold() << 8);
                    original |= value;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], original);
                }
                else
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], (ushort)(processingContext.LastItem2 & 0xFF00));
                }
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item));
            }

            processingContext.LastItem0 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
            processingContext.LastItem1 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
            processingContext.LastItem2 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);
        }
        else
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item, processingContext.LastItem0);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[sizeof(ushort)..], processingContext.LastItem1);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(item[(2 * sizeof(ushort))..], processingContext.LastItem2);
        }
    }

    private void CreateAndInitModelsAndDecompressors(uint context, ReadOnlySpan<byte> item)
    {
        // first create all entropy models (if needed)
        var contextToInitialize = this.contexts[context];

        // then init entropy models
        _ = contextToInitialize.ByteUsedModel.Initialize();
        _ = contextToInitialize.RgbDiffModels0.Initialize();
        _ = contextToInitialize.RgbDiffModels1.Initialize();
        _ = contextToInitialize.RgbDiffModels2.Initialize();
        _ = contextToInitialize.RgbDiffModels3.Initialize();
        _ = contextToInitialize.RgbDiffModels4.Initialize();
        _ = contextToInitialize.RgbDiffModels5.Initialize();

        // init current context from item
        contextToInitialize.LastItem0 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item);
        contextToInitialize.LastItem1 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[sizeof(ushort)..]);
        contextToInitialize.LastItem2 = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(item[(2 * sizeof(ushort))..]);

        contextToInitialize.Unused = false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context(IEntropyDecoder decoder)
    {
        public readonly ISymbolModel ByteUsedModel = decoder.CreateSymbolModel(ArithmeticCoder.HalfModelCount);

        public readonly ISymbolModel RgbDiffModels0 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        public readonly ISymbolModel RgbDiffModels1 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        public readonly ISymbolModel RgbDiffModels2 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        public readonly ISymbolModel RgbDiffModels3 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        public readonly ISymbolModel RgbDiffModels4 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        public readonly ISymbolModel RgbDiffModels5 = decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);

        public ushort LastItem0;
        public ushort LastItem1;
        public ushort LastItem2;

        public bool Unused;
    }
}