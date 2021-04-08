using InjectionBuilder.Mapper;
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
                .Select(p => ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(string.Concat("_", p.Identifier.ValueText)), 
                    BinaryExpression(SyntaxKind.CoalesceExpression, IdentifierName(p.Identifier.ValueText), ThrowExpression(ObjectCreationExpression(IdentifierName("ArgumentNullException")).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(InvocationExpression(IdentifierName("nameof")).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName(p.Identifier.ValueText)))))))))))))));
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
        private ExpressionStatementSyntax ExpressionSyntaxFor(string name, TypeSyntax type, ArgumentListSyntax argList)
        {
            return ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression, IdentifierName(
                            string.Concat("_", name)),
                        ObjectCreationExpression(type)
                        .WithArgumentList(argList)));
        }

        private ArgumentListSyntax GetMockObjects(ParameterListSyntax parameterList)
        {
            return ArgumentList(SeparatedList(parameterList.Parameters
                .Select(p => Argument(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(string.Concat("_", p.Identifier.ValueText)),
                    IdentifierName("Object"))))));
        }

        private BlockSyntax InitializeBody(ConstructorDeclarationSyntax ctor, IParameterTransform transform)
        {
            return Block(ctor.ParameterList.Parameters
                .Select(p => ExpressionSyntaxFor(p.Identifier.ValueText, transform.ChangeType(p), ArgumentList()))
                .Concat(new ExpressionStatementSyntax[] 
                { 
                    ExpressionSyntaxFor(ctor.Identifier.VariableName(), ParseTypeName(ctor.Identifier.ValueText), GetMockObjects(ctor.ParameterList))
                }));
        }

        public MemberDeclarationSyntax Build(ConstructorDeclarationSyntax ctor, IParameterTransform transform)
        {
            return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "Initialize")
                .WithBody(InitializeBody(ctor, transform))
                .WithModifiers(TokenList(new SyntaxToken[] { Token(SyntaxKind.PrivateKeyword) }));
        }
    }
}
