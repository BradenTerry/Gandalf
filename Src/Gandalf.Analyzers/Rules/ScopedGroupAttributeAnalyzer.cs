using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gandalf.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ScopedGroupAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GANDALF0012";
        private static readonly LocalizableString Title = "ScopedGroup must be used with Inject(InstanceType.Scoped)";
        private static readonly LocalizableString MessageFormat = "[ScopedGroup] can only be applied to properties or parameters with [Inject(InstanceType.Scoped)]";
        private static readonly LocalizableString Description = "Enforces that ScopedGroup is only used with Inject(InstanceType.Scoped).";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property, SymbolKind.Parameter);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = context.Symbol;
            var scopedGroupAttr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "ScopedGroupAttribute");
            if (scopedGroupAttr == null)
                return;

            var injectAttr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "InjectAttribute");
            if (injectAttr == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0]));
                return;
            }

            // Check InstanceType.Scoped (enum value is 1)
            var args = injectAttr.ConstructorArguments;
            if (args.Length == 1 && args[0].Value is int intValue && intValue == 1)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0]));
        }
    }
}
