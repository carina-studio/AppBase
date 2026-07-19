using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.Ffi;

// Managed layout of native ffi_type structure.
[StructLayout(LayoutKind.Sequential)]
unsafe struct FfiType
{
    // Fields must preserve memory-layout order.
    public nuint Size;
    public ushort Alignment;
    public ushort Type;
    public FfiType** Elements;
}
