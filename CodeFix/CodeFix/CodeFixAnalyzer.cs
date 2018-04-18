using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeFix
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CodeFixAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "CodeFix";
        private static readonly LocalizableString Title = new LocalizableResourceString(
            nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(
            nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(
            nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule
            = new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                MessageFormat,
                Category,
                DiagnosticSeverity.Warning,
                true,
                Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
            => context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            if (context.Symbol is IMethodSymbol symbol && symbol.MethodKind == MethodKind.Constructor)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name));
            }
        }
    }
}
