using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Tests of <see cref="Class"/> and its members.
    /// </summary>
    [TestFixture]
    public unsafe class ClassTests
    {
        // Structure for testing.
        [StructLayout(LayoutKind.Sequential)]
        public struct TestStructure : IEquatable<TestStructure>
        {
            public byte ByteField;
            public int IntField;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public ushort[] CharArrayField;
            public nint IntPtrField;
            public double DoubleField;

            public bool Equals(TestStructure ts) =>
                this.ByteField == ts.ByteField
                && this.IntField == ts.IntField
                && this.CharArrayField.SequenceEqual(ts.CharArrayField)
                && this.IntField == ts.IntField
                && this.DoubleField.Equals(ts.DoubleField);

            public override bool Equals([NotNullWhen(true)] object? obj) =>
                obj is TestStructure ts && this.Equals(ts);

            public override int GetHashCode() =>
                0;
        }


        /// <summary>
        /// Test for instance variables of class.
        /// </summary>
        [Test]
        public void InstanceVariableTest()
        {
            // prepare
            var varInfoList = new (/* name */ string, /* native type */ Type, object, int, string)[] {
                ("_boolVar", typeof(bool), true, sizeof(bool), "B"),
                ("_byteVar", typeof(byte), byte.MaxValue, sizeof(byte), "C"),
                ("_sbyteVar", typeof(sbyte), sbyte.MinValue, sizeof(sbyte), "c"),
                ("_shortVar", typeof(short), short.MinValue, sizeof(short), "s"),
                ("_ushortVar", typeof(ushort), ushort.MaxValue, sizeof(ushort), "S"),
                ("_charVar", typeof(char), char.MaxValue, sizeof(ushort), "S"),
                ("_intVar", typeof(int), int.MinValue, sizeof(int), "i"),
                ("_uintVar", typeof(uint), uint.MaxValue, sizeof(uint), "I"),
                ("_longVar", typeof(long), long.MinValue, sizeof(long), "q"),
                ("_ulongVar", typeof(ulong), ulong.MaxValue, sizeof(ulong), "Q"),
                ("_floatVar", typeof(float), float.PositiveInfinity, sizeof(float), "f"),
                ("_doubleVar", typeof(double), double.NegativeInfinity, sizeof(double), "d"),
                ("_nintVar", typeof(nint), (nint)12345, sizeof(nint), "^v"),
                ("_nuintVar", typeof(nuint), (nuint)67890, sizeof(nuint), "^v"),
                ("_structVar", typeof(TestStructure), new TestStructure
                    {
                        ByteField = 127,
                        IntField = int.MinValue,
                        CharArrayField = new ushort[]{ 'H', 'e', 'l', 'l', 'o' },
                        IntPtrField = nint.MaxValue,
                        DoubleField = double.NaN,
                    }, 
                    Marshal.SizeOf<TestStructure>(), "{TestStructure=Ci[5S]^vd}"),
                ("_nsObjVar", typeof(NSObject), new NSString("Hello World"), sizeof(nint), "@"),
                ("_clsVar", typeof(Class), Class.GetClass("NSString").AsNonNull(), sizeof(nint), "#"),
                ("_selVar", typeof(Selector), Selector.FromName("hash"), sizeof(nint), ":"),

                ("_boolArrayVar", typeof(bool[]), new bool[] { true, false, true, }, 3 * sizeof(bool), "[3B]"),
                ("_byteArrayVar", typeof(byte[]), new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 8 * sizeof(byte), "[8C]"),
                ("_intArrayVar", typeof(int[]), new int[] { int.MinValue, int.MaxValue }, 2 * sizeof(int), "[2i]"),
                ("_nsObjArrayVar", typeof(NSObject[]), new NSString[] { new("Hello"), new("World") }, 2 * sizeof(nint), "[2@]"),
            };
            var invalidVarInfoList = new (/* name */ string, /* native type */ Type, object, int, string)[] {
                ("_objVar", typeof(object), new object(), sizeof(nint), "^v"),
                ("_objArrayVar", typeof(nint[]), new object[] { "Hello", "World", "!", new object() }, 4 * Marshal.SizeOf(typeof(nint)), "[4^v]"),
            };

            // define class
            var cls = Class.DefineClass("Test_ClassTests_InstanceVariableTest", cls =>
            {
                foreach (var varInfo in varInfoList)
                {
                    var array = varInfo.Item3 as Array;
                    cls.DefineInstanceVariable(varInfo.Item1, varInfo.Item3.GetType(), array?.Length ?? 1);
                }
                foreach (var varInfo in invalidVarInfoList)
                {
                    var array = varInfo.Item3 as Array;
                    try
                    {
                        cls.DefineInstanceVariable(varInfo.Item1, varInfo.Item3.GetType(), array?.Length ?? 1);
                        Assert.Fail($"Should not support defining variable with {varInfo.Item2.Name}.");
                    }
                    catch
                    { }
                }
            });

            // verify variables
            var instance = NSObject.Initialize(cls.Allocate());
            foreach (var varInfo in varInfoList)
            {
                var ivar = cls.GetInstanceVariable(varInfo.Item1);
                Assert.IsNotNull(ivar);
                
                // check name
                Assert.AreEqual(varInfo.Item1, ivar!.Name);

                // check value type
                if (varInfo.Item2 == typeof(char))
                    Assert.AreEqual(typeof(ushort), ivar.Type);
                else if (varInfo.Item2 == typeof(nuint))
                    Assert.AreEqual(typeof(nint), ivar.Type);
                else if (varInfo.Item2 == typeof(TestStructure))
                    Assert.AreEqual(typeof(byte[]), ivar.Type);
                else
                    Assert.AreEqual(varInfo.Item2, ivar.Type);
                
                // check data size
                if (varInfo.Item2 == typeof(TestStructure))
                {
                    Assert.AreEqual(Marshal.SizeOf<TestStructure>(), ivar.Size);
                    Assert.AreEqual(Marshal.SizeOf<TestStructure>(), ivar.ElementCount);
                }
                else
                    Assert.AreEqual(varInfo.Item4, ivar.Size);

                // check type encoding
                Assert.AreEqual(varInfo.Item5, ivar.TypeEncoding);

                // check value after setting and getting
                NSObject.SetVariable(instance, ivar, varInfo.Item3);
                Assert.AreEqual(varInfo.Item3, NSObject.GetVariable(instance, ivar, varInfo.Item3.GetType()));
            }

            // complete
            NSObject.Release(instance);
        }


        /// <summary>
        /// Test for method calling and handling.
        /// </summary>
        [Test]
        public void MethodTest()
        {
            // prepare
            var ts = new TestStructure
            {
                ByteField = 127,
                IntField = int.MinValue,
                CharArrayField = new ushort[]{ 'H', 'e', 'l', 'l', 'o' },
                IntPtrField = nint.MaxValue,
                DoubleField = double.NaN,
            };
            var methodInfoList = new (Selector, Delegate, Type?, object?)[] {
                (Selector.FromName("boolMethod"), new Func<IntPtr, Selector, bool>((self, cmd) => true), typeof(bool), null),
                (Selector.FromName("boolMethod1"), new Func<IntPtr, Selector, bool, bool>((self, cmd, arg) => arg), typeof(bool), true),
                (Selector.FromName("byteMethod"), new Func<IntPtr, Selector, byte>((self, cmd) => byte.MaxValue), typeof(byte), null),
                (Selector.FromName("byteMethod1"), new Func<IntPtr, Selector, byte, byte>((self, cmd, arg) => arg), typeof(byte), byte.MaxValue),
                (Selector.FromName("sbyteMethod"), new Func<IntPtr, Selector, sbyte>((self, cmd) => sbyte.MinValue), typeof(sbyte), null),
                (Selector.FromName("sbyteMethod1"), new Func<IntPtr, Selector, sbyte, sbyte>((self, cmd, arg) => arg), typeof(sbyte), sbyte.MinValue),
                (Selector.FromName("intMethod"), new Func<IntPtr, Selector, int>((self, cmd) => int.MinValue), typeof(int), null),
                (Selector.FromName("intMethod1"), new Func<IntPtr, Selector, int, int>((self, cmd, arg) => arg), typeof(int), int.MinValue),
                (Selector.FromName("uintMethod"), new Func<IntPtr, Selector, uint>((self, cmd) => uint.MaxValue), typeof(uint), null),
                (Selector.FromName("uintMethod1"), new Func<IntPtr, Selector, uint, uint>((self, cmd, arg) => arg), typeof(uint), uint.MaxValue),
                (Selector.FromName("tsMethod"), new Func<IntPtr, Selector, TestStructure>((self, cmd) => ts), typeof(TestStructure), null),
                (Selector.FromName("tsMethod1"), new Func<IntPtr, Selector, TestStructure, TestStructure>((self, cmd, arg) => arg), typeof(TestStructure), ts),
                (Selector.FromName("nsObjMethod"), new Func<IntPtr, Selector, NSString>((self, cmd) => new NSString("Foo")), typeof(NSString), null),
                (Selector.FromName("nsObjMethod1"), new Func<IntPtr, Selector, NSString, NSString>((self, cmd, arg) => arg), typeof(NSString), new NSString("Bar")),
            };

            // define class
            var verifySelfAndCmdSelector = Selector.FromName("verifySelfAndCmd");
            var cls = Class.DefineClass("Test_ClassTests_MethodTest", cls =>
            {
                // define method to verify self and selector
                cls.DefineMethod<IntPtr>(verifySelfAndCmdSelector, (self, sel, expectedSelf) =>
                {
                    Assert.AreEqual(expectedSelf, self);
                    Assert.AreEqual(verifySelfAndCmdSelector, sel);
                });

                // define method returning value without parameter
                var funcType = typeof(Func<object>).GetGenericTypeDefinition();
                foreach (var methodInfo in methodInfoList)
                    cls.DefineMethod(methodInfo.Item1, methodInfo.Item2);
            });

            // verify
            var instance = NSObject.Initialize(cls.Allocate());
            NSObject.SendMessageCore(instance, verifySelfAndCmdSelector, null, instance);
            foreach (var methodInfo in methodInfoList)
            {
                var expectedReturnValue = methodInfo.Item4 == null 
                    ? methodInfo.Item2.DynamicInvoke(instance, methodInfo.Item1)
                    : methodInfo.Item2.DynamicInvoke(instance, methodInfo.Item1, methodInfo.Item4);
                var returnValue = methodInfo.Item4 == null
                    ? NSObject.SendMessageCore(instance, methodInfo.Item1, methodInfo.Item3)
                    : NSObject.SendMessageCore(instance, methodInfo.Item1, methodInfo.Item3, methodInfo.Item4);
                Assert.AreEqual(expectedReturnValue, returnValue);
            }

            // complete
            NSObject.Release(instance);
        }
    }
}