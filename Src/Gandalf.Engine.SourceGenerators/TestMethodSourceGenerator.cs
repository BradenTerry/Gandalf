using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gandalf.Engine.SourceGenerators
{
    [Generator]
    public class TestMethodSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var methodDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is MethodDeclarationSyntax,
                    transform: (ctx, _) => (MethodDeclarationSyntax)ctx.Node)
                .Where(m => m != null);

            var compilationAndMethods = context.CompilationProvider.Combine(methodDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndMethods, (spc, source) =>
            {
                var (compilation, methods) = source;
                var testMethods = new List<(string assembly, string ns, string className, string methodName, bool isStatic)>();

                foreach (var method in methods)
                {
                    var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
                    var symbol = semanticModel.GetDeclaredSymbol(method);
                    if (symbol == null)
                        continue;

                    if (symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "TestAttribute"))
                    {
                        var classSymbol = symbol.ContainingType;
                        var ns = classSymbol.ContainingNamespace.ToDisplayString();
                        var assembly = classSymbol.ContainingAssembly.Name;
                        var isStatic = symbol.IsStatic;
                        testMethods.Add((assembly, ns, classSymbol.Name, symbol.Name, isStatic));
                    }
                }

                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using Gandalf.Core.Models;");
                sb.AppendLine("namespace Gandalf.Engine.SourceGenerators");
                sb.AppendLine("{");
                sb.AppendLine("    public static class DiscoveredTests");
                sb.AppendLine("    {");
                sb.AppendLine("        public static IReadOnlyList<DiscoveredTest> All { get; } = new List<DiscoveredTest>");
                sb.AppendLine("        {");
                foreach (var (assembly, ns, cls, method, isStatic) in testMethods)
                {
                    var fqType = string.IsNullOrEmpty(ns) ? cls : $"{ns}.{cls}";
                    var call = isStatic
                        ? $"() => {fqType}.{method}()"
                        : $"() => new {fqType}().{method}()";
                    sb.AppendLine($"            new DiscoveredTest(\"{assembly}\", \"{ns}\", \"{cls}\", \"{method}\", {call}),");
                }
                sb.AppendLine("        };");
                sb.AppendLine("    }");
                sb.AppendLine("}");

                spc.AddSource("DiscoveredTests.g.cs", sb.ToString());
            });
        }
    }
}
