// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.To.Copc.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

using Altemiq.IO.Las.To.Copc;

/// <summary>
/// The <c>to copc</c> extensions.
/// </summary>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <c>to copc</c> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddToCopc<T>(this T command)
        where T : Command
    {
        command.Add(CreateCommand());
        return command;

        static Command CreateCommand()
        {
            var maxDepthOption = new Option<int>("--depth", "-d") { DefaultValueFactory = static _ => -1 };
            var maxPointsPerOctantOption = new Option<ulong>("--max-points") { DefaultValueFactory = static _ => 100000UL };
            var gridSpacingOption = new Option<float>("--grid-spacing") { DefaultValueFactory = static _ => 50F };
            var command = new Command("copc", Tool.Properties.v1_4.Resources.Command_ToCopcDescription)
            {
                Arguments.Input,
                Options.To.Output,
                maxDepthOption,
                maxPointsPerOctantOption,
                gridSpacingOption,
            };

            command.SetAction(parseResult => Processor.Process(
                parseResult.GetServices(),
                parseResult.GetRequiredValue(Arguments.Input),
                parseResult.GetRequiredValue(Options.To.Output),
                parseResult.GetValue(maxDepthOption),
                parseResult.GetValue(maxPointsPerOctantOption),
                parseResult.GetValue(gridSpacingOption)));

            return command;
        }
    }
}