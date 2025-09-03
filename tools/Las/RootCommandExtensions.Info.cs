// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.Info.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using Microsoft.Extensions.DependencyInjection;

/// <content>
/// The INFO command extensions.
/// </content>
public static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds INFO to the root command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input command.</returns>
    public static T AddInfo<T>(this T command)
        where T : RootCommand
    {
        command.Add(CreateCommand());

        return command;

        static Command CreateCommand()
        {
            var noMinMaxOption = new Option<bool>("--no-min-max");

            var noReturnsOption = new Option<bool>("--no-returns");

            var outputOption = new Option<FileInfo>("-o", "--output");

            var command = new Command("info", Tool.Properties.Resources.Command_InfoDescription)
            {
                Arguments.Inputs,
                noMinMaxOption,
                noReturnsOption,
                Options.Output,
                Options.InsideRectangle,
            };

            command.SetAction(parseResult =>
            {
                var console = parseResult.CreateConsole(outputOption);
                var noMinMax = parseResult.GetValue(noMinMaxOption);
                var noReturns = parseResult.GetValue(noReturnsOption);
                var boundingBox = parseResult.GetValue(Options.InsideRectangle);
                foreach (var file in parseResult.GetRequiredValue(Arguments.Inputs).Select(f => f.LocalPath))
                {
                    console.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "las info report for '{0}'", file), AnsiConsoleStyles.Title);
                    using var stream = File.OpenRead(file);
                    Info.Processor.Process(stream, console, System.Globalization.CultureInfo.InvariantCulture, noMinMax, noReturns, boundingBox);
                }
            });

            return command;
        }
    }
}