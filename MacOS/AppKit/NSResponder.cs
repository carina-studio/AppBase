using System;
using CarinaStudio.MacOS.ObjectiveC;

namespace CarinaStudio.MacOS.AppKit
{
    /// <summary>
    /// NSResponder.
    /// </summary>
    public class NSResponder : NSObject
    {
        // Static fields.
        static readonly Class? NSResponderClass;


        // Static initializer.
        static NSResponder()
        {
            if (Platform.IsNotMacOS)
                return;
            NSResponderClass = Class.GetClass("NSResponder");
        }


        /// <summary>
        /// Initialize new <see cref="NSResponder"/> instance.
        /// </summary>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="verifyClass">True to verify whether instance is NSResponder or not.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        protected NSResponder(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, ownsInstance)
        {
            if (verifyClass)
                this.VerifyClass(NSResponderClass!);
        }
        

        /// <summary>
        /// Initialize new <see cref="NSResponder"/> instance.
        /// </summary>
        /// <param name="cls">Class of instance.</param>
        /// <param name="handle">Handle of instance.</param>
        /// <param name="ownsInstance">True to own the instance.</param>
        protected NSResponder(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
        { }


        // Constructor.
        NSResponder(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
        { }
    }
}