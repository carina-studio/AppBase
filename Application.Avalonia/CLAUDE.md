# Application.Avalonia

**Package**: `CarinaStudio.AppBase.Application.Avalonia`
**Namespace**: `CarinaStudio`
**Targets**: `net6.0` – `net10.0`
**AOT**: Yes
**Depends on**: `Application`, `Avalonia`

Concrete Avalonia-based implementation of the `IApplication` and related abstractions defined in the `Application` project. This is the entry point for desktop apps built with AppBase + Avalonia.

## Key Types

| File | Purpose |
|---|---|
| `Application.cs` | Main `IApplication` implementation using Avalonia's app lifecycle |
| `IAvaloniaApplication.cs` | Extended interface adding Avalonia-specific capabilities |
| `Controls/` | Application-level controls (splash screen, main window base, etc.) |
| `Animation/` | Avalonia-integrated animation support |

## Conventions

- Consumer apps subclass `Application` from this project (not from the abstract `Application` project)
- Use `IAvaloniaApplication` when Avalonia-specific features (resource dictionaries, styles) are needed beyond `IApplication`
