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

    [Inject(InstanceType.Scoped)]
    public required TestClass Scoped { get; init; }

    [Inject(InstanceType.Scoped)]
    public required TestClass Scoped2 { get; init; }

    private static TestClass? _scopedTestClass;
    private static TestClass? _scopedTestClass2;
    [Test]
    [Argument(1)]
    [Argument(2)]
    public Task ScopedTests_DifferentInstancePerParameter(int num)
    {
        if (Scoped == null)
        {
            throw new Exception("Scoped is not initialized");
        }

        if (Scoped2 == null)
        {
            throw new Exception("Scoped2 is not initialized");
        }

        if (Scoped == Scoped2)
            throw new Exception("Scoped and Scoped2 are the same instance");

        if (num == 1)
        {
            _scopedTestClass = Scoped;
            _scopedTestClass2 = Scoped2;
            return Task.CompletedTask;
        }

        // Check that the same instance is used
        if (_scopedTestClass != Scoped || _scopedTestClass2 != Scoped2)
            throw new Exception("Scoped is not the same instance as the one captured");

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
