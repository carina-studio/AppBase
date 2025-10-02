using CarinaStudio.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Animation;

/// <summary>
/// General purpose animator.
/// </summary>
public class Animator : IThreadDependent
{
    /// <summary>
    /// Default value of <see cref="Interval"/>.
    /// </summary>
    public static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(8);


    // Timer of animation for each thread.
    class AnimationTimer
    {
        // Fields.
        readonly ScheduledAction animateAction;
        int animatorCount;
        long scheduledAnimationTime;
        Animator? scheduledAnimators;
        readonly Stopwatch stopwatch = new();

        // Constructor.
        public AnimationTimer()
        {
            this.animateAction = new ScheduledAction(() =>
            {
                var currentTime = this.stopwatch.ElapsedMilliseconds;
                var animator = this.scheduledAnimators;
                while (animator != null && animator.nextAnimationTime <= currentTime)
                {
                    // remove animator from list
                    var nextAnimator = animator.nextAnimator;
                    this.scheduledAnimators = nextAnimator;
                    animator.nextAnimator = null;
                    if (nextAnimator != null)
                        nextAnimator.prevAnimator = null;
                    
                    // animate
                    animator.Animate();
                    animator = this.scheduledAnimators;
                }
                this.ScheduleNextAnimation();
            });
        }

        // Cancel scheduled animation.
        public void CancelAnimation(Animator animator)
        {
            var isFirstAnimator = this.scheduledAnimators == animator;
            if (animator.prevAnimator != null)
                animator.prevAnimator.nextAnimator = animator.nextAnimator;
            else if (this.scheduledAnimators == animator)
                this.scheduledAnimators = animator.nextAnimator;
            if (animator.nextAnimator != null)
            {
                animator.nextAnimator.prevAnimator = animator.prevAnimator;
                if (this.scheduledAnimators == animator)
                    this.scheduledAnimators = animator.nextAnimator;
            }
            animator.prevAnimator = null;
            animator.nextAnimator = null;
            if (isFirstAnimator)
                this.ScheduleNextAnimation();
        }

        // Get current time for animation.
        public long CurrentTimeMilliseconds => this.stopwatch.ElapsedMilliseconds;

        // Check whether animation of given animator is scheduled or not.
        public bool IsAnimationScheduled(Animator animator) =>
            this.scheduledAnimators == animator
            || animator.prevAnimator != null
            || animator.nextAnimator != null;

        // Register animator.
        public void Register()
        {
            ++this.animatorCount;
            if (this.animatorCount == 1)
                this.stopwatch.Start();
        }

        // Schedule next animation.
        public void ScheduleNextAnimation(Animator animator)
        {
            // remove from list first
            if (animator.prevAnimator != null)
                animator.prevAnimator.nextAnimator = animator.nextAnimator;
            if (animator.nextAnimator != null)
            {
                animator.nextAnimator.prevAnimator = animator.prevAnimator;
                if (this.scheduledAnimators == animator)
                    this.scheduledAnimators = animator.nextAnimator;
            }

            // add to list according to next animation time
            if (animator.nextAnimationTime <= 0)
                return;
            var prevAnimator = (Animator?)null;
            var nextAnimator = this.scheduledAnimators;
            while (nextAnimator != null && nextAnimator.nextAnimationTime < animator.nextAnimationTime)
            {
                prevAnimator = nextAnimator;
                nextAnimator = nextAnimator.nextAnimator;
            }
            if (prevAnimator != null)
            {
                animator.prevAnimator = prevAnimator;
                animator.nextAnimator = prevAnimator.nextAnimator;
                prevAnimator.nextAnimator = animator;
                if (animator.nextAnimator != null)
                    animator.nextAnimator.prevAnimator = animator;
            }
            else
            {
                animator.prevAnimator = null;
                animator.nextAnimator = this.scheduledAnimators;
                if (animator.nextAnimator != null)
                    animator.nextAnimator.prevAnimator = animator;
                this.scheduledAnimators = animator;
            }

            // schedule
            if (this.scheduledAnimators == animator)
                this.ScheduleNextAnimation();
        }
        void ScheduleNextAnimation()
        {
            var animator = this.scheduledAnimators;
            if (animator == null)
                return;
            if (!this.animateAction.IsScheduled || this.scheduledAnimationTime > animator.nextAnimationTime)
            {
                this.scheduledAnimationTime = animator.nextAnimationTime;
                this.animateAction.Reschedule((int)Math.Max(0, this.scheduledAnimationTime - this.stopwatch.ElapsedMilliseconds));
            }
        }

        // Unregister animator.
        public void Unregister()
        {
            --this.animatorCount;
            if (this.animatorCount <= 0)
                this.stopwatch.Reset();
        }
    }


    // Static fields.
    [ThreadStatic]
    static AnimationTimer? _CurrentAnimationTimer;


    // Fields.
    readonly AnimationTimer animationTimer;
    long completionTime;
    TimeSpan delay = TimeSpan.Zero;
    TimeSpan duration;
    TimeSpan interval = DefaultInterval;
    long nextAnimationTime;
    Animator? nextAnimator;
    long prevAnimationTime;
    Animator? prevAnimator;
    long startTime = -1;
    readonly Thread thread;


    /// <summary>
    /// Initialize new <see cref="Animator"/> instance.
    /// </summary>
    public Animator()
    {
        this.animationTimer = _CurrentAnimationTimer ?? new AnimationTimer().Also(it => _CurrentAnimationTimer = it);
        this.animationTimer.Register();
        this.SynchronizationContext = SynchronizationContext.Current.AsNonNull();
        this.thread = Thread.CurrentThread;
    }


    /// <inheritdoc/>
    ~Animator() => this.animationTimer.Unregister();


    // Perform a single step of animation.
    void Animate()
    {
        // calculate original progress
        var currentTime = this.animationTimer.CurrentTimeMilliseconds;
        var progress = ((double)currentTime - this.startTime - this.delay.TotalMilliseconds) / this.duration.TotalMilliseconds;

        // complete animation
        if (progress >= 1)
        {
            this.Complete();
            return;
        }

        // update progress
        this.Progress = this.Interpolator(progress);
        this.OnProgressChanged(EventArgs.Empty);
        if (!this.IsStarted || this.animationTimer.IsAnimationScheduled(this))
            return;

        // schedule next update
        currentTime = this.animationTimer.CurrentTimeMilliseconds;
        while (this.nextAnimationTime <= currentTime)
        {
            this.prevAnimationTime = this.nextAnimationTime;
            this.nextAnimationTime += (long)this.interval.TotalMilliseconds;
        }
        this.nextAnimationTime = Math.Min(this.completionTime, this.nextAnimationTime);
        this.animationTimer.ScheduleNextAnimation(this);
    }


    /// <summary>
    /// Cancel current animation.
    /// </summary>
    public void Cancel()
    {
        this.VerifyAccess();
        if (this.IsStarted)
        {
            this.startTime = -1;
            this.completionTime = 0;
            this.prevAnimationTime = 0;
            this.nextAnimationTime = 0;
            this.Progress = 0;
            this.animationTimer.CancelAnimation(this);
            this.OnCancelled(EventArgs.Empty);
        }
    }


    /// <summary>
    /// Raised when current animation has been cancelled.
    /// </summary>
    public event EventHandler? Cancelled;


    /// <inheritdoc/>
    public bool CheckAccess() => this.thread == Thread.CurrentThread;


    // Complete current animation.
    void Complete()
    {
        this.startTime = -1;
        this.completionTime = 0;
        this.prevAnimationTime = 0;
        this.nextAnimationTime = 0;
        this.Progress = 0;
        this.animationTimer.CancelAnimation(this);
        this.OnCompleted(EventArgs.Empty);
    }


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
            if (this.IsStarted)
            {
                var currentTime = this.animationTimer.CurrentTimeMilliseconds;
                var scheduledStartingTime = this.startTime + (long)this.delay.TotalMilliseconds;
                this.completionTime = this.startTime + (long)(this.delay + this.duration).TotalMilliseconds;
                if (currentTime <= scheduledStartingTime)
                {
                    this.prevAnimationTime = scheduledStartingTime;
                    this.nextAnimationTime = this.prevAnimationTime + (long)this.interval.TotalMilliseconds;
                }
                else
                    this.nextAnimationTime = Math.Min(this.completionTime, this.nextAnimationTime);
                this.animationTimer.ScheduleNextAnimation(this);
            }
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
            if (this.IsStarted)
            {
                var currentTime = this.animationTimer.CurrentTimeMilliseconds;
                var scheduledStartingTime = this.startTime + (long)this.delay.TotalMilliseconds;
                this.completionTime = this.startTime + (long)(this.delay + this.duration).TotalMilliseconds;
                if (currentTime <= scheduledStartingTime)
                {
                    this.prevAnimationTime = scheduledStartingTime;
                    this.nextAnimationTime = this.prevAnimationTime + (long)this.interval.TotalMilliseconds;
                }
                else
                    this.nextAnimationTime = Math.Min(this.completionTime, this.nextAnimationTime);
                this.animationTimer.ScheduleNextAnimation(this);
            }
        }
    }


    /// <summary>
    /// Get or set <see cref="Func{Double, Double}"/> to interpolate progress when animating.
    /// </summary>
    public Func<double, double> Interpolator { get; set; } = Interpolators.Default;


    /// <summary>
    /// Get or set interval between progress updating. Default value is <see cref="DefaultInterval"/>.
    /// </summary>
    public TimeSpan Interval
    {
        get => this.interval;
        set
        {
            this.VerifyAccess();
            if (value.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Negative interval is not supported.");
            if (value == this.interval)
                return;
            if (value.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), "Interval in milliseconds greater than Int32.MaxValue is not supported.");
            this.interval = value;
            if (this.IsStarted)
            {
                this.nextAnimationTime = Math.Min(this.completionTime, this.prevAnimationTime + (long)value.TotalMilliseconds);
                this.animationTimer.ScheduleNextAnimation(this);
            }
        }
    }


    /// <summary>
    /// Check whether animation is started or not.
    /// </summary>
    public bool IsStarted => this.startTime >= 0;


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
        this.startTime = this.animationTimer.CurrentTimeMilliseconds;
        this.completionTime = this.startTime + (long)(this.delay + this.duration).TotalMilliseconds;
        this.prevAnimationTime = this.startTime + (long)this.delay.TotalMilliseconds;
        this.nextAnimationTime = this.prevAnimationTime + (long)this.interval.TotalMilliseconds;
        this.animationTimer.ScheduleNextAnimation(this);
    }


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="completedAction">Action when completed.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Animator Start(TimeSpan duration, Action<double> progressChangedAction, Action? completedAction = null) =>
        Start(duration, DefaultInterval, Interpolators.Default, progressChangedAction, completedAction);
    

    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="interpolator">Interpolator.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="completedAction">Action when completed.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Animator Start(TimeSpan duration, Func<double, double> interpolator, Action<double> progressChangedAction, Action? completedAction = null) => 
        Start(duration, DefaultInterval, interpolator, progressChangedAction, completedAction);
    

    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="interval">Interval between progress updating.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="completedAction">Action when completed.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Animator Start(TimeSpan duration, TimeSpan interval, Action<double> progressChangedAction, Action? completedAction = null) => 
        Start(duration, interval, Interpolators.Default, progressChangedAction, completedAction);


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="interval">Interval between progress updating.</param>
    /// <param name="interpolator">Interpolator.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="completedAction">Action when completed.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Animator Start(TimeSpan duration, TimeSpan interval, Func<double, double> interpolator, Action<double> progressChangedAction, Action? completedAction = null) => new Animator().Also(it =>
    {
        it.Duration = duration;
        if (completedAction != null)
            it.Completed += (_, _) => completedAction();
        it.Interpolator = interpolator;
        it.Interval = interval;
        it.ProgressChanged += (_, _) => progressChangedAction(it.Progress);
        it.Start();
    });


    /// <summary>
    /// Start animation and wait for completion or cancellation.
    /// </summary>
    /// <returns>Task of waiting.</returns>
    public Task StartAndWaitForCompletionAsync() =>
        this.StartAndWaitForCompletionAsync(CancellationToken.None);


    /// <summary>
    /// Start animation and wait for completion or cancellation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task of waiting.</returns>
    public Task StartAndWaitForCompletionAsync(CancellationToken cancellationToken)
    {
        // start animation
        this.Start();
        if (!this.IsStarted)
            return Task.CompletedTask;

        // wait for completion
        return this.WaitForCompletionAsync(cancellationToken);
    }


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Task StartAndWaitForCompletionAsync(TimeSpan duration, Action<double> progressChangedAction) =>
        StartAndWaitForCompletionAsync(duration, DefaultInterval, Interpolators.Default, progressChangedAction, CancellationToken.None);


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="interval">Interval between progress updating.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Task StartAndWaitForCompletionAsync(TimeSpan duration, TimeSpan interval, Action<double> progressChangedAction) =>
        StartAndWaitForCompletionAsync(duration, interval, Interpolators.Default, progressChangedAction, CancellationToken.None);


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Task StartAndWaitForCompletionAsync(TimeSpan duration, Action<double> progressChangedAction, CancellationToken cancellationToken) =>
        StartAndWaitForCompletionAsync(duration, DefaultInterval, Interpolators.Default, progressChangedAction, cancellationToken);


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="interval">Interval between progress updating.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Task StartAndWaitForCompletionAsync(TimeSpan duration, TimeSpan interval, Action<double> progressChangedAction, CancellationToken cancellationToken) =>
        StartAndWaitForCompletionAsync(duration, interval, Interpolators.Default, progressChangedAction, cancellationToken);


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="interpolator">Interpolator.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Task StartAndWaitForCompletionAsync(TimeSpan duration, Func<double, double> interpolator, Action<double> progressChangedAction, CancellationToken cancellationToken) =>
        StartAndWaitForCompletionAsync(duration, DefaultInterval, interpolator, progressChangedAction, cancellationToken);


    /// <summary>
    /// Start new animation with default interval.
    /// </summary>
    /// <param name="duration">Duration.</param>
    /// <param name="interval">Interval between progress updating.</param>
    /// <param name="interpolator">Interpolator.</param>
    /// <param name="progressChangedAction">Action when progress changed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="Animator"/> which handles the animation.</returns>
    public static Task StartAndWaitForCompletionAsync(TimeSpan duration, TimeSpan interval, Func<double, double> interpolator, Action<double> progressChangedAction, CancellationToken cancellationToken) => new Animator().Also(it =>
    {
        it.Duration = duration;
        it.Interpolator = interpolator;
        it.Interval = interval;
        it.ProgressChanged += (_, _) => progressChangedAction(it.Progress);
        it.Start();
    }).StartAndWaitForCompletionAsync(cancellationToken);


    /// <inheritdoc/>
    public SynchronizationContext SynchronizationContext { get; }


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
            this.SynchronizationContext.Post(() =>
            {
                this.Cancelled -= completedOrCancelledHandler;
                this.Completed -= completedOrCancelledHandler;
                taskCompletionSource.TrySetCanceled();
            });
        });

        // start waiting
        this.Cancelled += completedOrCancelledHandler;
        this.Completed += completedOrCancelledHandler;
        return taskCompletionSource.Task;
    }
}