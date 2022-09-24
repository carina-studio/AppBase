using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;

namespace CarinaStudio
{
    /// <summary>
    /// <see cref="IApplication"/> which based-on Avalonia.
    /// </summary>
    public interface IAvaloniaApplication : IApplication, IResourceNode, IResourceHost
    { 
        /// <summary>
        /// Get clipboard.
        /// </summary>
        IClipboard? Clipboard { get; }
    }
}
