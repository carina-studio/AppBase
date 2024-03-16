using Avalonia.Controls;

namespace CarinaStudio.Animation;

/// <summary>
/// <see cref="RenderingAnimator"/> to animate <see cref="double"/>.
/// </summary>
public class DoubleRenderingAnimator : ValueRenderingAnimator<double>
{
    /// <summary>
    /// Initialize new <see cref="DoubleRenderingAnimator"/> instance.
    /// </summary>
    /// <param name="topLevel"><see cref="TopLevel"/> to provide synchronization with rendering event.</param>
    /// <param name="startValue">Start value.</param>
    /// <param name="endValue">End value.</param>
    public DoubleRenderingAnimator(TopLevel topLevel, double startValue, double endValue) : base(topLevel, startValue, endValue)
    { }
    
    
    /// <inheritdoc/>
    protected override double GenerateValue(double progress) =>
        this.StartValue + (this.EndValue - this.StartValue) * progress;
}