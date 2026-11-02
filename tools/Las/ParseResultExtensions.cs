// -----------------------------------------------------------------------
// <copyright file="ParseResultExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

#pragma warning disable SA1101

/// <summary>
/// The <see cref="ParseResult"/> extensions.
/// </summary>
internal static class ParseResultExtensions
{
    /// <content>
    /// The <see cref="ParseResult"/> extensions.
    /// </content>
    /// <param name="parseResult">The parse result.</param>
    extension(ParseResult? parseResult)
    {
        /// <summary>
        /// Creates the <see cref="IAnsiConsole"/> instance.
        /// </summary>
        /// <param name="outputOption">The output option.</param>
        /// <returns>The ANSI console.</returns>
        public IAnsiConsole CreateConsole(Option<FileInfo>? outputOption = default)
        {
            if (parseResult is null)
            {
                return global::Spectre.Console.AnsiConsole.Console;
            }

            var (ansiSupport, writer) = outputOption is { } option && parseResult.GetValue(option) is { } outputFile
                ? (AnsiSupport.No, outputFile.CreateText())
                : (GetAnsiSupport(parseResult), parseResult.InvocationConfiguration.Output);

            return global::Spectre.Console.AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = ansiSupport,
                ColorSystem = ColorSystemSupport.Detect,
                Out = new AnsiConsoleOutput(writer),
            });

            static AnsiSupport GetAnsiSupport(ParseResult parseResult)
            {
                return parseResult.InvocationConfiguration.Output == Console.Out ? AnsiSupport.Detect : AnsiSupport.No;
            }
        }
    }
}