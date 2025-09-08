// -----------------------------------------------------------------------
// <copyright file="IEntropyEncoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The entropy encoder.
/// </summary>
internal interface IEntropyEncoder : IEntropyCoder
{
    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="stream">The output stream.</param>
    /// <returns><see langword="true"/> if successfully initialized; otherwise <see langword="false"/>.</returns>
    bool Initialize(Stream stream);

    /// <summary>
    /// Encode a bit with modelling.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="sym">The symbol to write.</param>
    void EncodeBit(IBitModel model, uint sym);

    /// <summary>
    /// Encode a symbol with modelling.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="sym">The symbol to write.</param>
    void EncodeSymbol(ISymbolModel model, uint sym);

    /// <summary>
    /// Encode a bit without modelling.
    /// </summary>
    /// <param name="sym">The symbol to write.</param>
    void WriteBit(uint sym);

    /// <summary>
    /// Encode bits without modelling.
    /// </summary>
    /// <param name="bits">The bits.</param>
    /// <param name="sym">The symbol to write.</param>
    void WriteBits(uint bits, uint sym);

    /// <summary>
    /// Encode an unsigned char without modelling.
    /// </summary>
    /// <param name="sym">The symbol to write.</param>
    void WriteByte(byte sym);

    /// <summary>
    /// Encode an unsigned short without modelling.
    /// </summary>
    /// <param name="sym">The symbol to write.</param>
    void WriteShort(ushort sym);

    /// <summary>
    /// Encode an unsigned int without modelling.
    /// </summary>
    /// <param name="sym">The symbol to write.</param>
    void WriteInt(uint sym);

    /// <summary>
    /// Encode a float without modelling.
    /// </summary>
    /// <param name="sym">The symbol to write.</param>
    void WriteFloat(float sym);

    /// <summary>
    /// Encode an unsigned 64 bit int without modelling.
    /// </summary>
    /// <param name="sym">The symbol to write.</param>
    void WriteInt64(ulong sym);

    /// <summary>
    /// Encode a double without modelling.
    /// </summary>
    /// <param name="sym">The symbol to write.</param>
    void WriteDouble(double sym);

    /// <summary>
    /// Gets the stream.
    /// </summary>
    /// <returns>The stream.</returns>
    Stream GetStream();
}