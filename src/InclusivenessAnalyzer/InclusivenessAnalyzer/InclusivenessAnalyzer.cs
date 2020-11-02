using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace InclusivenessAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InclusivenessAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Inclusive";
        private static readonly Dictionary<string, string> InclusiveTerms = new Dictionary<string, string>() { 
            {"WhiteList", "AllowList" },
            {"BlackList", "DenyList" }
        };

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string HelpLinkUri = "https://github.com/merill/InclusivenessAnalyzer/";
        private const string Category = "Microsoft.Inclusive";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, helpLinkUri: HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            try
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                context.EnableConcurrentExecution();

                context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field,
                    SymbolKind.Event, SymbolKind.Namespace, SymbolKind.Parameter);

                context.RegisterSyntaxNodeAction(CheckComments, SyntaxKind.VariableDeclaration, SyntaxKind.CatchDeclaration, SyntaxKind.NamespaceDeclaration,
                    SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.EnumDeclaration, SyntaxKind.DelegateDeclaration,
                    SyntaxKind.EnumMemberDeclaration, SyntaxKind.FieldDeclaration, SyntaxKind.EventFieldDeclaration, SyntaxKind.MethodDeclaration, SyntaxKind.OperatorDeclaration,
                    SyntaxKind.ConversionOperatorDeclaration, SyntaxKind.ConstructorDeclaration, SyntaxKind.DestructorDeclaration, SyntaxKind.PropertyDeclaration,
                    SyntaxKind.EventDeclaration, SyntaxKind.IndexerDeclaration, SyntaxKind.GetAccessorDeclaration, SyntaxKind.SetAccessorDeclaration,
                    SyntaxKind.AddAccessorDeclaration, SyntaxKind.RemoveAccessorDeclaration, SyntaxKind.UnknownAccessorDeclaration);
            }
            catch { }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            try
            {
                var symbol = context.Symbol;

                // Find just those named type symbols with non-inclusive terms.
                foreach (KeyValuePair<string, string> entry in InclusiveTerms)
                {
                    if (symbol.Name.IndexOf(entry.Key, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        // For all such symbols, produce a diagnostic.
                        var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name, entry.Value);

                        context.ReportDiagnostic(diagnostic);
                        break;
                    }
                }
            }
            catch { }
        }

        private void CheckComments(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var node = context.Node;

                var xmlTrivia = node.GetLeadingTrivia()
                    .Select(i => i.GetStructure())
                    .OfType<DocumentationCommentTriviaSyntax>()
                    .FirstOrDefault();

                if (xmlTrivia == null) return;

                var content = xmlTrivia.ToFullString();
                foreach (KeyValuePair<string, string> entry in InclusiveTerms)
                {
                    if (content.IndexOf(entry.Key, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        // For all such symbols, produce a diagnostic.
                        var diagnostic = Diagnostic.Create(Rule, xmlTrivia.GetLocation(), entry.Key, entry.Value);

                        context.ReportDiagnostic(diagnostic);
                        break;
                    }
                }
            }
            catch { }
        }
    }
}