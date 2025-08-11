using System;
using System.Collections;
using Gandalf.Core.Attributes;
using Gandalf.Tests;
using Microsoft.Extensions.DependencyInjection;

#if CSHARP_11_0_OR_GREATER
[assembly: TestServiceProvider<MyServiceProvider>()]
#else
[assembly: TestServiceProvider(typeof(MyServiceProvider))]
#endif

namespace Gandalf.Tests;

public class DependencyTests
{

    [Test]
    public Task ServiceProviderAttribute_Should_Initialize_Correctly([Inject] TestClass testClass)
    {
        if (testClass == null)
            throw new Exception("TestClass instance is null");

        return Task.CompletedTask;
    }

    [Test]
    [Argument(1,2,3)]
    public Task ServiceProviderAttribute_Should_Initialize_Correctly2(int arg1, int arg2, int arg3, [Inject] TestClass testClass)
    {
        if (arg1 != 1 || arg2 != 2 || arg3 != 3)
            throw new Exception("Arguments did not match expected values");

        if (!testClass.HasValue)
            throw new Exception("TestClass instance is null");

        return Task.CompletedTask;
    }
}

public class TestClass()
{
    public bool HasValue => true;
}
