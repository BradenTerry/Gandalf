using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gandalf.Analyzers.Rules
{
        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        public class InjectPropertyAnalyzer : DiagnosticAnalyzer
        {
            public const string DiagnosticId = "GANDALF004";
            private static readonly LocalizableString Title = "[Inject] property must be required and have get; init;";
            private static readonly LocalizableString MessageFormat = "Property '{0}' with [Inject] must be 'required' and have 'get; init;'";
            private static readonly LocalizableString Description = "Properties marked with [Inject] must be required and have get; init; accessors.";
            private const string Category = "Usage";

            private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
                DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

            public override void Initialize(AnalysisContext context)
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                context.EnableConcurrentExecution();
                context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
            }

            private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
            {
                var propertyDecl = (PropertyDeclarationSyntax)context.Node;
                var symbol = context.SemanticModel.GetDeclaredSymbol(propertyDecl);
                if (symbol == null)
                    return;

                // Check for [Inject] attribute
                var hasInject = symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "InjectAttribute");
                if (!hasInject)
                    return;

                // Check for 'required' modifier
                bool isRequired = propertyDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword));

                // Check for get; init;
                var accessorList = propertyDecl.AccessorList;
                bool hasGet = false, hasInit = false;
                if (accessorList != null)
                {
                    foreach (var accessor in accessorList.Accessors)
                    {
                        if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                            hasGet = true;
                        if (accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
                            hasInit = true;
                    }
                }

                if (!isRequired || !hasGet || !hasInit)
                {
                    var diagnostic = Diagnostic.Create(Rule, propertyDecl.GetLocation(), symbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
}
