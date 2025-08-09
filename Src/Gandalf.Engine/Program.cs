using System.Reflection;
using Gandalf.Engine;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;

ITestApplicationBuilder testApplicationBuilder = await TestApplication.CreateBuilderAsync(args);

testApplicationBuilder.AddTestingFramework(() => new[] { Assembly.GetEntryAssembly()! });

// In-process & out-of-process extensions
// Register the testing framework command line options
testApplicationBuilder.CommandLine.AddProvider(() => new TestingFrameworkCommandLineOptions());

// In-process extensions
testApplicationBuilder.TestHost.AddTestHostApplicationLifetime(serviceProvider
    => new DisplayTestApplicationLifecycleCallbacks(serviceProvider.GetOutputDevice()));
testApplicationBuilder.TestHost.AddTestSessionLifetimeHandle(serviceProvider
    => new DisplayTestSessionLifeTimeHandler(serviceProvider.GetOutputDevice()));
testApplicationBuilder.TestHost.AddDataConsumer(serviceProvider
    => new DisplayDataConsumer(serviceProvider.GetOutputDevice()));

// Out-of-process extensions
testApplicationBuilder.TestHostControllers.AddEnvironmentVariableProvider(_6
    => new SetEnvironmentVariableForTestHost());
testApplicationBuilder.TestHostControllers.AddProcessLifetimeHandler(serviceProvider =>
    new MonitorTestHost(serviceProvider.GetOutputDevice()));

// In-process composite extension SessionLifeTimeHandler+DataConsumer
CompositeExtensionFactory<DisplayCompositeExtensionFactorySample> compositeExtensionFactory = new(serviceProvider => new DisplayCompositeExtensionFactorySample(serviceProvider.GetOutputDevice()));
testApplicationBuilder.TestHost.AddTestSessionLifetimeHandle(compositeExtensionFactory);
testApplicationBuilder.TestHost.AddDataConsumer(compositeExtensionFactory);

// Register public extensions
// Trx
// testApplicationBuilder.AddTrxReportProvider();

using ITestApplication testApplication = await testApplicationBuilder.BuildAsync();
return await testApplication.RunAsync();

public class Configuration : IConfiguration
{
    public string? this[string key] => "";
}