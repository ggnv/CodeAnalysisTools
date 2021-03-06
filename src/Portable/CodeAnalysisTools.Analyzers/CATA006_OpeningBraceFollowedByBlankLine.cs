﻿using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class OpeningBraceFollowedByBlankLineAnalyzer : SyntaxTreeAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(Rule);
			}
		}

		public override string DiagnosticId
		{
			get
			{
				return "CATA006";
			}
		}

		public override string Title
		{
			get
			{
				return "Opening curly bracket is followed by a blank line.";
			}
		}

		public override string Description
		{
			get
			{
				return "The opening curly bracket should not be followed by a blank line.";
			}
		}

		public override string MessageFormat
		{
			get
			{
				return "Opening curly bracket is followed by a blank line.";
			}
		}

		public override string Category
		{
			get
			{
				return "Readability";
			}
		}

		public async override Task AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
		{
			var root = await context.Tree.GetRootAsync().ConfigureAwait(false);
			var openingBrace = root.DescendantTokens().Where(x => x.IsKind(SyntaxKind.OpenBraceToken));

			foreach (var brace in openingBrace)
			{
				var nextToken = brace.GetNextToken();
				if (brace.IsFollowedByBlankLine())
				{
					var diagnostic = Diagnostic.Create(Rule, brace.GetLocation(), string.Empty);

					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}
}