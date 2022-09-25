using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Descriptor of member of class.
    /// </summary>
    public class MemberDescriptor
    {
        // Constructor.
        internal MemberDescriptor(Class cls, IntPtr handle, string name)
        {
            this.Class = cls;
            this.Handle = handle;
            this.Name = name;
        }


        /// <summary>
        /// Get class which owns this member.
        /// </summary>
        public Class Class { get; }


        /// <summary>
        /// Get handle of member.
        /// </summary>
        public IntPtr Handle { get; }


        /// <summary>
        /// Get name of member.
        /// </summary>
        public string Name { get; }


        /// <inheritdoc/>
        public override string ToString() =>
            this.Name;
    }


    /// <summary>
    /// Descriptor of property of class.
    /// </summary>
    public unsafe class PropertyDescriptor : MemberDescriptor
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr property_copyAttributeValue(IntPtr property, string attributeName);


        // Constructor.
        internal PropertyDescriptor(Class cls, IntPtr handle, string name) : base(cls, handle, name)
        {
            this.Getter = property_copyAttributeValue(handle, "G").Let(it =>
            {
                if (it != IntPtr.Zero)
                {
                    var selector = Selector.FromName(new string((sbyte*)it));
                    NativeMemory.Free((void*)it);
                    return selector;
                }
                return Selector.FromUid(name);
            });
            this.IsReadOnly = property_copyAttributeValue(handle, "R").Let(it =>
            {
                if (it != IntPtr.Zero)
                {
                    NativeMemory.Free((void*)it);
                    return true;
                }
                return false;
            });
            this.Setter = this.IsReadOnly ? null : property_copyAttributeValue(handle, "S").Let(it =>
            {
                if (it != IntPtr.Zero)
                {
                    var selector = Selector.FromName(new string((sbyte*)it));
                    NativeMemory.Free((void*)it);
                    return selector;
                }
                return Selector.FromUid(name);
            });
        }
        

        /// <summary>
        /// Get selector of getter of property.
        /// </summary>
        public Selector? Getter { get; }


        /// <summary>
        /// Check whether this is a read-only property or not.
        /// </summary>
        public bool IsReadOnly { get; }


        /// <summary>
        /// Get selector of setter of property.
        /// </summary>
        public Selector? Setter { get; }
    }
}