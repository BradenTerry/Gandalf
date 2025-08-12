# Gandalf Architecture Overview

Gandalf is built for speed, extensibility, and magic! Here’s a high-level look at how it works:

## Key Components
- **Gandalf.Core**: Attributes, models, and helpers for test authoring and discovery.
- **Gandalf.Engine**: The test runner and execution engine.
- **Gandalf.Engine.SourceGenerators**: Source generators for test discovery and DI wiring.
- **Gandalf.Analyzers**: Roslyn analyzers for code quality and best practices.

## How Test Discovery Works
- Source generators scan for `[Test]` methods at compile time.
- Test metadata is generated and registered automatically.
- No runtime reflection needed—startup is fast!

## Dependency Injection
- Properties marked with `[Inject]` are resolved via a generated service provider.
- Supports Singleton, Scoped, and Transient lifetimes.

## Extensibility
- Add new attributes or analyzers to extend Gandalf’s capabilities.
- Plug in custom test lifecycle handlers.

---

For more details, see the source code and [attributes.md](attributes.md).
