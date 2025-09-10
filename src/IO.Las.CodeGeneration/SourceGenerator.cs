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
        if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace))
        {
            rootNamespace = typeof(SourceGenerator).Namespace!.Replace(".CodeGeneration", string.Empty);
        }

        var @namespace = GetNamespace(rootNamespace);

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