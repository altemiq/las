// -----------------------------------------------------------------------
// <copyright file="WaveformPacketDescriptor.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// These records contain information that describes the configuration of the waveform packets.
/// Since system configuration may vary throughout a dataset, the LAS file supports up to 255 Waveform Packet Descriptors.
/// </summary>
/// <remarks>
/// The digitizer gain and offset are used to convert the raw digitized value to an absolute digitizer voltage using the formula:
/// <c>VOLTS = OFFSET + GAIN * Raw_Waveform_Amplitude</c>.
/// </remarks>
public sealed record WaveformPacketDescriptor : VariableLengthRecord
{
    /// <summary>
    /// The minimum tag record ID.
    /// </summary>
    public const ushort MinTagRecordId = 100;

    /// <summary>
    /// The maximum tag record ID.
    /// </summary>
    public const ushort MaxTagRecordId = 354;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveformPacketDescriptor"/> class.
    /// </summary>
    /// <param name="recordId">The record ID.</param>
    public WaveformPacketDescriptor(int recordId)
        : base(new VariableLengthRecordHeader
        {
            UserId = VariableLengthRecordHeader.SpecUserId,
            RecordId = CheckRecordId(recordId),
            RecordLengthAfterHeader = 26,
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveformPacketDescriptor"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    internal WaveformPacketDescriptor(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : base(header)
    {
        this.BitsPerSample = data[0];
        this.WaveformCompressionType = data[1];
        this.NumberOfSamples = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[2..6]);
        this.TemporalSampleSpacing = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[6..10]);
        this.DigitizerGain = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[10..18]);
        this.DigitizerOffset = System.Buffers.Binary.BinaryPrimitives.ReadDoubleLittleEndian(data[18..26]);
    }

    /// <summary>
    /// Gets the bits per sample.
    /// </summary>
    /// <remarks>2 through 32 bits are supported.</remarks>
    public required byte BitsPerSample { get; init; }

    /// <summary>
    /// Gets the waveform compression type.
    /// </summary>
    /// <remarks>It is expected that in the future standard compression types will be adopted by the LAS committee.
    /// This field will indicate the compression algorithm used for the waveform packets associated with this descriptor.
    /// A value of 0 indicates no compression.
    /// Zero is the only value currently supported.
    /// </remarks>
    public required byte WaveformCompressionType { get; init; }

    /// <summary>
    /// Gets the number of samples.
    /// </summary>
    /// <remarks>
    /// The number of samples associated with this waveform packet type.
    /// This value always represents the fully decompressed waveform packet.
    /// </remarks>
    public required uint NumberOfSamples { get; init; }

    /// <summary>
    /// Gets the temporal sample spacing.
    /// </summary>
    /// <remarks>
    /// The temporal sample spacing in picoseconds.
    /// Example values might be 500, 1000, 2000, and so on, representing digitizer frequencies of 2 GHz, 1 GHz, and 500 MHz respectively.
    /// </remarks>
    public required uint TemporalSampleSpacing { get; init; }

    /// <summary>
    /// Gets the digitizer gain.
    /// </summary>
    public required double DigitizerGain { get; init; }

    /// <summary>
    /// Gets the digitizer offset.
    /// </summary>
    public required double DigitizerOffset { get; init; }

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        var data = destination[VariableLengthRecordHeader.Size..];

        data[0] = this.BitsPerSample;
        data[1] = this.WaveformCompressionType;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data[2..6], this.NumberOfSamples);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(data[6..10], this.TemporalSampleSpacing);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(data[10..18], this.DigitizerGain);
        System.Buffers.Binary.BinaryPrimitives.WriteDoubleLittleEndian(data[18..26], this.DigitizerOffset);

        return VariableLengthRecordHeader.Size + 26;
    }

    private static ushort CheckRecordId(int recordId) => recordId is >= MinTagRecordId and <= MaxTagRecordId ? (ushort)recordId : throw new ArgumentOutOfRangeException(nameof(recordId));
}