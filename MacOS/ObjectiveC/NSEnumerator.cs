using System;
namespace CarinaStudio.MacOS.ObjectiveC;

/// <summary>
/// NSEnumerator.
/// </summary>
public class NSEnumerator: NSObject
{
    // Static fields.
    static readonly Property? AllObjsProperty;
    static readonly Selector? NextObjSelector;
    static readonly Class? NSEnumeratorClass;


    // Static initializer.
    static NSEnumerator()
    {
        if (Platform.IsNotMacOS)
            return;
        NSEnumeratorClass = Class.GetClass("NSEnumerator").AsNonNull();
        AllObjsProperty = NSEnumeratorClass.GetProperty("allObjects");
        NextObjSelector = Selector.FromName("nextObject");
    }


    // Constructor.
    NSEnumerator(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSEnumeratorClass!);
    NSEnumerator(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }
    

    /// <summary>
    /// Get array of enumerated objects.
    /// </summary>
    public NSArray<NSObject> AllObjects => this.GetProperty<NSArray<NSObject>>(AllObjsProperty!);


    /// <summary>
    /// Get next object from collection.
    /// </summary>
    /// <returns>Object from collection, or Null if no more object to enumerate.</returns>
    public NSObject? NextObject() =>
        this.SendMessage<NSObject?>(NextObjSelector!);
}