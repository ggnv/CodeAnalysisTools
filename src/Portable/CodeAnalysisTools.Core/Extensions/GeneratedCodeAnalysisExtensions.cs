﻿using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalysisTools
{
	public static class GeneratedCodeAnalysisExtensions
	{
		public static bool IsGenerated(this SyntaxNodeAnalysisContext context) => (context.SemanticModel?.SyntaxTree?.IsGenerated() ?? false); //|| (context.Node?.IsGenerated() ?? false);

		public static bool IsGenerated(this SyntaxTreeAnalysisContext context) => context.Tree?.IsGenerated() ?? false;

		////public static bool IsGenerated(this SymbolAnalysisContext context)
		////{
		////	if (context.Symbol == null) return false;
		////	foreach (var syntaxReference in context.Symbol.DeclaringSyntaxReferences)
		////	{
		////		if (syntaxReference.SyntaxTree.IsGenerated()) return true;
		////		var root = syntaxReference.SyntaxTree.GetRoot();
		////		var node = root?.FindNode(syntaxReference.Span);
		////		if (node.IsGenerated()) return true;
		////	}
		////	return false;
		////}
		//// public static bool IsGenerated(this SyntaxNode node) => node.HasAttributeOnAncestorOrSelf("DebuggerNonUserCode", "GeneratedCode");

		public static bool IsGenerated(this SyntaxTree tree) => (tree.FilePath?.IsOnGeneratedFile() ?? false) || tree.HasAutoGeneratedComment();

		public static bool HasAutoGeneratedComment(this SyntaxTree tree)
		{
			var root = tree.GetRoot();
			if (root == null) return false;
			var firstToken = root.GetFirstToken();
			SyntaxTriviaList trivia;
			if (firstToken == default(SyntaxToken))
			{
				var token = ((CompilationUnitSyntax)root).EndOfFileToken;
				if (!token.HasLeadingTrivia) return false;
				trivia = token.LeadingTrivia;
			}
			else
			{
				if (!firstToken.HasLeadingTrivia) return false;
				trivia = firstToken.LeadingTrivia;
			}
			var commentLines = trivia.Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia)).Take(2).ToList();
			if (commentLines.Count != 2) return false;
			return commentLines[1].ToString() == "// <auto-generated>";
		}

		public static bool IsOnGeneratedFile(this string filePath) =>
			Regex.IsMatch(
				filePath, 
				@"(\\service|\\TemporaryGeneratedFile_.*|\\assemblyinfo|\\assemblyattributes|\.(g\.i|g|designer|generated|assemblyattributes))\.(cs|vb)$",
				RegexOptions.IgnoreCase);
	}
}
