// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.To.Sorted.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <content>
/// The <c>sort</c> extensions.
/// </content>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <c>sort</c> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddToSorted<T>(this T command)
        where T : Command
    {
        command.Add(CreateCommand());

        return command;

        static Command CreateCommand()
        {
            var command = new Command("sorted", Tool.Properties.v1_4.Resources.Command_ToSortedDescription)
            {
                Arguments.Inputs,
                Options.To.Output,
                Options.Indexing.Threshold,
                Options.Indexing.Append,
            };

            command.SetAction(parseResult => To.Sorted.Processor.Process(parseResult.GetServices(), parseResult.InvocationConfiguration.Output, parseResult.GetRequiredValue(Arguments.Inputs), parseResult.GetValue(Options.To.Output), parseResult.GetValue(Options.Indexing.Append)));

            return command;
        }
    }
}