using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InjectionBuilder.Mapper
{
    using static SyntaxFactory;

    public class MappingGenerator
    {
        private SyntaxNode GetRootNode(string source)
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            return tree.GetRoot();
        }

        private ExpressionSyntax SetFrom(IdentifierNameSyntax fromClassVarIdentifier, PropertyDeclarationSyntax property, HashSet<string> fromProperties)
        {
            if (fromProperties.Contains(property.Identifier.Text))
                return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        fromClassVarIdentifier,
                        IdentifierName(property.Identifier.Text));

            return LiteralExpression(SyntaxKind.DefaultLiteralExpression, Token(SyntaxKind.DefaultKeyword));
        }

        private IEnumerable<SyntaxNodeOrToken> Assignments(SyntaxNode toRoot, SyntaxNode fromRoot)
        {
            var addComma = false;
            var fromClassVarIdentifier = IdentifierName(fromRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault().Identifier.VariableName());
            var fromProperties = new HashSet<string>(fromRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>().Select(pd => pd.Identifier.Text));
            foreach (var prop in toRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>().Where(p => p.AccessorList.Accessors.Count == 2)) 
            {
                if (addComma)
                    yield return Token(SyntaxKind.CommaToken);

                yield return AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                                  IdentifierName(prop.Identifier.Text).WithTrailingTrivia(Whitespace(" ")),
                                                  SetFrom(fromClassVarIdentifier, prop, fromProperties)
                                                  .WithLeadingTrivia(Whitespace(" ")))
                            .WithLeadingTrivia(EndOfLine(string.Concat(Environment.NewLine, "\t")));

                addComma = true;
            }
        }             

        public string BuildMap(string sourceTo, string sourceFrom) 
        {
            var toRoot = GetRootNode(sourceTo);
            var fromRoot = GetRootNode(sourceFrom);            
            var toClass = toRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            var variableName = toClass.Identifier.VariableName();
            return FieldDeclaration(
                    VariableDeclaration(
                        IdentifierName("var"))
                       .WithTrailingTrivia(Whitespace(" "))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(variableName))
                        .WithInitializer(
                            EqualsValueClause(
                                ObjectCreationExpression(
                                    IdentifierName(toClass.Identifier.Text))
                                .NormalizeWhitespace()
                                .WithLeadingTrivia(Whitespace(" "))
                                .WithArgumentList(
                                        ArgumentList())
                                .WithInitializer(
                                    InitializerExpression(
                                        SyntaxKind.ObjectInitializerExpression,
                                        Token(SyntaxKind.OpenBraceToken)
                                            .WithLeadingTrivia(Whitespace(Environment.NewLine)),
                                        SeparatedList<ExpressionSyntax>(Assignments(toRoot, fromRoot)),
                                        Token(SyntaxKind.CloseBraceToken)
                                            .WithLeadingTrivia(Whitespace(Environment.NewLine))))
                                .WithLeadingTrivia(Whitespace(" ")))
                            .WithLeadingTrivia(Whitespace(" "))))))
                .ToFullString();
        }
    }
}
