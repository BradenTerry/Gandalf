using System;
using System.Reflection;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.Services;

namespace Gandalf.Engine;

public static class TestingFrameworkExtensions
{
    public static void AddTestingFramework(this ITestApplicationBuilder builder, Func<Assembly[]> assemblies)
        => builder.RegisterTestFramework(_ => new TestingFrameworkCapabilities(),
            (capabilities, serviceProvider) => new GandalfTestingFramework(
                serviceProvider.GetConfiguration(),
                new Logger(),
                serviceProvider.GetOutputDevice(),
                assemblies));
}

public class TestingFrameworkCapabilities : ITestFrameworkCapabilities
{
    public IReadOnlyCollection<ITestFrameworkCapability> Capabilities => [];
}

public class Logger : ILogger<GandalfTestingFramework>
{
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine(formatter(state, exception));
    }

    public Task LogAsync<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine(formatter(state, exception));

        return Task.CompletedTask;
    }
}
