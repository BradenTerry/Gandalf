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

                var assemblyName = compilation.AssemblyName ?? "UnknownAssembly";
                var safeClassName = assemblyName.Replace('.', '_') + "DiscoveredTests";
                var safeNamespace = assemblyName; // or use a transformation if you want

                var sb = new StringBuilder();
                sb.AppendLine("// Auto-generated file");
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Runtime.CompilerServices;");
                sb.AppendLine("using Gandalf.Core.Helpers;");
                sb.AppendLine("using Gandalf.Core.Models;");
                sb.AppendLine();
                sb.AppendLine($"namespace {safeNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public static class {safeClassName}");
                sb.AppendLine("    {");
                sb.AppendLine($"       [ModuleInitializer]");
                sb.AppendLine($"       public static void Initialize()");
                sb.AppendLine("        {");
                foreach (var (assembly, ns, cls, method, isStatic) in testMethods)
                {
                    var fqType = string.IsNullOrEmpty(ns) ? cls : $"{ns}.{cls}";
                    var call = isStatic
                        ? $"() => {fqType}.{method}()"
                        : $"() => new {fqType}().{method}()";
                    sb.AppendLine($"            DiscoveredTests.Register(new DiscoveredTest(\"{assembly}\", \"{ns}\", \"{cls}\", \"{method}\", {call}));");
                }
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine("}");

                var fileName = $"{assemblyName}.DiscoveredTests.g.cs";
                spc.AddSource(fileName, sb.ToString());
            });
        }
    }
}
