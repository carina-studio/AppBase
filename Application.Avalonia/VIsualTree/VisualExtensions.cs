using Avalonia;
using Avalonia.VisualTree;
using System;

namespace CarinaStudio.VIsualTree
{
    /// <summary>
    /// Extensions for <see cref="IVisual"/>.
    /// </summary>
    public static class VisualExtensions
    {
        /// <summary>
        /// Find descendant <see cref="IVisual"/> with given type and name.
        /// </summary>
        /// <typeparam name="T">Type of descendant.</typeparam>
        /// <param name="visual">Root <see cref="IVisual"/>.</param>
        /// <param name="name">Name of descendant.</param>
        /// <param name="includeSelf">True to check <paramref name="visual"/> also.</param>
        /// <returns>Found descendant.</returns>
        public static T? FindDescendantOfTypeAndName<T>(this IVisual visual, string name, bool includeSelf = false) where T : class, IVisual, INamed
        {
            if (includeSelf && visual is INamed named && named.Name == name && typeof(T).IsAssignableFrom(visual.GetType()))
                return (T)visual;
            foreach (var childVisual in visual.GetVisualDescendants())
            {
                var descendant = childVisual.FindDescendantOfTypeAndName<T>(name, true);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}
