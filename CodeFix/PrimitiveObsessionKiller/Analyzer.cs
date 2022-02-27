using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PrimitiveObsessionKiller
{
    public abstract class Analyzer<T> : DiagnosticAnalyzer
    {
        private readonly DiagnosticDescriptor _rule;

        public Analyzer(string id, string message)
        {
            this._rule = new DiagnosticDescriptor(
                $"C{id}",
                message,
                string.Empty,
                string.Empty,
                DiagnosticSeverity.Error,
                true,
                message);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(this._rule);

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
            if(this.AttributeIsOfAnalyzedType(context) && this.ContextViolatesRule(context))
            {
                this.ReportViolation(context);
            }
        }

        private bool AttributeIsOfAnalyzedType(SyntaxNodeAnalysisContext context)
        {
            var attributeIsOfProperType = context.SemanticModel.GetTypeInfo(context.Node).ConvertedType.Equals(
                context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(T).FullName)
            );

            return attributeIsOfProperType;
        }

        protected abstract bool ContextViolatesRule(SyntaxNodeAnalysisContext context);

        private void ReportViolation(SyntaxNodeAnalysisContext context)
        {
            var diagnostics = Diagnostic.Create(
                id: this._rule.Id,
                category: this._rule.Category,
                message: this._rule.Title,
                severity: DiagnosticSeverity.Error,
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                warningLevel: 0,
                location: context.Node.GetLocation()
            );

            context.ReportDiagnostic(diagnostics);
        }
    }
}