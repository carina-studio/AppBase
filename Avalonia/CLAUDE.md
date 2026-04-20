# Avalonia

**Package**: `CarinaStudio.AppBase.Avalonia`
**Namespace**: `CarinaStudio`
**Targets**: `net6.0` – `net10.0`
**AOT**: Yes
**Depends on**: `MacOS`, `Avalonia 11.0.10`, `Microsoft.Extensions.Logging.Abstractions`

Avalonia UI controls, extensions, and utilities — framework-level additions that are not tied to the AppBase `IApplication` abstraction.

## Key Areas

| Directory / File | Purpose |
|---|---|
| `Controls/` | Custom Avalonia controls and attached properties |
| `Collections/` | Observable collections integrated with Avalonia's binding system |
| `Data/` | Data binding helpers and converters |
| `Input/` | Command and gesture utilities |
| `Media/` | Brush, color, and image utilities |
| `Theme/` | Theme resource helpers |
| `Threading/` | Avalonia dispatcher integration |
| `VIsualTree/` | Visual tree traversal helpers |
| `AvaloniaObjectExtensions.cs` | Extension methods for `AvaloniaObject` |
| `CachedResource.cs` | Cached resource lookup by key |
| `FormattedString.cs` | Runtime-formatted string observable |

## Conventions

- `InternalsVisibleTo` is granted from `Core` — use internal Core APIs where needed
- MacOS dependency is for native Avalonia rendering hints on macOS
