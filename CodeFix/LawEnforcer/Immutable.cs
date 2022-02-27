using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LawEnforcer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class Immutable : Attribute
    {
        public Immutable(string reason = "")
        {

        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ImmutableAnalyzer : Analyzer<Immutable>
    {
        public ImmutableAnalyzer() : base("002", $"Mutable fields and properties are not allowed in this {nameof(AttributeTargets.Class).ToLower()} or {nameof(AttributeTargets.Struct).ToLower()}")
        {

        }

        protected override bool ContextViolatesRule(SyntaxNodeAnalysisContext context)
        {
            var result = this.AnyPropertyHasSetter(context)
                || this.AnyFieldIsNotReadonly(context);

            return result;
        }

        private bool AnyPropertyHasSetter(SyntaxNodeAnalysisContext context)
        {
            var properties = this.MembersToAnalyze(context)
                .OfType<PropertyDeclarationSyntax>()
                .Select(e => context.SemanticModel.GetDeclaredSymbol(e));
            var anyPropertyHasSetter = properties.Any(e => e.IsReadOnly is false);

            return anyPropertyHasSetter;
        }

        private bool AnyFieldIsNotReadonly(SyntaxNodeAnalysisContext context)
        {
            var fields = this.MembersToAnalyze(context)
                .OfType<FieldDeclarationSyntax>()
                .SelectMany(e => e.Declaration.Variables)
                .Select(e => context.SemanticModel.GetDeclaredSymbol(e))
                .OfType<IFieldSymbol>();
            var anyFieldIsNotReadonly = fields.Any(e => e.IsReadOnly is false);

            return anyFieldIsNotReadonly;
        }

        private IEnumerable<MemberDeclarationSyntax> MembersToAnalyze(SyntaxNodeAnalysisContext context)
        {
            TypeDeclarationSyntax classDecoratedWithAttribute = context.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            TypeDeclarationSyntax structDecoratedWithAttribute = context.Node.FirstAncestorOrSelf<StructDeclarationSyntax>();

            return (classDecoratedWithAttribute ?? structDecoratedWithAttribute).Members;
        }
    }
}