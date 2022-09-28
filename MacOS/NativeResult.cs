using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS;

[StructLayout(LayoutKind.Sequential)]
struct NativeResult1
{
    nint Value1; 
}


[StructLayout(LayoutKind.Sequential)]
struct NativeResult2
{
    public unsafe NativeResult2(nint* values)
    {
        this.Value1 = values[0];
        this.Value2 = values[1];
    }

    nint Value1; 
    nint Value2;
}


[StructLayout(LayoutKind.Sequential)]
struct NativeResult3
{
    public unsafe NativeResult3(nint* values)
    {
        this.Value1 = values[0];
        this.Value2 = values[1];
        this.Value3 = values[2];
    }

    nint Value1; 
    nint Value2;
    nint Value3;
}


[StructLayout(LayoutKind.Sequential)]
struct NativeResult4
{
    public unsafe NativeResult4(nint* values)
    {
        this.Value1 = values[0];
        this.Value2 = values[1];
        this.Value3 = values[2];
        this.Value4 = values[3];
    }

    nint Value1; 
    nint Value2;
    nint Value3; 
    nint Value4;
}


[StructLayout(LayoutKind.Sequential)]
struct NativeFpResult1
{
    double Value1;
}


[StructLayout(LayoutKind.Sequential)]
struct NativeFpResult2
{
    public unsafe NativeFpResult2(double* values)
    {
        this.Value1 = values[0];
        this.Value2 = values[1];
    }

    double Value1; 
    double Value2;
}


[StructLayout(LayoutKind.Sequential)]
struct NativeFpResult3
{
    public unsafe NativeFpResult3(double* values)
    {
        this.Value1 = values[0];
        this.Value2 = values[1];
        this.Value3 = values[2];
    }

    double Value1; 
    double Value2;
    double Value3;
}


[StructLayout(LayoutKind.Sequential)]
struct NativeFpResult4
{
    public unsafe NativeFpResult4(double* values)
    {
        this.Value1 = values[0];
        this.Value2 = values[1];
        this.Value3 = values[2];
        this.Value4 = values[3];
    }

    double Value1; 
    double Value2;
    double Value3; 
    double Value4;
}