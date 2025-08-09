using Microsoft.Testing.Platform.Capabilities.TestFramework;

public class TestFrameworkCapabilities : ITestFrameworkCapabilities
{
    public IReadOnlyCollection<ITestFrameworkCapability> Capabilities => Array.Empty<ITestFrameworkCapability>();
}
