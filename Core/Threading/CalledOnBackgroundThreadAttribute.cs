using System;

namespace CarinaStudio.Threading;

/// <summary>
/// Indicates that the method will/should be called on a thread which is different from main/primary thread.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CalledOnBackgroundThreadAttribute : Attribute
{
    /// <summary>
    /// Initialize new <see cref="CalledOnBackgroundThreadAttribute"/> instance.
    /// </summary>
    public CalledOnBackgroundThreadAttribute() { }
    
    
    /// <summary>
    /// Initialize new <see cref="CalledOnBackgroundThreadAttribute"/> instance.
    /// </summary>
    /// <param name="threadNames">List of names of threads which will/should call the method.</param>
    public CalledOnBackgroundThreadAttribute(params string[] threadNames) { }
}