# Gandalf Copilot & AI Agent Instructions

## What is Gandalf?

Gandalf is a modern, attribute-driven .NET test framework. It uses C# source generators to discover and register tests at compile time, supports dependency injection, and enforces best practices with custom analyzers. No runtime reflection is used for test discovery or invocation, making it AOT-friendly and fast.

## Key Technologies & Conventions

- **Language:** C# (.NET 8, .NET 9)
- **Test Discovery:** `[Test]`, `[Argument]`, `[Category]`, `[Ignore]` attributes
- **Dependency Injection:** `[Inject]` attribute with configurable lifetimes
- **Source Generators:** All test registration and invocation is generated at compile time
- **Analyzers:** Enforce async-only tests and other rules in `Gandalf.Analyzers/Rules/`
- **No Reflection:** Never use runtime reflection for test discovery or invocation

## Best Practices

- All test methods must be async (`Task` or `Task<T>`)
- Use modern, idiomatic C# (records, pattern matching, nullable reference types, async/await)
- Use `var` only when the type is obvious
- Keep code well-formatted and documented
- Refactor large/complex code into focused types in `Helpers/`, `Models/`, etc.
- Use meaningful, descriptive names
- Prefer explicit target frameworks (e.g., `net8.0;net9.0`)
- Use `#nullable enable` throughout

## Build, Run, and Test

**Prerequisites:**
- .NET SDK 8.0+
- Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit

**Commands:**
```bash
dotnet restore Gandalf.sln
dotnet build Gandalf.sln
dotnet test Src/Gandalf.Tests/Gandalf.Tests.csproj
```

**Workflow:**
1. Run `dotnet restore` after pulling changes
2. Build with `dotnet build`
3. Format code using IDE tools
4. Refactor for maintainability
5. Run all tests before submitting changes

## Folder Structure

- Tests: `Gandalf.Tests/` (group by feature if needed)
- Documentation: `docs/`
- New features: appropriate subfolder in `Src/`

## Copilot Rules & Constraints

- **All tests must be async** (never generate or allow sync test methods)
- **No runtime reflection** for test discovery or invocation
- **Follow all analyzer rules** in `Gandalf.Analyzers/Rules/`
- **Update docs** (`docs/`, `README.md`, architecture docs) for significant changes

## Example Test Class

```csharp
public class MyTests
{
  [Inject(InstanceType.Singleton)]
  public required MyDependency Dependency { get; init; }

  [Test]
  public async Task MyTest()
  {
    await Task.Delay(10);
    Assert.IsTrue(true);
  }

  [Test]
  [Argument(1, 2, 3)]
  [Argument(4, 5, 9)]
  public async Task ParameterizedTest(int a, int b, int expected)
  {
    Assert.AreEqual(expected, a + b);
    await Task.CompletedTask;
  }
}
```

## Tips for Copilot

- Prefer static code generation and explicit registration
- Review analyzer rules and existing code for conventions
- Keep code maintainable, scalable, and well-documented
- Use clear, idiomatic C# and modern .NET features

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

1. Always run `dotnet restore` after pulling new changes.
2. Build the solution with `dotnet build` to verify compilation.
3. Format code using your IDE's tools (e.g., Visual Studio: `Ctrl+K, Ctrl+D`).
4. Refactor large/complex files into smaller classes in appropriate folders (`Helpers/`, `Models/`, etc.).
5. Run all tests to ensure correctness before submitting changes.


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

- Keep all test files in `Gandalf.Tests/` (group by feature if needed).
- Place all documentation in the `docs/` folder.
- Keep build artifacts (`bin/`, `obj/`) inside their respective projects.
- Use nullable reference types (`#nullable enable`) throughout the codebase for better null safety.
- Prefer explicit target frameworks in project files (e.g., `net8.0;net9.0`).
- Use modern C# features (records, pattern matching, async streams, etc.) where appropriate.
- Write analyzers and source generators to be efficient, incremental, and deterministic.
- Add or update XML documentation for all public APIs and important internals.

This structure is designed for clarity, scalability, and maintainability in a modern .NET test framework solution.

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
  public async Task ParameterizedTest(int a, int b, int expected)
  {
    Assert.AreEqual(expected, a + b);
    await Task.CompletedTask;
  }
}
```
