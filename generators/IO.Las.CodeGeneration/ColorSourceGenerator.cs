// -----------------------------------------------------------------------
// <copyright file="ColorSourceGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;

/// <summary>
/// The color source generator.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class ColorSourceGenerator : BaseSourceGenerator
{
    /// <inheritdoc/>
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        base.Initialize(context);
        var rootNamespaceAndExcludeFromCodeCoverage = GetRootNamespaceAndExcludeFromCodeCoverage(context);
        var color = context.CompilationProvider
            .Combine(rootNamespaceAndExcludeFromCodeCoverage)
            .Select(static (c, _) => c.Left.GetTypeByMetadataName($"{c.Right.Left}.{nameof(System.Drawing.Color)}"))
            .Combine(rootNamespaceAndExcludeFromCodeCoverage);

        context.RegisterSourceOutput(color, static (context, colorWithNamespace) =>
        {
            if (colorWithNamespace.Left is { } color)
            {
                context.AddSource($"{color.Name}.Known.cs", GetSourceText(Colors.CreateKnownColors(GetNamespace(colorWithNamespace.Right.Left))));
            }
        });
    }
}