using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gandalf.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArgumentAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GANDALF001";
        private static readonly LocalizableString Title = "Argument count mismatch";
        private static readonly LocalizableString MessageFormat = "The number of arguments in [Argument] ({0}) does not match the number of method parameters ({1})";
        private static readonly LocalizableString Description = "The number of arguments in the Argument attribute must match the method's parameter count.";
        private const string Category = "Usage";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
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

            foreach (var attrList in methodDecl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attrSymbol = context.SemanticModel.GetSymbolInfo(attr).Symbol as IMethodSymbol;
                    if (attrSymbol == null)
                        continue;

                    var attrClass = attrSymbol.ContainingType;
                    if (attrClass.ToDisplayString() != "Gandalf.Core.Attributes.ArgumentAttribute")
                        continue;

                    // Count arguments in the attribute
                    int argCount = attr.ArgumentList?.Arguments.Count ?? 0;
                    int paramCount = methodSymbol.Parameters.Length;

                    if (argCount != paramCount)
                    {
                        var diagnostic = Diagnostic.Create(
                            Rule,
                            attr.GetLocation(),
                            argCount, paramCount);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
