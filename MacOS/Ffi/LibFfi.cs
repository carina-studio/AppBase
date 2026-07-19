using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.Ffi;

// Bindings to system libffi library which supports calling native function with signature described at runtime.
static unsafe class LibFfi
{
    // Size of native ffi_cif structure in bytes. Allocated with margin to cover extra fields defined by specific architectures.
    const int CifSize = 64;

    // Type code of structure defined by libffi (FFI_TYPE_STRUCT).
    public const ushort StructureTypeCode = 13;


    // Default ABI defined by libffi for current architecture (FFI_SYSV on Arm64, FFI_UNIX64 on x64).
    static readonly int DefaultAbi;

    // Predefined type descriptors exported from libffi.
    public static readonly FfiType* TypeDouble;
    public static readonly FfiType* TypeFloat;
    public static readonly FfiType* TypePointer;
    public static readonly FfiType* TypeSInt16;
    public static readonly FfiType* TypeSInt32;
    public static readonly FfiType* TypeSInt64;
    public static readonly FfiType* TypeSInt8;
    public static readonly FfiType* TypeUInt16;
    public static readonly FfiType* TypeUInt32;
    public static readonly FfiType* TypeUInt64;
    public static readonly FfiType* TypeUInt8;
    public static readonly FfiType* TypeVoid;


    // Static initializer.
    static LibFfi()
    {
        if (Platform.IsNotMacOS)
            return;
        var libHandle = NativeLibrary.Load(NativeLibraryNames.Ffi);
        DefaultAbi = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? 1 : 2;
        TypeDouble = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_double");
        TypeFloat = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_float");
        TypePointer = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_pointer");
        TypeSInt16 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_sint16");
        TypeSInt32 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_sint32");
        TypeSInt64 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_sint64");
        TypeSInt8 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_sint8");
        TypeUInt16 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_uint16");
        TypeUInt32 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_uint32");
        TypeUInt64 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_uint64");
        TypeUInt8 = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_uint8");
        TypeVoid = (FfiType*)NativeLibrary.GetExport(libHandle, "ffi_type_void");
    }


    // Call native function through prepared call interface.
    public static void Call(IntPtr cif, void* func, void* returnBuffer, void** argPointers) =>
        ffi_call(cif, func, returnBuffer, argPointers);


    // Create call interface for given signature. Returned call interface is allocated in native heap and is expected to be cached for process lifetime.
    public static IntPtr CreateCif(Type? returnType, Type[] parameterTypes, out nuint returnSize, out bool isStructureReturned)
    {
        var paramCount = parameterTypes.Length;
        var returnFfiType = returnType is not null ? FfiTypeDescriptors.Get(returnType) : TypeVoid;
        var paramFfiTypes = (FfiType**)NativeMemory.Alloc((nuint)Math.Max(1, paramCount), (nuint)sizeof(FfiType*));
        for (var i = paramCount - 1; i >= 0; --i)
            paramFfiTypes[i] = FfiTypeDescriptors.Get(parameterTypes[i]);
        var cif = (IntPtr)NativeMemory.AllocZeroed(CifSize);
        var status = ffi_prep_cif(cif, DefaultAbi, (uint)paramCount, returnFfiType, paramFfiTypes);
        if (status != FfiStatus.Ok)
        {
            NativeMemory.Free(paramFfiTypes);
            NativeMemory.Free((void*)cif);
            throw new NotSupportedException($"Unable to prepare libffi call interface, result: {status}.");
        }
        returnSize = returnFfiType->Size;
        isStructureReturned = returnFfiType->Type == StructureTypeCode;
        return cif;
    }


    // Native symbols.
    [DllImport(NativeLibraryNames.Ffi)]
    static extern void ffi_call(IntPtr cif, void* fn, void* rvalue, void** avalue);
    [DllImport(NativeLibraryNames.Ffi)]
    static extern FfiStatus ffi_prep_cif(IntPtr cif, int abi, uint nargs, FfiType* rtype, FfiType** atypes);
}
