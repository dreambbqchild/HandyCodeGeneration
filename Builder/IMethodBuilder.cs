using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InjectionBuilder.Builder
{
    using static SyntaxFactory;

    public interface IMethodBuilder
    {
        MemberDeclarationSyntax Build(ConstructorDeclarationSyntax ctor, IParameterTransform transform);
    }

    public sealed class ConstructorBuilder : IMethodBuilder
    {
        private BlockSyntax ConstructorBody(ConstructorDeclarationSyntax ctor)
        {
            return Block(ctor.ParameterList.Parameters
                .Select(p => ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(string.Concat("_", p.Identifier.ValueText)), IdentifierName(p.Identifier.ValueText)))));
        }

        public MemberDeclarationSyntax Build(ConstructorDeclarationSyntax ctor, IParameterTransform transform)
        {
            return ConstructorDeclaration(ctor.Identifier)
                .WithParameterList(transform.TransformToParameters(ctor.ParameterList))
                .WithBody(ConstructorBody(ctor))
                .WithModifiers(TokenList(new SyntaxToken[] { Token(SyntaxKind.PublicKeyword) }));
        }
    }

    public sealed class InitalizeBuilder : IMethodBuilder
    {
        private BlockSyntax InitializeBody(ConstructorDeclarationSyntax ctor, IParameterTransform transform)
        {
            return Block(ctor.ParameterList.Parameters
                .Select(p => ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(string.Concat("_", p.Identifier.ValueText)), ObjectCreationExpression(transform.ChangeType(p)).WithArgumentList(ArgumentList())))));
        }

        public MemberDeclarationSyntax Build(ConstructorDeclarationSyntax ctor, IParameterTransform transform)
        {
            return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "Initialize")
                .WithBody(InitializeBody(ctor, transform))
                .WithModifiers(TokenList(new SyntaxToken[] { Token(SyntaxKind.PrivateKeyword) }));
        }
    }
}
