# Gandalf Attribute Reference

This document describes all attributes available in the Gandalf testing framework.

---

## `[Test]`

Marks a method as a test case to be discovered and run by Gandalf.

**Usage:**
```csharp
[Test]
public Task MyTest() { ... }
```
- Can be applied to any public method.
- Methods can be async (`Task`) or sync (`void`).
- Required for test discovery.

---

## `[Argument]`

Defines a set of arguments for a parameterized test method. Each `[Argument(...)]` creates a separate test case with the specified parameters.

**Usage:**
```csharp
[Test]
[Argument(1, 2)]
[Argument(3, 4)]
public Task Add(int a, int b) { ... }
```
- Can be applied multiple times to a method.
- Arguments are passed in order to the test method.
- The number and types of arguments must match the method parameters (excluding `[Inject]` properties).

---

## `[Inject]`

Marks a property or parameter for dependency injection. Controls the lifetime of the injected instance.

**Usage:**
```csharp
[Inject(InstanceType.Singleton)]
public required MyService Service { get; init; }
```
- Can be applied to properties or parameters.
- Supports lifetimes:
  - `Transient` (default): new instance each time
  - `Scoped`: one per test class
  - `Singleton`: shared for all tests

**Enum:**
```csharp
public enum InstanceType
{
    Transient, // Created each time it is requested
    Scoped,    // Created once per class
    Singleton  // Created once and shared throughout the application's lifetime
}
```

---

For more details and advanced usage, see the main README or source code in `Src/Gandalf.Core/Attributes/`.
