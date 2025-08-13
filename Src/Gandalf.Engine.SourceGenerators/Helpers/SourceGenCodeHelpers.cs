using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Gandalf.Engine.SourceGenerators.Helpers
{
    internal static class SourceGenCodeHelpers
    {
        public static string BuildInjectedObjectInitializer(INamedTypeSymbol classSymbol)
        {
            var injectedProps = classSymbol.GetMembers()
                .Where(m => m is IPropertySymbol)
                .Cast<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute"))
                .ToList();

            if (!injectedProps.Any())
                return "()";

            var props = new List<string>();
            foreach (var prop in injectedProps)
            {
                var injectAttr = prop.GetAttributes().First(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.InjectAttribute");
                var instanceTypeValue = injectAttr.ConstructorArguments.Length > 0
                    ? injectAttr.ConstructorArguments[0].Value
                    : 0; // Transient is 0

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

                string propRef;
                if (instanceTypeInt == 0) // Transient = 0
                {
                    propRef = $"new {prop.Type.ToDisplayString(Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat)}()";
                }
                else if (instanceTypeInt == 1 || instanceTypeInt == 2) // Scoped = 1, Singleton = 2
                {
                    propRef = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                }
                else
                {
                    propRef = $"new {prop.Type.ToDisplayString(Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat)}()";
                }

                props.Add($"{prop.Name} = {propRef}");
            }
            return $" {{ {string.Join(", ", props)} }}";
        }

        public static List<string> BuildCallArguments(IEnumerable<IParameterSymbol> parameters, string[] argList = null)
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
