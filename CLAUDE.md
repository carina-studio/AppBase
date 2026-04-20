# AppBase Solution

A set of NuGet-published .NET libraries providing foundational infrastructure for building desktop and mobile applications. Current version: **2.3.0.0**.

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
- **Target frameworks**: `net6.0` through `net10.0` (Android projects: `net8.0-android` through `net10.0-android`)
- **Language**: C# with `LangVersion=preview`
- **AOT**: All projects are `IsAotCompatible=true` except `MacOS` (uses P/Invoke in ways incompatible with AOT)
- **Packaging scripts**: [`BuildPackages.bat`](BuildPackages.bat) / [`BuildPackages.sh`](BuildPackages.sh)

## Workflow

When solving a bug or adding a feature, **always present a plan first** and wait for explicit user approval before making any code changes.

After a code change is confirmed, check whether the change affects the architecture of a library project. If so, ask the user whether to update the corresponding `CLAUDE.md` in that project's folder, and update it only upon confirmation.

## Code Conventions

### General

- Nullable reference types are enabled (`#nullable enable`) everywhere.
- Unsafe blocks are allowed globally (set in `Directory.Build.props`).
- All public async methods return `Task` or `ValueTask`; UI-thread operations use the application's dispatcher.
- Root namespace: `CarinaStudio` (or `CarinaStudio.<Module>` for platform-specific projects).
- `InternalsVisibleTo` is set in the library's `.csproj`, not in `AssemblyInfo`.
- `IsTrimmable=True` assembly metadata is set on all AOT-compatible projects.

### File and Type Organization

- One type per file; file name matches the type name exactly.
- Each subsystem gets its own subfolder (e.g. `Threading/`, `Collections/`, `ViewModels/`).
- Namespace matches the folder path.
- Companion types for an interface (`Extensions`, enums) go in separate files in the same folder.
- Inner types within a class/file are ordered **alphabetically** by name.
- Members within a type (enum values, properties, methods) are also ordered **alphabetically**. Exception: struct fields with `[StructLayout(LayoutKind.Sequential)]` must preserve their memory-layout order.

### Interfaces and Extensions

- Every public member carries an XML doc comment (`/// <summary>`); use `/// <inheritdoc/>` in implementations.
- Extension method classes are named `XxxExtensions` and placed in their own file.
- XML documentation is generated for both Debug and Release configurations.

### Platform-Specific Code

- Suppress `CA1416` only when calling APIs annotated with `[SupportedOSPlatform]` by the .NET runtime.
- Custom P/Invoke definitions do **not** carry that annotation and do not require CA1416 suppression at their call sites.
- `MacOS` project is excluded from AOT (`IsAotCompatible=false`) due to Objective-C runtime interop.

## Testing

Each library project has a companion `*.Tests` project. Run tests with:

```sh
dotnet test
```

Test projects are not published to NuGet and do not have their own `CLAUDE.md`.
