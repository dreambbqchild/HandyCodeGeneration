using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InjectionBuilder.Mapper
{
    public static class SyntaxTokenExtension
    {
        public static string VariableName(this SyntaxToken identifier)
        {
            return string.Concat(Char.ToLower(identifier.Text[0]), identifier.Text.Substring(1));
        }
    }
}
