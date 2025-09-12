// -----------------------------------------------------------------------
// <copyright file="Platforms.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

/// <summary>
/// Platform generation.
/// </summary>
internal static class Platforms
{
    /// <summary>
    /// Creates the platorms.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="platforms">The platforms to create.</param>
    /// <returns>The created platforms.</returns>
    public static CompilationUnitSyntax CreatePlatforms(BaseNamespaceDeclarationSyntax @namespace, IEnumerable<(string? Id, string? Type, char Code)> platforms)
    {
        return Generation.CreateClass(@namespace, nameof(Platforms), "The platforms.", GetFields(platforms));

        static IEnumerable<MemberDeclarationSyntax> GetFields(IEnumerable<(string? Id, string? Type, char Code)> platforms)
        {
            foreach (var platform in platforms)
            {
                if (platform.Id is null || platform.Type is null)
                {
                    continue;
                }

                yield return GetField(platform.Id, platform.Type, platform.Code);
            }

            static FieldDeclarationSyntax GetField(string id, string type, char code)
            {
                var idName = Generation.CleanupIdentifier(id);
                var typeName = Generation.CleanupIdentifier(type);
                return FieldDeclaration(
                    VariableDeclaration(IdentifierName("Platform"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(idName))
                            .WithInitializer(
                                EqualsValueClause(
                                    ObjectCreationExpression(IdentifierName("Platform"))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SeparatedList<ArgumentSyntax>(
                                                new SyntaxNodeOrToken[]
                                                {
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("PlatformId"),
                                                            IdentifierName(idName))),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("PlatformType"),
                                                            IdentifierName(typeName))),
                                                    Token(SyntaxKind.CommaToken),
                                                    Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.CharacterLiteralExpression,
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
                                                                        DocumentationCommentExterior("    ///")),
                                                                    $" {id}.",
                                                                    $" {id}.",
                                                                    TriviaList()),
                                                                XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                XmlTextLiteral(
                                                                    TriviaList(
                                                                        DocumentationCommentExterior("    ///")),
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

    /// <summary>
    /// Creates the platform parsing methods.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="platforms">The platforms.</param>
    /// <param name="excludeFromCodeCoverage">Set to <see langword="true"/> to exclude methods from code coverage.</param>
    /// <returns>The platform parsing methods.</returns>
    public static CompilationUnitSyntax CreatePlatformParse(BaseNamespaceDeclarationSyntax @namespace, IReadOnlyCollection<(string? Id, string? Type, char Code)> platforms, bool excludeFromCodeCoverage)
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
                            Identifier("Platform"))
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
                                    GetParseMethod(platforms, excludeFromCodeCoverage),
                                    CreateTryParseMethod(platforms, excludeFromCodeCoverage),
                                ]))
                        .WithCloseBraceToken(
                            Token(SyntaxKind.CloseBraceToken))))));

        static MemberDeclarationSyntax GetParseMethod(IEnumerable<(string? Id, string? Type, char Code)> platforms, bool excludeFromCodeCoverage)
        {
            var methodDeclaration = MethodDeclaration(
                IdentifierName("Platform"),
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
                            Identifier("c"))
                        .WithType(
                            PredefinedType(
                                Token(SyntaxKind.CharKeyword))))))
                .WithExpressionBody(
                ArrowExpressionClause(
                    SwitchExpression(
                        IdentifierName("c"))
                    .WithArms(
                        SeparatedList<SwitchExpressionArmSyntax>(
                            GetSwitchExpressionArms(platforms)))))
                .WithSemicolonToken(
                Token(SyntaxKind.SemicolonToken));

            static IEnumerable<SyntaxNodeOrToken> GetSwitchExpressionArms(IEnumerable<(string? Id, string? Type, char Code)> platforms)
            {
                foreach (var platform in platforms)
                {
                    if (platform.Id is null)
                    {
                        continue;
                    }

                    yield return GetSwitchExpressionArm(platform.Id, platform.Code);
                    yield return Token(SyntaxKind.CommaToken);
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
                                        ObjectCreationExpression(
                                            PredefinedType(
                                                Token(SyntaxKind.StringKeyword)))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SeparatedList<ArgumentSyntax>(
                                                    new SyntaxNodeOrToken[]
                                                    {
                                                        Argument(
                                                            IdentifierName("c")),
                                                        Token(SyntaxKind.CommaToken),
                                                        Argument(
                                                            LiteralExpression(
                                                                SyntaxKind.NumericLiteralExpression,
                                                                Literal(1))),
                                                    })))))))));
                yield return Token(SyntaxKind.CommaToken);

                static SwitchExpressionArmSyntax GetSwitchExpressionArm(string id, char code)
                {
                    return SwitchExpressionArm(
                        ConstantPattern(
                            LiteralExpression(
                                SyntaxKind.CharacterLiteralExpression,
                                Literal(code))),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Platforms"),
                            IdentifierName(Generation.CleanupIdentifier(id))));
                }
            }
        }

        static MemberDeclarationSyntax CreateTryParseMethod(IEnumerable<(string? Id, string? Type, char Code)> platforms, bool excludeFromCodeCoverage)
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
                                Identifier("c"))
                            .WithType(
                                PredefinedType(
                                    Token(SyntaxKind.CharKeyword))),
                            Token(SyntaxKind.CommaToken),
                            Parameter(
                                Identifier("platform"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.OutKeyword)))
                            .WithType(
                                IdentifierName("Platform")),
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
                                            IdentifierName("platform")),
                                    })),
                            SwitchExpression(
                                IdentifierName("c"))
                            .WithArms(
                                SeparatedList<SwitchExpressionArmSyntax>(
                                    GetSwitchExpressionArms(platforms))))),
                    ReturnStatement(
                        IdentifierName("result"))));

            static IEnumerable<SyntaxNodeOrToken> GetSwitchExpressionArms(IEnumerable<(string? Id, string? Type, char Code)> platforms)
            {
                foreach (var (id, _, code) in platforms)
                {
                    if (id is null)
                    {
                        continue;
                    }

                    yield return GetSwitchExpressionArm(id, code);
                    yield return Token(SyntaxKind.CommaToken);
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

                static SwitchExpressionArmSyntax GetSwitchExpressionArm(string id, char code)
                {
                    return SwitchExpressionArm(
                        ConstantPattern(
                            LiteralExpression(
                                SyntaxKind.CharacterLiteralExpression,
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
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("Platforms"),
                                            IdentifierName(Generation.CleanupIdentifier(id)))),
                                })));
                }
            }
        }
    }

    /// <summary>
    /// Creates the platform IDs.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="ids">The IDs.</param>
    /// <param name="excludeFromCodeCoverage">Set to <see langword="true"/> to exclude methods from code coverage.</param>
    /// <returns>The created IDs.</returns>
    public static CompilationUnitSyntax CreatePlatformIds(BaseNamespaceDeclarationSyntax @namespace, IReadOnlyCollection<string?> ids, bool excludeFromCodeCoverage) => Generation.CreateEnum(@namespace, "PlatformId", "The platforms.", ids, excludeFromCodeCoverage);

    /// <summary>
    /// Creates the platform types.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <param name="ids">The IDs.</param>
    /// <param name="excludeFromCodeCoverage">Set to <see langword="true"/> to exclude methods from code coverage.</param>
    /// <returns>The created IDs.</returns>
    public static CompilationUnitSyntax CreatePlatformTypes(BaseNamespaceDeclarationSyntax @namespace, IReadOnlyCollection<string?> ids, bool excludeFromCodeCoverage) => Generation.CreateEnum(@namespace, "PlatformType", "The platform type.", ids, excludeFromCodeCoverage);

    /// <summary>
    /// Gets the IDs and Types from the CSV.
    /// </summary>
    /// <param name="input">The input CSV.</param>
    /// <returns>The IDs and Types.</returns>
    public static (IReadOnlyCollection<string?> Ids, IReadOnlyCollection<string?> Types) GetIdsAndTypes(AdditionalText input)
    {
        var records = input.GetLines().Select(static line => line.Split(',')).ToList();

        var ids = records.Select(static record => record[0]).ToArray();
        var types = records.Select(static record => record[1]).Distinct(StringComparer.Ordinal).ToArray();

        return (ids, types);
    }

    /// <summary>
    /// Gets the platforms from the CSV.
    /// </summary>
    /// <param name="input">The input CSV.</param>
    /// <returns>The platforms.</returns>
    public static IReadOnlyCollection<(string? Id, string? Type, char Code)> GetPlatforms(AdditionalText input)
    {
        return [.. input.GetLines()
            .Select(static line => line.Split(','))
            .Select(static record => (Id: CheckNull(record[0]), Type: CheckNull(record[1]), Code: record[2][0])),];

        static string? CheckNull(string input)
        {
            return string.IsNullOrEmpty(input) ? null : input;
        }
    }
}