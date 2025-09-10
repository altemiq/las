// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.To.Las.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <c>to las</c> extensions.
/// </summary>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <c>to las</c> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddToLas<T>(this T command)
        where T : Command
    {
        command.Add(CreateCommand());
        return command;

        static Command CreateCommand()
        {
            var command = new Command("las", Tool.Properties.Resources.Command_ToLasDescription)
            {
                Arguments.Input,
                Options.Output,
            };

            command.SetAction(static parseResult => To.Las.Processor.Process(parseResult.GetServices(), parseResult.GetRequiredValue(Arguments.Input), parseResult.GetRequiredValue(Options.Output)));

            return command;
        }
    }
}