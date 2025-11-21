// -----------------------------------------------------------------------
// <copyright file="PlatformSourceGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;

/// <summary>
/// The LAS source generators.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class PlatformSourceGenerator : BaseSourceGenerator
{
    /// <inheritdoc/>
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        base.Initialize(context);
        var platformCsv = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileNameWithoutExtension(file.Path) is "platforms" && Path.GetExtension(file.Path) is ".csv")
            .Combine(GetRootNamespaceAndExcludeFromCodeCoverage(context));

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
    }
}