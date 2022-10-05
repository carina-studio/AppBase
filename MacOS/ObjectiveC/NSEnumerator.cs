namespace CarinaStudio.MacOS.ObjectiveC;

/// <summary>
/// NSEnumerator.
/// </summary>
public class NSEnumerator : NSObject
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
    NSEnumerator(InstanceHolder instance, bool ownsInstance) : this(instance, true, ownsInstance)
    { }
    internal NSEnumerator(InstanceHolder instance, bool checkType, bool ownsInstance) : base(instance, ownsInstance) =>
        this.VerifyClass(NSEnumeratorClass!);
    

    /// <summary>
    /// Get array of unenumerated objects.
    /// </summary>
    public NSArray<NSObject> AllObjects { get => this.GetProperty<NSArray<NSObject>>(AllObjsProperty!); }


    /// <summary>
    /// Get next object from collection.
    /// </summary>
    /// <returns>Object from collection, or Null if no more object to enumerate.</returns>
    public NSObject? NextObject() =>
        this.SendMessage<NSObject>(NextObjSelector!);
}