// -----------------------------------------------------------------------
// <copyright file="IEntropyDecoder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The entropy decoder.
/// </summary>
internal interface IEntropyDecoder : IEntropyCoder
{
    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <param name="reallyInit">Set to <see langword="true"/> to really initialize.</param>
    /// <returns><see langword="true"/> if this was initialized correctly.</returns>
    bool Initialize(BinaryReader? reader, bool reallyInit = true);

    /// <summary>
    /// Decodes a bit with modelling.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The decoded bit.</returns>
    uint DecodeBit(IBitModel model);

    /// <summary>
    /// Decodes a symbol with modelling.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The decoded symbol.</returns>
    uint DecodeSymbol(ISymbolModel model);

    /// <summary>
    /// Decodes the bit without modelling.
    /// </summary>
    /// <returns>The bit.</returns>
    uint ReadBit();

    /// <summary>
    /// Decodes the bits without modelling.
    /// </summary>
    /// <param name="bits">The input bits.</param>
    /// <returns>The bits.</returns>
    uint ReadBits(uint bits);

    /// <summary>
    /// Decodes an unsigned char without modelling.
    /// </summary>
    /// <returns>An unsigned char.</returns>
    byte ReadByte();

    /// <summary>
    /// Decodes an unsigned short without modelling.
    /// </summary>
    /// <returns>An unsigned short.</returns>
    ushort ReadUInt16();

    /// <summary>
    /// Decodes an unsigned int without modelling.
    /// </summary>
    /// <returns>An unsigned int.</returns>
    uint ReadUInt32();

    /// <summary>
    /// Decodes a float without modelling.
    /// </summary>
    /// <returns>A float.</returns>
    float ReadSingle();

    /// <summary>
    /// Decodes an unsigned long without modelling.
    /// </summary>
    /// <returns>An unsigned long.</returns>
    ulong ReadUInt64();

    /// <summary>
    /// Decodes a double without modelling.
    /// </summary>
    /// <returns>A double.</returns>
    double ReadDouble();

    /// <summary>
    /// Gets the binary reader.
    /// </summary>
    /// <returns>The binary reader.</returns>
    BinaryReader GetBinaryReader();
}