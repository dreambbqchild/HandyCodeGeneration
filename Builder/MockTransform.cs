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

    public class MockTransform : IParameterTransform
    {
        private static readonly GenericNameSyntax genericMock;
            
        static MockTransform()
        {
            var mockTemplate = CSharpSyntaxTree.ParseText("private Mock<IFoo> thing;");
            var root = mockTemplate.GetRoot();
            genericMock = root.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault();
        }

        public TypeSyntax ChangeType(ParameterSyntax param)
        {
            return genericMock.WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(param.Type)));
        }

        public VariableDeclarationSyntax TransformToVariable(ParameterSyntax param)
        {
            return VariableDeclaration(ChangeType(param));
        }

        public ParameterListSyntax TransformToParameters(ParameterListSyntax @params)
        {
            return ParameterList(SeparatedList(@params.Parameters.Select(p => Parameter(p.Identifier).WithType(ChangeType(p)))));
        }
    }
}
