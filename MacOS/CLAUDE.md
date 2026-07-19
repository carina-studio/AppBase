# MacOS

**Package**: `CarinaStudio.AppBase.MacOS`
**Namespace**: `CarinaStudio.MacOS`
**Targets**: `net6.0` – `net10.0`
**AOT**: Yes (`IsAotCompatible=true`). No dynamic code generation anywhere: message sends and defined-method trampolines go through libffi (`Ffi/`), and structure marshalling uses reflection-based layout computation (`NativeStructureLayout.cs`) instead of `Marshal.SizeOf(Type)`/`PtrToStructure(Type)`. Fields of the library's own structure types are preserved under trimming via the embedded `ILLink.Descriptors.xml`; consumer-defined structures passed to native methods must have their fields preserved by the consumer (e.g. `[DynamicDependency]` or direct field usage)
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
| `ILLink.Descriptors.xml` | Preserves structure fields for reflection-based layout under trimming/NativeAOT |
| `NativeLibraryHandles.cs` | Centralized `dlopen` handles for macOS frameworks |
| `NativeLibraryNames.cs` | Framework dylib path constants |
| `NativeMethodInfo.cs` | Reflection helpers for native method metadata |
| `NativeStructureLayout.cs` | Computes native structure layouts and reads/writes structures without runtime marshalling |
| `NativeTypeConversion.cs` | Marshalling helpers between managed and native types |

## Conventions

- All P/Invoke calls go through named library handles in `NativeLibraryHandles` — never hardcode dylib paths inline
- `InternalsVisibleTo` grants access to `CarinaStudio.AppBase.MacOS.NativeBridge` and `CarinaStudio.AppBase.MacOS.Tests`
- Sending messages with arbitrary signatures and dispatching defined methods go through libffi (`Ffi/LibFfi.cs`), not `Reflection.Emit`; call interfaces, closures and struct type descriptors are cached in native memory for the process lifetime
- Structure types added to the library which may pass through native calls must also be listed in `ILLink.Descriptors.xml`
