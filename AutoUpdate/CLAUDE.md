# AutoUpdate

**Package**: `CarinaStudio.AppBase.AutoUpdate`
**Namespace**: `CarinaStudio.AutoUpdate`
**Targets**: `net6.0` – `net10.0`
**AOT**: Yes
**Depends on**: `Application`

Self-update / auto-update support for portable (non-Store) desktop applications.

## Key Areas

| Directory / File | Purpose |
|---|---|
| `Updater.cs` | Orchestrates the full update lifecycle: resolve → download → install |
| `IUpdaterComponent.cs` | Interface for pluggable updater components |
| `BaseUpdaterComponent.cs` | Base implementation for updater components |
| `Resolvers/` | Update resolver implementations (check for new versions from a source) |
| `Installers/` | Package installer implementations (extract, copy, replace files) |
| `ViewModels/` | ViewModel for driving update UI |

## Conventions

- The update flow is composable: pair a `Resolver` (finds the update) with an `Installer` (applies it)
- `Updater` is the top-level coordinator — use it from the ViewModel or app startup code
- Designed for portable apps (ZIP/archive distribution), not installer-based or Store apps
