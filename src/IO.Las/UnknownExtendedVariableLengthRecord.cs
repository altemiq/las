// -----------------------------------------------------------------------
// <copyright file="UnknownExtendedVariableLengthRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents an unknown variable length record.
/// </summary>
/// <param name="Header">The header.</param>
/// <param name="Data">The data.</param>
public sealed record UnknownExtendedVariableLengthRecord(ExtendedVariableLengthRecordHeader Header, byte[] Data) : ExtendedVariableLengthRecord(Header with { RecordLengthAfterHeader = (ushort)Data.Length })
{
    /// <inheritdoc />
    public override int CopyTo(Span<byte> destination)
    {
        this.Header.CopyTo(destination);
        this.Data.AsSpan().CopyTo(destination[VariableLengthRecordHeader.Size..]);
        return VariableLengthRecordHeader.Size + this.Data.Length;
    }
}