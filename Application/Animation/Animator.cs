using CarinaStudio.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Animation
{
    /// <summary>
    /// General purpose animator.
    /// </summary>
    public class Animator : IThreadDependent
    {
        /// <summary>
        /// Default value of <see cref="Interval"/>.
        /// </summary>
        public static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(8);


        // Static fields.
        static readonly Stopwatch Stopwatch = new Stopwatch().Also(it => it.Start());


        // Fields.
        readonly ScheduledAction animateAction;
        long completionTime;
        TimeSpan delay = TimeSpan.Zero;
        TimeSpan duration;
        TimeSpan interval = DefaultInterval;
        long nextAnimationTime;
        long prevAnimationTime;
        long startTime = -1;
        readonly Thread thread;


        /// <summary>
        /// Initialize new <see cref="Animator"/> instance.
        /// </summary>
        public Animator()
        {
            this.animateAction = new ScheduledAction(this.Animate);
            this.SynchronizationContext = SynchronizationContext.Current.AsNonNull();
            this.thread = Thread.CurrentThread;
        }


        // Perform a single step of animation.
        void Animate()
        {
            // calculate original progress
            var currentTime = Stopwatch.ElapsedMilliseconds;
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
            if (!this.IsStarted || this.animateAction.IsScheduled)
                return;

            // schedule next update
            currentTime = Stopwatch.ElapsedMilliseconds;
            while (this.nextAnimationTime <= currentTime)
            {
                this.prevAnimationTime = this.nextAnimationTime;
                this.nextAnimationTime += (long)this.interval.TotalMilliseconds;
            }
            this.nextAnimationTime = Math.Min(this.completionTime, this.nextAnimationTime);
            this.ScheduleNextAnimating();
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
                this.animateAction.Cancel();
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
            this.animateAction.Cancel();
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
                    throw new ArgumentOutOfRangeException("Negative delay is not supported.");
                if (value == this.delay)
                    return;
                if (value.TotalMilliseconds > int.MaxValue)
                    throw new ArgumentOutOfRangeException("Delay in milliseconds greater than Int32.MaxValue is not supported.");
                this.delay = value;
                if (this.IsStarted)
                {
                    var currentTime = Stopwatch.ElapsedMilliseconds;
                    var scheduledStartingTime = this.startTime + (long)this.delay.TotalMilliseconds;
                    this.completionTime = this.startTime + (long)(this.delay + this.duration).TotalMilliseconds;
                    if (currentTime <= scheduledStartingTime)
                    {
                        this.prevAnimationTime = scheduledStartingTime;
                        this.nextAnimationTime = this.prevAnimationTime + (long)this.interval.TotalMilliseconds;
                        this.animateAction.Reschedule((int)(scheduledStartingTime - currentTime));
                    }
                    else
                    {
                        this.nextAnimationTime = Math.Min(this.completionTime, this.nextAnimationTime);
                        this.ScheduleNextAnimating();
                    }
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
                    throw new ArgumentOutOfRangeException("Negative duration is not supported.");
                if (value == this.duration)
                    return;
                if (value.TotalMilliseconds > int.MaxValue)
                    throw new ArgumentOutOfRangeException("Duration in milliseconds greater than Int32.MaxValue is not supported.");
                this.duration = value;
                if (this.IsStarted)
                {
                    var currentTime = Stopwatch.ElapsedMilliseconds;
                    var scheduledStartingTime = this.startTime + (long)this.delay.TotalMilliseconds;
                    this.completionTime = this.startTime + (long)(this.delay + this.duration).TotalMilliseconds;
                    if (currentTime <= scheduledStartingTime)
                    {
                        this.prevAnimationTime = scheduledStartingTime;
                        this.nextAnimationTime = this.prevAnimationTime + (long)this.interval.TotalMilliseconds;
                        this.animateAction.Reschedule((int)(scheduledStartingTime - currentTime));
                    }
                    else
                    {
                        this.nextAnimationTime = Math.Min(this.completionTime, this.nextAnimationTime);
                        this.ScheduleNextAnimating();
                    }
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
                    throw new ArgumentOutOfRangeException("Negative interval is not supported.");
                if (value == this.interval)
                    return;
                if (value.TotalMilliseconds > int.MaxValue)
                    throw new ArgumentOutOfRangeException("Interval in milliseconds greater than Int32.MaxValue is not supported.");
                this.interval = value;
                if (this.IsStarted)
                {
                    this.nextAnimationTime = Math.Min(this.completionTime, this.prevAnimationTime + (long)value.TotalMilliseconds);
                    this.ScheduleNextAnimating();
                }
            }
        }


        /// <summary>
        /// Check whether animation is started or not.
        /// </summary>
        public bool IsStarted { get => this.startTime >= 0; }


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


        // Schedule next call to Animate().
        void ScheduleNextAnimating()
        {
            var delay = (this.nextAnimationTime - Stopwatch.ElapsedMilliseconds);
            this.animateAction.Reschedule((int)delay);
        }


        /// <summary>
        /// Start animation. The current animation whill be cancelled before starting new one.
        /// </summary>
        public void Start()
        {
            // check state
            this.VerifyAccess();
            this.Cancel();
            if (this.startTime > 0)
                return;

            // start animation
            this.startTime = Stopwatch.ElapsedMilliseconds;
            this.completionTime = this.startTime + (long)(this.delay + this.duration).TotalMilliseconds;
            this.prevAnimationTime = this.startTime + (long)this.delay.TotalMilliseconds;
            this.nextAnimationTime = this.prevAnimationTime + (long)this.interval.TotalMilliseconds;
            this.animateAction.Reschedule((int)this.delay.TotalMilliseconds);
        }


        /// <summary>
        /// Start new animation with default interval.
        /// </summary>
        /// <param name="duration">Duration.</param>
        /// <param name="progressChangedAction">Action when progress changed.</param>
        /// <param name="completedAction">Action when completed.</param>
        /// <returns><see cref="Animator"/> which handles the animation.</returns>
        public static Animator Start(TimeSpan duration, Action<double> progressChangedAction, Action? completedAction = null) =>
            Start(duration, Interpolators.Default, progressChangedAction, completedAction);


        /// <summary>
        /// Start new animation with default interval.
        /// </summary>
        /// <param name="duration">Duration.</param>
        /// <param name="interpolator">Interpolator.</param>
        /// <param name="progressChangedAction">Action when progress changed.</param>
        /// <param name="completedAction">Action when completed.</param>
        /// <returns><see cref="Animator"/> which handles the animation.</returns>
        public static Animator Start(TimeSpan duration, Func<double, double> interpolator, Action<double> progressChangedAction, Action? completedAction = null) => new Animator().Also(it =>
        {
            it.Duration = duration;
            if (completedAction != null)
                it.Completed += (_, e) => completedAction();
            it.Interpolator = interpolator;
            it.ProgressChanged += (_, e) => progressChangedAction(it.Progress);
            it.Start();
        });


        /// <summary>
        /// Start animation and wait for completion or cancellation.
        /// </summary>
        /// <returns>Task of waiting.</returns>
        public Task StartAndWaitForCompletionAsync() =>
            this.StartAndWaitForCompletionAsync(new CancellationToken());


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
            StartAndWaitForCompletionAsync(duration, DefaultInterval, Interpolators.Default, progressChangedAction, new CancellationToken());


        /// <summary>
        /// Start new animation with default interval.
        /// </summary>
        /// <param name="duration">Duration.</param>
        /// <param name="interval">Interval between progress updating.</param>
        /// <param name="progressChangedAction">Action when progress changed.</param>
        /// <returns><see cref="Animator"/> which handles the animation.</returns>
        public static Task StartAndWaitForCompletionAsync(TimeSpan duration, TimeSpan interval, Action<double> progressChangedAction) =>
            StartAndWaitForCompletionAsync(duration, interval, Interpolators.Default, progressChangedAction, new CancellationToken());


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
            it.ProgressChanged += (_, e) => progressChangedAction(it.Progress);
            it.Start();
        }).StartAndWaitForCompletionAsync(cancellationToken);


        /// <inheritdoc/>
        public SynchronizationContext SynchronizationContext { get; }


        /// <summary>
        /// Wait for completion or cancellation.
        /// </summary>
        /// <returns>Task of waiting.</returns>
        public Task WaitForCompletionAsync() =>
            this.WaitForCompletionAsync(new CancellationToken());


        /// <summary>
        /// Wait for completion or cancellation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task of waiting.</returns>
        public async Task WaitForCompletionAsync(CancellationToken cancellationToken)
        {
            // check state
            this.VerifyAccess();
            if (!this.IsStarted)
                return;

            // prepare
            var syncLock = new object();
            var completedOrCancelledHandler = new EventHandler((_, e) =>
            {
                lock (syncLock)
                {
                    Monitor.Pulse(syncLock);
                }
            });

            // wait for completion
            this.Cancelled += completedOrCancelledHandler;
            this.Completed += completedOrCancelledHandler;
            try
            {
                await Task.Run(() =>
                {
                    lock (syncLock)
                    {
                        Monitor.Wait(syncLock);
                    }
                }, cancellationToken);
            }
            finally
            {
                this.Cancelled -= completedOrCancelledHandler;
                this.Completed -= completedOrCancelledHandler;
            }
        }
    }
}
