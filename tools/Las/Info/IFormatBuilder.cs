// -----------------------------------------------------------------------
// <copyright file="IFormatBuilder.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The format builder.
/// </summary>
public interface IFormatBuilder
{
    /// <summary>
    /// Appends the formated data.
    /// </summary>
    /// <param name="style">The style.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The format builder.</returns>
    IFormatBuilder AppendFormat(Style? style, string format, params object?[] args);

    /// <summary>
    /// Appends the value.
    /// </summary>
    /// <param name="value">The text to append.</param>
    /// <param name="style">The style.</param>
    /// <returns>The format builder.</returns>
    IFormatBuilder Append(string value, Style? style = default);

    /// <summary>
    /// Appends the line.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="style">The style.</param>
    /// <returns>The format builder.</returns>
    IFormatBuilder AppendLine(string? value = default, Style? style = default);
}