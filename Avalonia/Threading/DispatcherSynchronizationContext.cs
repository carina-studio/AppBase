using Avalonia.Threading;
using System;
using System.Threading;

namespace CarinaStudio.Threading;

/// <summary>
/// Implementation of <see cref="SynchronizationContext"/> based-on <see cref="Dispatcher"/>.
/// </summary>
[ThreadSafe]
public class DispatcherSynchronizationContext : SynchronizationContext
{
    // Stub of delayed call-back.
    class DelayedCallbackStub : IDelayedCallbackStub
    {
        // Fields.
        readonly Action? actionCallback;
        public readonly Dispatcher Dispatcher;
        volatile bool isCancellable = true;
        volatile bool isCancelled;
        readonly DispatcherPriority priority;
        readonly SendOrPostCallback? sendOrPostCallback;
        readonly object? state;
        readonly Lock syncLock = new();
        
        // Constructor.
        public DelayedCallbackStub(Dispatcher dispatcher, Action action, DispatcherPriority priority)
        {
            this.actionCallback = action;
            this.Dispatcher = dispatcher;
            this.priority = priority;
        }
        public DelayedCallbackStub(Dispatcher dispatcher, SendOrPostCallback callback, object? state, DispatcherPriority priority)
        {
            this.Dispatcher = dispatcher;
            this.priority = priority;
            this.sendOrPostCallback = callback;
            this.state = state;
        }

        /// <inheritdoc/>
        void IDelayedCallbackStub.Callback() =>
            this.Dispatcher.Post(this.CallbackEntry, this.priority);
        
        // Entry of call-back.
        void CallbackEntry()
        {
            lock (syncLock)
            {
                if (this.isCancelled)
                    return;
                this.isCancellable = false;
            }
            if (this.actionCallback is not null)
                this.actionCallback();
            else if (this.sendOrPostCallback is not null)
                this.sendOrPostCallback(this.state);
        }

        /// <inheritdoc/>
        bool IDelayedCallbackStub.Cancel()
        {
            lock (syncLock)
            {
                if (this.isCancelled || !this.isCancellable)
                    return false;
                this.isCancelled = true;
            }
            return true;
        }
    }
    
    
    // Static fields.
    static volatile DispatcherSynchronizationContext? UIThreadInstance;
    static readonly Lock SyncLock = new();


    // Fields.
    readonly Dispatcher dispatcher;


    /// <summary>
    /// Initialize new <see cref="DispatcherSynchronizationContext"/> instance.
    /// </summary>
    /// <param name="dispatcher"><see cref="Dispatcher"/>.</param>
    public DispatcherSynchronizationContext(Dispatcher dispatcher)
    {
        this.dispatcher = dispatcher;
    }
    
    
    /// <summary>
    /// Cancel posted delayed call-back.
    /// </summary>
    /// <param name="token">Token returned from <see cref="PostDelayed(SendOrPostCallback, object?, DispatcherPriority, int)"/> or <see cref="PostDelayed(Action, DispatcherPriority, int)"/>.</param>
    /// <returns>True if call-back cancelled successfully.</returns>
    [ThreadSafe]
    public bool CancelDelayed(object token)
    {
        if (!DelayedCallbacks.TryGetCallbackStub(token, out var callbackStub)
            || callbackStub is not DelayedCallbackStub delayedCallbackStub
            || delayedCallbackStub.Dispatcher != this.dispatcher)
        {
            return false;
        }
        return DelayedCallbacks.Cancel(token);
    }


    /// <summary>
    /// Get <see cref="Dispatcher"/> which is related to this instance.
    /// </summary>
    [ThreadSafe]
    public Dispatcher Dispatcher => this.dispatcher;


    /// <summary>
    /// Get <see cref="DispatcherSynchronizationContext"/> instance for UI thread with <see cref="DispatcherPriority.Normal"/> priority.
    /// </summary>
    /// <returns><see cref="DispatcherSynchronizationContext"/>.</returns>
    [ThreadSafe]
    public static DispatcherSynchronizationContext UIThread
    {
        get
        {
            if (UIThreadInstance is not null)
                return UIThreadInstance;
            lock (SyncLock)
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (UIThreadInstance is null)
                    UIThreadInstance = new(Dispatcher.UIThread);
                return UIThreadInstance;
            }
        }
    }


    /// <inheritdoc/>
    [ThreadSafe]
    public override void Post(SendOrPostCallback d, object? state) =>
        this.dispatcher.Post(() => d(state), DispatcherPriority.Normal);
    
    
    /// <summary>
    /// Post call-back with given priority.
    /// </summary>
    /// <param name="d">Call-back.</param>
    /// <param name="state">State.</param>
    /// <param name="priority">Priority.</param>
    [ThreadSafe]
    public void Post(SendOrPostCallback d, object? state, DispatcherPriority priority) =>
        this.dispatcher.Post(() => d(state), priority);
    
    
    /// <summary>
    /// Post action with given priority.
    /// </summary>
    /// <param name="action">Action.</param>
    /// <param name="priority">Priority.</param>
    [ThreadSafe]
    public void Post(Action action, DispatcherPriority priority) =>
        this.dispatcher.Post(action, priority);
    
    
    /// <summary>
    /// Post delayed call-back.
    /// </summary>
    /// <param name="callback">Call-back.</param>
    /// <param name="priority">Priority.</param>
    /// <param name="delayMillis">Delayed time in milliseconds.</param>
    /// <returns>Token of posted delayed call-back.</returns>
    [ThreadSafe]
    public object PostDelayed(Action callback, DispatcherPriority priority, int delayMillis) =>
        DelayedCallbacks.Schedule(new DelayedCallbackStub(this.dispatcher, callback, priority), delayMillis);


    /// <summary>
    /// Post delayed call-back.
    /// </summary>
    /// <param name="callback">Call-back.</param>
    /// <param name="state">Custom state pass to call-back.</param>
    /// <param name="priority">Priority.</param>
    /// <param name="delayMillis">Delayed time in milliseconds.</param>
    /// <returns>Token of posted delayed call-back.</returns>
    [ThreadSafe]
    public object PostDelayed(SendOrPostCallback callback, object? state, DispatcherPriority priority, int delayMillis) =>
        DelayedCallbacks.Schedule(new DelayedCallbackStub(this.dispatcher, callback, state, priority), delayMillis);


    /// <inheritdoc/>
    [ThreadSafe]
    public override void Send(SendOrPostCallback d, object? state) =>
        this.dispatcher.Invoke(() => d(state), DispatcherPriority.Normal);
    
    
    /// <summary>
    /// Send call-back with given priority and wait for completion.
    /// </summary>
    /// <param name="d">Call-back.</param>
    /// <param name="state">State.</param>
    /// <param name="priority">Priority.</param>
    [ThreadSafe]
    public void Send(SendOrPostCallback d, object? state, DispatcherPriority priority) =>
        this.dispatcher.Invoke(() => d(state), priority);
    
    
    /// <summary>
    /// Send action with given priority and wait for completion.
    /// </summary>
    /// <param name="action">Action.</param>
    /// <param name="priority">Priority.</param>
    [ThreadSafe]
    public void Send(Action action, DispatcherPriority priority) =>
        this.dispatcher.Invoke(action, priority);
}