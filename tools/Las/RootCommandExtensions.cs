// -----------------------------------------------------------------------
// <copyright file="RootCommandExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="RootCommand"/> extensions.
/// </summary>
public static partial class RootCommandExtensions
{
    /// <summary>
    /// Adds all the tool commands.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    /// <returns>The input builder.</returns>
    public static T AddAllTools<T>(this T command)
        where T : RootCommand
    {
        VariableLengthRecordProcessor.Instance.RegisterTiling();
        return command
            .AddInfo();
    }
}