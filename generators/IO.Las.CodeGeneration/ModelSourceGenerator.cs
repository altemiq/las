// -----------------------------------------------------------------------
// <copyright file="ModelSourceGenerator.cs" company="Altemiq">
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
public class ModelSourceGenerator : BaseSourceGenerator
{
    /// <inheritdoc/>
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        base.Initialize(context);
        var modelCsv = context.AdditionalTextsProvider
            .Where(static file => Path.GetFileNameWithoutExtension(file.Path) is "models" && Path.GetExtension(file.Path) is ".csv")
            .Combine(GetRootNamespaceAndExcludeFromCodeCoverage(context));

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
    }
}