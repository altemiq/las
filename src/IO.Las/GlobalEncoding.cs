// -----------------------------------------------------------------------
// <copyright file="GlobalEncoding.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The global encoding.
/// </summary>
[Flags]
public enum GlobalEncoding : ushort
{
    /// <summary>
    /// No encoding.
    /// </summary>
    None = default,

    /// <summary>
    /// The GPS Time is standard GPS Time (satellite GPS Time) minus 1 x 10⁹ (Adjusted Standard GPS Time).
    /// The offset moves the time back to near zero to improve floating-point resolution.
    /// The origin of standard GPS Time is defined as midnight of the morning of January 6, 1980.
    /// </summary>
    StandardGpsTime = 1 << 0,

#if LAS1_3_OR_GREATER
    /// <summary>
    /// The waveform data packets are located within this file.
    /// </summary>
    WaveformDataPacketsInternal = 1 << 1,

    /// <summary>
    /// The waveform data packets are located externally in an auxiliary file with the same base name as this file but the extension *.wdp.
    /// </summary>
    WaveformDataPacketsExternal = 1 << 2,

    /// <summary>
    /// The point return numbers in the point data records have been synthetically generated.
    /// This could be the case, for example, when a composite file is created by combining a First Return File and a Last Return File, or when simulating return numbers for a system not directly supporting multiple returns.
    /// </summary>
    SyntheticReturnNumbers = 1 << 3,
#endif

#if LAS1_4_OR_GREATER
    /// <summary>
    /// The Coordinate Reference System (CRS) is WKT.
    /// </summary>
    Wkt = 1 << 4,
#endif

#if LAS1_5_OR_GREATER
    /// <summary>
    /// GPS Time is defined as Offset GPS Time. The Offset GPS Time is standard GPS Time (satellite GPS Time) minus 1 x 10⁶ x Time Offset.
    /// The Time Offset is specified in the LAS header.
    /// The offset moves time to near zero to improve floating-point resolution for a given time period.
    /// The origin of standard GPS Time is defined as midnight of the morning of January 6, 1980.
    /// The GPS Time Type flag must be set if the Time Offset Flag is set.
    /// </summary>
    TimeOffsetFlag = 1 << 6,
#endif
}