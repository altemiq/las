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
        this.Data.AsSpan().CopyTo(destination[ExtendedVariableLengthRecordHeader.Size..]);
        return ExtendedVariableLengthRecordHeader.Size + this.Data.Length;
    }

    /// <inheritdoc/>
    public bool Equals(UnknownExtendedVariableLengthRecord? other) => base.Equals(other) && EnumerableEqualityComparer.Instance<byte>().Equals(this.Data, other.Data);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        System.HashCode hashCode = default;
        hashCode.Add(base.GetHashCode());
        hashCode.Add(this.Data, EnumerableEqualityComparer.Instance<byte>());
        return hashCode.ToHashCode();
    }
}