// -----------------------------------------------------------------------
// <copyright file="INearInfraredPointDataRecord.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Represents a point data record with near-infrared information.
/// </summary>
public interface INearInfraredPointDataRecord : IColorPointDataRecord
{
    /// <summary>
    /// Gets the NIR (near infrared) channel value associated with this point.
    /// </summary>
    ushort NearInfrared { get; init; }
}