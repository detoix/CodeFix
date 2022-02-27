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
    public sealed class POCOAttribute : Attribute
    {
        public POCOAttribute(string reason = "")
        {

        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class POCOAnalyzer : Analyzer<POCOAttribute>
    {
        public POCOAnalyzer() : base("003", $"Complex features are not allowed since this {nameof(AttributeTargets.Class).ToLower()} or {nameof(AttributeTargets.Struct).ToLower()} is designed to be serializable")
        {

        }

        protected override bool ContextViolatesRule(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = this.TypeDeclaration(context);
            var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
            var typeIsPublicAndSimple = typeSymbol.DeclaredAccessibility == Accessibility.Public
                && typeSymbol.IsAbstract == false
                && typeSymbol.IsOverride == false
                && typeSymbol.IsStatic == false
                && typeSymbol.IsVirtual == false;
            var onlyPropertiesWithPublicGettersAndSetters = typeDeclaration.Members.All(e => e is PropertyDeclarationSyntax propertyNode
                && propertyNode.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration) && a.Body is null)
                && propertyNode.AccessorList.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) && a.Body is null)
                && context.SemanticModel.GetDeclaredSymbol(propertyNode) is IPropertySymbol property
                && property.DeclaredAccessibility == Accessibility.Public
                && property.IsDefinition == true
                && property.IsAbstract == false
                && property.IsReadOnly == false
                && property.IsWriteOnly == false
                && property.IsIndexer == false
                && property.IsWithEvents == false
                && property.ReturnsByRef == false
                && property.IsOverride == false
                && property.IsVirtual == false
                && property.SetMethod.DeclaredAccessibility == Accessibility.Public
                && property.GetMethod.DeclaredAccessibility == Accessibility.Public);
            var result = !typeIsPublicAndSimple || !onlyPropertiesWithPublicGettersAndSetters;

            return result;
        }

        private TypeDeclarationSyntax TypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            TypeDeclarationSyntax classDecoratedWithAttribute = context.Node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            TypeDeclarationSyntax structDecoratedWithAttribute = context.Node.FirstAncestorOrSelf<StructDeclarationSyntax>();

            return (classDecoratedWithAttribute ?? structDecoratedWithAttribute);
        }
    }
}