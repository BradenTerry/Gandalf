using Microsoft.Extensions.DependencyInjection;

#if CSHARP_11_0_OR_GREATER
#else
#endif

namespace Gandalf.Tests;

public class MyServiceProvider : IServiceProvider
{
    private readonly IServiceProvider _services;

    public MyServiceProvider()
    {
        var a = new ServiceCollection();
        a.AddSingleton<TestClass>();
        _services = a.BuildServiceProvider();
    }

    public object? GetService(Type serviceType)
    {
        return _services.GetService(serviceType);
    }
}
