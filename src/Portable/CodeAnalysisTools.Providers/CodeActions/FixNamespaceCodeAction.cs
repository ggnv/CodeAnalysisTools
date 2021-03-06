﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeAnalysisTools.CodeActions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace CodeAnalysisTools.Providers.CodeActions
{
	public class FixNamespaceCodeAction : CodeAnalysisCodeAction
	{
		private readonly Document document;
		private readonly NamespaceDeclarationSyntax namespaceDeclaration;

		public FixNamespaceCodeAction(Document document, NamespaceDeclarationSyntax namespaceDeclaration)
		{
			this.document = document;
			this.namespaceDeclaration = namespaceDeclaration;
		}

		public override string Title
		{
			get
			{
				return "Fix Namespace";
			}
		}

		protected async override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);

			var folders = this.document.Folders;
			var newNamespaceName = this.GetNewNamespaceName(this.document);
			var newNameSpace = this.namespaceDeclaration.WithName(
				newNamespaceName.WithLeadingTrivia(this.namespaceDeclaration.Name.GetLeadingTrivia())
								.WithTrailingTrivia(this.namespaceDeclaration.Name.GetTrailingTrivia()));

			return this.document.WithSyntaxRoot(root.ReplaceNode(this.namespaceDeclaration, newNameSpace));
		}

		private NameSyntax GetNewNamespaceName(Document document)
		{
			var newNamespaceName = document.Project.Name;
			if (document.Folders.Any())
			{
				newNamespaceName += "." + string.Join(".", document.Folders);
			}

			return SyntaxFactory.ParseName(newNamespaceName);
		}
	}
}
