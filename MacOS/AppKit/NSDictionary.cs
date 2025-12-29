using CarinaStudio.MacOS.CoreFoundation;
using CarinaStudio.MacOS.ObjectiveC;
using System;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace CarinaStudio.MacOS.AppKit;

/// <summary>
/// NSDictionary.
/// </summary>
public class NSDictionary : NSObject
{
    // Static fields.
    static Selector? AllKeysSelector;
    static Selector? AllValuesSelector;
    static Property? CountProperty;
    static Selector? KeyEnumeratorSelector;
    static readonly Class? NSDictionaryClass = Platform.IsMacOS ? Class.GetClass("NSDictionary") : null;
    static Selector? ObjectEnumeratorSelector;
    static Selector? ObjectForKeySelector;
    
    
    // Constructor.
#pragma warning disable IDE0051
    NSDictionary(IntPtr handle, bool ownsInstance) : base(handle, ownsInstance) =>
        this.VerifyClass(NSDictionaryClass!);
#pragma warning restore IDE0051
    NSDictionary(Class cls, IntPtr handle, bool ownsInstance) : base(cls, handle, ownsInstance)
    { }
    
    
    /// <summary>
    /// A new array containing the dictionary’s keys, or an empty array if the dictionary has no entries.
    /// </summary>
    public NSArray<NSObject> AllKeys
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            AllKeysSelector ??= Selector.FromName("allKeys");
            return this.SendMessage<NSArray<NSObject>>(AllKeysSelector);
        }
    }
    
    
    /// <summary>
    /// A new array containing the dictionary’s values, or an empty array if the dictionary has no entries.
    /// </summary>
    public NSArray<NSObject> AllObjects
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            AllValuesSelector ??= Selector.FromName("allValues");
            return this.SendMessage<NSArray<NSObject>>(AllValuesSelector);
        }
    }


    /// <summary>
    /// The number of entries in the dictionary.
    /// </summary>
    public int Count
    {
#if NET7_0_OR_GREATER
        [RequiresDynamicCode(GetPropertyRdcMessage)]
#endif
        get
        {
            CountProperty ??= NSDictionaryClass!.GetProperty("count").AsNonNull();
            return this.GetProperty<int>(CountProperty);
        }
    }
    
    
    /// <summary>
    /// Returns the value associated with a given key.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public T? GetCFObject<T>(NSObject aKey) where T : CFObject
    {
        ObjectForKeySelector ??= Selector.FromName("objectForKey:");
        return this.SendMessage<T?>(ObjectForKeySelector, aKey);
    }


    /// <summary>
    /// Provides an enumerator to access the keys in the dictionary.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSEnumerator GetKeyEnumerator()
    {
        KeyEnumeratorSelector ??= Selector.FromName("keyEnumerator");
        return this.SendMessage<NSEnumerator>(KeyEnumeratorSelector);
    }
    
    
    /// <summary>
    /// Returns the value associated with a given key.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public T? GetObject<T>(NSObject aKey) where T : NSObject
    {
        ObjectForKeySelector ??= Selector.FromName("objectForKey:");
        return this.SendMessage<T?>(ObjectForKeySelector, aKey);
    }
    
    
    /// <summary>
    /// Returns an enumerator object that lets you access each value in the dictionary.
    /// </summary>
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(CallMethodRdcMessage)]
#endif
    public NSEnumerator GetObjectEnumerator()
    {
        ObjectEnumeratorSelector ??= Selector.FromName("objectEnumerator");
        return this.SendMessage<NSEnumerator>(ObjectEnumeratorSelector);
    }
}