using Avalonia.Controls;
using System;

namespace CarinaStudio
{
    /// <summary>
    /// <see cref="IApplication"/> which based-on Avalonia.
    /// </summary>
    public interface IAvaloniaApplication : IApplication, IResourceNode, IResourceHost
    { }
}
