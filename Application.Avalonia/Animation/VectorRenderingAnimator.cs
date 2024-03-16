using Avalonia;
using Avalonia.Controls;

namespace CarinaStudio.Animation;

/// <summary>
/// <see cref="RenderingAnimator"/> to animate <see cref="Vector"/>.
/// </summary>
public class VectorRenderingAnimator : ValueRenderingAnimator<Vector>
{
    /// <summary>
    /// Initialize new <see cref="VectorRenderingAnimator"/> instance.
    /// </summary>
    /// <param name="topLevel"><see cref="TopLevel"/> to provide synchronization with rendering event.</param>
    /// <param name="startValue">Start value.</param>
    /// <param name="endValue">End value.</param>
    public VectorRenderingAnimator(TopLevel topLevel, Vector startValue, Vector endValue) : base(topLevel, startValue, endValue)
    { }
    
    
    /// <inheritdoc/>
    protected override Vector GenerateValue(double progress)
    {
        var offsetX = (this.EndValue.X - this.StartValue.X) * progress;
        var offsetY = (this.EndValue.Y - this.StartValue.Y) * progress;
        return new Vector(this.StartValue.X + offsetX, this.StartValue.Y + offsetY);
    }
}