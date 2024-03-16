using Avalonia;
using Avalonia.Controls;

namespace CarinaStudio.Animation;

/// <summary>
/// <see cref="RenderingAnimator"/> to animate <see cref="Size"/>.
/// </summary>
public class SizeRenderingAnimator : ValueRenderingAnimator<Size>
{
    /// <summary>
    /// Initialize new <see cref="SizeRenderingAnimator"/> instance.
    /// </summary>
    /// <param name="topLevel"><see cref="TopLevel"/> to provide synchronization with rendering event.</param>
    /// <param name="startValue">Start value.</param>
    /// <param name="endValue">End value.</param>
    public SizeRenderingAnimator(TopLevel topLevel, Size startValue, Size endValue) : base(topLevel, startValue, endValue)
    { }
    
    
    /// <inheritdoc/>
    protected override Size GenerateValue(double progress)
    {
        var offsetX = (this.EndValue.Width - this.StartValue.Width) * progress;
        var offsetY = (this.EndValue.Height - this.StartValue.Height) * progress;
        return new Size(this.StartValue.Width + offsetX, this.StartValue.Height + offsetY);
    }
}