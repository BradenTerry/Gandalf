# 🧙‍♂️ Gandalf Testing Framework

Welcome to **Gandalf** — a modern, source-generator-powered .NET testing framework designed for speed, clarity, and extensibility.

---

## ✨ Features

- **Source Generator Test Discovery**  
  Tests are discovered at compile time for lightning-fast startup and IDE integration.

- **Custom Attributes for Test Authoring**  
  Write expressive, parameterized tests using simple attributes.

- **Parallel-safe Output Capture**  
  Each test's output is isolated, even when running in parallel.

---

## 🚀 Quick Start

1. **Mark your test methods with `[Test]`:**

    ```csharp
    using Gandalf.Core.Attributes;

    public class MathTests
    {
        [Test]
        public void Addition_Works()
        {
            Console.WriteLine("Running addition test!");
            Assert.Equal(2, 1 + 1);
        }
    }
    ```

2. **Parameterize tests with `[Argument]`:**

    ```csharp
    public class MathTests
    {
        [Test]
        [Argument(1, 2)]
        [Argument(3, 4)]
        public void Add(int a, int b)
        {
            Console.WriteLine($"Adding {a} + {b}");
            Assert.True(a + b > 0);
        }
    }
    ```

---

## 🏷️ Attribute Reference

### `[Test]`

Marks a method as a test case to be discovered and run by Gandalf.

```csharp
[Test]
public void MyTest() { ... }
```

- Can be applied to any public method.
- Methods can be async or sync.

---

### `[Argument]`

Defines a set of arguments for a parameterized test method.

```csharp
[Test]
[Argument(1, 2)]
[Argument(3, 4)]
public void Add(int a, int b) { ... }
```

- Each `[Argument(...)]` creates a separate test case with the specified parameters.
- Arguments are passed in order to the test method.

---

## 🗂️ Project Structure

```
Src/
  Gandalf.Core/           # Core attributes and helpers
  Gandalf.Engine/         # Test framework implementation
  Gandalf.Engine.SourceGenerators/ # Source generator for test discovery
  Gandalf.Tests/          # Example and user tests
.github/
  workflows/              # CI configuration
  copilot-instructions.md # Copilot agent onboarding
README.md                 # This file
```

---

## 🛠️ Build & Test

- **Restore:**  
  `dotnet restore`
- **Build:**  
  `dotnet build`
- **Test:**  
  `dotnet test Src/Gandalf.Tests/Gandalf.Tests.csproj`

---

## 🤝 Contributing

Pull requests are welcome! Please see `.github/copilot-instructions.md` for agent onboarding and contribution guidelines.

---

## 🧙‍♂️ You Shall Not Pass... Without Tests!

Happy testing with Gandalf!