using Avalonia;
using Avalonia.Input;
using CarinaStudio.VisualTree;

namespace CarinaStudio.Input;

/// <summary>
/// Extension methods for <see cref="IInputElement"/>.
/// </summary>
public static class InputElementExtensions
{
    /// <summary>
    /// Find the topmost active <see cref="IInputElement"/> with specific type at a point on an <see cref="IInputElement"/>. 
    /// </summary>
    /// <param name="inputElement"><see cref="IInputElement"/>.</param>
    /// <param name="point">Point.</param>
    /// <typeparam name="T">Type of <see cref="IInputElement"/>.</typeparam>
    /// <returns><see cref="IInputElement"/> at given point, or Null if not found.</returns>
    public static T? InputHitTest<T>(this IInputElement inputElement, Point point) where T : class, IInputElement
    {
        var hitElement = inputElement.InputHitTest(point);
        if (hitElement is T targetElement)
            return targetElement;
        if (hitElement is not Visual hitVisual)
            return null;
        return hitVisual.FindAncestor((visual, ref interrupted) =>
        {
            if (visual is T)
                return true;
            if (ReferenceEquals(visual, inputElement))
                interrupted = true;
            return false;
        }) as T;
    }
}