// -----------------------------------------------------------------------
// <copyright file="LasPointSpan.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The point data record and extra bytes.
/// </summary>
/// <param name="pointDataRecord">The point data record.</param>
/// <param name="extraBytes">The extra bytes.</param>
public readonly ref struct LasPointSpan(IBasePointDataRecord pointDataRecord, ReadOnlySpan<byte> extraBytes)
{
    /// <summary>
    /// Gets the point data record.
    /// </summary>
    public IBasePointDataRecord? PointDataRecord { get; } = pointDataRecord;

    /// <summary>
    /// Gets the extra bytes.
    /// </summary>
    public ReadOnlySpan<byte> ExtraBytes { get; } = extraBytes;

    /// <summary>
    /// Converts a <see cref="LasPointSpan"/> to a <see cref="LasPointMemory"/>.
    /// </summary>
    /// <param name="pointSpan">The point.</param>
    /// <returns>The memory based point.</returns>
    public static explicit operator LasPointMemory(LasPointSpan pointSpan) => new(pointSpan.PointDataRecord!, pointSpan.ExtraBytes.ToArray());
}