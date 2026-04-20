# MacOS

**Package**: `CarinaStudio.AppBase.MacOS`
**Namespace**: `CarinaStudio.MacOS`
**Targets**: `net6.0` – `net10.0`
**AOT**: No (P/Invoke-heavy; not AOT compatible)
**Depends on**: `Core`

Low-level macOS native API bindings via P/Invoke and Objective-C runtime interop.

## Key Areas

| Directory / File | Purpose |
|---|---|
| `ObjectiveC/` | Objective-C runtime bindings (class, selector, message send) |
| `AppKit/` | AppKit framework bindings (NSApplication, NSWindow, etc.) |
| `CoreFoundation/` | CoreFoundation types (CFString, CFArray, CFRunLoop, etc.) |
| `CoreGraphics/` | CoreGraphics types (CGRect, CGPoint, CGSize, etc.) |
| `ImageIO/` | ImageIO framework bindings |
| `NativeLibraryHandles.cs` | Centralized `dlopen` handles for macOS frameworks |
| `NativeLibraryNames.cs` | Framework dylib path constants |
| `NativeMethodInfo.cs` | Reflection helpers for native method metadata |
| `NativeTypeConversion.cs` | Marshalling helpers between managed and native types |

## Conventions

- All P/Invoke calls go through named library handles in `NativeLibraryHandles` — never hardcode dylib paths inline
- `InternalsVisibleTo` grants access to `CarinaStudio.AppBase.MacOS.NativeBridge` and `CarinaStudio.AppBase.MacOS.Tests`
- This library is intentionally excluded from AOT compilation (`IsAotCompatible=false`)
