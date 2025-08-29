// -----------------------------------------------------------------------
// <copyright file="ExtraBytesOptions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Encoding of <see cref="ExtraBytesItem.Options"/> Bit Field.
/// </summary>
[Flags]
public enum ExtraBytesOptions : byte
{
    /// <summary>
    /// Not set.
    /// </summary>
    None = default,

    /// <summary>
    /// The no data value is relevant.
    /// </summary>
    NoData = 1 << 0,

    /// <summary>
    /// The min value is relevant.
    /// </summary>
    Min = 1 << 1,

    /// <summary>
    /// The max value is relevant.
    /// </summary>
    Max = 1 << 2,

    /// <summary>
    /// Each value should be multiplied by the corresponding scale value (before applying the offset).
    /// </summary>
    Scale = 1 << 3,

    /// <summary>
    /// Each value should be translated by the corresponding offset value (after applying the scaling).
    /// </summary>
    Offset = 1 << 4,

    /// <summary>
    /// All options are relevant.
    /// </summary>
    All = NoData | Min | Max | Scale | Offset,
}