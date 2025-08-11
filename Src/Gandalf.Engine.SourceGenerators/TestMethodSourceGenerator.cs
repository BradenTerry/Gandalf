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

                    if (!symbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.TestAttribute"))
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
                        // For standard tests (no [Argument] attributes)
                        var testUid = $"{ns}.{cls}.{methodName}";

                        // Build call arguments, handling [Inject] parameters
                        var callArguments = new List<string>();
                        foreach (var param in symbol.Parameters)
                        {
                            bool isInject = param.GetAttributes()
                                .Any(attr => attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute");

                            if (isInject)
                            {
                                var paramType = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                callArguments.Add($"({param.Type.ToDisplayString()})Gandalf.Engine.Helpers.TestServiceProvider.GetAssemblyServiceProvider().GetService(typeof({paramType}))");
                            }
                            else
                            {
                                // No [Argument] attribute and not [Inject]: use default value or null
                                callArguments.Add(param.HasExplicitDefaultValue
                                    ? (param.ExplicitDefaultValue is string s ? $"\"{s}\"" : param.ExplicitDefaultValue?.ToString() ?? "null")
                                    : "default");
                            }
                        }
                        var callArgsString = string.Join(", ", callArguments);
                        var c = $"() => new {fqType}().{methodName}({callArgsString})";
                        registrations.Add(
                            $"DiscoveredTests.Register(new DiscoveredTest(\"{testUid}\", \"{assembly}\", \"{ns}\", \"{cls}\", \"{methodName}\", {c}, \"{filePath}\", {startLine}, {startCharacter}, {endLine}, {endCharacter}));"
                        );
                    }
                    else
                    {
                        // For parameterized tests:
                        var parentUid = $"{ns}.{cls}.{methodName}";
                        var count = 0;
                        
                        // Get all parameters of the method
                        var paramterList = symbol.Parameters.ToList();
                        
                        foreach (var argAttr in argumentAttributes)
                        {
                            var args = argAttr.ConstructorArguments.FirstOrDefault();
                            var argList = args.Values.Select(v =>
                                v.Kind == TypedConstantKind.Primitive
                                    ? v.Value is string s ? $"\"{s}\"" : v.Value?.ToString() ?? "null"
                                    : v.ToCSharpString()
                            ).ToArray();
                            
                            // Build call arguments, handling [Inject] parameters differently
                            var callArguments = new List<string>();
                            int argIndex = 0;
                            
                            foreach (var param in paramterList)
                            {
                                bool isInject = param.GetAttributes()
                                    .Any(attr => attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute");

                                if (isInject)
                                {
                                    // Use TestServiceProvider for [Inject] parameters
                                    var paramType = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                    callArguments.Add($"({param.Type.ToDisplayString()})Gandalf.Engine.Helpers.TestServiceProvider.GetAssemblyServiceProvider().GetService(typeof({paramType}))");
                                }
                                else
                                {
                                    callArguments.Add(argList[argIndex]);
                                    // Use argument value for non-injected parameters
                                    if (argIndex < argList.Length)
                                    {

                                    }
                                        argIndex++;
                                }
                            }
                            
                            var callArgsString = string.Join(", ", callArguments);
                            var c = $"() => new {fqType}().{methodName}({callArgsString})";
                            var childUid = $"{ns}.{cls}.{methodName}-{count}";
                            
                            // Use the original argString for the objects array to preserve the test parameters
                            var argString = string.Join(", ", argList);

                            registrations.Add(
                                $"DiscoveredTests.Register(new DiscoveredTest(\"{childUid}\", \"{assembly}\", \"{ns}\", \"{cls}\", \"{methodName}\", {c}, \"{filePath}\", {startLine}, {startCharacter}, {endLine}, {endCharacter}, new object[] {{ {argString} }}, \"{parentUid}\"));"
                            );

                            count++;
                        }
                    }

                    var safeNamespace = assembly;
                    var safeClassName = $"{cls}_{methodName}_DiscoveredTest";

                    var registrationBlock = string.Join("\n            ", registrations);

                    var code =
$@"// <auto-generated />
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
