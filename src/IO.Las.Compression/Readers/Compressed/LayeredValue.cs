// -----------------------------------------------------------------------
// <copyright file="LayeredValue.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Readers.Compressed;

/// <summary>
/// A property bag for compressed layered values.
/// </summary>
/// <param name="requested">Set to <see langword="true"/> to indicated that this is requested.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "This is designed for quick access.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The held MemoryStream has no unmanaged resources; disposal would be a no-op and LayeredValue instances outlive individual chunks by design.")]
internal sealed class LayeredValue(bool requested)
{
    /// <summary>
    /// The decoder.
    /// </summary>
    public readonly ArithmeticDecoder Decoder = new ArithmeticDecoder();

    /// <summary>
    /// The value indicating whether this instance is requested.
    /// </summary>
    public readonly bool Requested = requested;

    /// <summary>
    /// The reusable stream over the current chunk's slice of the shared byte
    /// buffer. Initialised to an empty non-expandable MemoryStream and
    /// re-targeted on every chunk via the <c>SetBuffer</c> extension method,
    /// instead of being reallocated. Eliminates what used to be
    /// N-layers-per-chunk <see cref="MemoryStream"/> allocations.
    /// </summary>
    private readonly MemoryStream bufferStream = new([], index: 0, count: 0, writable: false, publiclyVisible: true);

    /// <summary>
    /// Gets a value indicating whether this instance has changed.
    /// </summary>
    public bool Changed { get; private set; }

    /// <summary>
    /// Gets or sets the byte count.
    /// </summary>
    public uint ByteCount { get; set; }

    /// <summary>
    /// Initializes the layered value.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="buffer">The byte buffer.</param>
    /// <param name="index">The index in <paramref name="buffer"/> to use.</param>
    /// <returns>The number of bytes read.</returns>
    public uint Initialize(Stream stream, byte[] buffer, int index = 0)
    {
        var count = (int)this.ByteCount;
        _ = stream.Read(buffer, index, count);
        this.bufferStream.SetBuffer(buffer, index, count);
        _ = this.Decoder.Initialize(this.bufferStream);
        return this.ByteCount;
    }

    /// <summary>
    /// Initializes the layered value.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="buffer">The byte buffer.</param>
    /// <param name="index">The index in <paramref name="buffer"/> to use.</param>
    /// <returns>The number of bytes read.</returns>
    public uint InitializeIfRequested(Stream stream, byte[] buffer, int index = 0)
    {
        if (this.ByteCount is not 0)
        {
            if (this.Requested)
            {
                var count = (int)this.ByteCount;
                _ = stream.Read(buffer, index, count);
                this.bufferStream.SetBuffer(buffer, index, count);
                _ = this.Decoder.Initialize(this.bufferStream);
                this.Changed = true;
                return this.ByteCount;
            }

            stream.Position += this.ByteCount;
        }

        this.Changed = false;
        return default;
    }

    /// <summary>
    /// Gets the count if <see cref="Requested"/> is <see langword="true"/>.
    /// </summary>
    /// <returns>The byte count if <see cref="Requested"/> is <see langword="true"/>; otherwise zero.</returns>
    public uint GetByteCountIfRequested() => this.Requested ? this.ByteCount : default;
}