// -----------------------------------------------------------------------
// <copyright file="CommandExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The <see cref="Command"/> extensions.
/// </summary>
internal static class CommandExtensions
{
    /// <content>
    /// The <see cref="Command"/> extensions.
    /// </content>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <param name="command">The command.</param>
    extension<T>(T command)
        where T : Command
    {
        /// <summary>
        /// Adds the specified figlet to the command help.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="console">The console.</param>
        /// <returns>The command for chaining.</returns>
        public T AddFiglet(string text, ConsoleColor color, IAnsiConsole? console = default) => AddFiglet(command, text, Spectre.Console.Color.FromConsoleColor(color), console);

        /// <summary>
        /// Adds the specified figlet to the command help.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="console">The console.</param>
        /// <returns>The command for chaining.</returns>
        public T AddFiglet(string text, System.Drawing.Color color, IAnsiConsole? console = default) => AddFiglet(command, text, new Spectre.Console.Color(color.R, color.G, color.B), console);

        /// <summary>
        /// Adds the specified figlet to the command help.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="console">The console.</param>
        /// <returns>The command for chaining.</returns>
        public T AddFiglet(string text, Spectre.Console.Color color, IAnsiConsole? console = default) => AddFiglet(command, () => new FigletText(text).Color(color), console);

        /// <summary>
        /// Adds the specified figlet to the command help.
        /// </summary>
        /// <param name="font">The figlet font.</param>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="console">The console.</param>
        /// <returns>The command for chaining.</returns>
        public T AddFiglet(FigletFont font, string text, Spectre.Console.Color color, IAnsiConsole? console = default) => AddFiglet(command, () => new FigletText(font, text).Color(color), console);

        /// <summary>
        /// Adds the specified figlet to the command help.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="console">The console.</param>
        /// <returns>The command for chaining.</returns>
        public T AddFiglet(FigletText text, IAnsiConsole? console = default) => AddFiglet(command, () => text, console);
    }

    private static T AddFiglet<T>(T command, Func<FigletText> getText, IAnsiConsole? console = default)
        where T : Command
    {
        if (GetRootCommand(command)?.Options.OfType<System.CommandLine.Help.HelpOption>().SingleOrDefault() is not { Action: System.CommandLine.Invocation.SynchronousCommandLineAction action } option)
        {
            throw new InvalidOperationException(Tool.Properties.Resources.Option_HelpNotFound);
        }

        option.Action = new FigletHelpCommand(command, action, getText, console);

        return command;

        static RootCommand? GetRootCommand(Symbol? symbol)
        {
            return symbol switch
            {
                null => default,
                RootCommand rootCommand => rootCommand,
                { Parents: var parents } => parents.Select(GetRootCommand).FirstOrDefault(static p => p is not null),
            };
        }
    }

    private sealed class FigletHelpCommand(Command command, System.CommandLine.Invocation.SynchronousCommandLineAction action, Func<FigletText> getText, IAnsiConsole? console = default)
        : System.CommandLine.Invocation.SynchronousCommandLineAction
    {
        public override int Invoke(ParseResult parseResult)
        {
            if (parseResult.CommandResult.Command == command)
            {
                GetValueOrDefault(console, global::Spectre.Console.AnsiConsole.Console).Write(getText());

                static T GetValueOrDefault<T>(T? value, T defaultValue)
                {
                    return value ?? defaultValue;
                }
            }

            return action.Invoke(parseResult);
        }
    }
}