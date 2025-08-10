using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

                foreach (var method in methods)
                {
                    var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
                    var symbol = semanticModel.GetDeclaredSymbol(method);
                    if (symbol == null)
                        continue;

                    if (!symbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "TestAttribute"))
                        continue;

                    var classSymbol = symbol.ContainingType;
                    var ns = classSymbol.ContainingNamespace.ToDisplayString();
                    var assembly = classSymbol.ContainingAssembly.Name;
                    var cls = classSymbol.Name;
                    var methodName = symbol.Name;

                    // Get file location information
                    var filePath = method.SyntaxTree.FilePath;
                    var location = method.Identifier.GetLocation();
                    var lineSpan = location.GetLineSpan();
                    var startLine = lineSpan.StartLinePosition.Line + 1;
                    var startCharacter = lineSpan.StartLinePosition.Character + 1;
                    var endLine = lineSpan.EndLinePosition.Line + 1;
                    var endCharacter = lineSpan.EndLinePosition.Character + 1;

                    var fqType = string.IsNullOrEmpty(ns) ? cls : $"{ns}.{cls}";

                    // Find all [Argument] attributes
                    var argumentAttributes = symbol.GetAttributes()
                        .Where(attr => attr.AttributeClass?.Name == "ArgumentAttribute")
                        .ToList();

                    var registrations = new List<string>();
                    if (argumentAttributes.Count == 0)
                    {
                        // For standard tests (no parameters):
                        var testUid = $"{ns}.{cls}.{methodName}";
                        var call = $"() => new {fqType}().{methodName}()";
                        registrations.Add(
                            $"DiscoveredTests.Register(new DiscoveredTest(\"{testUid}\", \"{assembly}\", \"{ns}\", \"{cls}\", \"{methodName}\", {call}, \"{filePath}\", {startLine}, {startCharacter}, {endLine}, {endCharacter}));"
                        );
                    }
                    else
                    {
                        // For parameterized tests:
                        var parentUid = $"{ns}.{cls}.{methodName}";
                        var count = 0;
                        foreach (var argAttr in argumentAttributes)
                        {
                            var args = argAttr.ConstructorArguments.FirstOrDefault();
                            var argList = args.Values.Select(v =>
                                v.Kind == TypedConstantKind.Primitive
                                    ? v.Value is string s ? $"\"{s}\"" : v.Value?.ToString() ?? "null"
                                    : v.ToCSharpString()
                            ).ToArray();
                            var argString = string.Join(", ", argList);
                            var call = $"() => new {fqType}().{methodName}({argString})";
                            var childUid = $"{ns}.{cls}.{methodName}-{count}";

                            registrations.Add(
                                $"DiscoveredTests.Register(new DiscoveredTest(\"{childUid}\", \"{assembly}\", \"{ns}\", \"{cls}\", \"{methodName}\", {call}, \"{filePath}\", {startLine}, {startCharacter}, {endLine}, {endCharacter}, new object[] {{ {argString} }}, \"{parentUid}\"));"
                            );

                            count++;
                        }
                    }

                    var safeNamespace = assembly;
                    var safeClassName = $"{cls}_{methodName}_DiscoveredTest";

                    var registrationBlock = string.Join("\n            ", registrations);

                    var code =
$@"// Auto-generated file
using System;
using System.Runtime.CompilerServices;
using Gandalf.Core.Helpers;
using Gandalf.Core.Models;

namespace {safeNamespace}
{{
    public static class {safeClassName}
    {{
        [ModuleInitializer]
        public static void Initialize()
        {{
            {registrationBlock}
        }}
    }}
}}";
                    var fileName = $"{assembly}.{cls}.{methodName}.DiscoveredTest.g.cs";
                    spc.AddSource(fileName, code);
                }
            });
        }
    }
}
