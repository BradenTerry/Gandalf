# 🧙‍♂️ Gandalf - The Magical .NET Test Framework

> "You shall not pass... unless your tests do!"

## Welcome to Gandalf!

Gandalf is not your average .NET test framework. It's lightweight, fast, and powered by C# source generators that automatically discover and register your test methods—so you can focus on writing spells (uh, tests), not boilerplate.

✨ **Features:**
- 🪄 Zero-boilerplate test discovery
- 🧩 Attribute-based test magic ([see all attributes here](Docs/attributes.md))
- 🧪 Dependency injection with lifetimes (Transient, Scoped, Singleton)
- 🚀 Blazing fast test runs
- 🧹 Roslyn analyzers to keep your code clean

## Getting Started

### Prerequisites
- .NET SDK 6.0 or later
- Visual Studio 2022 or JetBrains Rider

### Build & Test in 3 Easy Steps

```bash
# 1. Restore your magical NuGet packages
$ dotnet restore Gandalf.sln

# 2. Build the solution (wave your staff)
$ dotnet build Gandalf.sln

# 3. Run the tests (let the magic happen)
$ dotnet test Src/Gandalf.Tests/Gandalf.Tests.csproj
```

## Project Map

- **Gandalf.Core**: Core attributes, models, and helpers
- **Gandalf.Engine**: The test runner and execution engine
- **Gandalf.Engine.SourceGenerators**: Source generators for test discovery
- **Gandalf.Analyzers**: Roslyn analyzers for code quality

Want to see all the magical attributes you can use? [Check out the Attribute Reference!](Docs/attributes.md)

## How Does It Work?

1. **Source Generators**: Find your tests and wire them up—automagically.
2. **[Test] Attribute**: Mark your methods, and Gandalf will find them.
3. **[Inject] Attribute**: Add dependencies to your tests with DI (supports Transient, Scoped, Singleton).

## Example: Cast a Test Spell

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

## Contributing

Pull requests, issues, and magical suggestions are welcome! Help Gandalf become the most powerful test wizard in .NET land.

---

🧙‍♂️ **May your tests always pass, and your bugs be forever banished!**
