// -----------------------------------------------------------------------
// <copyright file="SourceGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// The LAS source generators.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_SOURCE_GENERATOR
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif
        var rootNamespaceAndExcludeFromCodeCoverage = context.AnalyzerConfigOptionsProvider.Select(static (options, _) => GetRootNamespace(options))
            .Combine(context.CompilationProvider.Select(static (compilation, _) => compilation.GetTypeByMetadataName("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute") is not null));

        var platformCsv = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileNameWithoutExtension(file.Path) is "platforms" && Path.GetExtension(file.Path) is ".csv")
            .Combine(rootNamespaceAndExcludeFromCodeCoverage);

        var modelCsv = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileNameWithoutExtension(file.Path) is "models" && Path.GetExtension(file.Path) is ".csv")
            .Combine(rootNamespaceAndExcludeFromCodeCoverage);

#if LAS1_2_OR_GREATER
        var color = context.CompilationProvider
            .Combine(rootNamespaceAndExcludeFromCodeCoverage)
            .Select(static (c, _) => c.Left.GetTypeByMetadataName($"{c.Right.Left}.{nameof(System.Drawing.Color)}"))
            .Combine(rootNamespaceAndExcludeFromCodeCoverage);
#endif

        context.RegisterSourceOutput(platformCsv, static (context, platformCsvWithNamespace) =>
        {
            var platformCsv = platformCsvWithNamespace.Left;
            var (ids, types) = Platforms.GetIdsAndTypes(platformCsv);
            var @namespace = GetNamespace(platformCsvWithNamespace.Right.Left);
            var excludeFromCodeCoverage = platformCsvWithNamespace.Right.Right;
            context.AddSource("PlatformId.cs", GetSourceText(Platforms.CreatePlatformIds(@namespace, ids, excludeFromCodeCoverage)));
            context.AddSource("PlatformType.cs", GetSourceText(Platforms.CreatePlatformTypes(@namespace, types, excludeFromCodeCoverage)));

            var platforms = Platforms.GetPlatforms(platformCsv);
            context.AddSource($"{nameof(Platforms)}.cs", GetSourceText(Platforms.CreatePlatforms(@namespace, platforms)));
            context.AddSource("Platform.Parse.cs", GetSourceText(Platforms.CreatePlatformParse(@namespace, platforms, excludeFromCodeCoverage)));
        });

        context.RegisterSourceOutput(modelCsv, static (context, modelCsvWithNamespace) =>
        {
            var modelCsv = modelCsvWithNamespace.Left;
            var models = Models.GetModels(modelCsv);
            var @namespace = GetNamespace(modelCsvWithNamespace.Right.Left);
            var excludeFromCodeCoverage = modelCsvWithNamespace.Right.Right;

            context.AddSource("Brand.cs", GetSourceText(Models.CreateBrands(@namespace, Models.GetBrands(modelCsv), excludeFromCodeCoverage)));
            context.AddSource($"{nameof(Models)}.cs", GetSourceText(Models.CreateModels(@namespace, models)));
            context.AddSource("Model.Parse.cs", GetSourceText(Models.CreateModelsParse(@namespace, models, excludeFromCodeCoverage)));
        });

#if LAS1_2_OR_GREATER
        context.RegisterSourceOutput(color, static (context, colorWithNamespace) =>
        {
            if (colorWithNamespace.Left is { } color)
            {
                context.AddSource($"{color.Name}.Known.cs", GetSourceText(Colors.CreateKnownColors(GetNamespace(colorWithNamespace.Right.Left))));
            }
        });
#endif

        static string GetRootNamespace(Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider options)
        {
            return options.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace)
                ? rootNamespace
                : typeof(SourceGenerator).Namespace?.Replace(".CodeGeneration", string.Empty) ?? string.Empty;
        }

        static Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax GetNamespace(string rootNamespace)
        {
            return SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(rootNamespace));
        }

        static Microsoft.CodeAnalysis.Text.SourceText GetSourceText(SyntaxNode compilationUnitSyntax)
        {
            return compilationUnitSyntax.NormalizeWhitespace().GetText(System.Text.Encoding.UTF8);
        }
    }
}