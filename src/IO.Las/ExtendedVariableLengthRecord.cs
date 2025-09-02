// -----------------------------------------------------------------------
// <copyright file="ExtendedVariableLengthRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The base for an extended variable length record.
/// </summary>
/// <param name="Header">The header.</param>
public abstract record ExtendedVariableLengthRecord(ExtendedVariableLengthRecordHeader Header)
{
    /// <summary>
    /// Writes this instance to the destination.
    /// </summary>
    /// <param name="destination">The destination.</param>
    /// <returns>The number of bytes written.</returns>
    public abstract int Write(Span<byte> destination);

    /// <summary>
    /// Returns the sizes of this instance.
    /// </summary>
    /// <returns>The size.</returns>
    public ulong Size() => Size(this.Header.RecordLengthAfterHeader);

    private static ulong Size(ulong dataLength) => ExtendedVariableLengthRecordHeader.Size + (sizeof(byte) * dataLength);
}