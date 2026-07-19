namespace CarinaStudio.MacOS.Ffi;

// Result of preparing libffi call interface.
enum FfiStatus
{
    BadAbi = 2,
    BadArgType = 3,
    BadTypedef = 1,
    Ok = 0,
}
