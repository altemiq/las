// -----------------------------------------------------------------------
// <copyright file="BaseSourceGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;

/// <summary>
/// The base <see cref="IIncrementalGenerator"/>.
/// </summary>
public abstract class BaseSourceGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public virtual void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG_SOURCE_GENERATOR
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif
    }

    /// <summary>
    /// Get the namespace syntax.
    /// </summary>
    /// <param name="namespace">The root namespace.</param>
    /// <returns>The namespace syntax.</returns>
    protected static Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax GetNamespace(string @namespace) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.FileScopedNamespaceDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName(@namespace));

    /// <summary>
    /// Gets the source text.
    /// </summary>
    /// <param name="compilationUnitSyntax">The compilation unit syntax.</param>
    /// <returns>The source text.</returns>
    protected static Microsoft.CodeAnalysis.Text.SourceText GetSourceText(SyntaxNode compilationUnitSyntax) => compilationUnitSyntax.NormalizeWhitespace().GetText(System.Text.Encoding.UTF8);

    /// <summary>
    /// Gets the root namespace, and whether we have code coverage attributes.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The incremental value provider.</returns>
    protected static IncrementalValueProvider<(string Left, bool Right)> GetRootNamespaceAndExcludeFromCodeCoverage(IncrementalGeneratorInitializationContext context)
    {
        return context.AnalyzerConfigOptionsProvider.Select(static (options, _) => GetRootNamespace(options))
            .Combine(context.CompilationProvider.Select(static (compilation, _) => compilation.GetTypeByMetadataName("System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute") is not null));

        static string GetRootNamespace(Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider options)
        {
            return options.GlobalOptions.TryGetValue("build_property.rootnamespace", out var rootNamespace)
                ? rootNamespace
                : typeof(BaseSourceGenerator).Namespace?.Replace(".CodeGeneration", string.Empty) ?? string.Empty;
        }
    }
}