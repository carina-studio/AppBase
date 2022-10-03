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
        /// <param name="ownsInstance">True to own the instance.</param>
        protected NSResponder(InstanceHolder instance, bool ownsInstance) : base(instance, ownsInstance) =>
            this.VerifyClass(NSResponderClass!);
    }
}