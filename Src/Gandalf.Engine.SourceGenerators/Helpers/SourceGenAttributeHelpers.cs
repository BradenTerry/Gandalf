using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gandalf.Engine.SourceGenerators.Helpers
{
    internal static class SourceGenAttributeHelpers
    {
        public static bool HasTestAttribute(IMethodSymbol symbol) =>
            symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.TestAttribute");

        public static (List<string> categories, string ignoreReason) ExtractCategoriesAndIgnore(IMethodSymbol symbol)
        {
            var categories = new List<string>();
            string ignoreReason = null;
            // Method-level
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.CategoryAttribute")
                {
                    if (attr.ConstructorArguments.Length > 0)
                    {
                        var arg = attr.ConstructorArguments[0];
                        var cat = arg.Value?.ToString() ?? string.Empty;
                        categories.Add(cat);
                    }
                }
                if (attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.IgnoreAttribute")
                {
                    if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string reason)
                        ignoreReason = reason;
                    else
                        ignoreReason = "Ignored";
                }
            }
            // Class-level
            var classSymbol = symbol.ContainingType;
            foreach (var attr in classSymbol.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.CategoryAttribute")
                {
                    if (attr.ConstructorArguments.Length > 0)
                    {
                        var arg = attr.ConstructorArguments[0];
                        var cat = arg.Value?.ToString() ?? string.Empty;
                        categories.Add(cat);
                    }
                }
                if (attr.AttributeClass?.ToDisplayString() == "Gandalf.Core.Attributes.IgnoreAttribute")
                {
                    if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string reason)
                        ignoreReason = reason;
                    else
                        ignoreReason = "Ignored";
                }
            }
            return (categories, ignoreReason);
        }
    }
}
