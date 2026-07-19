# MacOS

**Package**: `CarinaStudio.AppBase.MacOS`
**Namespace**: `CarinaStudio.MacOS`
**Targets**: `net6.0` – `net10.0`
**AOT**: No. Remaining blockers: `Class.DefineMethod` emits IL for method trampolines, and `NativeTypeConversion` relies on `Marshal.SizeOf(Type)`/`PtrToStructure(Type)`. `NSObject.SendMessage` itself no longer requires dynamic code generation (it dispatches through libffi, see `Ffi/`)
**Depends on**: `Core`

Low-level macOS native API bindings via P/Invoke and Objective-C runtime interop.

## Key Areas

| Directory / File | Purpose |
|---|---|
| `ObjectiveC/` | Objective-C runtime bindings (class, selector, message send) |
| `AppKit/` | AppKit framework bindings (NSApplication, NSWindow, etc.) |
| `CoreFoundation/` | CoreFoundation types (CFString, CFArray, CFRunLoop, etc.) |
| `CoreGraphics/` | CoreGraphics types (CGRect, CGPoint, CGSize, etc.) |
| `Ffi/` | Internal bindings to system libffi (`/usr/lib/libffi.dylib`); builds runtime-described call interfaces (`ffi_cif`) so `NSObject.SendMessage` can call `objc_msgSend` with arbitrary signatures without dynamic code generation |
| `ImageIO/` | ImageIO framework bindings |
| `NativeLibraryHandles.cs` | Centralized `dlopen` handles for macOS frameworks |
| `NativeLibraryNames.cs` | Framework dylib path constants |
| `NativeMethodInfo.cs` | Reflection helpers for native method metadata |
| `NativeTypeConversion.cs` | Marshalling helpers between managed and native types |

## Conventions

- All P/Invoke calls go through named library handles in `NativeLibraryHandles` — never hardcode dylib paths inline
- `InternalsVisibleTo` grants access to `CarinaStudio.AppBase.MacOS.NativeBridge` and `CarinaStudio.AppBase.MacOS.Tests`
- This library is intentionally excluded from AOT compilation (`IsAotCompatible=false`); see the AOT note above for the remaining blockers
- Sending messages with arbitrary signatures goes through libffi (`Ffi/LibFfi.cs`), not `Reflection.Emit`; call interfaces and struct type descriptors are cached in native memory for the process lifetime
