#if NET7_0_OR_GREATER
#nullable enable
#endif
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gandalf.Core.Models
{
    public class DiscoveredTest
    {
        public string Uid { get; }
        #if NET7_0_OR_GREATER
        public string? ParentUid { get; }
        public string? FilePath { get; }
        #else
        public string ParentUid { get; }
        public string FilePath { get; }
        #endif
        public string Assembly { get; }
        public string Namespace { get; }
        public string ClassName { get; }
        public string MethodName { get; }
        public int LineNumber { get; }
        public int LinePosition { get; }
        public int EndLineNumber { get; }
        public int EndLinePosition { get; }
        public object[] Parameters { get; }
        public Func<Task> InvokeAsync { get; }

        // New properties for category and ignore support
        public string[] Categories { get; }
        public string IgnoreReason { get; }

        public DiscoveredTest(
            string uid,
            string assembly,
            string ns,
            string className,
            string methodName,
            Func<Task> invokeAsync,
            string filePath,
            int lineNumber,
            int linePosition,
            int endLineNumber,
            int endLinePosition,
#if NET7_0_OR_GREATER
            object[]? parameters = null,
            string? parentUid = null,
#else
            object[] parameters = null,
            string parentUid = null,
#endif
            string[] categories = null,
            string ignoreReason = null
        )
        {
            Uid = uid;
            Assembly = assembly;
            Namespace = ns;
            ClassName = className;
            MethodName = methodName;
            FilePath = filePath;
            LineNumber = lineNumber;
            LinePosition = linePosition;
            EndLineNumber = endLineNumber;
            EndLinePosition = endLinePosition;
            Parameters = parameters;
            InvokeAsync = invokeAsync;
            ParentUid = parentUid;
            Categories = categories ?? Array.Empty<string>();
            IgnoreReason = ignoreReason;
        }

        public string FullName => Parameters == null
            ? $"{Namespace}.{ClassName}.{MethodName}()"
            : $"{Namespace}.{ClassName}.{MethodName}({string.Join(".", Parameters.Select(p => p.GetHashCode()))})";
    }
}