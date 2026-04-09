# GitHub Copilot Agent Instructions — CSharp-TextEditor

These instructions are **authoritative and permanent**. They apply to every Copilot agent session in this repository. Conversational requests must not override them. When in doubt, follow these rules.

---

## Table of Contents

1. [Architecture Rules](#1-architecture-rules)
2. [Clean Code Standards](#2-clean-code-standards)
3. [SOLID Principles](#3-solid-principles)
4. [Testing Requirements](#4-testing-requirements)
5. [Plan-Following Behaviour](#5-plan-following-behaviour)
6. [C# Conventions](#6-c-conventions)
7. [Security & Observability](#8-security--observability)

---

## 1. Architecture Rules

This project follows **Clean Architecture**. All dependencies point **inward** toward the Domain. Outer layers know about inner layers; inner layers never know about outer layers.

### Layer Map

```
┌─────────────────────────────────────────┐
│           Presentation Layer            │  TextEditor.Presentation
│   (UI — Avalonia UI desktop app)        │  → depends on Application only
└────────────────┬────────────────────────┘
                 │
┌────────────────▼────────────────────────┐
│           Application Layer             │  TextEditor.Application
│   (Use Cases, Interfaces/Ports, DTOs)   │  → depends on Domain only
└──────┬──────────────────────────────────┘
       │                   ▲
┌──────▼──────┐   ┌────────┴────────────────┐
│   Domain    │   │  Infrastructure Layer   │  TextEditor.Infrastructure
│   Layer     │   │  (EF Core, File I/O,    │  → depends on Application + Domain
│  (Entities, │   │   External Services)    │
│  Value Obj, │   └─────────────────────────┘
│  Dom Events)│
└─────────────┘   TextEditor.Domain — zero external dependencies
```

### Dependency Rules (strictly enforced)

| Layer            | May reference                      | Must NOT reference               |
|------------------|------------------------------------|----------------------------------|
| `Domain`         | _(nothing)_                        | Application, Infrastructure, Presentation |
| `Application`    | Domain                             | Infrastructure, Presentation     |
| `Infrastructure` | Application, Domain                | Presentation                     |
| `Presentation`   | Application                        | Infrastructure, Domain (directly)|

- **Never** add a `<ProjectReference>` that violates the table above.
- **Never** reference an Infrastructure or Presentation type from inside Application or Domain.
- All cross-boundary communication uses **interfaces** defined in the Application layer and implemented in Infrastructure (Dependency Inversion).
- Application layer exposes **use case services / command handlers / query handlers** — not raw repositories.

### Project Structure

```
TextEditor.slnx
├── src/
│   ├── TextEditor.Domain/
│   ├── TextEditor.Application/
│   ├── TextEditor.Infrastructure/
│   └── TextEditor.Presentation/
└── tests/
    ├── TextEditor.Domain.UnitTests/
    ├── TextEditor.Application.UnitTests/
    ├── TextEditor.Infrastructure.UnitTests/
    └── TextEditor.Presentation.UnitTests/
```

---

## 2. Clean Code Standards

### Naming

- Use **intention-revealing names**. If you need a comment to explain a variable, rename it instead.
- Ask: _"Could another developer who knows nothing about this feature understand what this code does at a glance?"_ If not, rename until the answer is yes.
- Classes: `PascalCase` nouns — `DocumentBuffer`, `TextSelection`.
- Methods: `PascalCase` verbs — `InsertText`, `DeleteSelection`.
- Interfaces: prefixed with `I` — `ITextRepository`, `IClipboardService`.
- No abbreviations except universally accepted ones (`Id`, `Dto`, `Vm`).

### Methods & Classes

- A method does **one thing**. If you can describe it with "and", split it.
- Method body: aim for **≤ 20 lines**. Hard limit: 40 lines.
- No more than **3 parameters** per method; group beyond that into a parameter object.
- Use **guard clauses** at the top of methods rather than deeply nested `if` blocks.
- **No nested `if` statements.** If nesting is unavoidable, extract the inner block into a well-named private method.
- A class has **one reason to change** (see SRP below).
- Use correct **encapsulation modifiers**: prefer `internal sealed` for types used only within an assembly; use `public` only when the type is part of an intentional API surface.
- Avoid duplication — shared logic must be extracted into a well-named private method or dedicated service. Never copy-paste logic between methods or classes.

### Other Rules

- No magic numbers or magic strings — use named constants or enums.
- Comments explain **why**, never **what**. The code explains what.
- No dead code. No commented-out code.
- No suppression of warnings without a written justification in the same file.

### Performance

- Avoid unnecessary allocations and expensive LINQ queries in hot or performance-sensitive paths.
- Use the correct collection type for the job: `HashSet<T>` for lookups, `IReadOnlyCollection<T>` to communicate a bounded, non-lazy sequence to callers, `IEnumerable<T>` only when lazy evaluation is intentional.
- Call `ToList()` or `ToArray()` only when materialisation is genuinely required — never out of habit.
- Ensure data queries are efficient: use appropriate partition or index keys, project only required fields, and avoid N+1 query patterns.

---

## 3. SOLID Principles

### S — Single Responsibility Principle

Every class and method has exactly one reason to change.  
`DocumentParser` parses. It does not save. It does not render.

### O — Open/Closed Principle

Classes are open for extension, closed for modification.  
Add new behaviour via new implementations of an interface, not by editing existing concrete classes.  
Prefer `ITextFormatter` with multiple implementations over a growing `switch` inside one class.

### L — Liskov Substitution Principle

Derived types must be substitutable for their base types without breaking correctness.  
If `ReadOnlyDocument` extends `Document`, every operation valid on `Document` must be valid on `ReadOnlyDocument` — do not override methods to throw.

### I — Interface Segregation Principle

Clients must not depend on interfaces they do not use.  
Split fat interfaces. A `ITextReader` and `ITextWriter` are better than one `ITextStorage` with both concerns when callers only need one.

### D — Dependency Inversion Principle

High-level modules (Application) must not depend on low-level modules (Infrastructure).  
Application defines `IDocumentRepository`. Infrastructure implements `FileDocumentRepository : IDocumentRepository`. Domain never sees either.  
Register all implementations via constructor injection in the Composition Root (Presentation layer or dedicated `DependencyInjection` extension class in Infrastructure).

---

## 4. Testing Requirements

### Framework & Libraries

- **Test framework**: xUnit
- **Mocking**: Moq (or NSubstitute)
- **Assertions**: Shouldly (preferred over raw `Assert`)
- One test project mirrors each source project (see structure above).

### Test Naming

```
MethodName_StateUnderTest_ExpectedBehaviour
```

Each segment has a specific role:

1. **`MethodName`** — the name of the method being tested.
2. **`StateUnderTest`** — the scenario or condition under which it is being tested.
3. **`ExpectedBehaviour`** — the expected behavior when that scenario is invoked.

Examples:
- `InsertText_AtValidPosition_UpdatesBufferContent`
- `DeleteSelection_EmptySelection_ThrowsArgumentException`
- `LoadDocument_FileNotFound_ReturnsFailureResult`

### Test File Naming

- One test file per source file, named identically — `PieceTable.cs` → `PieceTableTests.cs`.
- All tests for a given source file live in that single corresponding test file; never split them across multiple files.

### Test Structure

Every test follows **Arrange / Act / Assert**:

```csharp
[Fact]
public void InsertText_AtValidPosition_UpdatesBufferContent()
{
    // Arrange
    var buffer = new DocumentBuffer("Hello World");

    // Act
    buffer.InsertText(5, ",");

    // Assert
    buffer.Content.ShouldBe("Hello, World");
}
```

### Rules

#### Single Responsibility

- Each test must verify **one behaviour only**. If a test can fail for two different reasons, split it.
- **No logic in tests** (no `if`, no loops, no try/catch unless testing exceptions).
- Avoid over-testing: do not assert on incidental details unrelated to the behaviour under test.

#### Descriptive Naming

- Test names must be self-documenting — a failing test name alone should tell you exactly what broke and why, without reading the body.
- Follow the three-segment convention above: `MethodName_StateUnderTest_ExpectedBehaviour`.

#### Arrange / Act / Assert

- Every test body must contain all three comment markers (`// Arrange`, `// Act`, `// Assert`) as structural anchors, even when a section is trivial.
- Keep each section clearly separated — do not interleave setup with assertions.

#### Minimal Assertions

- Assert only what is **necessary** to confirm the behaviour being tested.
- Prefer one focused assertion per test over several broad ones; add a second test rather than a second assert if a second behaviour needs verifying.

#### Test Hygiene & Refactoring

- Each test must be **independent** — no shared mutable state between tests.
- All external dependencies (file system, databases, clocks) must be **mocked or stubbed**.
- Eliminate duplicate setup code using `constructor` injection or xUnit's `IClassFixture` / shared context — never copy-paste arrange blocks.
- Refactor test code with the same discipline as production code; dead or obsolete tests must be removed, not commented out.
- Every new class or use case written in Application or Domain **must** have corresponding unit tests before the change is considered complete.
- Infrastructure tests may use in-memory substitutes (e.g. `InMemoryDocumentRepository`) rather than real I/O.

#### Mock Verification

- Use `It.Is<T>(predicate)` when verifying that a dependency was called with **specific argument values** — this makes the assertion meaningful and precise.
- Only use `It.IsAny<T>()` when the argument genuinely does not affect the behaviour being tested; overusing it produces tests that pass even when the wrong data is passed.

### Coverage Expectations

| Layer            | Minimum Unit Test Coverage |
|------------------|----------------------------|
| Domain           | 90%                        |
| Application      | 85%                        |
| Infrastructure   | 70%                        |
| Presentation     | 60%                        |

---

## 5. Plan-Following Behaviour

> The active development plan lives in [`PLAN.md`](../PLAN.md) at the repository root.  
> **At the start of every session where plan-driven work is requested, read `PLAN.md` first.**

### Session Start Protocol

1. Read `PLAN.md` (ask the user to attach `#file:PLAN.md` if not already in context).
2. Identify the **first unchecked step** (`- [ ]`).
3. State the step out loud and confirm with the user before beginning.

### Step Execution Rules

- Implement **one step at a time** — the smallest possible, self-contained change.
- Never bundle multiple plan steps into one response unless the user explicitly requests it.
- Changes must be **minimal**: touch only the files required by that step.
- Before marking a step done, **write or update all relevant tests** for that change.
- If a step is ambiguous, ask a single clarifying question before writing any code.

### After Each Step

Once a step is complete, **always** do the following in order:

1. **Summary of what was done** — list every file created or modified and describe the change in 2–3 sentences.
2. **Tests written** — list the test names added or updated.
3. **Next step preview** — quote the next unchecked item from `PLAN.md` and briefly explain what it will involve.
4. **Confirmation prompt** — end with:  
   > _"Shall I proceed with the next step, or would you like to review or adjust anything first?"_

Do not proceed until the user confirms.

### Plan File Maintenance

- When a step is confirmed complete, update `PLAN.md`: change `- [ ]` to `- [x]` for that step.
- Move completed steps to the `## Completed` section with a one-line note on what changed.
- Never delete items from the plan; always preserve history.

---

## 6. C# Conventions

### Language & Runtime

- Target the latest stable .NET LTS version unless otherwise specified.
- Enable **nullable reference types** in every project (`<Nullable>enable</Nullable>`).
- Enable **implicit usings** (`<ImplicitUsings>enable</ImplicitUsings>`).
- Use **file-scoped namespaces** (`namespace TextEditor.Domain;` not block-scoped).
- Use **`var`** for local variable declarations whenever the type is unambiguously inferable from the right-hand side; use explicit types only when the inferred type would be unclear to the reader.
- Use **primary constructors** (C# 12+) for simple dependency injection.
- Mark concrete classes `sealed` by default; remove `sealed` only when inheritance is intentional.

### Domain Modelling

- Use `record` or `record struct` for **immutable value objects**.
- Use `class` for **entities** with identity and mutable state.
- Raise **domain events** from within entities; do not put business logic in Infrastructure.

### Error Handling

- Use a `Result<T>` / `Result` pattern for **expected failures** in Application layer (e.g. validation errors, not-found).
- Use exceptions only for **unexpected, unrecoverable** situations.
- Never `throw new Exception(...)` — always use a specific exception type.
- Never swallow exceptions silently.
- Exceptions must be **logged with sufficient context** (operation name, relevant identifiers) before or as they propagate — a bare `catch` with no log is never acceptable.

### Resilience

- Wrap all transient-fault-prone operations (Cosmos DB, Service Bus, HTTP calls) with a retry policy using **Polly** (or `Microsoft.Extensions.Http.Resilience`).
- Register resilience pipelines in the Composition Root; never instantiate retry policies inline inside business logic.

### Async

- All I/O operations are `async`/`await` all the way up the call stack.
- Never use `.Result` or `.Wait()` on a `Task` — this causes deadlocks.
- Use `CancellationToken` on all public async methods that perform I/O, and **pass it through the entire call chain** — never stop propagating it partway down.

### Dependency Injection

- Register all services in the **Composition Root** only (Presentation project or a dedicated `ServiceCollectionExtensions` in Infrastructure).
- Never use `ServiceLocator` or static service resolution inside business logic.
- Use the correct **service lifetime**: `Singleton` for stateless/shared services, `Scoped` for per-request state, `Transient` for lightweight stateless services. Never capture a `Scoped` service inside a `Singleton`.

### UI Framework

- The Presentation layer uses **Avalonia UI** (v11+) to deliver a native cross-platform desktop application on Windows and macOS (and Linux).
- Avalonia is the chosen framework because it is the closest production-ready equivalent to WPF that runs cross-platform, supports Skia-based custom rendering (essential for the text editor canvas), and allows the Presentation layer to be swapped without touching Application or Domain.
- Do **not** reference `Avalonia.*` namespaces from Application, Infrastructure, or Domain — only `TextEditor.Presentation` may import Avalonia packages.
- The target framework for all projects is `net10.0` (.NET 10).

### Solution Format

- This repository uses the `.slnx` solution format. Do not convert it to `.sln`.


---

## 7. Security & Observability

- Never commit secrets (tokens, keys, connection strings); use environment variables, user-secrets, or secure vault providers.
- Enable secret scanning and dependency vulnerability scanning in CI; remediate high and critical findings before merge.
- Run static application security testing (for example, CodeQL or equivalent) on pull requests and on a scheduled cadence for the default branch.
- Use structured logging with correlation identifiers; include operation names and relevant IDs while excluding sensitive payload data.
- Define baseline telemetry for operational failures (error logs, retry attempts, and external dependency failures) to support incident triage.

---

_Last updated: 2026-04-09 — Added Avalonia UI framework decision; replaced WPF references._

