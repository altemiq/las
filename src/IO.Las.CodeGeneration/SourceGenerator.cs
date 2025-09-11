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
[Generator]
public class SourceGenerator : ISourceGenerator
{
    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "Checked, this is not applicable as an incremental source generator")]
    public void Execute(GeneratorExecutionContext context)
    {
        var modelsCsv = context.AdditionalFiles.Single(static file => Path.GetFileNameWithoutExtension(file.Path) is "models" && Path.GetExtension(file.Path) is ".csv");
        var platformsCsv = context.AdditionalFiles.Single(static file => Path.GetFileNameWithoutExtension(file.Path) is "platforms" && Path.GetExtension(file.Path) is ".csv");

        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.Rootnamespace", out var rootNamespace))
        {
            rootNamespace = typeof(SourceGenerator).Namespace?.Replace(".CodeGeneration", string.Empty) ?? string.Empty;
        }

        var excludeFromCodeCoverage = context.Compilation.GetTypeByMetadataName("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute") is not null;

        var @namespace = GetNamespace(rootNamespace);

        var (ids, types) = Platforms.GetIdsAndTypes(platformsCsv);
        context.AddSource("PlatformId.cs", GetSourceText(Platforms.CreatePlatformIds(@namespace, ids, excludeFromCodeCoverage)));
        context.AddSource("PlatformType.cs", GetSourceText(Platforms.CreatePlatformTypes(@namespace, types, excludeFromCodeCoverage)));

        var platforms = Platforms.GetPlatforms(platformsCsv);
        context.AddSource($"{nameof(Platforms)}.cs", GetSourceText(Platforms.CreatePlatforms(@namespace, platforms)));
        context.AddSource("Platform.Parse.cs", GetSourceText(Platforms.CreatePlatformParse(@namespace, platforms, excludeFromCodeCoverage)));

        context.AddSource("Brand.cs", GetSourceText(Models.CreateBrands(@namespace, Models.GetBrands(modelsCsv), excludeFromCodeCoverage)));

        var models = Models.GetModels(modelsCsv);
        context.AddSource($"{nameof(Models)}.cs", GetSourceText(Models.CreateModels(@namespace, models)));
        context.AddSource("Model.Parse.cs", GetSourceText(Models.CreateModelsParse(@namespace, models, excludeFromCodeCoverage)));

#if LAS1_2_OR_GREATER
        context.AddSource($"{nameof(System.Drawing.Color)}.cs", GetSourceText(Colors.CreateKnownColors(@namespace)));
#endif

        static Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax GetNamespace(string rootNamespace)
        {
            return SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(rootNamespace));
        }

        static Microsoft.CodeAnalysis.Text.SourceText GetSourceText(SyntaxNode compilationUnitSyntax)
        {
            return compilationUnitSyntax.NormalizeWhitespace().GetText(System.Text.Encoding.UTF8);
        }
    }

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG_SOURCE_GENERATOR
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#else
        // This must be implemented
#endif
    }
}