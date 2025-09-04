// -----------------------------------------------------------------------
// <copyright file="LasItemType.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents the LAS item type.
/// </summary>
/// <remarks>The number in the name, e.g. in <see cref="LasItemType.Point10"/>, refers to the LAS and LAZ version where that type got added.</remarks>
public enum LasItemType : ushort
{
    /// <summary>
    /// Extra bytes that are appended to a Standard LAS Point Data Record Format 0 to 5.
    /// </summary>
    Byte,

    /// <summary>
    /// <see cref="short"/> item type.
    /// </summary>
    /// <remarks>reserved, unsupported.</remarks>
    Short,

    /// <summary>
    /// <see cref="int"/> item type.
    /// </summary>
    /// <remarks>reserved, unsupported.</remarks>
    Int,

    /// <summary>
    /// <see cref="long"/> item type.
    /// </summary>
    /// <remarks>reserved, unsupported.</remarks>
    Long,

    /// <summary>
    /// <see cref="float"/> item type.
    /// </summary>
    /// <remarks>reserved, unsupported.</remarks>
    Float,

    /// <summary>
    /// <see cref="double"/> item type.
    /// </summary>
    /// <remarks>reserved, unsupported.</remarks>
    Double,

    /// <summary>
    /// LAS Point Data Record Format 0, containing the core fields that are shared between LAS Point Data Record Format 0 to 5.
    /// </summary>
    Point10,

    /// <summary>
    /// GPS Time field that is added for LAS Point Data Record Format 1 to a Point Data Record Format 0.
    /// </summary>
    GpsTime11,

    /// <summary>
    /// R, G and B fields (unsigned short) that are added for LAS Point Data Record Format 2 to a Point Data Record Format 0.
    /// </summary>
    Rgb12,

#if LAS1_3_OR_GREATER
    /// <summary>
    /// 7 fields for the Waveform packet that are added for LAS Point Data Record Format 4 to a Point Data Record Format 1.
    /// </summary>
    WavePacket13,
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// LAS Point Data Record Format 6, containing the core fields that are shared between LAS Point Data Record Format 6 to 10.
    /// </summary>
    Point14,

    /// <summary>
    /// R, G and B fields (unsigned short) that are added for LAS Point Data Record Format 7 to a Point Data Record Format 6.
    /// </summary>
    Rgb14,

    /// <summary>
    /// R, G, B and NIR (near infrared) fields that are added for LAS Point Data Record Format 8 to a Point Data Record Format 6.
    /// </summary>
    RgbNir14,

    /// <summary>
    /// 7 fields for the Waveform packet that are added for LAS Point Data Record Format 9 to a Point Data Record Format 6.
    /// </summary>
    WavePacket14,

    /// <summary>
    /// Extra bytes that are appended to a Standard LAS Point Data Record Format 6 to 10.
    /// </summary>
    Byte14,
#endif
}