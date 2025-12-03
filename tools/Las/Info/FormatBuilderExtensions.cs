// -----------------------------------------------------------------------
// <copyright file="FormatBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.Info;

/// <summary>
/// The <see cref="IFormatBuilder"/> extensions.
/// </summary>
internal static class FormatBuilderExtensions
{
    extension(IFormatBuilder builder)
    {
#pragma warning disable S2325, SA1101
        /// <summary>
        /// Writes the format values.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendFormat(string format, params object?[] args) => builder.AppendFormat(default, format, args);

        /// <summary>
        /// Writes the major header.
        /// </summary>
        /// <param name="value">The header value.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendMajorHeader(string value) => builder.Append(value, AnsiConsoleStyles.MajorHeader);

        /// <summary>
        /// Writes the major header.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendMajorHeader(string format, params object[] args) => builder.AppendFormat(AnsiConsoleStyles.MajorHeader, format, args);

        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="value">The header value.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendHeader(string value) => builder.Append(value, AnsiConsoleStyles.Header);

        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendHeader(string format, params object[] args) => builder.AppendFormat(AnsiConsoleStyles.Header, format, args);

        /// <summary>
        /// Writes the minor header.
        /// </summary>
        /// <param name="value">The header value.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendMinorHeader(string value) => builder.Append(value, AnsiConsoleStyles.MinorHeader);

        /// <summary>
        /// Writes the minor header.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendMinorHeader(string format, params object[] args) => builder.AppendFormat(AnsiConsoleStyles.MinorHeader, format, args);

        /// <summary>
        /// Writes the caption.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendCaption(string value) => builder.Append(value, AnsiConsoleStyles.Caption);

        /// <summary>
        /// Writes the caption.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendCaption(string format, params object[] args) => builder.AppendFormat(AnsiConsoleStyles.Caption, format, args);

        /// <summary>
        /// Writes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendValue(string value) => builder.Append(value, AnsiConsoleStyles.Value);

        /// <summary>
        /// Writes the count.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendCount(string value) => builder.Append(value, AnsiConsoleStyles.Count);

        /// <summary>
        /// Writes the count.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The input console.</returns>
        public IFormatBuilder AppendCount(string format, params object[] args) => builder.AppendFormat(AnsiConsoleStyles.Count, format, args);
#pragma warning restore S2325, SA1101
    }
}