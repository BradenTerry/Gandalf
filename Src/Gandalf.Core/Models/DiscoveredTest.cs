using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gandalf.Core.Models
{
    public record DiscoveredTest(string Assembly, string Namespace, string ClassName, string MethodName, Func<Task> InvokeAsync)
    {
        public string FullName => $"{Namespace}.{ClassName}.{MethodName}";
    }
}