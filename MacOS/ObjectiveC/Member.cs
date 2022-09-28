using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Represent member of class.
    /// </summary>
    public class Member
    {
        // Constructor.
        internal Member(Class cls, IntPtr handle, string name)
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
    /// Represent method of class.
    /// </summary>
    public unsafe class Method : Member
    {
        // Constructor.
        internal Method(Class cls, IntPtr handle, Selector selector) : base(cls, handle, selector.Name)
        {
            this.Selector = selector;
        }


        /// <summary>
        /// Get selector of this method.
        /// </summary>
        public Selector Selector { get; }
    }


    /// <summary>
    /// Represent property of class.
    /// </summary>
    public unsafe class Property : Member
    {
        // Native symbols.
        [DllImport(NativeLibraryNames.ObjectiveC)]
        static extern IntPtr property_copyAttributeValue(IntPtr property, string attributeName);


        // Constructor.
        internal Property(Class cls, IntPtr handle, string name) : base(cls, handle, name)
        {
            this.Getter = property_copyAttributeValue(handle, "G").Let(it =>
            {
                if (it != IntPtr.Zero)
                {
                    var selector = Selector.FromUid(new string((sbyte*)it));
                    NativeMemory.Free((void*)it);
                    return selector;
                }
                return Selector.FromName(name);
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
                    var selector = Selector.FromUid(new string((sbyte*)it));
                    NativeMemory.Free((void*)it);
                    return selector;
                }
                return Selector.FromName($"set{char.ToUpper(name[0])}{name.Substring(1)}:");
            });
        }
        

        /// <summary>
        /// Get getter of property.
        /// </summary>
        public Selector? Getter { get; }


        /// <summary>
        /// Check whether this is a read-only property or not.
        /// </summary>
        public bool IsReadOnly { get; }


        /// <summary>
        /// Get setter of property.
        /// </summary>
        public Selector? Setter { get; }
    }
}