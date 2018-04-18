using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeFix
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixCodeFixProvider)), Shared]
    public class CodeFixCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(CodeFixAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context
                .Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root.FindToken(diagnosticSpan.Start).Parent;

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Update properties",
                    e => UpdateProperties(context.Document, declaration, e)),
                diagnostic);
        }

        private async Task<Document> UpdateProperties(Document document, SyntaxNode constructor, CancellationToken cancellationToken)
        {
            if (constructor is ConstructorDeclarationSyntax declaration)
            {
                var parameters = declaration.ParameterList.Parameters;
                var properties = parameters.Select(e => e.ConvertToProperty()).ToArray();
                var assignments = parameters
                    .Zip(properties, (parameter, property) => parameter.AssignTo(property))
                    .ToArray();
                var syntaxTree = await document
                    .GetSyntaxTreeAsync(cancellationToken);
                var newConstructor = declaration
                    .WithBody(declaration.Body.AddStatements(assignments));

                var newSyntaxTree = syntaxTree
                    .GetRoot()
                    .TrackNodes(constructor);
                var withProperties = newSyntaxTree
                    .InsertNodesBefore(
                        newSyntaxTree.GetCurrentNode(constructor),
                        properties);
                var withAssignments = withProperties.ReplaceNode(
                        withProperties.GetCurrentNode(constructor),
                        newConstructor);

                document = document.WithSyntaxRoot(withAssignments);
            }

            return document;
        }
    }

    public static class Helpers
    {
        public static PropertyDeclarationSyntax ConvertToProperty(this ParameterSyntax parameter)
            => SyntaxFactory.PropertyDeclaration(parameter.Type, parameter.Identifier.Text.CapitalizeFirstLetter())
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

        public static StatementSyntax AssignTo(this ParameterSyntax parameter, PropertyDeclarationSyntax property)
            => SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(property.Identifier),
                    SyntaxFactory.IdentifierName(parameter.Identifier)));

        public static string CapitalizeFirstLetter(this string s)
            => char.ToUpper(s[0]) + s.Substring(1);
    }
}
