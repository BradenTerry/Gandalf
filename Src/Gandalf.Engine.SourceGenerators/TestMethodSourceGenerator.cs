using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

                // Group methods by class
                var methodsByClass = methods
                    .Select(m => (Method: m, Symbol: compilation.GetSemanticModel(m.SyntaxTree).GetDeclaredSymbol(m)))
                    .Where(t => t.Symbol != null && HasTestAttribute(t.Symbol))
                    .GroupBy(t => t.Symbol.ContainingType, SymbolEqualityComparer.Default);

                foreach (var classGroup in methodsByClass)
                {
                    var classSymbol = classGroup.Key;
                    var ns = classSymbol.ContainingNamespace.ToDisplayString();
                    var safeNamespace = string.IsNullOrEmpty(ns) ? "Gandalf.Generated" : ns;
                    var cls = classSymbol.Name;
                    var safeClassName = $"{cls}_Discovered_Tests";
                    var fqType = classSymbol.ToDisplayString();
                    var assembly = classSymbol.ContainingAssembly.Name;
                    var allRegistrations = new List<string>();
                    
                    var classMethods = classGroup.ToList();
                    var classMethodSymbols = classMethods.Select(t => t.Symbol).ToList();
                    
                    // Find properties with [Inject] attribute
                    var injectProperties = (classSymbol as INamedTypeSymbol)?.GetMembers()
                        .Where(m => m is IPropertySymbol)
                        .Cast<IPropertySymbol>()
                        .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute"))
                        .ToList() ?? new List<IPropertySymbol>();

                    // Generate test registrations
                    foreach (var (method, symbol) in classGroup)
                    {
                        var (startLine, startCharacter, endLine, endCharacter) = GetLineInfo(method);
                        var methodName = method.Identifier.Text;
                        var filePath = method.SyntaxTree.FilePath;

                        // If this method has argument attributes, create parameterized tests
                        var argumentAttributes = symbol.GetAttributes()
                            .Where(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.ArgumentAttribute")
                            .ToList();

                        var registrations = argumentAttributes.Count > 0
                            ? GenerateParameterizedTestRegistrations(symbol, argumentAttributes, fqType, ns, cls, methodName, assembly, filePath, startLine, startCharacter, endLine, endCharacter)
                            : GenerateStandardTestRegistration(symbol, fqType, ns, cls, methodName, assembly, filePath, startLine, startCharacter, endLine, endCharacter);
                        allRegistrations.AddRange(registrations);
                    }

                    var registrationBlock = string.Join("\n            ", allRegistrations);
                    
                    // Generate declarations for injected properties - but only for Singleton and Scoped
                    var injectDeclarations = new List<string>();
                    foreach (var prop in injectProperties)
                    {
                        string varName = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                        string typeName = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        
                        // Check for InjectAttribute to determine lifetime
                        var injectAttr = prop.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute");
                        var instanceTypeValue = injectAttr.ConstructorArguments.Length > 0 
                            ? injectAttr.ConstructorArguments[0].Value
                            : 0; // Transient is 0
                        
                        // Convert to int as the enum value might be stored as a number
                        int instanceTypeInt = 0;
                        if (instanceTypeValue != null)
                        {
                            if (int.TryParse(instanceTypeValue.ToString(), out int parsed))
                            {
                                instanceTypeInt = parsed;
                            }
                            else if (instanceTypeValue is int intValue)
                            {
                                instanceTypeInt = intValue;
                            }
                        }

                        if (instanceTypeInt == 2) // Singleton = 2
                        {
                            // Register and retrieve singleton
                            injectDeclarations.Add($"Gandalf.Core.Helpers.TestDependencyInjection.RegisterDependency<{typeName}>(new {typeName}());");
                            injectDeclarations.Add($"var {varName} = Gandalf.Core.Helpers.TestDependencyInjection.GetDependency<{typeName}>();");
                        }
                        else if (instanceTypeInt == 1) // Scoped = 1
                        {
                            // Only create class-level instances for Scoped dependencies, not Transient
                            injectDeclarations.Add($"var {varName} = new {typeName}();");
                        }
                        // Transient dependencies will be created inline in each test's object initializer
                    }
                    var injectBlock = string.Join("\n            ", injectDeclarations);
                    
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
            {injectBlock}
            {registrationBlock}
        }}
    }}
}}";
                    var fileName = $"{assembly}.{cls}.DiscoveredTests.g.cs";
                    spc.AddSource(fileName, code);
                }
            });
        }

        private static bool HasTestAttribute(IMethodSymbol symbol) =>
            symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.TestAttribute");

        private static (int startLine, int startCharacter, int endLine, int endCharacter) GetLineInfo(MethodDeclarationSyntax method)
        {
            var location = method.Identifier.GetLocation();
            var lineSpan = location.GetLineSpan();
            return (
                lineSpan.StartLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1,
                lineSpan.EndLinePosition.Line + 1,
                lineSpan.EndLinePosition.Character + 1
            );
        }

        private static List<string> GenerateStandardTestRegistration(
            IMethodSymbol symbol, string fqType, string ns, string cls, string methodName,
            string assembly, string filePath, int startLine, int startCharacter, int endLine, int endCharacter)
        {
            var testUid = $"{ns}.{cls}.{methodName}";
            var callArguments = BuildCallArguments(symbol.Parameters);
            var callArgsString = string.Join(", ", callArguments);
            
            // Create object initializer for injected properties if any exist
            var injectedProps = symbol.ContainingType.GetMembers()
                .Where(m => m is IPropertySymbol)
                .Cast<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute"))
                .ToList();
            
            var objInitializer = "()";
            if (injectedProps.Any())
            {
                var props = new List<string>();
                foreach (var prop in injectedProps)
                {
                    var injectAttr = prop.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute");
                    var instanceTypeValue = injectAttr.ConstructorArguments.Length > 0 
                        ? injectAttr.ConstructorArguments[0].Value
                        : 0; // Transient is 0
                    
                    // Convert to int as the enum value might be stored as a number
                    int instanceTypeInt = 0;
                    if (instanceTypeValue != null)
                    {
                        if (int.TryParse(instanceTypeValue.ToString(), out int parsed))
                        {
                            instanceTypeInt = parsed;
                        }
                        else if (instanceTypeValue is int intValue)
                        {
                            instanceTypeInt = intValue;
                        }
                    }
                    
                    // Based on the lifetime, determine how to initialize the property
                    string propRef;
                    if (instanceTypeInt == 0) // Transient = 0
                    {
                        // Create a new instance directly in the initializer for Transient dependencies
                        propRef = $"new {prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()";
                    }
                    else if (instanceTypeInt == 1 || instanceTypeInt == 2) // Scoped = 1, Singleton = 2
                    {
                        // Use the pre-created instance for both Singleton and Scoped
                        propRef = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                    }
                    else
                    {
                        // Fallback - should not happen
                        propRef = $"new {prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()";
                    }
                    
                    props.Add($"{prop.Name} = {propRef}");
                }
                objInitializer = $" {{ {string.Join(", ", props)} }}";
            }
            
            var invokeAsync = $"() => new {fqType}{objInitializer}.{methodName}({callArgsString})";
            return new List<string>
            {
                $@"DiscoveredTests.Register(
                    new DiscoveredTest(
                        ""{testUid}"",
                        ""{assembly}"",
                        ""{ns}"",
                        ""{cls}"",
                        ""{methodName}"",
                        {invokeAsync},
                        ""{filePath}"",
                        {startLine},
                        {startCharacter},
                        {endLine},
                        {endCharacter}
                    )
                );"
            };
        }

        private static List<string> GenerateParameterizedTestRegistrations(
            IMethodSymbol symbol, List<AttributeData> argumentAttributes, string fqType, string ns, string cls, string methodName,
            string assembly, string filePath, int startLine, int startCharacter, int endLine, int endCharacter)
        {
            var registrations = new List<string>();
            var parameters = symbol.Parameters.ToList();
            var parentUid = $"{ns}.{cls}.{methodName}";
            int count = 0;

            // Create object initializer for injected properties if any exist
            var injectedProps = symbol.ContainingType.GetMembers()
                .Where(m => m is IPropertySymbol)
                .Cast<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute"))
                .ToList();
            
            var objInitializer = "()";
            if (injectedProps.Any())
            {
                var props = new List<string>();
                foreach (var prop in injectedProps)
                {
                    var injectAttr = prop.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute");
                    var instanceTypeValue = injectAttr.ConstructorArguments.Length > 0 
                        ? injectAttr.ConstructorArguments[0].Value
                        : 0; // Transient is 0
                    
                    // Convert to int as the enum value might be stored as a number
                    int instanceTypeInt = 0;
                    if (instanceTypeValue != null)
                    {
                        if (int.TryParse(instanceTypeValue.ToString(), out int parsed))
                        {
                            instanceTypeInt = parsed;
                        }
                        else if (instanceTypeValue is int intValue)
                        {
                            instanceTypeInt = intValue;
                        }
                    }
                    
                    // Based on the lifetime, determine how to initialize the property
                    string propRef;
                    if (instanceTypeInt == 0) // Transient = 0
                    {
                        // Create a new instance directly in the initializer for Transient dependencies
                        propRef = $"new {prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()";
                    }
                    else if (instanceTypeInt == 1 || instanceTypeInt == 2) // Scoped = 1, Singleton = 2
                    {
                        // Use the pre-created instance for both Singleton and Scoped
                        propRef = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                    }
                    else
                    {
                        // Fallback - should not happen
                        propRef = $"new {prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}()";
                    }
                    
                    props.Add($"{prop.Name} = {propRef}");
                }
                objInitializer = $" {{ {string.Join(", ", props)} }}";
            }

            foreach (var argAttr in argumentAttributes)
            {
                var args = argAttr.ConstructorArguments.FirstOrDefault();
                var argList = args.Values.Select(v =>
                    v.Kind == TypedConstantKind.Primitive
                        ? v.Value is string s ? $"\"{s}\"" : v.Value?.ToString() ?? "null"
                        : v.ToCSharpString()
                ).ToArray();

                var callArguments = BuildCallArguments(parameters, argList);
                var callArgsString = string.Join(", ", callArguments);
                var invokeAsync = $"() => new {fqType}{objInitializer}.{methodName}({callArgsString})";
                var childUid = $"{ns}.{cls}.{methodName}-{count}";
                var argString = string.Join(", ", argList);

                registrations.Add(
                    $@"DiscoveredTests.Register(
                        new DiscoveredTest(
                            ""{childUid}"",
                            ""{assembly}"",
                            ""{ns}"",
                            ""{cls}"",
                            ""{methodName}"",
                            {invokeAsync},
                            ""{filePath}"",
                            {startLine},
                            {startCharacter},
                            {endLine},
                            {endCharacter},
                            new object[] {{ {argString} }},
                            ""{parentUid}""
                        )
                    );"
                );

                count++;
            }

            return registrations;
        }

        private static List<string> BuildCallArguments(IEnumerable<IParameterSymbol> parameters, string[] argList = null)
        {
            var callArguments = new List<string>();
            var parametersList = parameters.ToList();

            for (int i = 0; i < parametersList.Count; i++)
            {
                var parameter = parametersList[i];
                if (argList != null && i < argList.Length)
                {
                    callArguments.Add(argList[i]);
                }
                else if (parameter.Type.SpecialType == SpecialType.System_String)
                {
                    callArguments.Add("\"\"");
                }
                else if (parameter.Type.TypeKind == TypeKind.Enum)
                {
                    callArguments.Add("default");
                }
                else if (parameter.Type.IsReferenceType || parameter.Type.TypeKind == TypeKind.TypeParameter)
                {
                    callArguments.Add("null");
                }
                else
                {
                    callArguments.Add("default");
                }
            }

            return callArguments;
        }
    }
}
