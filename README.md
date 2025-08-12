
# 🧙‍♂️ Gandalf - .NET Test Framework

> "You shall not pass... unless your tests do!"


Welcome to Gandalf! This is a modern, lightweight .NET test framework designed for speed, clarity, and extensibility. Gandalf uses C# source generators to automatically discover and register your test methods, so you can focus on writing tests, not boilerplate.

## ✨ Features
- Fast, zero-boilerplate test discovery
- Attribute-based test authoring ([see all attributes here](Docs/attributes.md))
- Dependency injection with support for Transient, Scoped, and Singleton lifetimes
- Roslyn analyzers for code quality

## 🚀 Getting Started

### Prerequisites
- .NET SDK 6.0 or later
- Visual Studio 2022 or JetBrains Rider

### Build & Test

```bash
dotnet restore Gandalf.sln
dotnet build Gandalf.sln
dotnet test Src/Gandalf.Tests/Gandalf.Tests.csproj
```

## 🗂️ Project Structure

- **Gandalf.Core**: Core attributes, models, and helpers
- **Gandalf.Engine**: The test runner and execution engine
- **Gandalf.Engine.SourceGenerators**: Source generators for test discovery
- **Gandalf.Analyzers**: Roslyn analyzers for code quality

See all available attributes in the [Attribute Reference](Docs/attributes.md).

## 🛠️ How It Works

1. **Source Generators**: Discover and register your tests at compile time.
2. **[Test] Attribute**: Mark your methods, and Gandalf will find them.
3. **[Inject] Attribute**: Add dependencies to your tests with DI (supports Transient, Scoped, Singleton).

## Example

```csharp
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
        await Task.Delay(10); // Simulate async work
        Assert.AreEqual(expected, a + b);
    }
}
```

## 🤝 Contributing

Pull requests and issues are welcome! Help Gandalf become the best .NET test framework for everyone.

----

**Happy testing with Gandalf!**
