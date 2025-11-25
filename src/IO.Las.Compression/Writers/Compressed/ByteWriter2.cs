// -----------------------------------------------------------------------
// <copyright file="ByteWriter2.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// The compressed writer for <see cref="byte"/> values, version 2.
/// </summary>
internal sealed class ByteWriter2 : ISimpleWriter
{
    private readonly IEntropyEncoder encoder;

    private readonly byte[] lastItem;

    private readonly ISymbolModel[] bytesModels;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByteWriter2"/> class.
    /// </summary>
    /// <param name="encoder">The encoder.</param>
    /// <param name="number">The number of values.</param>
    public ByteWriter2(IEntropyEncoder encoder, uint number)
    {
        ArgumentNullException.ThrowIfNull(encoder);
        this.encoder = encoder;
        this.bytesModels = new ISymbolModel[number];

        for (var i = 0; i < number; i++)
        {
            this.bytesModels[i] = encoder.CreateSymbolModel(ArithmeticCoder.ModelCount);
        }

        this.lastItem = new byte[number];
    }

    /// <inheritdoc/>
    public bool Initialize(ReadOnlySpan<byte> item)
    {
        // init models and integer compressors
        foreach (var byteModel in this.bytesModels)
        {
            _ = byteModel.Initialize();
        }

        item[..this.lastItem.Length].CopyTo(this.lastItem);

        return true;
    }

    /// <inheritdoc/>
    public void Write(Span<byte> item)
    {
        // compress
        for (var i = 0; i < this.bytesModels.Length; i++)
        {
            var diff = item[i] - this.lastItem[i];
            this.encoder.EncodeSymbol(this.bytesModels[i], diff.Fold());
        }

        item[..this.lastItem.Length].CopyTo(this.lastItem);
    }

    /// <inheritdoc/>
    public ValueTask WriteAsync(Memory<byte> item, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.Write(item.Span);
        return default;
    }
}