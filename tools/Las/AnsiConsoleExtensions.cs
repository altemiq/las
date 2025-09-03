// -----------------------------------------------------------------------
// <copyright file="AnsiConsoleExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="IAnsiConsole"/> extensions.
/// </summary>
internal static class AnsiConsoleExtensions
{
    /// <summary>
    /// Write the plain text.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="text">The text.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WritePlain(this IAnsiConsole console, string text)
    {
        console.Write(text);
        return console;
    }

    /// <summary>
    /// Writes the format values.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteFormat(this IAnsiConsole console, IFormatProvider? formatProvider, string format, params object[] args)
    {
        console.Write(string.Format(formatProvider, format, args));
        return console;
    }

    /// <summary>
    /// Writes the major header.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteMajorHeader(this IAnsiConsole console, string value)
    {
        console.Write(value, AnsiConsoleStyles.MajorHeader);
        return console;
    }

    /// <summary>
    /// Writes the major header.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteMajorHeader(this IAnsiConsole console, IFormatProvider? formatProvider, string format, params object[] args)
    {
        console.Write(string.Format(formatProvider, format, args), AnsiConsoleStyles.MajorHeader);
        return console;
    }

    /// <summary>
    /// Writes the header.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteHeader(this IAnsiConsole console, string value)
    {
        console.Write(value, AnsiConsoleStyles.Header);
        return console;
    }

    /// <summary>
    /// Writes the header.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteHeader(this IAnsiConsole console, IFormatProvider? formatProvider, string format, params object[] args)
    {
        console.Write(string.Format(formatProvider, format, args), AnsiConsoleStyles.Header);
        return console;
    }

    /// <summary>
    /// Writes the minor header.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteMinorHeader(this IAnsiConsole console, string value)
    {
        console.Write(value, AnsiConsoleStyles.MinorHeader);
        return console;
    }

    /// <summary>
    /// Writes the minor header.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <param name="format">The format.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteMinorHeader(this IAnsiConsole console, IFormatProvider? formatProvider, string format, params object[] args)
    {
        console.Write(string.Format(formatProvider, format, args), AnsiConsoleStyles.MinorHeader);
        return console;
    }

    /// <summary>
    /// Writes the caption.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="value">The value.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteCaption(this IAnsiConsole console, string value)
    {
        console.Write(value, AnsiConsoleStyles.Caption);
        return console;
    }

    /// <summary>
    /// Writes the value.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="value">The value.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteValue(this IAnsiConsole console, string value)
    {
        console.Write(value, AnsiConsoleStyles.Value);
        return console;
    }

    /// <summary>
    /// Writes the count.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="value">The value.</param>
    /// <returns>The input console.</returns>
    public static IAnsiConsole WriteCount(this IAnsiConsole console, string value)
    {
        console.Write(value, AnsiConsoleStyles.Count);
        return console;
    }
}