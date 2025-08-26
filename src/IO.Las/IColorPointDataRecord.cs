// -----------------------------------------------------------------------
// <copyright file="IColorPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a point data record with color.
/// </summary>
public interface IColorPointDataRecord : IBasePointDataRecord
{
    /// <summary>
    /// Gets the color.
    /// </summary>
    Color Color { get; init; }
}