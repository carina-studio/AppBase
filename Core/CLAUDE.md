# Core

**Package**: `CarinaStudio.AppBase.Core`
**Namespace**: `CarinaStudio`
**Targets**: `net6.0` – `net10.0`
**AOT**: Yes

The foundation library for AppBase. All other projects depend on this one.

## Key Areas

| Directory / File | Contents |
|---|---|
| `Collections/` | Observable and specialized collection types |
| `Threading/` | `SynchronizationContextScheduler`, synchronization utilities |
| `Buffers/` | Memory buffer helpers |
| `ComponentModel/` | Property change notification helpers |
| `Diagnostics/` | Lightweight diagnostics utilities |
| `IO/` | Stream and file I/O extensions |
| `Net/` | Network utility extensions |
| `ObservableValue.cs` | Base class for observable values; `MutableObservableValue<T>`, `FixedObservableValue<T>`, `CachedObservableValue<T>` |
| `BaseDisposable.cs` | Base class with disposal pattern; `BaseAsyncDisposable`, `BaseShareableDisposable` |
| `Platform.cs` | Runtime OS/platform detection |
| `StringPool.cs` | Interned-string pooling |
| `WeakObserver.cs` | Weak-reference observer to avoid memory leaks |

## Conventions

- `InternalsVisibleTo` grants access to `CarinaStudio.AppBase.Core.Tests` and `CarinaStudio.AppBase.Avalonia`
- Uses `Backport.System.Threading.Lock` on targets older than `net9.0` to polyfill `System.Threading.Lock`
- `AllowUnsafeBlocks` is enabled solution-wide via `Directory.Build.props`
