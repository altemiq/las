// -----------------------------------------------------------------------
// <copyright file="Models.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <summary>
/// Model generation.
/// </summary>
internal static class Models
{
    /// <summary>
    /// Creates the models.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="models">The models to create.</param>
    /// <returns>The created models.</returns>
    public static CompilationUnitSyntax CreateModels(BaseNamespaceDeclarationSyntax @namespace, IReadOnlyCollection<(string? Brand, string? Model, string? Code)> models)
    {
        return Generation.CreateClass(@namespace, nameof(Models), "The models.", GetBrandClasses(models));

        static IEnumerable<ClassDeclarationSyntax> GetBrandClasses(IReadOnlyCollection<(string? Brand, string? Model, string? Code)> models)
        {
            // get the brands
            foreach (var brand in models.Select(m => m.Brand).Distinct(StringComparer.Ordinal))
            {
                if (brand is null)
                {
                    continue;
                }

                yield return ClassDeclaration(Generation.CleanupIdentifier(brand))
                    .WithModifiers(
                            TokenList(
                                Token(
                                    TriviaList(
                                        Trivia(
                                            DocumentationCommentTrivia(
                                                SyntaxKind.SingleLineDocumentationCommentTrivia,
                                                List(
                                                    new XmlNodeSyntax[]
                                                    {
                                                        XmlText()
                                                        .WithTextTokens(
                                                            TokenList(
                                                                XmlTextLiteral(
                                                                    TriviaList(
                                                                        DocumentationCommentExterior("///")),
                                                                    " ",
                                                                    " ",
                                                                    TriviaList()))),
                                                        XmlExampleElement(
                                                            SingletonList<XmlNodeSyntax>(
                                                                XmlText()
                                                                .WithTextTokens(
                                                                    TokenList(
                                                                        XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                        XmlTextLiteral(
                                                                            TriviaList(
                                                                                DocumentationCommentExterior("///")),
                                                                            $" {brand}",
                                                                            $" {brand}",
                                                                            TriviaList()),
                                                                        XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                        XmlTextLiteral(
                                                                            TriviaList(
                                                                                DocumentationCommentExterior("///")),
                                                                            " ",
                                                                            " ",
                                                                            TriviaList())))))
                                                        .WithStartTag(XmlElementStartTag(XmlName(Identifier("summary"))))
                                                        .WithEndTag(XmlElementEndTag(XmlName(Identifier("summary")))),
                                                        XmlText()
                                                        .WithTextTokens(TokenList(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false))),
                                                    })))),
                                    SyntaxKind.PublicKeyword,
                                    TriviaList()),
                                Token(SyntaxKind.StaticKeyword)))
                    .WithMembers(List(GetModelClasses(brand, [.. models.Where(m => string.Equals(m.Brand, brand, StringComparison.Ordinal)).Select(m => (m.Model, m.Code))])));
            }

            static IEnumerable<MemberDeclarationSyntax> GetModelClasses(string brand, IReadOnlyCollection<(string? Model, string? Code)> models)
            {
                foreach (var model in models.Select(m => m.Model).Distinct(StringComparer.Ordinal))
                {
                    if (model is null)
                    {
                        continue;
                    }

                    // see how many there are
                    var fields = models
                        .Where(m => string.Equals(m.Model, model, StringComparison.Ordinal) && m.Code is not null)
                        .Select(m => m.Code)
                        .Cast<string>()
                        .ToArray();
                    switch (fields.Length)
                    {
                        case > 1:
                            // return a class
                            yield return ClassDeclaration(Generation.CleanupIdentifier(model))
                                .WithModifiers(
                                    TokenList(
                                        Token(
                                            TriviaList(
                                                Trivia(
                                                    DocumentationCommentTrivia(
                                                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                                                        List(
                                                            new XmlNodeSyntax[]
                                                            {
                                                                XmlText()
                                                                    .WithTextTokens(
                                                                        TokenList(
                                                                            XmlTextLiteral(
                                                                                TriviaList(
                                                                                    DocumentationCommentExterior("///")),
                                                                                " ",
                                                                                " ",
                                                                                TriviaList()))),
                                                                XmlExampleElement(
                                                                        SingletonList<XmlNodeSyntax>(
                                                                            XmlText()
                                                                                .WithTextTokens(
                                                                                    TokenList(
                                                                                        XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                                        XmlTextLiteral(
                                                                                            TriviaList(
                                                                                                DocumentationCommentExterior("///")),
                                                                                            $" {model}",
                                                                                            $" {model}",
                                                                                            TriviaList()),
                                                                                        XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                                        XmlTextLiteral(
                                                                                            TriviaList(
                                                                                                DocumentationCommentExterior("///")),
                                                                                            " ",
                                                                                            " ",
                                                                                            TriviaList())))))
                                                                    .WithStartTag(XmlElementStartTag(XmlName(Identifier("summary"))))
                                                                    .WithEndTag(XmlElementEndTag(XmlName(Identifier("summary")))),
                                                                XmlText()
                                                                    .WithTextTokens(TokenList(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false))),
                                                            })))),
                                            SyntaxKind.PublicKeyword,
                                            TriviaList()),
                                        Token(SyntaxKind.StaticKeyword)))
                                .WithMembers(List(GetFields(fields.Select(f => (brand, model, f)), useModelAsIdentifier: false)));
                            break;

                        case > 0:
                            foreach (var field in GetFields(fields.Select(f => (brand, model, f)), useModelAsIdentifier: true))
                            {
                                yield return field;
                            }

                            break;
                    }
                }

                static IEnumerable<MemberDeclarationSyntax> GetFields(IEnumerable<(string Brand, string Model, string Code)> models, bool useModelAsIdentifier)
                {
                    foreach (var (brand, model, code) in models)
                    {
                        yield return GetField(brand, model, code, GetIdentifier(brand, model, code, useModelAsIdentifier));
                    }

                    static FieldDeclarationSyntax GetField(string brand, string model, string code, string identifierName)
                    {
                        var brandName = Generation.CleanupIdentifier(brand);

                        return FieldDeclaration(
                            VariableDeclaration(IdentifierName("Model"))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(Identifier(identifierName))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ObjectCreationExpression(IdentifierName("Model"))
                                            .WithArgumentList(
                                                ArgumentList(
                                                    SeparatedList<ArgumentSyntax>(
                                                        new SyntaxNodeOrToken[]
                                                        {
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("Brand"),
                                                            IdentifierName(brandName))),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(model))),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(code))),
                                                        }))))))))
                            .WithModifiers(
                            TokenList(
                                Token(
                                    TriviaList(
                                        Trivia(
                                            DocumentationCommentTrivia(
                                                SyntaxKind.SingleLineDocumentationCommentTrivia,
                                                List(
                                                    new XmlNodeSyntax[]
                                                    {
                                                XmlText()
                                                .WithTextTokens(
                                                    TokenList(
                                                        XmlTextLiteral(
                                                            TriviaList(
                                                                DocumentationCommentExterior("///")),
                                                            " ",
                                                            " ",
                                                            TriviaList()))),
                                                XmlExampleElement(
                                                    SingletonList<XmlNodeSyntax>(
                                                        XmlText()
                                                        .WithTextTokens(
                                                            TokenList(
                                                                XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                XmlTextLiteral(
                                                                    TriviaList(
                                                                        DocumentationCommentExterior("///")),
                                                                    $" {brand} - {model}",
                                                                    $" {brand} - {model}",
                                                                    TriviaList()),
                                                                XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                XmlTextLiteral(
                                                                    TriviaList(
                                                                        DocumentationCommentExterior("///")),
                                                                    " ",
                                                                    " ",
                                                                    TriviaList())))))
                                                .WithStartTag(XmlElementStartTag(XmlName(Identifier("summary"))))
                                                .WithEndTag(XmlElementEndTag(XmlName(Identifier("summary")))),
                                                XmlText()
                                                .WithTextTokens(TokenList(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false))),
                                                    })))),
                                    SyntaxKind.PublicKeyword,
                                    TriviaList()),
                                Token(SyntaxKind.StaticKeyword),
                                Token(SyntaxKind.ReadOnlyKeyword)));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates the model parsing methods.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="models">The models.</param>
    /// <param name="excludeFromCodeCoverage">Set to <see langword="true"/> to exclude methods from code coverage.</param>
    /// <returns>The model parsing methods.</returns>
    public static CompilationUnitSyntax CreateModelsParse(BaseNamespaceDeclarationSyntax @namespace, IReadOnlyCollection<(string? Brand, string? Model, string? Code)> models, bool excludeFromCodeCoverage)
    {
        return CompilationUnit()
            .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                @namespace
                .WithNamespaceKeyword(
                    Token(
                        TriviaList(Comment("// <autogenerated />")),
                        SyntaxKind.NamespaceKeyword,
                        TriviaList()))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        RecordDeclaration(
                            Token(SyntaxKind.RecordKeyword),
                            Identifier("Model"))
                        .WithModifiers(
                            TokenList(
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.PartialKeyword)))
                        .WithClassOrStructKeyword(
                            Token(SyntaxKind.StructKeyword))
                        .WithOpenBraceToken(
                            Token(SyntaxKind.OpenBraceToken))
                        .WithMembers(
                            List(
                            [
                                CreateParseMethod(models, excludeFromCodeCoverage),
                                CreateTryParseMethod(models, excludeFromCodeCoverage),
                            ]))
                        .WithCloseBraceToken(
                            Token(SyntaxKind.CloseBraceToken))))));

        static MemberDeclarationSyntax CreateParseMethod(IEnumerable<(string? Brand, string? Model, string? Code)> models, bool excludeFromCodeCoverage)
        {
            var methodDeclaration = MethodDeclaration(
                IdentifierName("Model"),
                Identifier("Parse"));

            if (excludeFromCodeCoverage)
            {
                methodDeclaration = methodDeclaration.WithAttributeLists(
                    SingletonList(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName(nameof(System)),
                                                IdentifierName(nameof(System.Diagnostics))),
                                            IdentifierName(nameof(System.Diagnostics.CodeAnalysis))),
                                        IdentifierName("ExcludeFromCodeCoverage")))))));
            }

            return methodDeclaration
                .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.PartialKeyword)))
                .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                            Identifier("s"))
                        .WithType(
                            PredefinedType(
                                Token(SyntaxKind.StringKeyword))))))
                .WithExpressionBody(
                ArrowExpressionClause(
                    SwitchExpression(
                        IdentifierName("s"))
                    .WithArms(
                        SeparatedList<SwitchExpressionArmSyntax>(
                            GetSwitchExpressionArms(models)))))
                .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken));

            static IEnumerable<SyntaxNodeOrToken> GetSwitchExpressionArms(IEnumerable<(string? Brand, string? Model, string? Code)> models)
            {
                var brandLookup = models
                    .Where(static m => m.Brand is not null && m.Model is not null && m.Code is not null)
                    .ToLookup(
                        static x => x.Brand!,
                        static x => (x.Model!, x.Code!),
                        StringComparer.Ordinal);

                foreach (var brandGroup in brandLookup)
                {
                    var brand = brandGroup.Key;
                    var modelLookup = brandGroup
                        .ToLookup(
                            static i => i.Item1,
                            static i => i.Item2,
                            StringComparer.Ordinal);

                    foreach (var modelGroup in modelLookup)
                    {
                        var model = modelGroup.Key;
                        var useModelAsIdentifier = !modelGroup.Skip(1).Any();
                        foreach (var code in modelGroup)
                        {
                            yield return GetSwitchExpressionArm(brand, model, code, useModelAsIdentifier);
                            yield return Token(SyntaxKind.CommaToken);
                        }
                    }
                }

                yield return SwitchExpressionArm(
                    DiscardPattern(),
                    ThrowExpression(
                        ObjectCreationExpression(
                            IdentifierName(nameof(KeyNotFoundException)))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        IdentifierName("s")))))));
                yield return Token(SyntaxKind.CommaToken);

                static SwitchExpressionArmSyntax GetSwitchExpressionArm(string brand, string model, string code, bool useModelAsIdentifier)
                {
                    return SwitchExpressionArm(
                        ConstantPattern(
                            LiteralExpression(
                                SyntaxKind.CharacterLiteralExpression,
                                Literal(code))),
                        GetModelAccess(brand, model, code, useModelAsIdentifier));
                }
            }
        }

        static MemberDeclarationSyntax CreateTryParseMethod(IEnumerable<(string? Brand, string? Model, string? Code)> models, bool excludeFromCodeCoverage)
        {
            var methodDeclaration = MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.BoolKeyword)),
                Identifier("TryParse"));

            if (excludeFromCodeCoverage)
            {
                methodDeclaration = methodDeclaration.WithAttributeLists(
                    SingletonList(
                        AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName(nameof(System)),
                                                IdentifierName(nameof(System.Diagnostics))),
                                            IdentifierName(nameof(System.Diagnostics.CodeAnalysis))),
                                        IdentifierName("ExcludeFromCodeCoverage")))))));
            }

            return methodDeclaration
                .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword),
                    Token(SyntaxKind.PartialKeyword)))
                .WithParameterList(
                ParameterList(
                    SeparatedList<ParameterSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            Parameter(
                                Identifier("s"))
                            .WithType(
                                PredefinedType(
                                    Token(SyntaxKind.StringKeyword))),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("model"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.OutKeyword)))
                            .WithType(
                                IdentifierName("Model")),
                        })))
                .WithBody(
                Block(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            TupleExpression(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                        Argument(
                                            DeclarationExpression(
                                                IdentifierName(
                                                    Identifier(
                                                        TriviaList(),
                                                        SyntaxKind.VarKeyword,
                                                        "var",
                                                        "var",
                                                        TriviaList())),
                                                SingleVariableDesignation(
                                                    Identifier("result")))),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(
                                            IdentifierName("model")),
                                    })),
                            SwitchExpression(
                                IdentifierName("s"))
                            .WithArms(
                                SeparatedList<SwitchExpressionArmSyntax>(
                                    GetSwitchExpressionArms(models))))),
                    ReturnStatement(
                        IdentifierName("result"))));

            static IEnumerable<SyntaxNodeOrToken> GetSwitchExpressionArms(IEnumerable<(string? Brand, string? Model, string? Code)> models)
            {
                var brandLookup = models
                    .Where(static m => m.Brand is not null && m.Model is not null && m.Code is not null)
                    .ToLookup(
                        static x => x.Brand!,
                        static x => (x.Model!, x.Code!),
                        StringComparer.Ordinal);

                foreach (var brandGroup in brandLookup)
                {
                    var brand = brandGroup.Key;
                    var modelLookup = brandGroup
                        .ToLookup(
                            static i => i.Item1,
                            static i => i.Item2,
                            StringComparer.Ordinal);

                    foreach (var modelGroup in modelLookup)
                    {
                        var model = modelGroup.Key;
                        var useModelAsIdentifier = !modelGroup.Skip(1).Any();
                        foreach (var code in modelGroup)
                        {
                            yield return GetSwitchExpressionArm(brand, model, code, useModelAsIdentifier);
                            yield return Token(SyntaxKind.CommaToken);
                        }
                    }
                }

                yield return SwitchExpressionArm(
                    DiscardPattern(),
                    TupleExpression(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.FalseLiteralExpression)),
                                Token(SyntaxKind.CommaToken),
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token(SyntaxKind.DefaultKeyword))),
                            })));
                yield return Token(SyntaxKind.CommaToken);

                static SwitchExpressionArmSyntax GetSwitchExpressionArm(string brand, string model, string code, bool useModelAsIdentifier)
                {
                    return SwitchExpressionArm(
                        ConstantPattern(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(code))),
                        TupleExpression(
                            SeparatedList<ArgumentSyntax>(
                                new SyntaxNodeOrToken[]
                                {
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.TrueLiteralExpression)),
                                    Token(SyntaxKind.CommaToken),
                                    Argument(
                                        GetModelAccess(brand, model, code, useModelAsIdentifier)),
                                })));
                }
            }
        }
    }

    /// <summary>
    /// Creates the brands.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="ids">The brand identifiers.</param>
    /// <param name="excludeFromCodeCoverage">Set to <see langword="true"/> to exclude methods from code coverage.</param>
    /// <returns>The brands.</returns>
    public static CompilationUnitSyntax CreateBrands(BaseNamespaceDeclarationSyntax @namespace, IReadOnlyCollection<string?> ids, bool excludeFromCodeCoverage) => Generation.CreateEnum(@namespace, "Brand", "The brands.", ids, excludeFromCodeCoverage);

    /// <summary>
    /// Gets the brands from the CSV.
    /// </summary>
    /// <param name="input">The input CSV.</param>
    /// <returns>The brands.</returns>
    public static IReadOnlyCollection<string?> GetBrands(AdditionalText input) => [.. input.GetLines().Select(static line => line.Substring(0, line.IndexOf(','))).Distinct(StringComparer.Ordinal)];

    /// <summary>
    /// Gets the models from the CSV.
    /// </summary>
    /// <param name="input">The input CSV.</param>
    /// <returns>The models.</returns>
    public static IReadOnlyCollection<(string? Brand, string? Model, string? Code)> GetModels(AdditionalText input)
    {
        return [.. input.GetLines()
            .Select(static line => line.Split(','))
            .Select(static record => (Brand: CheckNull(record[0]), Model: CheckNull(record[1]), Code: CheckNull(record[2]))),];

        static string? CheckNull(string input)
        {
            return string.IsNullOrEmpty(input) ? null : input;
        }
    }

    private static string GetIdentifier(string brand, string model, string code, bool useModelAsIdentifier)
    {
        var identifierName = useModelAsIdentifier
            ? Generation.CleanupIdentifier(model)
            : Generation.CleanupIdentifier(code);

        if (char.IsDigit(identifierName[0]))
        {
            var temp = useModelAsIdentifier
                ? Generation.CleanupIdentifier(brand)
                : Generation.CleanupIdentifier(model);
            identifierName = temp + identifierName;
        }

        if (char.IsDigit(identifierName[0])
            && !useModelAsIdentifier)
        {
            identifierName = Generation.CleanupIdentifier(brand) + identifierName;
        }

        return identifierName;
    }

    private static MemberAccessExpressionSyntax GetModelAccess(string brand, string model, string code, bool useModelAsIdentifier)
    {
        var identifierName = IdentifierName(GetIdentifier(brand, model, code, useModelAsIdentifier));
        var modelsIdentifier = IdentifierName(nameof(Models));
        var brandIdentifier = IdentifierName(Generation.CleanupIdentifier(brand));
        var modelIdentifier = IdentifierName(Generation.CleanupIdentifier(model));

        return useModelAsIdentifier
            ? MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    modelsIdentifier,
                    brandIdentifier),
                identifierName)
            : MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        modelsIdentifier,
                        brandIdentifier),
                    modelIdentifier),
                identifierName);
    }
}