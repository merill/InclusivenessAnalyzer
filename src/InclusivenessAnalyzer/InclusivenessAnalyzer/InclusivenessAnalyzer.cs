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
            {"whitelist", "allow list, access list, permit" },
            {"white list", "allow list, access list, permit" },
            {"blacklist", "deny list, blocklist, exclude list" },
            {"black list", "deny list, blocklist, exclude list" },
            {"culture fit", "values fit, cultural combtribution" },
            {"master", "primary, main, default, leader" },
            {"slave", "replica, standby, secondary, follower" },
            {"minority", "marginalized groups, underrepresented groups" },
            {"minorities", "marginalized groups, underrepresented groups" },
            {"brownbag", "learning session, lunch-and-learn, sack lunch" },
            {"brown bag", "learning session, lunch-and-learn, sack lunch" },
            {"whitebox", "openbox" },
            {"white-box", "open-box" },
            {"blackbox", "closedbox" },
            {"black-box", "closed-box" },
            {"guys", "folks, you all, y'all, people, teammates, team" },
            {"he", "they" },
            {"his", "their, theirs" },
            {"him", "them" },
            {"she", "they" },
            {"her", "their" },
            {"hers", "theirs" },
            {"manpower", "person hours, engineer hours, work, workforce, personnel, team, workers" },
            {"man hours", "person hours, engineer hours, work, workforce, personnel, team, workers" },
            {"chairman", "chairperson, spokesperson, moderator, discussion leader, chair" },
            {"foreman", "chairperson, spokesperson, moderator, discussion leader, chair" },
            {"middleman", "middle person, intermediary, agent, dealer" },
            {"mother", "parent" },
            {"mothering", "parenting" },
            {"father", "parent" },
            {"fathering", "parenting" },
            {"wife", "spouse, partner, significant other" },
            {"husband", "spouse, partner, significant other" },
            {"boyfriend", "partner, significant other" },
            {"girlfriend", "partner, significant other" },
            {"girl", "woman" },
            {"girls", "women" },
            {"female", "woman" },
            {"females", "women" },
            {"boy", "man" },
            {"boys", "men" },
            {"male", "man" },
            {"males", "men" },
            {"mom test", "user test" },
            {"girlfriend test", "user test" },
            {"ninja", "professional" },
            {"rockstar", "professional" },
            {"housekeeping", "maintenance, cleanup, preparation" },
            {"opposite sex", "different sex" },
            {"grandfathered in", "exempt" },
            {"grandfathered", "exempt" },
            {"normal", "typical, healthy" },
            {"crazy", "unexpected, unpredictable, surprising" },
            {"insane", "unexpected, unpredictable, surprising" },
            {"freaky", "unexpected, unpredictable, surprising" },
            {"OCD", "organized, detail-oriented" },
            {"handicapped", "person with disabilities" },
            {"disabled", "person with disabilities" },
            {"sanity check", "quick check, confidence check, coherence check" },
            {"sane", "correct, adequate, sufficient, valid, sensible, coherent" },
            {"retard", "person with disabilities, mentally limited" },
            {"dummy value", "placeholder value, sample value" },
            {"citizen", "resident" }
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