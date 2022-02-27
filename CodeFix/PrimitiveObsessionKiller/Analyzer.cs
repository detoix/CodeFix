using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PrimitiveObsessionKiller
{
    public abstract class Analyzer : DiagnosticAnalyzer
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

        protected abstract void AnalyzeSyntaxNodeAndReportDiagnostics(SyntaxNodeAnalysisContext context);

        protected Diagnostic CreateDiagnostic(Location location)
        {
            return Diagnostic.Create(
                id: this._rule.Id,
                category: this._rule.Category,
                message: this._rule.Title,
                severity: DiagnosticSeverity.Error,
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                warningLevel: 0,
                location: location
            );
        }
    }
}