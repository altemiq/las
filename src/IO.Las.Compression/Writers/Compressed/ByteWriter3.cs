// -----------------------------------------------------------------------
// <copyright file="ByteWriter3.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed writer for <see cref="byte"/> values, version 3.
/// </summary>
internal sealed class ByteWriter3 : IContextWriter
{
    private readonly IEntropyEncoder encoder;

    private readonly Context[] contexts = new Context[4];

    private readonly LayeredValue[] valueBytes;

    private uint currentContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteWriter3"/> class.
    /// </summary>
    /// <param name="encoder">The encoder.</param>
    /// <param name="number">The number of values.</param>
    public ByteWriter3(IEntropyEncoder encoder, uint number)
    {
        this.encoder = encoder;

        this.valueBytes = new LayeredValue[number];

        for (var i = 0; i < number; i++)
        {
            this.valueBytes[i] = new();
        }

        this.contexts[0] = new(this.valueBytes);
        this.contexts[1] = new(this.valueBytes);
        this.contexts[2] = new(this.valueBytes);
        this.contexts[3] = new(this.valueBytes);
    }

    /// <inheritdoc/>
    public bool ChunkBytes()
    {
        var writer = this.encoder.GetStream();

        // output the bytes of all layers
        foreach (var valueByte in this.valueBytes)
        {
            valueByte.CopyToIfChanged(writer);
        }

        return true;
    }

    /// <inheritdoc/>
    public bool ChunkSizes()
    {
        var stream = this.encoder.GetStream();

        // output the sizes of all layer (i.e. number of bytes per layer)
        foreach (var valueByte in this.valueBytes)
        {
            // finish the encoders
            valueByte.EncoderDone();
            stream.WriteUInt32LittleEndian(valueByte.GetByteCountIfChanged());
        }

        return true;
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item, ref uint context)
    {
        // on the first init create outstreams and encoders
        foreach (var valueByte in this.valueBytes)
        {
            valueByte.Initialize(0);
        }

        // mark the four scanner channel contexts as unused */
        for (var c = 0; c < 4; c++)
        {
            this.contexts[c].Unused = true;
        }

        // set scanner channel as current context
        this.currentContext = context; // all other items use context set by POINT14 writer

        // create and init entropy models and integer compressors (and init context from item)
        this.CreateAndInitModelsAndCompressors(this.currentContext, item);

        return true;
    }

    /// <inheritdoc/>
    public void Write(Span<byte> item, ref uint context)
    {
        // get last
        var processingContext = this.contexts[this.currentContext];
        var lastItem = processingContext.LastItem;

        // check for context switch
        if (this.currentContext != context)
        {
            this.currentContext = context; // all other items use context set by POINT14 writer
            processingContext = this.contexts[this.currentContext];
            if (processingContext.Unused)
            {
                this.CreateAndInitModelsAndCompressors(this.currentContext, lastItem);
                lastItem = processingContext.LastItem;
            }
        }

        if (processingContext.BytesModels is not { } bytesModels)
        {
            return;
        }

        // compress
        for (var i = 0; i < this.valueBytes.Length; i++)
        {
            var diff = item[i] - lastItem[i];
            this.valueBytes[i].Encoder.EncodeSymbol(bytesModels[i], diff.Fold());
            if (diff is 0)
            {
                continue;
            }

            this.valueBytes[i].Changed = true;
            lastItem[i] = item[i];
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
        // first create all entropy models and last items (if needed)
        var processingContext = this.contexts[context];

        // then init entropy models
        foreach (var byteModel in processingContext.BytesModels)
        {
            _ = byteModel.Initialize();
        }

        // init current context from item
        item[..processingContext.LastItem.Length].CopyTo(processingContext.LastItem);

        processingContext.Unused = false;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsShouldBePrivate", Justification = "This is used as an internal property bag.")]
    private sealed class Context
    {
        public readonly byte[] LastItem;

        public readonly ISymbolModel[] BytesModels;

        public bool Unused;

        public Context(LayeredValue[] valueBytes)
        {
            var bytesModels = new ISymbolModel[valueBytes.Length];
            for (var i = 0; i < valueBytes.Length; i++)
            {
                bytesModels[i] = valueBytes[i].Encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
                _ = bytesModels[i].Initialize();
            }

            this.BytesModels = bytesModels;
            this.LastItem = new byte[valueBytes.Length];
        }
    }
}