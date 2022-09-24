using System;
using System.Runtime.InteropServices; 

namespace CarinaStudio.MacOS
{
    internal unsafe class Program
    {
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyIvarList(IntPtr cls, out int outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr* class_copyPropertyList(IntPtr cls, out int outCount);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr class_getMethodImplementation(IntPtr cls, IntPtr name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern sbyte* ivar_getName(IntPtr v);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr objc_getProtocol(string name);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern sbyte* property_getAttributes(IntPtr property);
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern sbyte* property_getName(IntPtr property);

        [DllImport(NativeLibraryNames.ObjectiveC, EntryPoint = ObjectiveC.NSObject.SendMessageEntryPointName)]
        static extern int SendMessageInt32(IntPtr target, IntPtr selector);


        delegate IntPtr AllocFunc(IntPtr cls);


        static void Main(string[] args)
        {
            using var s1 = new ObjectiveC.NSString("Hello World");
            using var s2 = new ObjectiveC.NSString("Hello World");
            using var s3 = new ObjectiveC.NSString("12345");

            var result = s1.CompareTo(s2);
            result = s1.CompareTo(s3);
            result = s3.CompareTo(s1);

            /*
            while (cls != null)
            {
                var pList = class_copyPropertyList(ObjectiveC.Class.GetClass("NSObject")!.Handle, out var pCount);

                for (var i = 0; i < pCount; ++i)
                {
                    var name = new string(property_getName(pList[i]));
                    var attrs = new string(property_getAttributes(pList[i]));

                    //System.Diagnostics.Debug.WriteLine($"{cls.Name}.{name}: {attrs}");
                }

                NativeMemory.Free(pList);

                cls = cls.SuperClass;
            }*/
        }
    }
}