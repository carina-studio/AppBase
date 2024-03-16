using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Animation;

/// <summary>
/// General purpose animator which is synchronized with rendering event.
/// </summary>
public class RenderingAnimator
{
    // Static fields.
    static readonly Stopwatch stopwatch = new();
    
    
    // Fields.
    Action<TimeSpan>? animationCallback;
    TimeSpan delay;
    TimeSpan duration;
    long startTime = -1;


    /// <summary>
    /// Initialize new <see cref="RenderingAnimator"/> instance.
    /// </summary>
    /// <param name="topLevel"><see cref="TopLevel"/> to provide synchronization with rendering event.</param>
    public RenderingAnimator(TopLevel topLevel)
    {
        this.TopLevel = topLevel;
    }
    
    
    /// <summary>
    /// Cancel current animation.
    /// </summary>
    public void Cancel()
    {
        this.VerifyAccess();
        if (this.startTime >= 0)
        {
            this.startTime = -1;
            this.Progress = 0;
            this.OnCancelled(EventArgs.Empty);
        }
    }
    
    
    /// <summary>
    /// Raised when current animation has been cancelled.
    /// </summary>
    public event EventHandler? Cancelled;
    
    
    /// <summary>
    /// Raised when current animation has just completed.
    /// </summary>
    public event EventHandler? Completed;
    
    
    /// <summary>
    /// Get or set duration before starting animation.
    /// </summary>
    public TimeSpan Delay
    {
        get => this.delay;
        set
        {
            this.VerifyAccess();
            if (value.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Negative delay is not supported.");
            if (value == this.delay)
                return;
            if (value.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), "Delay in milliseconds greater than Int32.MaxValue is not supported.");
            this.delay = value;
        }
    }
    
    
    /// <summary>
    /// Get or set duration of animation.
    /// </summary>
    public TimeSpan Duration
    {
        get => this.duration;
        set
        {
            this.VerifyAccess();
            if (value.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Negative duration is not supported.");
            if (value == this.duration)
                return;
            if (value.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), "Duration in milliseconds greater than Int32.MaxValue is not supported.");
            this.duration = value;
        }
    }
    
    
    /// <summary>
    /// Get or set <see cref="Func{Double, Double}"/> to interpolate progress when animating.
    /// </summary>
    public Func<double, double> Interpolator { get; set; } = Interpolators.Default;
    
    
    /// <summary>
    /// Check whether animation is started or not.
    /// </summary>
    public bool IsStarted => this.startTime >= 0;


    // Called when animating.
    void OnAnimate(TimeSpan _)
    {
        // check state
        if (this.startTime < 0)
            return;
        
        // check delayed time
        var delay = (long)(this.delay.TotalMilliseconds + 0.5);
        var duration = stopwatch.ElapsedMilliseconds - this.startTime;
        if (duration <= delay)
        {
            if (this.Progress >= double.Epsilon)
            {
                this.Progress = 0;
                this.OnProgressChanged(EventArgs.Empty);
                if (this.startTime < 0)
                    return;
            }
            this.TopLevel.RequestAnimationFrame(this.animationCallback!);
            return;
        }
        duration -= delay;

        // update progress or complete animation
        var maxDuration = (long)(this.duration.TotalMilliseconds + 0.5);
        if (duration >= maxDuration)
        {
            this.startTime = -1;
            this.Progress = 0;
            this.OnCompleted(EventArgs.Empty);
        }
        else
        {
            this.Progress = this.Interpolator((double)duration / maxDuration);
            this.OnProgressChanged(EventArgs.Empty);
            if (this.startTime >= 0)
                this.TopLevel.RequestAnimationFrame(this.animationCallback!);
        }
    }
    
    
    /// <summary>
    /// Raise <see cref="Cancelled"/> event.
    /// </summary>
    /// <param name="e">Event data.</param>
    protected virtual void OnCancelled(EventArgs e) => this.Cancelled?.Invoke(this, e);


    /// <summary>
    /// Raise <see cref="Completed"/> event.
    /// </summary>
    /// <param name="e">Event data.</param>
    protected virtual void OnCompleted(EventArgs e) => this.Completed?.Invoke(this, e);


    /// <summary>
    /// Raise <see cref="ProgressChanged"/> event.
    /// </summary>
    /// <param name="e">Event data.</param>
    protected virtual void OnProgressChanged(EventArgs e) => this.ProgressChanged?.Invoke(this, e);
    
    
    /// <summary>
    /// Get progress of current animation. The default range is [0.0, 1.0].
    /// </summary>
    public double Progress { get; private set; }


    /// <summary>
    /// Raised when <see cref="Progress"/> changed when animating.
    /// </summary>
    public event EventHandler? ProgressChanged;
    
    
    /// <summary>
    /// Start animation. The current animation will be cancelled before starting new one.
    /// </summary>
    public void Start()
    {
        // check state
        this.VerifyAccess();
        this.Cancel();
        if (this.startTime > 0)
            return;

        // start animation
        if (!stopwatch.IsRunning)
            stopwatch.Start();
        this.startTime = stopwatch.ElapsedMilliseconds;
        this.animationCallback ??= this.OnAnimate;
        this.TopLevel.RequestAnimationFrame(this.animationCallback);
    }
    
    
    /// <summary>
    /// Get <see cref="TopLevel"/> to provide synchronization with rendering event.
    /// </summary>
    public TopLevel TopLevel { get; }
    
    
    /// <summary>
    /// Throw exception if current thread is not the UI thread.
    /// </summary>
    protected void VerifyAccess() =>
        this.TopLevel.VerifyAccess();
    
    
    /// <summary>
    /// Wait for completion or cancellation.
    /// </summary>
    /// <returns>Task of waiting.</returns>
    public Task WaitForCompletionAsync() =>
        this.WaitForCompletionAsync(CancellationToken.None);


    /// <summary>
    /// Wait for completion or cancellation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task of waiting.</returns>
    public Task WaitForCompletionAsync(CancellationToken cancellationToken)
    {
        // check state
        this.VerifyAccess();
        if (!this.IsStarted)
            return Task.CompletedTask;
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        // prepare
        var taskCompletionSource = new TaskCompletionSource();
        var completedOrCancelledHandler = default(EventHandler);
        completedOrCancelledHandler = (_, _) =>
        {
            this.Cancelled -= completedOrCancelledHandler;
            this.Completed -= completedOrCancelledHandler;
            taskCompletionSource.TrySetResult();
        };
        cancellationToken.Register(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.Cancelled -= completedOrCancelledHandler;
                this.Completed -= completedOrCancelledHandler;
                taskCompletionSource.TrySetCanceled();
            }, DispatcherPriority.Send);
        });

        // start waiting
        this.Cancelled += completedOrCancelledHandler;
        this.Completed += completedOrCancelledHandler;
        return taskCompletionSource.Task;
    }
}