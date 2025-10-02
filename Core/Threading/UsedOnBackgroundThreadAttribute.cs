using System;

namespace CarinaStudio.Threading;

/// <summary>
/// Indicates that the field or property will/should be used on a thread which is different from main/primary thread.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UsedOnBackgroundThreadAttribute : Attribute
{
    /// <summary>
    /// Initialize new <see cref="UsedOnBackgroundThreadAttribute"/> instance.
    /// </summary>
    public UsedOnBackgroundThreadAttribute() { }
    
    
    /// <summary>
    /// Initialize new <see cref="UsedOnBackgroundThreadAttribute"/> instance.
    /// </summary>
    /// <param name="threadNames">List of names of threads which will/should use the field or property.</param>
    public UsedOnBackgroundThreadAttribute(params string[] threadNames) { }
}