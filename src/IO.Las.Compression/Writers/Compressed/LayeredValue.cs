// -----------------------------------------------------------------------
// <copyright file="LayeredValue.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Writers.Compressed;

/// <summary>
/// A property bag for compressed layered values.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "This is designed for quick access.")]
internal sealed class LayeredValue
{
    /// <summary>
    /// The encoder.
    /// </summary>
    public readonly IEntropyEncoder Encoder = new ArithmeticEncoder();

    /// <summary>
    /// The value indicating whether this instance has changed.
    /// </summary>
    public bool Changed;

    private Stream? stream;

    /// <summary>
    /// Gets or sets the byte count.
    /// </summary>
    public uint ByteCount { get; set; }

    /// <summary>
    /// Initializes the <see cref="LayeredValue"/>.
    /// </summary>
    /// <param name="size">The size of the buffer.</param>
    public void Initialize(int size = ArithmeticCoder.BufferSize)
    {
        this.stream ??= new MemoryStream(size);
        this.stream.Position = 0;
        _ = this.Encoder.Initialize(this.stream);
        this.Changed = false;
    }

    /// <summary>
    /// Calls the <see cref="Compression.IEntropyCoder.Done"/> method for the <see cref="LayeredValue"/>.
    /// </summary>
    public void EncoderDone() => this.Encoder.Done();

    /// <summary>
    /// Calls the <see cref="Compression.IEntropyCoder.Done"/> method for the <see cref="LayeredValue"/> if <see cref="LayeredValue.Changed"/> is <see langword="true"/>.
    /// </summary>
    public void EncoderDoneIfChanged()
    {
        if (this.Changed)
        {
            this.EncoderDone();
        }
    }

    /// <summary>
    /// Gets the byte count.
    /// </summary>
    /// <returns>The byte count.</returns>
    public uint GetByteCount()
    {
        if (this.stream is { Position: var position })
        {
            var byteCount = (uint)position;
            this.ByteCount += byteCount;
            return byteCount;
        }

        return default;
    }

    /// <summary>
    /// Gets the byte count if <see cref="LayeredValue.Changed"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The byte count.</returns>
    public uint GetByteCountIfChanged() => this.Changed ? this.GetByteCount() : default;

    /// <summary>
    /// Copies the data in the <see cref="LayeredValue"/> to the specified <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public void CopyToStream(Stream stream)
    {
        if (this.stream is { Position: { } position } layeredValueStream)
        {
            var byteCount = (int)position;
            layeredValueStream.CopyToStream(stream, byteCount);
        }
    }

    /// <summary>
    /// Copies the data in the <see cref="LayeredValue"/> to the specified <see cref="BinaryWriter"/> if <see cref="LayeredValue.Changed"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public void CopyToStreamIfChanged(Stream stream)
    {
        if (this.Changed)
        {
            this.CopyToStream(stream);
        }
    }
}