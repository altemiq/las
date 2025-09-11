// -----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

/// <summary>
/// Extension methods.
/// </summary>
internal static class ExtensionMethods
{
    /// <summary>
    /// Gets the string lines from a <see cref="Microsoft.CodeAnalysis.AdditionalText"/>.
    /// </summary>
    /// <param name="text">The additional text.</param>
    /// <returns>The lines.</returns>
    public static IEnumerable<string> GetLines(this Microsoft.CodeAnalysis.AdditionalText text)
    {
        return GetLinesCore(text.GetText());

        static IEnumerable<string> GetLinesCore(Microsoft.CodeAnalysis.Text.SourceText? input)
        {
            return input?.Lines.Skip(1).Where(static line => line.End != line.Start).Select(line => line.ToString()) ?? [];
        }
    }
}