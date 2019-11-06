using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InjectionBuilder.Builder
{
    public interface IParameterTransform
    {
        TypeSyntax ChangeType(ParameterSyntax param);
        VariableDeclarationSyntax TransformToVariable(ParameterSyntax param);
        ParameterListSyntax TransformToParameters(ParameterListSyntax @params);
    }
}
