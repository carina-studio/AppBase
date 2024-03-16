using Avalonia;
using Avalonia.Controls;

namespace CarinaStudio.Animation;

/// <summary>
/// <see cref="RenderingAnimator"/> to animate <see cref="Thickness"/>.
/// </summary>
public class ThicknessRenderingAnimator : ValueRenderingAnimator<Thickness>
{
    /// <summary>
    /// Initialize new <see cref="ThicknessRenderingAnimator"/> instance.
    /// </summary>
    /// <param name="topLevel"><see cref="TopLevel"/> to provide synchronization with rendering event.</param>
    /// <param name="startValue">Start value.</param>
    /// <param name="endValue">End value.</param>
    public ThicknessRenderingAnimator(TopLevel topLevel, Thickness startValue, Thickness endValue) : base(topLevel, startValue, endValue)
    { }
    
    
    /// <inheritdoc/>
    protected override Thickness GenerateValue(double progress)
    {
        var start = this.StartValue;
        var end = this.EndValue;
        var offsetLeft = (end.Left - start.Left) * progress;
        var offsetTop = (end.Top - start.Top) * progress;
        var offsetRight = (end.Right - start.Right) * progress;
        var offsetBottom = (end.Bottom - start.Bottom) * progress;
        return new Thickness(start.Left + offsetLeft, start.Top + offsetTop, start.Right + offsetRight, start.Bottom + offsetBottom);
    }
}