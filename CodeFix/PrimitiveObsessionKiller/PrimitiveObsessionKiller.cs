using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PrimitiveObsessionKiller
{
    public sealed class PrimitiveObsessionKillerAttribute : Attribute
    {

    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CodeFixAnalyzer : DiagnosticAnalyzer
    {
        private const string Message = "Primitive types are not allowed in this method";

        private static readonly ISet<SpecialType> DisallowedTypes = new[]
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
        }
        .ToImmutableHashSet();

        private static DiagnosticDescriptor Rule
            = new DiagnosticDescriptor(
                "C001",
                Message,
                string.Empty,
                string.Empty,
                DiagnosticSeverity.Error,
                true,
                Message);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(Analyze);
        }

        private void Analyze(CompilationStartAnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                AnalyzeSyntaxNodeAndReportDiagnostics, SyntaxKind.Attribute);
        }

        private void AnalyzeSyntaxNodeAndReportDiagnostics(SyntaxNodeAnalysisContext context)
        {
            var attributeIsOfProperType = context.SemanticModel.GetTypeInfo(context.Node).ConvertedType.Equals(
                context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(PrimitiveObsessionKillerAttribute).FullName)
            );

            if(attributeIsOfProperType)
            {
                var methodDecoratedWithAttribute = context.Node.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
                var typesOfMethodArguments = methodDecoratedWithAttribute.ParameterList.Parameters
                    .Select(e => context.SemanticModel.GetDeclaredSymbol(e))
                    .Select(e => e.Type.SpecialType)
                    .ToImmutableHashSet();
                var primitiveMethodArguments = typesOfMethodArguments
                    .Intersect(DisallowedTypes);

                if (primitiveMethodArguments.Any())
                {
                    var problem = this.CreateDiagnostic(
                        Rule, context.Node.GetLocation());
                    context.ReportDiagnostic(problem);
                }
            }
        }

        private Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, Location location)
        {
            return Diagnostic.Create(
                diagnosticDescriptor.Id,
                diagnosticDescriptor.Category,
                message: Message,
                severity: DiagnosticSeverity.Error,
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                warningLevel: 0,
                location: location,
                helpLink: diagnosticDescriptor.HelpLinkUri,
                title: diagnosticDescriptor.Title
            );
        }
    }
}
