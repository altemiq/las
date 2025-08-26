// -----------------------------------------------------------------------
// <copyright file="IWaveformPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a point data record with waveform data.
/// </summary>
public interface IWaveformPointDataRecord : IGpsPointDataRecord
{
    /// <summary>
    /// Gets the wave packet descriptor index.
    /// </summary>
    byte WavePacketDescriptorIndex { get; init; }

    /// <summary>
    /// Gets the byte offset to waveform data.
    /// </summary>
    ulong ByteOffsetToWaveformData { get; init; }

    /// <summary>
    /// Gets the waveform package size in bytes.
    /// </summary>
    uint WaveformPacketSizeInBytes { get; init; }

    /// <summary>
    /// Gets temporal offset.
    /// </summary>
    float ReturnPointWaveformLocation { get; init; }

    /// <summary>
    /// Gets the parametric dx value.
    /// </summary>
    float ParametricDx { get; init; }

    /// <summary>
    /// Gets the parametric dy value.
    /// </summary>
    float ParametricDy { get; init; }

    /// <summary>
    /// Gets the parametric dz value.
    /// </summary>
    float ParametricDz { get; init; }
}