using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PrimitiveObsessionKiller
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
    public sealed class NoPrimitiveTypesAttribute : Attribute
    {
        public NoPrimitiveTypesAttribute(string reason = "")
        {

        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NoPrimitiveTypesAnalyzer : Analyzer<NoPrimitiveTypesAttribute>
    {
        public NoPrimitiveTypesAnalyzer() : base("001", "Primitive types are not allowed in this method")
        {

        }

        protected override bool ContextViolatesRule(SyntaxNodeAnalysisContext context)
        {
            var disallowedTypes = new[]
            {
                SpecialType.System_Boolean,
                SpecialType.System_Byte,
                SpecialType.System_Char,
                SpecialType.System_DateTime,
                SpecialType.System_Decimal,
                SpecialType.System_Double,
                SpecialType.System_Enum,
                SpecialType.System_Int16,
                SpecialType.System_Int32,
                SpecialType.System_Int64,
                SpecialType.System_Object,
                SpecialType.System_SByte,
                SpecialType.System_Single,
                SpecialType.System_String,
                SpecialType.System_UInt16,
                SpecialType.System_UInt32,
                SpecialType.System_UInt64
            };
            var methodDecoratedWithAttribute = context.Node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
            var typesOfMethodArguments = methodDecoratedWithAttribute.ParameterList.Parameters
                .Select(e => context.SemanticModel.GetDeclaredSymbol(e))
                .Select(e => e.Type.SpecialType);
            var primitiveMethodArguments = disallowedTypes
                .Intersect(typesOfMethodArguments);
            var anyPrimitiveArgument = primitiveMethodArguments.Any();

            return anyPrimitiveArgument;
        }
    }
}