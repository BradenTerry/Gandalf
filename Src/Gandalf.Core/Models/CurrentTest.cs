using System;
using System.IO;
using System.Threading;

namespace Gandalf.Core.Models
{
    public static class CurrentTest
    {
        internal static AsyncLocal<ITestContext> TestContext { get; } = new AsyncLocal<ITestContext>();
        
        public static ITestContext Context => TestContext.Value ?? throw new InvalidOperationException("No test context available");
    }
}
