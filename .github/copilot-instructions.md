# Gandalf - .NET Lightweight Test Framework

## Repository Overview

Gandalf is a lightweight .NET test framework designed to provide a simple and efficient way to write and run tests. It uses C# source generators to automatically discover and register test methods, eliminating boilerplate code.

- **Project Type**: .NET Test Framework
- **Languages**: C# 
- **Framework**: .NET Core/.NET 5+
- **Main Components**:
  - Core library (attributes, models)
  - Test engine (runner, reporting)
  - Source generators (test discovery, code generation)
  - Analyzers (code quality rules)
<HighLevelDetails>

- A summary of what the repository does.
- High level repository information, such as the size of the repo, the type of the project, the languages, frameworks, or target runtimes in use.
</HighLevelDetails>

## Build and Test Instructions

### Prerequisites

- .NET SDK 6.0 or later
- Visual Studio 2022 or JetBrains Rider

### Build Commands

```bash
# Restore NuGet packages
dotnet restore Gandalf.sln

# Build solution
dotnet build Gandalf.sln

# Run tests
dotnet test Src/Gandalf.Tests/Gandalf.Tests.csproj
```

### Development Workflow

1. Always run `dotnet restore` after pulling new changes
2. Build the solution with `dotnet build` to verify compilation
3. Run tests to ensure everything works correctly


## Project Structure

### Solution Layout

```
Gandalf.sln
README.md
docs/
  architecture.md
  attributes.md
Src/
  Gandalf.Core/
    Attributes/
      TestAttribute.cs
      InjectAttribute.cs
      ArgumentAttribute.cs
      CategoryAttribute.cs
      IgnoreAttribute.cs
    Helpers/
      TestDependencyInjection.cs
      AsyncLocalTextWriter.cs
      DiscoveredTests.cs
    Models/
      DiscoveredTest.cs
      CurrentTest.cs
      TestContext.cs
  Gandalf.Engine/
    Helpers/
    Program.cs
    GandalfTestingFramework.cs
    TestFrameworkCapabilities.cs
    TestingFrameworkCommandLineOptions.cs
    ...other engine files...
  Gandalf.Engine.SourceGenerators/
    TestMethodSourceGenerator.cs
    TestServiceProviderIncrementalSourceGenerator.cs
  Gandalf.Analyzers/
    Rules/
      ArgumentAttributeAnalyzer.cs
      ArgumentAttributeTypeAnalyzer.cs
      InjectAttributeAnalyzer.cs
      TestsReturnTasksAnalyzer.cs
  Gandalf.Tests/
    BasicTests.cs
    DependencyInjectionTests.cs
    CategoryAndIgnoreTests.cs
    ...other test files...
```

### Folder/Component Breakdown

- **Gandalf.Core**: Core attributes, models, and helpers for test definition and DI
- **Gandalf.Engine**: Test runner, execution engine, CLI, and lifecycle management
- **Gandalf.Engine.SourceGenerators**: Source generators for test discovery and DI
- **Gandalf.Analyzers**: Roslyn analyzers for code quality and test conventions
- **Gandalf.Tests**: All test projects and files (can be grouped by feature as needed)

### Best Practices

- Keep all test files in `Gandalf.Tests/` (group by feature if needed)
- Place all documentation in the `docs/` folder
- Keep build artifacts (`bin/`, `obj/`) inside their respective projects

This structure is designed for clarity, scalability, and maintainability in a .NET test framework solution.

### Key Architectural Patterns

1. **Source Generators**: Used for test discovery and code generation
2. **Attribute-Based Testing**: Tests are identified using `[Test]` attribute
3. **Dependency Injection**: Properties can be marked with `[Inject]` attribute for DI
   - Supports three lifetimes: Transient, Scoped, and Singleton

### Example Usage

```csharp
// Test class
public class MyTests
{
    [Inject(InstanceType.Singleton)]
    public required MyDependency Dependency { get; init; }
    
    [Test]
    public async Task MyTest()
    {
        // Test implementation
        await Task.Delay(10);
        Assert.IsTrue(true);
    }
    
    [Test]
    [Argument(1, 2, 3)]
    [Argument(4, 5, 9)]
    public void ParameterizedTest(int a, int b, int expected)
    {
        Assert.AreEqual(expected, a + b);
    }
}
```

