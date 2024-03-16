using Avalonia.Controls;
using System;

namespace CarinaStudio.Animation;

/// <summary>
/// <see cref="RenderingAnimator"/> which animates specific type of value.
/// </summary>
/// <typeparam name="T">Type of value to animate.</typeparam>
public abstract class ValueRenderingAnimator<T> : RenderingAnimator
{
    // Fields.
    T endValue;
    T startValue;
    
    
    /// <summary>
    /// Initialize new <see cref="ValueRenderingAnimator{T}"/> instance.
    /// </summary>
    /// <param name="topLevel"><see cref="TopLevel"/> to provide synchronization with rendering event.</param>
    /// <param name="startValue">Start value.</param>
    /// <param name="endValue">End value.</param>
    protected ValueRenderingAnimator(TopLevel topLevel, T startValue, T endValue) : base(topLevel)
    {
        this.endValue = endValue;
        this.startValue = startValue;
        this.Value = this.GenerateValue(0);
    }
    
    
    /// <summary>
    /// Get or set value when animation completed.
    /// </summary>
    public T EndValue
    {
        get => this.endValue;
        set
        {
            this.VerifyAccess();
            this.endValue = value;
            if (this.IsStarted)
                this.OnProgressChanged(EventArgs.Empty);
        }
    }
    
    
    /// <summary>
    /// Generate value by progress of animation.
    /// </summary>
    /// <param name="progress">Progress.</param>
    /// <returns>Value.</returns>
    protected abstract T GenerateValue(double progress);
    
    
    /// <inheritdoc/>
    protected override void OnCancelled(EventArgs e)
    {
        this.Value = this.GenerateValue(this.Progress);
        base.OnCancelled(e);
    }
    
    
    /// <inheritdoc/>
    protected override void OnCompleted(EventArgs e)
    {
        this.Value = this.endValue;
        base.OnCompleted(e);
    }


    /// <inheritdoc/>
    protected override void OnProgressChanged(EventArgs e)
    {
        this.Value = this.GenerateValue(this.Progress);
        base.OnProgressChanged(e);
    }
    
    
    /// <summary>
    /// Get or set value when starting animation.
    /// </summary>
    public T StartValue
    {
        get => this.startValue;
        set
        {
            this.VerifyAccess();
            this.startValue = value;
            if (this.IsStarted)
                this.OnProgressChanged(EventArgs.Empty);
            else
                this.Value = this.GenerateValue(0);
        }
    }


    /// <summary>
    /// Get current value when animating.
    /// </summary>
    public T Value { get; private set; }
}