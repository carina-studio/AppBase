# Tests

**Package**: `CarinaStudio.AppBase.Tests`
**Namespace**: `CarinaStudio.Tests`
**Targets**: `net6.0` – `net10.0`
**Depends on**: `Core`

Shared test helper library published on NuGet. Consumed by test projects in this solution and by downstream consumers writing tests against AppBase-based code.

## Key Types

| File | Purpose |
|---|---|
| `EventMonitor.cs` | Captures and asserts on `INotifyPropertyChanged` / event sequences |
| `NotifyPropertyChangedExtensions.cs` | Extension helpers for asserting property changes |
| `Random.cs` | Deterministic random data helpers for reproducible tests |

## Conventions

- This is a library, not a test runner project — it has no `[TestClass]` or runner dependencies
- Keep helpers generic and framework-agnostic so they work with any xUnit/NUnit/MSTest setup
