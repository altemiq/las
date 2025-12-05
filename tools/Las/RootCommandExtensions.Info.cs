// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.Info.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// The INFO command extensions.
/// </content>
internal static partial class RootCommandExtensions
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

            var jsonOption = new Option<bool>("-j", "--json");

            var command = new Command("info", Tool.Properties.Resources.Command_InfoDescription)
            {
                Arguments.Inputs,
                noMinMaxOption,
                noReturnsOption,
                jsonOption,
                Options.Output,
                Options.InsideRectangle,
            };

            command.SetAction(parseResult =>
            {
                var services = parseResult.GetServices();
                var console = parseResult.CreateConsole(Options.Output);
                var noMinMax = parseResult.GetValue(noMinMaxOption);
                var noReturns = parseResult.GetValue(noReturnsOption);
                var json = parseResult.GetValue(jsonOption);
                var boundingBox = parseResult.GetValue(Options.InsideRectangle);
                foreach (var file in parseResult.GetRequiredValue(Arguments.Inputs))
                {
                    using var stream = File.OpenRead(file, services);
                    Info.Processor.Process(stream, console, System.Globalization.CultureInfo.InvariantCulture, noMinMax, noReturns, json, boundingBox);
                }
            });

            return command;
        }
    }
}