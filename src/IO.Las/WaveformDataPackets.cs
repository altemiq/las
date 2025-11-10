// -----------------------------------------------------------------------
// <copyright file="WaveformDataPackets.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The packet of Raw Waveform Amplitude values for all records immediately follow this VLR header.
/// Note that when using a <c>bit</c> resolution that is not an even increment of 8, the last byte of each waveform packet must be padded such that the next waveform record will start on an even byte boundary.
/// </summary>
public sealed record WaveformDataPackets : ExtendedVariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 65535;

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveformDataPackets"/> class.
    /// </summary>
    /// <param name="data">The data.</param>
    public WaveformDataPackets(ReadOnlySpan<byte> data)
        : this(
            new()
            {
                UserId = ExtendedVariableLengthRecordHeader.SpecUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = (ushort)data.Length,
            },
            data)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WaveformDataPackets"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal WaveformDataPackets(ExtendedVariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : base(header) => this.Data = data.ToArray();

    /// <summary>
    /// Gets the data.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        this.Data.Span.CopyTo(destination[ExtendedVariableLengthRecordHeader.Size..]);
        return ExtendedVariableLengthRecordHeader.Size + this.Data.Length;
    }
}