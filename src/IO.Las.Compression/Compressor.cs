// -----------------------------------------------------------------------
// <copyright file="Compressor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The compressor.
/// </summary>
public enum Compressor : ushort
{
    /// <summary>
    /// Indicates an uncompressed standard LAS file (with a LAZ VLR).
    /// </summary>
    None = default,

    /// <summary>
    /// The data is stored in a single chunk, and no chunk table is used. Only for LAS point types 0 through 5.
    /// </summary>
    PointWise = 1,

    /// <summary>
    /// The data is stored using chunks, and a chunk table is used. Only for LAS point types 0 through 5.
    /// </summary>
    PointWiseChunked = 2,

#if LAS1_4_OR_GREATER
    /// <summary>
    /// The data is stored using chunks and layers, and a chunk table and layer tables are used. Only for LAS point types 6 through 10.
    /// </summary>
    LayeredChunked = 3,
#endif
}