// -----------------------------------------------------------------------
// <copyright file="ColorSourceGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Altemiq.IO.Las.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
                context.AddSource($"{color.Name}.Known.cs", GetSourceText(CreateKnownColors(GetNamespace(colorWithNamespace.Right.Left))));
            }
        });
    }

    private static CompilationUnitSyntax CreateKnownColors(BaseNamespaceDeclarationSyntax @namespace)
    {
        return CreateStruct(@namespace, nameof(System.Drawing.Color), Concat(GetConstructor(), GetColors()));

        static IEnumerable<ConstructorDeclarationSyntax> GetConstructor()
        {
            const string Value = "value";

            yield return ConstructorDeclaration(
                    Identifier(nameof(System.Drawing.Color)))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PrivateKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier(Value))
                                .WithType(
                                    PredefinedType(
                                        Token(SyntaxKind.LongKeyword))))))
                .WithBody(
                    Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(nameof(System.Drawing.Color.R))),
                                CheckedExpression(
                                    SyntaxKind.UncheckedExpression,
                                    CastExpression(
                                        PredefinedType(
                                            Token(SyntaxKind.UShortKeyword)),
                                        ParenthesizedExpression(
                                            BinaryExpression(
                                                SyntaxKind.RightShiftExpression,
                                                IdentifierName(Value),
                                                IdentifierName("RGBRedShift"))))))),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(nameof(System.Drawing.Color.G))),
                                CheckedExpression(
                                    SyntaxKind.UncheckedExpression,
                                    CastExpression(
                                        PredefinedType(
                                            Token(SyntaxKind.UShortKeyword)),
                                        ParenthesizedExpression(
                                            BinaryExpression(
                                                SyntaxKind.RightShiftExpression,
                                                IdentifierName(Value),
                                                IdentifierName("RGBGreenShift"))))))),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(nameof(System.Drawing.Color.B))),
                                CheckedExpression(
                                    SyntaxKind.UncheckedExpression,
                                    CastExpression(
                                        PredefinedType(
                                            Token(SyntaxKind.UShortKeyword)),
                                        ParenthesizedExpression(
                                            BinaryExpression(
                                                SyntaxKind.RightShiftExpression,
                                                IdentifierName(Value),
                                                IdentifierName("RGBBlueShift")))))))));
        }

        static IEnumerable<PropertyDeclarationSyntax> GetColors()
        {
            const int ArgbRedShift = 32;
            const int ArgbGreenShift = 16;
            const int ArgbBlueShift = 0;

            var type = typeof(System.Drawing.Color);
            foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                if (property.GetValue(null) is not System.Drawing.Color { A: byte.MaxValue } color)
                {
                    continue;
                }

                var red = (color.R << 8) | color.R;
                var green = (color.G << 8) | color.G;
                var blue = (color.B << 8) | color.B;

                var value = ((ulong)red << ArgbRedShift) | ((ulong)green << ArgbGreenShift) | ((ulong)blue << ArgbBlueShift);

                var stringValue = value.ToString("X12", System.Globalization.CultureInfo.InvariantCulture);

                yield return
                    PropertyDeclaration(
                            IdentifierName(nameof(System.Drawing.Color)),
                            Identifier(property.Name))
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
                                                                XmlText()
                                                                    .WithTextTokens(
                                                                        TokenList(
                                                                            XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                            XmlTextLiteral(
                                                                                TriviaList(
                                                                                    DocumentationCommentExterior("///")),
                                                                                " Gets a system-defined color that has an RGB value of ",
                                                                                " Gets a system-defined color that has an RGB value of ",
                                                                                TriviaList()))),
                                                                XmlExampleElement(
                                                                        SingletonList<XmlNodeSyntax>(
                                                                            XmlText()
                                                                                .WithTextTokens(
                                                                                    TokenList(
                                                                                        XmlTextLiteral(
                                                                                            TriviaList(),
                                                                                            $"#{stringValue}",
                                                                                            $"#{stringValue}",
                                                                                            TriviaList())))))
                                                                    .WithStartTag(
                                                                        XmlElementStartTag(
                                                                            XmlName(
                                                                                Identifier("c"))))
                                                                    .WithEndTag(
                                                                        XmlElementEndTag(
                                                                            XmlName(
                                                                                Identifier("c")))),
                                                                XmlText()
                                                                    .WithTextTokens(
                                                                        TokenList(
                                                                            XmlTextLiteral(
                                                                                TriviaList(),
                                                                                ".",
                                                                                ".",
                                                                                TriviaList()),
                                                                            XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                            XmlTextLiteral(
                                                                                TriviaList(
                                                                                    DocumentationCommentExterior("///")),
                                                                                " ",
                                                                                " ",
                                                                                TriviaList()))))
                                                            .WithStartTag(XmlElementStartTag(XmlName(Identifier("summary"))))
                                                            .WithEndTag(XmlElementEndTag(XmlName(Identifier("summary")))),
                                                        XmlText()
                                                            .WithTextTokens(
                                                                TokenList(
                                                                    XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false),
                                                                    XmlTextLiteral(
                                                                        TriviaList(
                                                                            DocumentationCommentExterior("///")),
                                                                        " ",
                                                                        " ",
                                                                        TriviaList()))),
                                                        XmlExampleElement(
                                                                XmlText()
                                                                    .WithTextTokens(
                                                                        TokenList(
                                                                            XmlTextLiteral(
                                                                                TriviaList(),
                                                                                "A ",
                                                                                "A ",
                                                                                TriviaList()))),
                                                                XmlNullKeywordElement()
                                                                    .WithAttributes(
                                                                        SingletonList<XmlAttributeSyntax>(
                                                                            XmlCrefAttribute(
                                                                                NameMemberCref(
                                                                                    IdentifierName(nameof(System.Drawing.Color)))))),
                                                                XmlText()
                                                                    .WithTextTokens(
                                                                        TokenList(
                                                                            XmlTextLiteral(
                                                                                TriviaList(),
                                                                                " representing a system-defined color.",
                                                                                " representing a system-defined color.",
                                                                                TriviaList()))))
                                                            .WithStartTag(XmlElementStartTag(XmlName(Identifier("returns"))))
                                                            .WithEndTag(XmlElementEndTag(XmlName(Identifier("returns")))),
                                                        XmlText()
                                                            .WithTextTokens(TokenList(XmlTextNewLine(Constants.NewLine, continueXmlDocumentationComment: false))),
                                                    })))),
                                    SyntaxKind.PublicKeyword,
                                    TriviaList()),
                                Token(SyntaxKind.StaticKeyword)))
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                ObjectCreationExpression(
                                        IdentifierName(nameof(System.Drawing.Color)))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.NumericLiteralExpression,
                                                        Literal(
                                                            "0x" + stringValue,
                                                            value))))))))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken));
            }
        }

        static CompilationUnitSyntax CreateStruct(BaseNamespaceDeclarationSyntax @namespace, string name, IEnumerable<MemberDeclarationSyntax> members)
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
                            StructDeclaration(name)
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.PartialKeyword)))
                            .WithMembers(List(members))))));
        }

        static IEnumerable<MemberDeclarationSyntax> Concat(params ReadOnlySpan<IEnumerable<MemberDeclarationSyntax>> values)
        {
            if (values.Length is 0)
            {
                return [];
            }

            var enumerable = values[0];

            for (var i = 1; i < values.Length; i++)
            {
                enumerable = enumerable.Concat(values[i]);
            }

            return enumerable;
        }
    }
}