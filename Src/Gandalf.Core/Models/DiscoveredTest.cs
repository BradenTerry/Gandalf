using System;
using System.Threading.Tasks;

namespace Gandalf.Core.Models
{
    public class DiscoveredTest
    {
        public string Assembly { get; }
        public string Namespace { get; }
        public string ClassName { get; }
        public string MethodName { get; }
        public Func<Task> InvokeAsync { get; }

        public DiscoveredTest(string assembly, string ns, string className, string methodName, Func<Task> invokeAsync)
        {
            Assembly = assembly;
            Namespace = ns;
            ClassName = className;
            MethodName = methodName;
            InvokeAsync = invokeAsync;
        }

        public string FullName => $"{Namespace}.{ClassName}.{MethodName}";
    }
}