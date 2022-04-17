using System;

namespace CarinaStudio
{
    /// <summary>
    /// Action performed on pointer to <see cref="byte"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="byte"/>.</param>
    public unsafe delegate void BytePointerAction(byte* pointer);


    /// <summary>
    /// Action performed on pointers to <see cref="byte"/>.
    /// </summary>
    /// <param name="pointer1">1st pointer to <see cref="byte"/>.</param>
    /// <param name="pointer2">2nd pointer to <see cref="byte"/>.</param>
    public unsafe delegate void BytePointersAction(byte* pointer1, byte* pointer2);


    /// <summary>
    /// Function performed on pointer to <see cref="byte"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="byte"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R BytePointerFunc<R>(byte* pointer);


    /// <summary>
    /// Function performed on pointers to <see cref="byte"/> and generate a value.
    /// </summary>
    /// <param name="pointer1">1st pointer to <see cref="byte"/>.</param>
    /// <param name="pointer2">2nd pointer to <see cref="byte"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R BytePointersFunc<R>(byte* pointer1, byte* pointer2);


    /// <summary>
    /// Action performed on pointer to <see cref="char"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="char"/>.</param>
    public unsafe delegate void CharPointerAction(char* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="char"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="char"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R CharPointerFunc<R>(char* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="double"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="double"/>.</param>
    public unsafe delegate void DoublePointerAction(double* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="double"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="double"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R DoublePointerFunc<R>(double* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="short"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="short"/>.</param>
    public unsafe delegate void Int16PointerAction(short* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="short"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="short"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R Int16PointerFunc<R>(short* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="int"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="int"/>.</param>
    public unsafe delegate void Int32PointerAction(int* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="int"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="int"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R Int32PointerFunc<R>(int* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="long"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="long"/>.</param>
    public unsafe delegate void Int64PointerAction(long* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="long"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="long"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R Int64PointerFunc<R>(long* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="sbyte"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="sbyte"/>.</param>
    public unsafe delegate void SBytePointerAction(sbyte* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="sbyte"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="sbyte"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R SBytePointerFunc<R>(sbyte* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="float"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="float"/>.</param>
    public unsafe delegate void SinglePointerAction(float* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="float"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="float"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R SinglePointerFunc<R>(float* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="ushort"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="ushort"/>.</param>
    public unsafe delegate void UInt16PointerAction(ushort* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="ushort"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="ushort"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R UInt16PointerFunc<R>(ushort* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="uint"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="uint"/>.</param>
    public unsafe delegate void UInt32PointerAction(uint* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="uint"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="uint"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R UInt32PointerFunc<R>(uint* pointer);


    /// <summary>
    /// Action performed on pointer to <see cref="ulong"/>.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="ulong"/>.</param>
    public unsafe delegate void UInt64PointerAction(ulong* pointer);


    /// <summary>
    /// Function performed on pointer to <see cref="ulong"/> and generate a value.
    /// </summary>
    /// <param name="pointer">Pointer to <see cref="ulong"/>.</param>
    /// <returns>Generated value.</returns>
    public unsafe delegate R UInt64PointerFunc<R>(ulong* pointer);
}