using System.Reflection;
using System.Threading.Tasks.Dataflow;
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

internal sealed class GandalfTestingFramework : ITestFramework, IDataProducer, IOutputDeviceDataProducer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GandalfTestingFramework> _logger;
    private readonly IOutputDevice _outputDevice;
    private readonly Assembly[] _assemblies;

    public GandalfTestingFramework(
        IConfiguration configuration,
        ILogger<GandalfTestingFramework> logger,
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
            await _outputDevice.DisplayAsync(this, new FormattedTextOutputDeviceData($"Gandalf version '{Version}' running tests") { ForegroundColor = new SystemConsoleColor() { ConsoleColor = ConsoleColor.Green } });

            var actionBlock = new ActionBlock<DiscoveredTest>(async test =>
            {
                if (runTestExecutionRequest.Filter is TestNodeUidListFilter filter)
                {
                    if (!filter.TestNodeUids.Any(testId => testId == test.Uid))
                    {
                        return;
                    }
                }

                AsyncLocalTextWriter.Current.Value = new StringWriter();
                    var asyncLocalOutput = new AsyncLocalTextWriter();
                    var originalOut = Console.Out;
                    Console.SetOut(asyncLocalOutput);
                    var testContext = new TestContext(
                        test.Uid,
                        test.MethodName,
                        test.Assembly,
                        asyncLocalOutput);

                    CurrentTest.TestContext.Value = testContext;
                    var startTime = DateTimeOffset.UtcNow;
                    DateTimeOffset? endTime = null;

                    var runningTestNode = new TestNode
                    {
                        Uid = test.Uid,
                        DisplayName = test.MethodName
                    };

                    // Notify the test explorer that the test is running
                    runningTestNode.Properties.Add(new InProgressTestNodeStateProperty());
                    if (test.ParentUid != null)
                        await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, runningTestNode, test.ParentUid));
                    else
                        await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, runningTestNode));

                    var testNode = new TestNode
                    {
                        Uid = test.Uid,
                        DisplayName = test.MethodName
                    };

                    try
                    {
                        try
                        {
                            await test.InvokeAsync();
                        }
                        finally
                        {
                            endTime = DateTimeOffset.UtcNow;
                        }

                        testNode.Properties.Add(new PassedTestNodeStateProperty());
                        testNode.Properties.Add(new TimingProperty(new TimingInfo(startTime, endTime.Value, endTime.Value - startTime)));

                        // Add captured output as a property
#pragma warning disable TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                        testNode.Properties.Add(new StandardOutputProperty(asyncLocalOutput.ToString()));
#pragma warning restore TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                        if (test.ParentUid != null)
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, testNode, test.ParentUid));
                        else
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, testNode));
                    }
                    catch (Exception ex)
                    {
                        testNode.Properties.Add(new FailedTestNodeStateProperty(ex));
                        testNode.Properties.Add(new TimingProperty(new TimingInfo(startTime, endTime!.Value, endTime.Value - startTime)));

                        // Add captured output even on failure
#pragma warning disable TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                        testNode.Properties.Add(new StandardOutputProperty(asyncLocalOutput.ToString()));
#pragma warning restore TPEXP // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                        if (test.ParentUid != null)
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, testNode, test.ParentUid));
                        else
                            await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, testNode));
                    }
                    finally
                    {
                        CurrentTest.TestContext.Value = null;
                        AsyncLocalTextWriter.Current.Value = null;
                        Console.SetOut(originalOut);
                    }
            });

        try
        {
            // Get category filter from CLI options - try different configuration keys
            var categoryFilter = new List<string>();
            var catStr = _configuration?["category"] ?? _configuration?["CategoryOption"] ?? _configuration?["TestingFrameworkCommandLineOptions:category"];
            
            if (!string.IsNullOrWhiteSpace(catStr))
            {
                categoryFilter.AddRange(catStr.Split(','));
            }
            
            // Debug output to see what configuration is available
            await _outputDevice.DisplayAsync(this, new FormattedTextOutputDeviceData($"Category filter: '{catStr}'") { ForegroundColor = new SystemConsoleColor() { ConsoleColor = ConsoleColor.Yellow } });

            var testsToRun = DiscoveredTests.All.Where(test => _assemblies.Any(assembly => assembly.GetName().Name == test.Assembly)).ToList();
            if (categoryFilter.Count > 0)
            {
                var originalCount = testsToRun.Count;
                testsToRun = testsToRun.Where(t => t.Categories != null && t.Categories.Any(cat => categoryFilter.Contains(cat))).ToList();
                await _outputDevice.DisplayAsync(this, new FormattedTextOutputDeviceData($"Filtered from {originalCount} to {testsToRun.Count} tests by categories: {string.Join(", ", categoryFilter)}") { ForegroundColor = new SystemConsoleColor() { ConsoleColor = ConsoleColor.Yellow } });
            }
            await _outputDevice.DisplayAsync(this, new FormattedTextOutputDeviceData($"Found {testsToRun.Count} tests to run.") { ForegroundColor = new SystemConsoleColor() { ConsoleColor = ConsoleColor.Green } });
            foreach (var testInfo in testsToRun)
            {
                if (!string.IsNullOrEmpty(testInfo.IgnoreReason))
                {
                    // Report as skipped
                    var skippedNode = new TestNode
                    {
                        Uid = testInfo.Uid,
                        DisplayName = testInfo.MethodName
                    };
                    skippedNode.Properties.Add(new SkippedTestNodeStateProperty(testInfo.IgnoreReason));
                    if (testInfo.ParentUid != null)
                        await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, skippedNode, testInfo.ParentUid));
                    else
                        await context.MessageBus.PublishAsync(this, new TestNodeUpdateMessage(runTestExecutionRequest.Session.SessionUid, skippedNode));
                    continue;
                }
                await actionBlock.SendAsync(testInfo);
            }

            actionBlock.Complete();
            await actionBlock.Completion;
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
