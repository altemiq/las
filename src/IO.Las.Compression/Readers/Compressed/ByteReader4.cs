// -----------------------------------------------------------------------
// <copyright file="ByteReader4.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// The compressed byte reader, version 4.
/// </summary>
internal sealed class ByteReader4 : IContextReader
{
    private readonly IEntropyDecoder decoder;

    private readonly Context[] contexts = new Context[4];

    private readonly LayeredValue[] valueBytes;

    private byte[] bytes = [];

    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteReader4"/> class.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="number">The number.</param>
    /// <param name="decompressSelective">The selective decompress value.</param>
    public ByteReader4(IEntropyDecoder decoder, uint number, DecompressSelections decompressSelective = DecompressSelections.All)
    {
        const int Byte0 = (int)DecompressSelections.Byte0;
        this.decoder = decoder;

        // zero instream and decoder pointer arrays
        this.valueBytes = new LayeredValue[number];

        // currently only the first 16 extra bytes can be selectively decompressed
        var min = Math.Min(number, 16U);

        for (var i = 0; i < min; i++)
        {
            this.valueBytes[i] = new(decompressSelective.HasFlag((DecompressSelections)(Byte0 << i)));
        }

        for (var i = min; i < number; i++)
        {
            this.valueBytes[i] = new(requested: false);
        }

        this.currentContext = default;
        this.contexts[0] = new(this.valueBytes);
        this.contexts[1] = new(this.valueBytes);
        this.contexts[2] = new(this.valueBytes);
        this.contexts[3] = new(this.valueBytes);
    }

    /// <inheritdoc/>
    public bool ChunkSizes()
    {
        foreach (var valueByte in this.valueBytes)
        {
            valueByte.ByteCount = this.decoder.GetStream().ReadUInt32LittleEndian();
        }

        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item, ref uint context)
    {
        var startIndex = item.Length - this.valueBytes.Length;
        item = item[startIndex..];

        // for layered compression 'decoder' only hands over the stream
        var stream = this.decoder.GetStream();

        // how many bytes do we need to read
        var byteCount = this.valueBytes.Sum(static valueBytes => valueBytes.GetByteCountIfRequested());

        // make sure the buffer is sufficiently large
        if (byteCount > this.bytes.Length)
        {
            this.bytes = new byte[byteCount];
        }

        // load the requested bytes and init the corresponding instreams and decoders
        _ = this.valueBytes.Aggregate(default(uint), (index, valueByte) => index + valueByte.InitializeIfRequested(stream, this.bytes, (int)index));

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
        var processingContext = this.contexts[this.currentContext];
        var lastItem = processingContext.LastItem;

        // check for context switch
        if (this.currentContext != context)
        {
            this.currentContext = context; // all other items use context set by POINT14 reader
            if (this.contexts[this.currentContext].Unused)
            {
                this.CreateAndInitModelsAndDecompressors(this.currentContext, lastItem);
            }

            processingContext = this.contexts[this.currentContext];
            lastItem = processingContext.LastItem;
        }

        // decompress
        var bytesModel = processingContext.BytesModels;
        for (var i = 0; i < this.valueBytes.Length; i++)
        {
            if (this.valueBytes[i] is { Changed: true } valueByte)
            {
                var value = (int)(lastItem[i] + valueByte.Decoder.DecodeSymbol(bytesModel[i]));
                item[i] = value.Fold();
                lastItem[i] = item[i];
            }
            else
            {
                item[i] = lastItem[i];
            }
        }
    }

    private void CreateAndInitModelsAndDecompressors(uint context, ReadOnlySpan<byte> item)
    {
        // first create all entropy models and last items (if needed)
        var contextToInitialize = this.contexts[context];
        var bytesModels = contextToInitialize.BytesModels;

        // then init entropy models
        for (var i = 0; i < this.valueBytes.Length; i++)
        {
            _ = bytesModels[i].Initialize();
        }

        // init current context from item
        item[..contextToInitialize.LastItem.Length].CopyTo(contextToInitialize.LastItem);

        contextToInitialize.Unused = false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context
    {
        public readonly ISymbolModel[] BytesModels;

        public readonly byte[] LastItem;

        public bool Unused;

        public Context(LayeredValue[] layeredValues)
        {
            this.BytesModels = new ISymbolModel[layeredValues.Length];
            for (var i = 0; i < layeredValues.Length; i++)
            {
                this.BytesModels[i] = layeredValues[i].Decoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                _ = this.BytesModels[i].Initialize();
            }

            // create last item
            this.LastItem = new byte[layeredValues.Length];
        }
    }
}