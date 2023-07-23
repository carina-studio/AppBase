using Avalonia;
using Avalonia.Controls;
using System;

namespace CarinaStudio;

/// <summary>
/// Extensions for <see cref="AvaloniaObject"/>.
/// </summary>
public static class AvaloniaObjectExtensions
{
    /// <summary>
    /// Bind property to given resource.
    /// </summary>
    /// <param name="obj">Target object.</param>
    /// <param name="property">Property to bind.</param>
    /// <param name="resourceHost"><see cref="IResourceHost"/> to find resource.</param>
    /// <param name="resourceKey">Resource key.</param>
    /// <returns>Token of binding.</returns>
    public static IDisposable BindToResource(this AvaloniaObject obj, AvaloniaProperty property, IResourceHost resourceHost, object resourceKey) =>
        obj.Bind(property, new CachedResource<object?>(resourceHost, resourceKey));
}