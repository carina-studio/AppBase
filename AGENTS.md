# AppBase Solution

A set of NuGet-published .NET libraries providing foundational infrastructure for building desktop and mobile applications.

## Solution Structure

| Project | NuGet Package | Purpose |
|---|---|---|
| `Core` | `CarinaStudio.AppBase.Core` | Foundation: extensions, threading, observables, collections |
| `Configuration` | `CarinaStudio.AppBase.Configuration` | Settings management with JSON/XML serializers |
| `Application` | `CarinaStudio.AppBase.Application` | Desktop application infrastructure and abstractions |
| `Avalonia` | `CarinaStudio.AppBase.Avalonia` | Avalonia UI controls and extensions |
| `Application.Avalonia` | `CarinaStudio.AppBase.Application.Avalonia` | Application implementation on top of Avalonia |
| `Application.Android` | `CarinaStudio.AppBase.Application.Android` | Application implementation for Android |
| `MacOS` | `CarinaStudio.AppBase.MacOS` | Native macOS API bindings via P/Invoke |
| `AutoUpdate` | `CarinaStudio.AppBase.AutoUpdate` | Self-update for portable applications |
| `Tests` | `CarinaStudio.AppBase.Tests` | Shared test helper utilities (also a NuGet package) |
| `Documentation` | — | DocFX-generated API docs site |

## Dependency Graph

```
Core
├── Configuration (→ Core)
│   └── Application (→ Configuration + Microsoft.Extensions.Logging.Abstractions)
│       ├── Application.Avalonia (→ Application + Avalonia)
│       ├── Application.Android (→ Application + Configuration + Core)
│       └── AutoUpdate (→ Application)
├── MacOS (→ Core)
│   └── Avalonia (→ MacOS + Microsoft.Extensions.Logging.Abstractions)
└── Tests (→ Core)
```

## Build & Packaging

- **Shared properties**: [`Directory.Build.props`](Directory.Build.props) — version, authors, Avalonia version, AOT settings
- **Target frameworks**: `net8.0` through `net11.0` (Android projects: `net8.0-android` through `net10.0-android`)
- **Language**: C# with `LangVersion=preview`
- **AOT**: All projects are `IsAotCompatible=true`
- **Packaging scripts**: [`BuildPackages.bat`](BuildPackages.bat) / [`BuildPackages.sh`](BuildPackages.sh)

## Workflow

When a change affects the architecture of a library project, update the corresponding `AGENTS.md` in that project's folder so the documentation stays in sync with the code.

## Code Conventions

### General

- Nullable reference types are enabled (`#nullable enable`) everywhere.
- Compare native handles against `IntPtr.Zero` explicitly (`handle == IntPtr.Zero`), not `default`.
- **Never pass `default` as an argument** — always use an explicit value (e.g. `CancellationToken.None`, `TimeSpan.Zero`).
- **`.Setup()` for `IDisposable` initialization** — when creating an `IDisposable` and setting its properties immediately, do not use object-initializer syntax (`new Foo { Prop = value }`): if the initializer throws, the instance is never disposed. Use the `.Setup(it => ...)` extension instead, which guarantees `Dispose()` is called when the setup action throws.
- **Time units** — milliseconds are the default. Bare `Timeout` / `Delay` / `Interval` names are always milliseconds; do not append `Ms`. Use a unit suffix only when the value is **not** in milliseconds (`SomethingSeconds`, `SomethingMicroseconds`, `SomethingTicks`).
- When a property needs custom accessor logic (validation, change notification, etc.), prefer the C# `field` keyword over a manually-declared backing field.
- To pin a managed array or buffer, prefer the `fixed` statement for scope-bound pinning; reach for `GCHandle.Alloc(…, GCHandleType.Pinned)` only when the pin must outlive the current scope.
- To read fields of a native interop struct from a raw byte buffer, mark the method `unsafe` and access the struct in place through a `fixed` pointer (`fixed (byte* p = buffer) { var header = (SomeHeader*)p; … header->field … }`) rather than copying the whole struct out (`*(SomeHeader*)p`) or `MemoryMarshal.Read<T>()`; use `sizeof(SomeHeader)` for the length check and offsets rather than a hardcoded byte count. If the method also needs to be awaitable, keep it non-`async` and return the `Task` directly (an `await` cannot sit in the `unsafe` context, and a pointer cannot be held across it).
- Do not combine assignment and evaluation into the same expression — assign in its own statement, then use the value (e.g. lazy caching is `Field ??= Create(); return Field;`, never `return Field ??= Create();` or an expression-bodied member doing both).
- Unsafe blocks are allowed globally (set in `Directory.Build.props`).
- All public async methods return `Task` or `ValueTask`; UI-thread operations use the application's dispatcher.
- Root namespace: `CarinaStudio` (or `CarinaStudio.<Module>` for platform-specific projects).
- `InternalsVisibleTo` is set in the library's `.csproj`, not in `AssemblyInfo`.
- `IsTrimmable=True` assembly metadata is set on all AOT-compatible projects.
- `ObservableProperty<T>` fields are named `XxxProp` (e.g. `MessageProp`), **never** `XxxProperty`, regardless of visibility. Reason: Avalonia 12 compiled bindings resolve any public static field named `<Member>Property` on the bound type as an `AvaloniaProperty` and fail compilation when it is not one; the `Prop` suffix is applied to all visibilities for consistency. `AvaloniaProperty` fields on controls keep the standard `XxxProperty` suffix — that convention is required by Avalonia and does not conflict. The existing `XxxProperty`-suffixed `ObservableProperty` fields (e.g. in `AutoUpdate/ViewModels/UpdatingSession.cs`) are renamed as part of the Avalonia 12 upgrade; use `Prop` in all new code.

### File and Type Organization

- One type per file; file name matches the type name exactly.
- Each subsystem gets its own subfolder (e.g. `Threading/`, `Collections/`, `ViewModels/`).
- Namespace matches the folder path.
- Companion types for an interface (`Extensions`, enums) go in separate files in the same folder.
- Inner types within a class/file are ordered **alphabetically** by name.
- Members within a type (enum values, properties, methods) are also ordered **alphabetically**. Exception: struct fields with `[StructLayout(LayoutKind.Sequential)]` must preserve their memory-layout order.
- `extension` blocks (C# 14 extension members) are placed **first** in the containing class, before all other members; they are not sorted with other members. Members inside an `extension` block are ordered alphabetically.
- **Blank lines between members** — two blank lines between members of a top-level type; one blank line between members of an inner (nested) type.

### Interfaces and Extensions

- Every public member carries an XML doc comment (`/// <summary>`); use `/// <inheritdoc/>` in implementations.
- Extension method classes are named `XxxExtensions` and placed in their own file.
- When extending a type, prefer an extension **property** inside an `extension(T value)` block over a `GetX()`-style extension method, whenever the accessor is a pure, side-effect-free projection that reads naturally as a property — so call sites read `value.BaseName` rather than `value.GetBaseName()`.
- XML documentation is generated for both Debug and Release configurations.

### Platform-Specific Code

- Suppress `CA1416` only when calling APIs annotated with `[SupportedOSPlatform]` by the .NET runtime.
- Custom P/Invoke definitions do **not** carry that annotation and do not require CA1416 suppression at their call sites.
- `MacOS` project achieves AOT compatibility by dispatching Objective-C runtime interop through libffi instead of dynamic code generation.

### Project-Specific Rules

Rules that apply only within one library project — everything above applies solution-wide. Each project's own `AGENTS.md` documents its architecture, not its rules.

- **`Application`** — logging goes through an `ILogger` obtained from `IApplication`; never construct loggers directly.
- **`Configuration`** — settings keys are strongly typed: use `SettingKey<T>` instances as keys rather than raw strings.
- **`MacOS`** —
  - All P/Invoke calls go through named library handles in `NativeLibraryHandles` — never hardcode dylib paths inline.
  - Sending messages with arbitrary signatures and dispatching defined methods go through libffi (`Ffi/LibFfi.cs`), not `Reflection.Emit`; call interfaces, closures and struct type descriptors are cached in native memory for the process lifetime.
  - Instance variables are read/written directly at `instance + Variable.Offset` — never through `object_getInstanceVariable`/`object_setInstanceVariable`, which treat the ivar as a single pointer-sized value (the value itself, not a buffer address). `Variable.Offset` re-resolves the `Ivar` handle by name on first use, because handles obtained during `Class.DefineClass` (before `objc_registerClassPair`) become dangling once later `class_addIvar` calls reallocate the ivar list.
  - Structure types added to the library which may pass through native calls must also be listed in `ILLink.Descriptors.xml`.

## Testing

Each library project has a companion `*.Tests` project. Run tests with:

```sh
dotnet test
```

Test projects are not published to NuGet and do not have their own `AGENTS.md`.

## Code Review Checklist

### Correctness
- Logic is correct for all paths, including edge cases (empty collections, null values, zero counts).
- Multi-step operations that must be atomic are protected by a lock or semaphore across all steps, not just individual operations.
- State mutations under a lock do not leak mutable references that can be read or written outside the lock.
- `async`/`await` is used correctly — no fire-and-forget unless intentional; no `.Result` or `.Wait()` blocking on async code.
- `CancellationToken` is propagated through all async calls; `OperationCanceledException` is not swallowed.
- `IDisposable` resources are disposed in all paths, including error paths.
- Native handles are released on every path, and compared against `IntPtr.Zero` rather than `default`.

### Thread Safety
- Shared mutable fields accessed from multiple threads are protected consistently.
- No TOCTOU (time-of-check/time-of-use) races — check and act happen under the same lock or synchronization primitive.
- Background-thread methods are marked `[CalledOnBackgroundThread]`; UI-thread calls are dispatched via `SynchronizationContext` or guarded with `CheckAccess()`.

### Error Handling
- Exceptions are not silently swallowed — at minimum log the error.
- Expected failure paths (file missing, unsupported platform) are logged at `Warning`; unexpected exceptions at `Error`.
- Best-effort operations (e.g. cleanup) catch and log per-item rather than aborting the entire operation.

### Style
- All coding style rules above are followed (naming, formatting, nullability, patterns).
- Unused `using` directives removed; correct namespaces imported for any new types introduced.
- `default` is not passed as an argument — explicit values used instead.
- Members within a type are still ordered alphabetically, and `extension` blocks still sit first in the containing class. Verify after adding, renaming, or moving any member.
- Every public member carries an XML doc comment; implementations and overrides use `/// <inheritdoc/>`.

### Documentation
- Check whether the change affects the architecture of a library project. If so, the corresponding project's `AGENTS.md` should be updated to match.
