// -----------------------------------------------------------------------
// <copyright file="Arguments.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// The LAS arguments.
/// </summary>
internal static class Arguments
{
    /// <summary>
    /// The input arguments.
    /// </summary>
    public static readonly Argument<Uri> Input = new("INPUT") { Description = Tool.Properties.Resources.Argument_InputDescription, CustomParser = System.CommandLine.Parsing.UriParser.Parse };

    /// <summary>
    /// The input arguments.
    /// </summary>
    public static readonly Argument<Uri[]> Inputs = new("INPUT") { Description = Tool.Properties.Resources.Argument_InputsDescription, CustomParser = System.CommandLine.Parsing.UriParser.ParseAll };

    /// <summary>
    /// Clones the specified <see cref="Argument{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type in the argument.</typeparam>
    /// <param name="argument">The argument to clone.</param>
    /// <returns>The cloned argument.</returns>
    public static Argument<T> Clone<T>(this Argument<T> argument)
    {
        var clone = new Argument<T>(argument.Name)
        {
            HelpName = argument.HelpName,
            Description = argument.Description,
            Arity = argument.Arity,
            CustomParser = argument.CustomParser,
            DefaultValueFactory = argument.DefaultValueFactory,
            Hidden = argument.Hidden,
        };

        clone.CompletionSources.AddRange(argument.CompletionSources);
        clone.Validators.AddRange(argument.Validators);

        return clone;
    }

    /// <summary>
    /// Sets the <see cref="Argument.Arity"/>.
    /// </summary>
    /// <typeparam name="T">The type of argument.</typeparam>
    /// <param name="argument">The argument.</param>
    /// <param name="argumentArity">The argument arity.</param>
    /// <returns>The input argument.</returns>
    public static T WithArity<T>(this T argument, ArgumentArity argumentArity)
        where T : Argument
    {
        argument.Arity = argumentArity;
        return argument;
    }
}