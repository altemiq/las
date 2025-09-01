// -----------------------------------------------------------------------
// <copyright file="TextAreaDescription.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// This VLR/EVLR is used for providing a textual description of the content of the LAS file.
/// </summary>
/// <remarks>It is a null-terminated, free-form ASCII string.</remarks>
public sealed record TextAreaDescription : VariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="TextAreaDescription"/> class.
    /// </summary>
    /// <param name="value">The text area description.</param>
    public TextAreaDescription(string value)
        : this(
            new()
            {
                UserId = VariableLengthRecordHeader.SpecUserId,
                RecordId = TagRecordId,
                RecordLengthAfterHeader = (ushort)(System.Text.Encoding.UTF8.GetByteCount(value) + 1),
            },
            value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextAreaDescription"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="data">The data.</param>
    internal TextAreaDescription(VariableLengthRecordHeader header, ReadOnlySpan<byte> data)
        : this(header, System.Text.Encoding.ASCII.GetNullTerminatedString(data))
    {
    }

    private TextAreaDescription(VariableLengthRecordHeader header, string value)
        : base(header) => this.Value = value;

    /// <summary>
    /// Gets the value.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override int Write(Span<byte> destination)
    {
        var d = destination;
        this.Header.Write(d);
        int bytesWritten = VariableLengthRecordHeader.Size;
        d = destination[bytesWritten..];

        bytesWritten += System.Text.Encoding.ASCII.GetBytes(this.Value, d);
        d[bytesWritten] = 0;

        return bytesWritten + 1;
    }
}