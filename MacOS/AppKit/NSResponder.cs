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
        /// <param name="instance">Instance.</param>
        protected NSResponder(InstanceHolder instance) : base(instance, false) =>
            this.VerifyClass(NSResponderClass!);
    }
}