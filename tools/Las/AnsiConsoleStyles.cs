// -----------------------------------------------------------------------
// <copyright file="AnsiConsoleStyles.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Style"/> values.
/// </summary>
internal static class AnsiConsoleStyles
{
    /// <summary>
    /// The style for the title.
    /// </summary>
    public static readonly Style Title = new(foreground: Spectre.Console.Color.DarkGoldenrod);

    /// <summary>
    /// The style for the major header.
    /// </summary>
    public static readonly Style MajorHeader = new(foreground: Spectre.Console.Color.Blue);

    /// <summary>
    /// The style for the header.
    /// </summary>
    public static readonly Style Header = new(foreground: Spectre.Console.Color.Green);

    /// <summary>
    /// The style for the minor header.
    /// </summary>
    public static readonly Style MinorHeader = new(foreground: Spectre.Console.Color.Grey);

    /// <summary>
    /// The style for a warning.
    /// </summary>
    public static readonly Style Warning = new(foreground: Spectre.Console.Color.OrangeRed1);

    /// <summary>
    /// The style for captions.
    /// </summary>
    public static readonly Style Caption = new(foreground: Spectre.Console.Color.Aqua);

    /// <summary>
    /// The style for values.
    /// </summary>
    public static readonly Style Value = new(foreground: Spectre.Console.Color.Default);

    /// <summary>
    /// The style for counts.
    /// </summary>
    public static readonly Style Count = new(foreground: Spectre.Console.Color.DeepSkyBlue1);
}