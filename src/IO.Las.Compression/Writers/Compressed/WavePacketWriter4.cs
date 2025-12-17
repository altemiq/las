// -----------------------------------------------------------------------
// <copyright file="WavePacketWriter4.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed writer for <see cref="Color"/> values, version 4.
/// </summary>
internal sealed class WavePacketWriter4 : IContextWriter
{
    private readonly IEntropyEncoder encoder;

    private readonly Context[] contexts = new Context[4];

    private readonly LayeredValue valueWavePacket;

    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="WavePacketWriter4"/> class.
    /// </summary>
    /// <param name="encoder">The encoder.</param>
    public WavePacketWriter4(IEntropyEncoder encoder)
    {
        this.encoder = encoder;
        this.valueWavePacket = new();
        this.contexts[0] = new(this.valueWavePacket.Encoder);
        this.contexts[1] = new(this.valueWavePacket.Encoder);
        this.contexts[2] = new(this.valueWavePacket.Encoder);
        this.contexts[3] = new(this.valueWavePacket.Encoder);
    }

    /// <inheritdoc/>
    public bool ChunkBytes()
    {
        var stream = this.encoder.GetStream();

        this.valueWavePacket.CopyToIfChanged(stream);

        return true;
    }

    /// <inheritdoc/>
    public bool ChunkSizes()
    {
        var stream = this.encoder.GetStream();

        // finish the encoders
        this.valueWavePacket.Encoder.Done();

        // output the sizes of all layer (i.e. number of bytes per layer)
        stream.WriteUInt32LittleEndian(this.valueWavePacket.GetByteCountIfChanged());

        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item, ref uint context)
    {
        this.valueWavePacket.Initialize();

        for (var c = 0; c < 4; c++)
        {
            this.contexts[c].Unused = true;
        }

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
            if (this.contexts[this.currentContext].Unused)
            {
                this.CreateAndInitModelsAndCompressors(this.currentContext, lastItem);
            }

            processingContext = this.contexts[this.currentContext];
            lastItem = processingContext.LastItem;
        }

        // compare arrays
        this.valueWavePacket.Changed |= !SpansEquals(item, lastItem, lastItem.Length);

        // compress
        this.valueWavePacket.Encoder.EncodeSymbol(processingContext.PacketIndex, item[0]);

        var currentWavePacket = new WavePacket13(item[1..]);
        var lastWavePacket = new WavePacket13(lastItem, 1);

        // calculate the difference between the two offsets
        var currDiff64 = BitConverter.UInt64BitsToInt64Bits(currentWavePacket.Offset) - BitConverter.UInt64BitsToInt64Bits(lastWavePacket.Offset);

        // if the current difference can be represented with 32 bits
        if (currDiff64.IsInt32())
        {
            var currDiff32 = (int)currDiff64;
            if (currDiff32 is 0)
            {
                // current difference is zero
                this.valueWavePacket.Encoder.EncodeSymbol(processingContext.OffsetDiff[processingContext.SymLastOffsetDiff], 0);
                processingContext.SymLastOffsetDiff = 0;
            }
            else if (currDiff32 == lastWavePacket.PacketSize)
            {
                this.valueWavePacket.Encoder.EncodeSymbol(processingContext.OffsetDiff[processingContext.SymLastOffsetDiff], 1);
                processingContext.SymLastOffsetDiff = 1;
            }
            else
            {
                this.valueWavePacket.Encoder.EncodeSymbol(processingContext.OffsetDiff[processingContext.SymLastOffsetDiff], 2);
                processingContext.SymLastOffsetDiff = 2;
                processingContext.IcOffsetDiff.Compress(processingContext.LastDiff32, currDiff32);
                processingContext.LastDiff32 = currDiff32;
            }
        }
        else
        {
            this.valueWavePacket.Encoder.EncodeSymbol(processingContext.OffsetDiff[processingContext.SymLastOffsetDiff], 3);
            processingContext.SymLastOffsetDiff = 3;

            this.valueWavePacket.Encoder.WriteInt64(currentWavePacket.Offset);
        }

        processingContext.IcPacketSize.Compress((int)lastWavePacket.PacketSize, (int)currentWavePacket.PacketSize);
        processingContext.IcReturnPoint.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.ReturnPoint), BitConverter.SingleToInt32Bits(currentWavePacket.ReturnPoint));
        processingContext.IcXyz.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.X), BitConverter.SingleToInt32Bits(currentWavePacket.X));
        processingContext.IcXyz.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.Y), BitConverter.SingleToInt32Bits(currentWavePacket.Y), 1);
        processingContext.IcXyz.Compress(BitConverter.SingleToInt32Bits(lastWavePacket.Z), BitConverter.SingleToInt32Bits(currentWavePacket.Z), 2);

        item[..lastItem.Length].CopyTo(lastItem);

        static bool SpansEquals(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <inheritdoc/>
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
        _ = contextToInitialize.PacketIndex.Initialize();
        _ = contextToInitialize.OffsetDiff[0].Initialize();
        _ = contextToInitialize.OffsetDiff[1].Initialize();
        _ = contextToInitialize.OffsetDiff[2].Initialize();
        _ = contextToInitialize.OffsetDiff[3].Initialize();

        contextToInitialize.IcOffsetDiff.Initialize();
        contextToInitialize.IcPacketSize.Initialize();
        contextToInitialize.IcReturnPoint.Initialize();
        contextToInitialize.IcXyz.Initialize();

        // init current context from item
        contextToInitialize.LastDiff32 = default;
        contextToInitialize.SymLastOffsetDiff = default;
        item[..contextToInitialize.LastItem.Length].CopyTo(contextToInitialize.LastItem);

        contextToInitialize.Unused = false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context(IEntropyEncoder encoder)
    {
        public readonly byte[] LastItem = new byte[29];

        public readonly ISymbolModel[] OffsetDiff =
        [
            encoder.CreateSymbolModel(4),
            encoder.CreateSymbolModel(4),
            encoder.CreateSymbolModel(4),
            encoder.CreateSymbolModel(4),
        ];

        public readonly ISymbolModel PacketIndex = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);

        public readonly IntegerCompressor IcOffsetDiff = new(encoder, 32);
        public readonly IntegerCompressor IcPacketSize = new(encoder, 32);
        public readonly IntegerCompressor IcReturnPoint = new(encoder, 32);
        public readonly IntegerCompressor IcXyz = new(encoder, 32, 3);

        public bool Unused = true;
        public int LastDiff32;
        public uint SymLastOffsetDiff;
    }
}