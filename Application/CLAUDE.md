# Application

**Package**: `CarinaStudio.AppBase.Application`
**Namespace**: `CarinaStudio`
**Targets**: `net6.0` – `net10.0`
**AOT**: Yes
**Depends on**: `Configuration`, `Microsoft.Extensions.Logging.Abstractions`

Desktop application infrastructure — defines the core abstractions that platform-specific projects implement.

## Key Areas

| Directory / File | Purpose |
|---|---|
| `IApplication.cs` | Central application interface: logging, settings, dispatcher, culture |
| `IApplicationObject.cs` | Base interface for objects that belong to an application |
| `BaseApplicationObject.cs` | Base implementation of `IApplicationObject` |
| `BaseDisposableApplicationObject.cs` | Disposable variant of `BaseApplicationObject` |
| `Threading/` | Application-owned `TaskFactory` and dispatcher abstractions |
| `ViewModels/` | Base ViewModel infrastructure tied to `IApplication` |
| `Windows/` | Abstract window/dialog management |
| `Animation/` | Animation timing utilities |
| `Logging/` | Logging extensions and helpers built on `Microsoft.Extensions.Logging` |
| `PlatformCommands.cs` | Cross-platform OS command invocation (open URL, open file, etc.) |

## Conventions

- All application objects hold a reference to `IApplication` and run operations through its dispatcher
- Logging is done via `ILogger` obtained from `IApplication` — never create loggers directly
