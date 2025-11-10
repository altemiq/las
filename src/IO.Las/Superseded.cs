// -----------------------------------------------------------------------
// <copyright file="Superseded.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// This LASF Record ID is used to negate an existing VLR/EVLR when rewriting the file (to remove the undesired VLR/EVLR).
/// </summary>
public sealed record Superseded : VariableLengthRecord
{
    /// <summary>
    /// The tag record ID.
    /// </summary>
    public const ushort TagRecordId = 7;

    /// <summary>
    /// The instance.
    /// </summary>
    public static readonly VariableLengthRecord Instance = new Superseded(
        new VariableLengthRecordHeader
        {
            UserId = VariableLengthRecordHeader.SpecUserId,
            RecordId = TagRecordId,
        });

    /// <summary>
    /// Initializes a new instance of the <see cref="Superseded"/> class.
    /// </summary>
    /// <param name="header">The header.</param>
    internal Superseded(VariableLengthRecordHeader header)
        : base(header)
    {
    }

    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        return VariableLengthRecordHeader.Size;
    }
}