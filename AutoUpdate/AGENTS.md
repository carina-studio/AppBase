# AutoUpdate

**Package**: `CarinaStudio.AppBase.AutoUpdate`
**Namespace**: `CarinaStudio.AutoUpdate`
**Targets**: `net8.0` – `net11.0`
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

## Notes

- The update flow is composable: pair a `Resolver` (finds the update) with an `Installer` (applies it)
- `IUpdaterComponent.StartAndWaitAsync()` waits until the component reaches a terminal state even when its `CancellationToken` is cancelled — the token requests cancellation, it does not abandon the wait. `Updater` relies on this: it rolls back an interrupted installation as soon as the wait returns, so returning early would let the installer keep writing files after the roll-back
- Cancellation is signalled with `OperationCanceledException`; catch that base type rather than `TaskCanceledException`, which the components do not always throw
- `Updater` is the top-level coordinator — use it from the ViewModel or app startup code
- Designed for portable apps (ZIP/archive distribution), not installer-based or Store apps
