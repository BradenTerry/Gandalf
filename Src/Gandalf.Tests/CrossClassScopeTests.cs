using Gandalf.Core.Attributes;
using Gandalf.Core.Models;

namespace Gandalf.Tests;

public class CrossClassScopeTests
{
    // This should share the same instance as DependencyInjectionTests.Scoped and Scoped2
    [ScopedGroup("Test2")]
    [Inject(InstanceType.Scoped)]
    public required TestClass SharedScoped { get; init; }

    // This should be a different instance since it has a different group
    [ScopedGroup("CrossClassGroup")]
    [Inject(InstanceType.Scoped)]
    public required TestClass DifferentGroupScoped { get; init; }

    // This should be unique to this instance since it has no group
    [Inject(InstanceType.Scoped)]
    public required TestClass UniqueScoped { get; init; }

    [Test]
    public Task CrossClassScopeTest()
    {
        if (SharedScoped == null)
            throw new Exception("SharedScoped is not initialized");

        if (DifferentGroupScoped == null)
            throw new Exception("DifferentGroupScoped is not initialized");

        if (UniqueScoped == null)
            throw new Exception("UniqueScoped is not initialized");

        // Store reference for comparison in other tests
        DependencyInjectionTests.CrossClassSharedInstance = SharedScoped;

        return Task.CompletedTask;
    }
}
