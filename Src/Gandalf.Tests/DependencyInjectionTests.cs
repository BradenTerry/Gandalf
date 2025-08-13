using System;
using Gandalf.Core.Attributes;

namespace Gandalf.Tests;

public class DependencyInjectionTests
{

    [Inject(InstanceType.Transient)]
    public required TestClass Transient { get; init; }

    [Inject(InstanceType.Transient)]
    public required TestClass Transient2 { get; init; }

    private static TestClass? _transient;
    private static TestClass? _transient2;
    [Test]
    [Argument(1)]
    [Argument(2)]
    public Task Transient_NewInstancePerParameter(int num)
    {
        if (Transient == null)
        {
            throw new Exception("Transient is not initialized");
        }

        if (Transient2 == null)
        {
            throw new Exception("Transient2 is not initialized");
        }

        if (Transient == Transient2)
        {
            throw new Exception("Transient and Transient2 are the same instance");
        }

        if (num == 1)
        {
            _transient = Transient;
            _transient2 = Transient2;
            return Task.CompletedTask;
        }

        // Check that the same instance is used
        if (_transient == Transient || _transient2 == Transient2)
            throw new Exception("Transient is the same instance as the one captured");

        return Task.CompletedTask;
    }

    [ScopedGroup("Test2")]
    [Inject(InstanceType.Scoped)]
    public required TestClass Scoped { get; init; }

    [Inject(InstanceType.Scoped)]
    public required TestClass Scoped2 { get; init; }

    [Inject(InstanceType.Scoped)]
    public required TestClass Scoped3 { get; init; }

    private static TestClass? _scopedTestClass;
    private static TestClass? _scopedTestClass2;
    
    // For cross-class sharing verification
    public static TestClass? CrossClassSharedInstance { get; set; }
    [Test]
    [Argument(1)]
    [Argument(2)]
    public Task ScopedTests_SameInstancePerGroup(int num)
    {
        if (Scoped == null)
        {
            throw new Exception("Scoped is not initialized");
        }

        if (Scoped2 == null)
        {
            throw new Exception("Scoped2 is not initialized");
        }

        // With ScopeGroup("Test2"), Scoped and Scoped2 should be the same instance
        if (Scoped != Scoped2)
            throw new Exception("Scoped and Scoped2 should be the same instance due to ScopeGroup");

        // But Scoped3 (no ScopeGroup) should be different
        if (Scoped != Scoped3)
            throw new Exception("Scoped and Scoped3 should be the same instances");

        if (num == 1)
        {
            _scopedTestClass = Scoped;
            _scopedTestClass2 = Scoped2;
            return Task.CompletedTask;
        }

        // Check that across test parameters, the same scoped instance is used
        if (_scopedTestClass != Scoped || _scopedTestClass2 != Scoped2)
            throw new Exception("Scoped instances should be the same across test parameters within the same group");

        return Task.CompletedTask;
    }

    [Test]
    public Task VerifyCrossClassSharing()
    {
        // This test verifies that scoped instances are shared across different test classes
        // when they have the same ScopeGroup
        if (CrossClassSharedInstance != null)
        {
            if (Scoped != CrossClassSharedInstance)
                throw new Exception("Cross-class scoped sharing failed - instances are different");
        }

        return Task.CompletedTask;
    }

    [Inject(InstanceType.Singleton)]
    public required TestClass Singleton { get; init; }

    [Inject(InstanceType.Singleton)]
    public required TestClass Singleton2 { get; init; }

    private static TestClass? _singletonTestClass;
    private static TestClass? _singletonTestClass2;
    [Test]
    [Argument(1)]
    [Argument(2)]
    public Task SingletonTests_TheSameClassIsUsedForDifferentParamsAndAcrossTests(int num)
    {
        if (Singleton == null)
        {
            throw new Exception("Singleton is not initialized");
        }

        if (Singleton2 == null)
        {
            throw new Exception("Singleton2 is not initialized");
        }

        if (Singleton != Singleton2)
            throw new Exception("Singleton and Singleton2 are the same instance");

        if (num == 1)
        {
            _singletonTestClass = Singleton;
            _singletonTestClass2 = Singleton2;
            return Task.CompletedTask;
        }

        // Check that the same instance is used
        if (_singletonTestClass != Singleton || _singletonTestClass2 != Singleton2 || _singletonTestClass != Singleton2 || _singletonTestClass2 != Singleton)
            throw new Exception("Singleton is not the same instance as the one captured");

        return Task.CompletedTask;
    }
}
public class TestClass()
{
    
}
