// -----------------------------------------------------------------------
// <copyright file="IMinMax.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The min/max interface.
/// </summary>
internal interface IMinMax
{
    /// <summary>
    /// Gets the minimum.
    /// </summary>
    object Minimum { get; }

    /// <summary>
    /// Gets the maximum.
    /// </summary>
    object Maximum { get; }

    /// <summary>
    /// Update the min/max.
    /// </summary>
    /// <param name="value">The value.</param>
    void Update(object value);
}