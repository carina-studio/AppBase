using System;

namespace CarinaStudio.MacOS.ObjectiveC
{
    /// <summary>
    /// Descriptor of member of class.
    /// </summary>
    public class MemberDescriptor
    {
        // Constructor.
        internal MemberDescriptor(IntPtr handle, string name)
        {
            this.Handle = handle;
            this.Name = name;
        }


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
    public class PropertyDescriptor : MemberDescriptor
    {
        // Constructor.
        internal PropertyDescriptor(IntPtr handle, string name, Selector? getter, Selector? setter) : base(handle, name)
        {
            this.Getter = getter;
            this.Setter = setter;
        }
        

        /// <summary>
        /// Get selector of getter of property.
        /// </summary>
        public Selector? Getter { get; }


        /// <summary>
        /// Get selector of setter of property.
        /// </summary>
        public Selector? Setter { get; }
    }
}