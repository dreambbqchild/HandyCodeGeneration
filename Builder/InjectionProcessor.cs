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
    public class InjectionProcessor
    {
        private IEnumerable<FieldDeclarationSyntax> GetFields(ConstructorDeclarationSyntax ctor, IParameterTransform transform, SyntaxToken[] fieldModifiers)
        {            
            foreach (var param in ctor.ParameterList.Parameters)
                yield return FieldDeclaration(transform.TransformToVariable(param)
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(string.Concat("_", param.Identifier.ValueText)))))
                    .WithModifiers(TokenList(fieldModifiers));
        }        

        public string CreateClass(string csharp, IMethodBuilder methodBuilder, IParameterTransform transform, params SyntaxKind[] fieldModifiers)
        {
            var fieldModifierTokens = fieldModifiers.Length > 0 ? fieldModifiers.Select(f => Token(f)).ToArray() 
                                                                : new SyntaxToken[] { Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword) };
            var tree = CSharpSyntaxTree.ParseText(csharp);
            var root = tree.GetRoot();
            var ctor = root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (ctor == null)
                return string.Empty;

            return ClassDeclaration(ctor.Identifier)
                .WithMembers(List(GetFields(ctor, transform, fieldModifierTokens).Concat(new MemberDeclarationSyntax[] { methodBuilder.Build(ctor, transform) })))                
                .WithModifiers(TokenList(new SyntaxToken[] { Token(SyntaxKind.PublicKeyword) }))
                .NormalizeWhitespace()
                .ToFullString();
        }
    }
}
