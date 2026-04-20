# Application.Android

**Package**: `CarinaStudio.AppBase.Application.Android`
**Namespace**: `CarinaStudio.Android`
**Targets**: `net8.0-android`, `net9.0-android`, `net10.0-android`
**Min Android API**: 29
**AOT**: Not set (Android uses its own AOT pipeline)
**Depends on**: `Application`, `Configuration`, `Core`, `Microsoft.Extensions.Logging`, `Xamarin.AndroidX.AppCompat`

Android-specific `IApplication` implementation and Activity base classes.

## Key Types

| File | Purpose |
|---|---|
| `Application.cs` | `IApplication` implementation for Android apps |
| `IAndroidApplication.cs` | Android-extended application interface |
| `IContextObject.cs` | Interface for objects that carry an Android `Context` |
| `Activity.cs` | Base `Activity` wired to `IAndroidApplication` |
| `AppCompatActivity.cs` | Base `AppCompatActivity` variant |
| `AndroidLogger.cs` | `ILogger` implementation backed by `Android.Util.Log` |
| `Configuration/` | Android-specific settings storage (shared preferences, etc.) |
| `Threading/` | Android main-thread dispatcher |

## Conventions

- Consumer apps subclass `Application` from this project
- Activity base classes handle lifecycle events and surface them via AppBase abstractions
- Minimum supported Android API is 29 (Android 10)
