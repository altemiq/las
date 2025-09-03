// -----------------------------------------------------------------------
// <copyright file="Options.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las;

/// <summary>
/// Options.
/// </summary>
internal static partial class Options
{
    /// <summary>
    /// The input option.
    /// </summary>
    public static readonly Option<Uri[]> Input = new("-i") { CustomParser = System.CommandLine.Parsing.UriParser.ParseAll };

    /// <summary>
    /// The output option.
    /// </summary>
    public static readonly Option<FileInfo> Output = new("-o", "--output") { Description = Tool.Properties.Resources.Option_OutputDescription, HelpName = "OUTPUT" };

    /// <summary>
    /// The inside rectangle option.
    /// </summary>
    public static readonly Option<BoundingBox?> InsideRectangle = new("--inside-rectangle")
    {
        AllowMultipleArgumentsPerToken = true,
        Arity = new(4, 6),
        CustomParser = static argumentResult =>
        {
            return argumentResult.Tokens switch
            {
                { Count: 4 } tokens => new BoundingBox(Parse(tokens[0]), Parse(tokens[1]), double.MinValue, Parse(tokens[2]), Parse(tokens[3]), double.MaxValue),
                { Count: 6 } tokens => new BoundingBox(Parse(tokens[0]), Parse(tokens[1]), Parse(tokens[2]), Parse(tokens[3]), Parse(tokens[4]), Parse(tokens[5])),
                _ => default,
            };

            static double Parse(System.CommandLine.Parsing.Token token)
            {
                return double.Parse(token.Value, System.Globalization.CultureInfo.CurrentCulture);
            }
        },
    };
}