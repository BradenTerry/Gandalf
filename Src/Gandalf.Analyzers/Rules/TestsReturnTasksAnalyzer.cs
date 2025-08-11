using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gandalf.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestsReturnTasksAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GANDALF003";
        private static readonly LocalizableString Title = "Test method must return Task";
        private static readonly LocalizableString MessageFormat = "Method '{0}' is marked with [Test] but does not return Task";
        private static readonly LocalizableString Description = "All methods marked with [Test] must return Task for async compatibility.";
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDecl = (MethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDecl);
            if (methodSymbol == null)
                return;

            // Check for [Test] attribute with the correct namespace
            bool hasTestAttribute = methodSymbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.TestAttribute");

            if (!hasTestAttribute)
                return;

            // Check if return type is Task
            var returnType = methodSymbol.ReturnType;
            if (returnType == null || returnType.ToDisplayString() != "System.Threading.Tasks.Task")
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodDecl.Identifier.GetLocation(),
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}