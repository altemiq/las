// -----------------------------------------------------------------------
// <copyright file="LasPointMemory.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The point data record and extra bytes.
/// </summary>
/// <param name="pointDataRecord">The point data record.</param>
/// <param name="extraBytes">The extra bytes.</param>
public readonly struct LasPointMemory(IBasePointDataRecord pointDataRecord, ReadOnlyMemory<byte> extraBytes)
{
    /// <summary>
    /// Gets the point data record.
    /// </summary>
    public IBasePointDataRecord? PointDataRecord { get; } = pointDataRecord;

    /// <summary>
    /// Gets the extra bytes.
    /// </summary>
    public ReadOnlyMemory<byte> ExtraBytes { get; } = extraBytes;

    /// <summary>
    /// Converts a <see cref="LasPointMemory"/> to a <see cref="LasPointSpan"/>.
    /// </summary>
    /// <param name="pointMemory">The point.</param>
    /// <returns>The span based point.</returns>
    public static implicit operator LasPointSpan(LasPointMemory pointMemory) => new(pointMemory.PointDataRecord!, pointMemory.ExtraBytes.Span);
}