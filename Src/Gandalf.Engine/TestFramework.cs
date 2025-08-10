using System.Reflection;
using Gandalf.Core;
using Gandalf.Core.Helpers;
using Gandalf.Core.Models;
using Microsoft.Testing.Extensions.TrxReport.Abstractions;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Requests;

internal sealed class TestingFramework : ITestFramework, IDataProducer, IOutputDeviceDataProducer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestingFramework> _logger;
    private readonly IOutputDevice _outputDevice;
    private readonly Assembly[] _assemblies;

    public TestingFramework(
        IConfiguration configuration,
        ILogger<TestingFramework> logger,
        IOutputDevice outputDevice,
        Func<Assembly[]> assemblies)
    {
        _configuration = configuration;
        _logger = logger;
        _outputDevice = outputDevice;
        _assemblies = assemblies();
    }

    public string Uid => "Gandalf";

    public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

    public string DisplayName => "Gandalf";

    public string Description => "Gandalf testing framework";

    public Type[] DataTypesProduced => new[] { typeof(TestNodeUpdateMessage), typeof(SessionFileArtifact) };

    public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
        => Task.FromResult(new CloseTestSessionResult() { IsSuccess = true });

    public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context)
        => Task.FromResult(new CreateTestSessionResult() { IsSuccess = true });

    public async Task ExecuteRequestAsync(ExecuteRequestContext context)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            await _logger.LogDebugAsync($"Executing request of type '{context.Request}'");
        }

        switch (context.Request)
        {
            case DiscoverTestExecutionRequest discoverTestExecutionRequest:
                {
                    await HandleTestDiscoveryAsync(context, discoverTestExecutionRequest);
                    break;
                }

            case RunTestExecutionRequest runTestExecutionRequest:
                {
                    await HandleTestExecutionAsync(context, runTestExecutionRequest);
                    break;
                }

            default:
                throw new NotSupportedException($"Request {context.GetType()} not supported");
        }
    }

    private async Task HandleTestExecutionAsync(ExecuteRequestContext context, RunTestExecutionRequest runTestExecutionRequest)
    {
        try
        {
            await _outputDevice.DisplayAsync(this, new FormattedTextOutputDeviceData($"Gandalf version '{Version}' running tests") { ForegroundColor = new SystemConsoleColor() { ConsoleColor = ConsoleColor.Green } });

            List<Task> results = new();
            var testsToRun = DiscoveredTests.All.Where(test => _assemblies.Any(assembly => assembly.GetName().Name == test.Assembly)).ToList();
            await _outputDevice.DisplayAsync(this, new FormattedTextOutputDeviceData($"Found {testsToRun.Count} tests to run.") { ForegroundColor = new SystemConsoleColor() { ConsoleColor = ConsoleColor.Green } });
            foreach (var testInfo in testsToRun)
            {
                if (runTestExecutionRequest.Filter is TestNodeUidListFilter filter)
                {
                    if (!filter.TestNodeUids.Any(testId => testId == testInfo.Uid))
                    {
                        continue;
                    }
                }

                results.Add(Task.Run(async () =>
                {
                    AsyncLocalTextWriter.Current.Value = new StringWriter();
                    var asyncLocalOutput = new AsyncLocalTextWriter();
                    var originalOut = Console.Out;
                    Console.SetOut(asyncLocalOutput);

                    try
                    {
                        var startTime = DateTimeOffset.UtcNow;
                        await testInfo.InvokeAsync();
                        var endTime = DateTimeOffset.UtcNow;

                        var successfulTestNode = new TestNode()
                        {
                            Uid = testInfo.Uid,
                            DisplayName = testInfo.MethodName,
                            Properties = new PropertyBag(PassedTestNodeStateProperty.CachedInstance)
                        };

                        successfulTestNode.Properties.Add(new TimingProperty(new TimingInfo(startTime, endTime, endTime - startTime)));

                        // Add captured output as a property
#pragma warning disable TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                        successfulTestNode.Properties.Add(new StandardOutputProperty(asyncLocalOutput.ToString()));
#pragma warning restore TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                        if (testInfo.ParentUid != null)
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, successfulTestNode, testInfo.ParentUid));
                        else
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, successfulTestNode));
                    }
                    catch (Exception ex)
                    {
                        var assertionFailedTestNode = new TestNode
                        {
                            Uid = testInfo.FullName,
                            DisplayName = testInfo.MethodName,
                            Properties = new PropertyBag(new FailedTestNodeStateProperty(ex)),
                        };

                        // Add captured output even on failure
#pragma warning disable TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                        assertionFailedTestNode.Properties.Add(new StandardOutputProperty(asyncLocalOutput.ToString()));
#pragma warning restore TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                        if (testInfo.ParentUid != null)
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, assertionFailedTestNode, testInfo.ParentUid));
                        else
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, assertionFailedTestNode));
                    }
                    finally
                    {
                        AsyncLocalTextWriter.Current.Value = null;
                        Console.SetOut(originalOut);
                    }
                }));
            }

            await Task.WhenAll(results);
        }
        finally
        {
            // Ensure to complete the request also in case of exception
            context.Complete();
        }
    }

    private async Task HandleTestDiscoveryAsync(ExecuteRequestContext context, DiscoverTestExecutionRequest discoverTestExecutionRequest)
    {
        try
        {
            var testsFound = DiscoveredTests.All.Where(test => _assemblies.Any(assembly => assembly.GetName().Name == test.Assembly));
            Console.WriteLine(testsFound.Count() + " tests discovered.");
            foreach (var testInfo in testsFound)
            {
                var testNode = new TestNode()
                {
                    Uid = testInfo.Uid,
                    DisplayName = testInfo.MethodName,
                    Properties = new PropertyBag(DiscoveredTestNodeStateProperty.CachedInstance)
                };

                TestMethodIdentifierProperty testMethodIdentifierProperty = new(
                    testInfo.Assembly,
                    testInfo.Namespace,
                    testInfo.ClassName,
                    testInfo.MethodName,
                    0, // TODO: Get MethodArity
                    [], // parameterTypeFullNames
                    typeof(Task).FullName!);

                testNode.Properties.Add(testMethodIdentifierProperty);
                // Add source file location properties
                testNode.Properties.Add(new TestFileLocationProperty(testInfo.FilePath, new LinePositionSpan(new LinePosition(testInfo.LineNumber, testInfo.LinePosition), new LinePosition(testInfo.EndLineNumber, testInfo.EndLinePosition))));

                if (testInfo.ParentUid != null)
                    await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(discoverTestExecutionRequest.Session.SessionUid, testNode, testInfo.ParentUid));
                else
                    await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(discoverTestExecutionRequest.Session.SessionUid, testNode));
            }
        }
        finally
        {
            context.Complete();
        }
    }

    private void FillTrxProperties(TestNode testNode, MethodInfo test, Exception? ex = null)
    {
        // testNode.Properties.Add(new TrxFullyQualifiedTypeNameProperty(test.DeclaringType!.FullName!));

        if (ex is not null)
        {
            // testNode.Properties.Add(new TrxExceptionProperty(ex.Message, ex.StackTrace));
        }
    }

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);
}
