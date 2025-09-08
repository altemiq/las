// -----------------------------------------------------------------------
// <copyright file="WavePacketReader3.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

using Altemiq.IO.Las.Compression;

/// <summary>
/// The compressed wave-packet reader, version 3.
/// </summary>
internal sealed class WavePacketReader3 : IContextReader
{
    private readonly IEntropyDecoder decoder;

    private readonly Context[] contexts = new Context[4];

    private readonly LayeredValue valueWavePacket;

    private byte[] bytes = [];

    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavePacketReader3"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="decompressSelective">The selective decompress value.</param>
    public WavePacketReader3(IEntropyDecoder decoder, DecompressSelections decompressSelective = DecompressSelections.All)
    {
        this.decoder = decoder;
        this.valueWavePacket = new(decompressSelective.HasFlag(DecompressSelections.WavePacket));
        this.contexts[0] = new(this.valueWavePacket);
        this.contexts[1] = new(this.valueWavePacket);
        this.contexts[2] = new(this.valueWavePacket);
        this.contexts[3] = new(this.valueWavePacket);
    }

    /// <inheritdoc/>
    public bool ChunkSizes()
    {
        this.valueWavePacket.ByteCount = this.decoder.GetStream().ReadUInt32LittleEndian();
        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item, ref uint context)
    {
        // for layered compression 'decoder' only hands over the stream
        var stream = this.decoder.GetStream();

        // make sure the buffer is sufficiently large
        var byteCount = this.valueWavePacket.GetByteCountIfRequested();
        if (byteCount > this.bytes.Length)
        {
            this.bytes = new byte[byteCount];
        }

        // load the requested bytes and init the corresponding instreams and decoders
        _ = this.valueWavePacket.InitializeIfRequested(stream, this.bytes);

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
        var lastItem = processingContext.LastItem;

        // check for context switch
        if (this.currentContext != context)
        {
            this.currentContext = context; // all other items use context set by POINT14 reader
            if (processingContext.Unused)
            {
                this.CreateAndInitModelsAndDecompressors(this.currentContext, lastItem);
                processingContext = this.contexts[this.currentContext];
                lastItem = processingContext.LastItem;
            }
        }

        // decompress
        if (this.valueWavePacket.Changed && processingContext.Requested)
        {
            item[0] = (byte)this.valueWavePacket.Decoder.DecodeSymbol(processingContext.PacketIndex);

            var lastWavePacket = new WavePacket13(lastItem, 1);

            processingContext.SymLastOffsetDiff = this.valueWavePacket.Decoder.DecodeSymbol(processingContext.OffsetDiff[processingContext.SymLastOffsetDiff]);

            var currentWavePacket = new WavePacket13(
                processingContext.SymLastOffsetDiff switch
                {
                    0 => lastWavePacket.Offset,
                    1 => lastWavePacket.Offset + lastWavePacket.PacketSize,
                    2 => lastWavePacket.Offset + GetLastDiff(processingContext),
                    _ => this.valueWavePacket.Decoder.ReadUInt64(),
                },
                (uint)processingContext.IcPacketSize.Decompress((int)lastWavePacket.PacketSize),
                BitConverter.Int32BitsToSingle(processingContext.IcReturnPoint.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.ReturnPoint))),
                BitConverter.Int32BitsToSingle(processingContext.IcXyz.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.X))),
                BitConverter.Int32BitsToSingle(processingContext.IcXyz.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.Y), 1)),
                BitConverter.Int32BitsToSingle(processingContext.IcXyz.Decompress(BitConverter.SingleToInt32Bits(lastWavePacket.Z), 2)));

            currentWavePacket.WriteTo(item[1..]);

            item[..lastItem.Length].CopyTo(lastItem);

            static ulong GetLastDiff(Context context)
            {
                context.LastDiff32 = context.IcOffsetDiff!.Decompress(context.LastDiff32);
                return (ulong)context.LastDiff32;
            }
        }
    }

    private void CreateAndInitModelsAndDecompressors(uint context, ReadOnlySpan<byte> item)
    {
        var contextToInitialize = this.contexts[context];
        if (contextToInitialize.Requested)
        {
            // then init entropy models
            _ = contextToInitialize.PacketIndex.Initialize();
            _ = contextToInitialize.OffsetDiff[0].Initialize();
            _ = contextToInitialize.OffsetDiff[1].Initialize();
            _ = contextToInitialize.OffsetDiff[2].Initialize();
            _ = contextToInitialize.OffsetDiff[3].Initialize();
            contextToInitialize.IcOffsetDiff.Initialize();
            contextToInitialize.IcPacketSize.Initialize();
            contextToInitialize.IcReturnPoint.Initialize();
            contextToInitialize.IcXyz.Initialize();
        }

        // init current context from item
        contextToInitialize.LastDiff32 = default;
        contextToInitialize.SymLastOffsetDiff = default;
        item[..contextToInitialize.LastItem.Length].CopyTo(contextToInitialize.LastItem);

        contextToInitialize.Unused = false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context
    {
        public readonly byte[] LastItem = new byte[29];

        public readonly ISymbolModel[] OffsetDiff = new ISymbolModel[4];

        public readonly ISymbolModel? PacketIndex;

        public readonly IntegerDecompressor? IcOffsetDiff;

        public readonly IntegerDecompressor? IcPacketSize;

        public readonly IntegerDecompressor? IcReturnPoint;

        public readonly IntegerDecompressor? IcXyz;

        public bool Unused;

        public int LastDiff32;

        public uint SymLastOffsetDiff;

        public Context(LayeredValue layeredValue)
        {
            if (layeredValue.Requested)
            {
                this.PacketIndex = layeredValue.Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                this.OffsetDiff[0] = layeredValue.Decoder.CreateSymbolModel(4);
                this.OffsetDiff[1] = layeredValue.Decoder.CreateSymbolModel(4);
                this.OffsetDiff[2] = layeredValue.Decoder.CreateSymbolModel(4);
                this.OffsetDiff[3] = layeredValue.Decoder.CreateSymbolModel(4);
                this.IcOffsetDiff = new(layeredValue.Decoder, 32);
                this.IcPacketSize = new(layeredValue.Decoder, 32);
                this.IcReturnPoint = new(layeredValue.Decoder, 32);
                this.IcXyz = new(layeredValue.Decoder, 32, 3);
            }
        }

        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(PacketIndex))]
        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(IcOffsetDiff))]
        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(IcPacketSize))]
        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(IcReturnPoint))]
        [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(IcXyz))]
        public bool Requested => this.PacketIndex is not null;
    }
}