// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.To.Laz.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <c>to laz</c> extensions.
/// </summary>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <c>to laz</c> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddToLaz<T>(this T command)
        where T : Command
    {
        command.Add(CreateCommand());
        return command;

        static Command CreateCommand()
        {
            var command = new Command("laz", "Converts a LA(S/Z) file to a new LAZ file.")
            {
                Arguments.Input,
                Options.To.Output,
            };

            command.SetAction(static parseResult => To.Laz.Processor.Process(parseResult.GetServices(), parseResult.GetRequiredValue(Arguments.Input), parseResult.GetRequiredValue(Options.To.Output)));

            return command;
        }
    }
}