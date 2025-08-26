// -----------------------------------------------------------------------
// <copyright file="IGpsPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a point data record with GPS time.
/// </summary>
public interface IGpsPointDataRecord : IBasePointDataRecord
{
    /// <summary>
    /// Gets the GPS time.
    /// </summary>
    double GpsTime { get; init; }
}