﻿using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace CodeAnalysisTools.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MultipleBlankLinesCodeFixProvider)), Shared]
	public class MultipleBlankLinesCodeFixProvider : CodeFixProvider
	{
		private const string title = "Remove multiple blank lines";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CATA001"); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start);

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					c => RemoveMultipleBlankLinesAsync(context.Document, declaration, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> RemoveMultipleBlankLinesAsync(Document document, SyntaxToken declaration, CancellationToken cancellationToken)
		{
			var newDeclaration = declaration
				.WithLeadingTrivia(declaration.LeadingTrivia.RemoveConsecutiveEndLines())
				.WithTrailingTrivia(declaration.TrailingTrivia.RemoveConsecutiveEndLines());

			var root = await document.GetSyntaxRootAsync();
			var newRoot = root.ReplaceToken(declaration, newDeclaration);

			// Return the new solution with the now-uppercase type name.
			return document.WithSyntaxRoot(newRoot);
		}
	}
}