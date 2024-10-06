using Avalonia;
using Avalonia.VisualTree;
using System;

namespace CarinaStudio.VisualTree;

/// <summary>
/// Extensions for <see cref="Visual"/>.
/// </summary>
public static class VisualExtensions
{
    /// <summary>
    /// Find the ancestor which matches the condition.
    /// </summary>
    /// <param name="visual"><see cref="Visual"/>.</param>
    /// <param name="predicate">Function to check whether the visual matches the condition or not.</param>
    /// <param name="includeSelf">True to include <paramref name="visual"/> to check.</param>
    /// <returns><see cref="Visual"/> which matches the condition, or Null if not found.</returns>
    public static Visual? FindAncestor(this Visual visual, Predicate<Visual> predicate, bool includeSelf = false)
    {
        if (includeSelf && predicate(visual))
            return visual;
        var parent = visual.GetVisualParent();
        while (parent is not null)
        {
            if (predicate(parent))
                return parent;
            parent = parent.GetVisualParent();
        }
        return null;
    }
    
    
    /// <summary>
    /// Find the ancestor which matches the condition.
    /// </summary>
    /// <param name="visual"><see cref="Visual"/>.</param>
    /// <param name="predicate">Function to check whether the visual matches the condition or not.</param>
    /// <param name="includeSelf">True to include <paramref name="visual"/> to check.</param>
    /// <returns><see cref="Visual"/> which matches the condition, or Null if not found.</returns>
    public static Visual? FindAncestor(this Visual visual, InterruptiblePredicate<Visual> predicate, bool includeSelf = false)
    {
        var interrupted = false;
        if (includeSelf)
        {
            if (predicate(visual, ref interrupted))
                return interrupted ? null : visual;
            if (interrupted)
                return null;
        }
        var parent = visual.GetVisualParent();
        while (parent is not null)
        {
            if (predicate(parent, ref interrupted))
                return interrupted ? null : parent;
            if (interrupted)
                return null;
            parent = parent.GetVisualParent();
        }
        return null;
    }
    
    
    /// <summary>
    /// Find the descendent which matches the condition.
    /// </summary>
    /// <param name="visual"><see cref="Visual"/>.</param>
    /// <param name="predicate">Function to check whether the visual matches the condition or not.</param>
    /// <param name="includeSelf">True to include <paramref name="visual"/> to check.</param>
    /// <returns><see cref="Visual"/> which matches the condition, or Null if not found.</returns>
    public static Visual? FindDescendant(this Visual visual, Predicate<Visual> predicate, bool includeSelf = false)
    {
        if (includeSelf && predicate(visual))
            return visual;
        foreach (var childVisual in visual.GetVisualDescendants())
        {
            if (predicate(childVisual))
                return childVisual;
            var descendant = childVisual.FindDescendant(predicate);
            if (descendant is not null)
                return descendant;
        }
        return null;
    }
    
    
    /// <summary>
    /// Find descendant <see cref="Visual"/> with given type and name.
    /// </summary>
    /// <typeparam name="T">Type of descendant.</typeparam>
    /// <param name="visual">Root <see cref="Visual"/>.</param>
    /// <param name="name">Name of descendant.</param>
    /// <param name="includeSelf">True to check <paramref name="visual"/> also.</param>
    /// <returns>Found descendant.</returns>
    public static T? FindDescendantOfTypeAndName<T>(this Visual visual, string name, bool includeSelf = false) where T : Visual, INamed
    {
        if (includeSelf && visual is INamed named && named.Name == name && visual is T visualT)
            return visualT;
        foreach (var childVisual in visual.GetVisualDescendants())
        {
            var descendant = childVisual.FindDescendantOfTypeAndName<T>(name, true);
            if (descendant is not null)
                return descendant;
        }
        return null;
    }
}
