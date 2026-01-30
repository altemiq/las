// -----------------------------------------------------------------------
// <copyright file="IMinMax{T}.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The min/max interface.
/// </summary>
/// <typeparam name="T">The type of value.</typeparam>
internal interface IMinMax<T>
{
    /// <summary>
    /// Gets the minimum.
    /// </summary>
    T Minimum { get; }

    /// <summary>
    /// Gets the maximum.
    /// </summary>
    T Maximum { get; }

    /// <summary>
    /// Updates the min/max.
    /// </summary>
    /// <param name="value">The value.</param>
    void Update(T value);

    /// <summary>
    /// Gets a value indicating whether this instance is the default value.
    /// </summary>
    /// <returns><see langword="true"/> if this instance is the default value; otherwise <see langword="false"/>.</returns>
    public bool IsDefault();
}