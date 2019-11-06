using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace InjectionBuilder.Builder
{
    using static SyntaxFactory;

    public class PassthroughTransform : IParameterTransform
    {
        public TypeSyntax ChangeType(ParameterSyntax param)
        {
            return param.Type;
        }

        public VariableDeclarationSyntax TransformToVariable(ParameterSyntax param)
        {
            return VariableDeclaration(param.Type);
        }

        public ParameterListSyntax TransformToParameters(ParameterListSyntax @params)
        {
            return @params;
        }
    }
}
