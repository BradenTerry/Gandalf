using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gandalf.Analyzers.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArgumentAttributeTypeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GANDALF002";
        private static readonly LocalizableString Title = "Argument type mismatch";
        private static readonly LocalizableString MessageFormat = "The type of argument {0} ('{1}') does not match the parameter type '{2}'";
        private static readonly LocalizableString Description = "Each argument in the Argument attribute must match the corresponding method parameter type.";
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

                    var paramCount = methodSymbol.Parameters.Length;
                    var argList = attr.ArgumentList?.Arguments;
                    if (argList == null)
                        continue;

                    for (int i = 0; i < argList.Value.Count && i < paramCount; i++)
                    {
                        var argExpr = argList.Value[i].Expression;
                        var argType = context.SemanticModel.GetTypeInfo(argExpr).Type;
                        var paramType = methodSymbol.Parameters[i].Type;

                        // REVERTED: Report an error if the types DO NOT match (are not implicitly convertible)
                        if (argType == null || !context.Compilation.ClassifyConversion(argType, paramType).IsImplicit)
                        {
                            var argTypeName = argType?.ToDisplayString() ?? "unknown";
                            var paramTypeName = paramType.ToDisplayString();
                            var diagnostic = Diagnostic.Create(
                                Rule,
                                argExpr.GetLocation(),
                                i + 1, argTypeName, paramTypeName);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}