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
        if (this.stream is not { Position: var position })
        {
            return default;
        }

        var byteCount = (uint)position;
        this.ByteCount += byteCount;
        return byteCount;
    }

    /// <summary>
    /// Gets the byte count if <see cref="LayeredValue.Changed"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The byte count.</returns>
    public uint GetByteCountIfChanged() => this.Changed ? this.GetByteCount() : default;

    /// <summary>
    /// Copies the data in the <see cref="LayeredValue"/> to the specified <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="destination">The stream to write to.</param>
    public void CopyTo(Stream destination)
    {
        if (this.stream is not { Position: var position } source)
        {
            return;
        }

        // reset the position to the start
        source.Position = 0;
        var bytesLeft = (int)position;

        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(ArithmeticCoder.BufferSize);
        try
        {
            while (bytesLeft > 0)
            {
                var bytesToRead = bytesLeft > buffer.Length ? buffer.Length : bytesLeft;
                var bytesRead = source.Read(buffer, 0, bytesToRead);

                destination.Write(buffer, 0, bytesRead);

                bytesLeft -= bytesRead;
            }

            source.Position = position;
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Copies the data in the <see cref="LayeredValue"/> to the specified <see cref="BinaryWriter"/> if <see cref="LayeredValue.Changed"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="destination">The stream to write to.</param>
    public void CopyToIfChanged(Stream destination)
    {
        if (this.Changed)
        {
            this.CopyTo(destination);
        }
    }
}