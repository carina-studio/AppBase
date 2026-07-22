using System;

namespace CarinaStudio.MacOS.ObjectiveC;

/// <summary>
/// NSNotificationCenter.
/// </summary>
public class NSNotificationCenter : NSObject
{
    // Static fields.
    static Selector? AddObserverSelector;
    static NSNotificationCenter? DefaultCenter;
    static Selector? DefaultCenterSelector;
    static readonly Class? NSNotificationCenterClass;
    static Selector? RemoveObserverSelector;
    static Selector? RemoveObserverWithNameSelector;


    // Static initializer.
    static NSNotificationCenter()
    {
        if (Platform.IsNotMacOS)
            return;
        NSNotificationCenterClass = Class.GetClass("NSNotificationCenter").AsNonNull();
    }


    /// <summary>
    /// Initialize new <see cref="NSNotificationCenter"/> instance.
    /// </summary>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="verifyClass">True to verify whether instance is NSNotificationCenter or not.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSNotificationCenter(IntPtr handle, bool verifyClass, bool ownsInstance) : base(handle, ownsInstance)
    {
        if (verifyClass)
            this.VerifyClass(NSNotificationCenterClass!);
    }


    /// <summary>
    /// Initialize new <see cref="NSNotificationCenter"/> instance.
    /// </summary>
    /// <param name="cls">Class of instance.</param>
    /// <param name="handle">Handle of instance.</param>
    /// <param name="ownsInstance">True to owns the instance.</param>
    protected NSNotificationCenter(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }


    // Constructor.
    NSNotificationCenter(IntPtr handle, bool ownsInstance) : this(handle, true, ownsInstance)
    { }


    /// <summary>
    /// Add an observer to receive notifications.
    /// </summary>
    /// <param name="observer">Object to receive notifications.</param>
    /// <param name="selector">Selector of method of observer to be called when receiving notification.</param>
    /// <param name="name">Name of notifications to receive, or Null to receive notifications with any name.</param>
    /// <param name="obj">Object whose notifications to receive, or Null to receive notifications from any object.</param>
    public void AddObserver(NSObject observer, Selector selector, NSString? name, NSObject? obj)
    {
        AddObserverSelector ??= Selector.FromName("addObserver:selector:name:object:");
        this.SendMessage(AddObserverSelector, observer, selector, name, obj);
    }


    /// <summary>
    /// Get the default notification center.
    /// </summary>
    public static NSNotificationCenter Default
    {
        get
        {
            if (DefaultCenter is null)
            {
                DefaultCenterSelector ??= Selector.FromName("defaultCenter");
                DefaultCenter = SendMessage<NSNotificationCenter>(NSNotificationCenterClass!.Handle, DefaultCenterSelector);
            }
            return DefaultCenter;
        }
    }


    /// <summary>
    /// Remove an observer from receiving all notifications.
    /// </summary>
    /// <param name="observer">Observer added by <see cref="AddObserver"/>.</param>
    public void RemoveObserver(NSObject observer)
    {
        RemoveObserverSelector ??= Selector.FromName("removeObserver:");
        this.SendMessage(RemoveObserverSelector, observer);
    }


    /// <summary>
    /// Remove an observer from receiving notifications with given name.
    /// </summary>
    /// <param name="observer">Observer added by <see cref="AddObserver"/>.</param>
    /// <param name="name">Name of notifications to stop receiving, or Null to stop receiving notifications with any name.</param>
    /// <param name="obj">Object whose notifications to stop receiving, or Null to stop receiving notifications from any object.</param>
    public void RemoveObserver(NSObject observer, NSString? name, NSObject? obj)
    {
        RemoveObserverWithNameSelector ??= Selector.FromName("removeObserver:name:object:");
        this.SendMessage(RemoveObserverWithNameSelector, observer, name, obj);
    }
}
