// -----------------------------------------------------------------------
// <copyright file="DecompressSelections.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Compression;

/// <summary>
/// The decompress selective enumeration.
/// </summary>
[Flags]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator.Design", "RCS1157:CompositeEnumValueContainsUndefinedFlag.", Justification = "This is defined externally")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Meziantou.Design", "MA0062:NonFlagsEnumsShouldNotBeMarkedWithFlagsAttribute", Justification = "This is defined externally")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S4070:Non-flags enums should not be marked with \"FlagsAttribute\"", Justification = "This is defined externally")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2346:Flags enumerations zero-value members should be named \"None\"", Justification = "This is the default, but not \"None\"")]
internal enum DecompressSelections : uint
{
    /// <summary>
    /// Channel returns XY.
    /// </summary>
    ChannelReturnsXY = default,

    /// <summary>
    /// Z channel.
    /// </summary>
    Z = 1 << 0,

    /// <summary>
    /// Classification.
    /// </summary>
    Classification = 1 << 1,

    /// <summary>
    /// Flags.
    /// </summary>
    Flags = 1 << 2,

    /// <summary>
    /// Intensity.
    /// </summary>
    Intensity = 1 << 3,

    /// <summary>
    /// Scan Angle.
    /// </summary>
    ScanAngle = 1 << 4,

    /// <summary>
    /// User data.
    /// </summary>
    UserData = 1 << 5,

    /// <summary>
    /// Point source ID.
    /// </summary>
    PointSource = 1 << 6,

    /// <summary>
    /// GPS time.
    /// </summary>
    GpsTime = 1 << 7,

    /// <summary>
    /// RGB.
    /// </summary>
    RGB = 1 << 8,

    /// <summary>
    /// NIR.
    /// </summary>
    NIR = 1 << 9,

    /// <summary>
    /// Wave packets.
    /// </summary>
    WavePacket = 1 << 10,

    /// <summary>
    /// Byte zero.
    /// </summary>
    Byte0 = 1 << 16,

    /// <summary>
    /// Byte one.
    /// </summary>
    Byte1 = 1 << 17,

    /// <summary>
    /// Byte two.
    /// </summary>
    Byte2 = 1 << 18,

    /// <summary>
    /// Byte three.
    /// </summary>
    Byte3 = 1 << 19,

    /// <summary>
    /// Byte four.
    /// </summary>
    Byte4 = 1 << 20,

    /// <summary>
    /// Byte five.
    /// </summary>
    Byte5 = 1 << 21,

    /// <summary>
    /// Byte six.
    /// </summary>
    Byte6 = 1 << 22,

    /// <summary>
    /// Byte seven.
    /// </summary>
    Byte7 = 1 << 23,

    /// <summary>
    /// Extra bytes.
    /// </summary>
    ExtraBytes = 0xFFFF0000,

    /// <summary>
    /// All.
    /// </summary>
    All = 0xFFFFFFFF,
}