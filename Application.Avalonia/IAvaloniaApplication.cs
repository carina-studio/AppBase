using Avalonia.Controls;
#if AVALONIA_11_0_0_P4
using Avalonia.Input.Platform;
#endif

namespace CarinaStudio
{
    /// <summary>
    /// <see cref="IApplication"/> which based-on Avalonia.
    /// </summary>
    public interface IAvaloniaApplication : IApplication, IResourceNode, IResourceHost
    { 
#if AVALONIA_11_0_0_P4
        /// <summary>
        /// Get clipboard.
        /// </summary>
        IClipboard? Clipboard { get; }
#endif
    }
}
