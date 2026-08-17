// -----------------------------------------------------------------------
// <copyright file="IColumnBuffer.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Arrow;

/// <summary>
/// The column buffer interface.
/// </summary>
internal interface IColumnBuffer
{
    /// <summary>
    /// Gets the count.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Adds an item to the buffer.
    /// </summary>
    /// <param name="value">The value to add.</param>
    void Add(object? value);

    /// <summary>
    /// Clears this instance.
    /// </summary>
    void Clear();

    /// <summary>
    /// Build the <see cref="IArrowArray"/> from the buffer.
    /// </summary>
    /// <returns>The <see cref="IArrowArray"/>.</returns>
    IArrowArray BuildArray();
}