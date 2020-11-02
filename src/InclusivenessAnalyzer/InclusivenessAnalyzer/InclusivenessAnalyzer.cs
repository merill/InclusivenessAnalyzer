using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace InclusivenessAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InclusivenessAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "InclusivenessAnalyzer";
        private static readonly Dictionary<string, string> InclusiveTerms = new Dictionary<string, string>() { 
            {"WhiteList", "AllowList" },
            {"BlackList", "DenyList" }
        };

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Microsoft.Inclusive";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Property, SymbolKind.Field, 
                SymbolKind.Event, SymbolKind.Namespace, SymbolKind.Parameter);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
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
    }
}
