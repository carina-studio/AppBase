# Configuration

**Package**: `CarinaStudio.AppBase.Configuration`
**Namespace**: `CarinaStudio.Configuration`
**Targets**: `net6.0` – `net10.0`
**AOT**: Yes
**Depends on**: `Core`

Settings/configuration management library.

## Key Types

| File | Purpose |
|---|---|
| `ISettings.cs` | Core interface for reading/writing typed key-value settings |
| `MemorySettings.cs` | In-memory `ISettings` implementation |
| `PersistentSettings.cs` | File-backed `ISettings` with change notification |
| `ISettingsSerializer.cs` | Interface for serializing settings to/from streams |
| `JsonSettingsSerializer.cs` | JSON-format serializer |
| `XmlSettingsSerializer.cs` | XML-format serializer |

## Conventions

- Settings keys are strongly typed — use `SettingKey<T>` instances as keys rather than raw strings
- `PersistentSettings` raises `INotifyPropertyChanged` events when values change
