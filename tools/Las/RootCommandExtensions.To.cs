// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.To.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <pre>to</pre> extensions.
/// </summary>
internal static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds the <pre>to</pre> command.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddTo<T>(this T command)
        where T : Command
    {
        command.Add(CreateCommand());
        return command;

        static Command CreateCommand()
        {
            return new Command("to", Tool.Properties.Resources.Command_ToDescription)
                .AddToLas()
                .AddToLaz()
#if LAS1_4_OR_GREATER
                .AddToCopc()
                .AddToSorted()
#endif
                .AddToExploded();
        }
    }
}