using Avalonia.Threading;
using System;
using System.Threading;

namespace CarinaStudio.Threading;

/// <summary>
/// Implementation of <see cref="SynchronizationContext"/> based-on <see cref="Dispatcher"/>.
/// </summary>
public class DispatcherSynchronizationContext : SynchronizationContext
{
    // Static fields.
    static volatile DispatcherSynchronizationContext? UIThreadInstance;


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
    /// Get <see cref="Dispatcher"/> which is related to this instance.
    /// </summary>
    public Dispatcher Dispatcher => this.dispatcher;


    /// <summary>
    /// Get <see cref="DispatcherSynchronizationContext"/> instance for UI thread with <see cref="DispatcherPriority.Normal"/> priority.
    /// </summary>
    /// <returns><see cref="DispatcherSynchronizationContext"/>.</returns>
    public static DispatcherSynchronizationContext UIThread
    {
        get
        {
            if (UIThreadInstance is not null)
                return UIThreadInstance;
            return typeof(DispatcherSynchronizationContext).Lock(() =>
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (UIThreadInstance is null)
                    UIThreadInstance = new(Dispatcher.UIThread);
                return UIThreadInstance;
            });
        }
    }


    /// <inheritdoc/>
    public override void Post(SendOrPostCallback d, object? state) =>
        this.dispatcher.Post(() => d(state), DispatcherPriority.Normal);
    
    
    /// <summary>
    /// Post call-back with given priority.
    /// </summary>
    /// <param name="d">Call-back.</param>
    /// <param name="state">State.</param>
    /// <param name="priority">Priority.</param>
    public void Post(SendOrPostCallback d, object? state, DispatcherPriority priority) =>
        this.dispatcher.Post(() => d(state), priority);
    
    
    /// <summary>
    /// Post action with given priority.
    /// </summary>
    /// <param name="action">Action.</param>
    /// <param name="priority">Priority.</param>
    public void Post(Action action, DispatcherPriority priority) =>
        this.dispatcher.Post(action, priority);


    /// <inheritdoc/>
    public override void Send(SendOrPostCallback d, object? state) =>
        this.dispatcher.Invoke(() => d(state), DispatcherPriority.Normal);
    
    
    /// <summary>
    /// Send call-back with given priority and wait for completion.
    /// </summary>
    /// <param name="d">Call-back.</param>
    /// <param name="state">State.</param>
    /// <param name="priority">Priority.</param>
    public void Send(SendOrPostCallback d, object? state, DispatcherPriority priority) =>
        this.dispatcher.Invoke(() => d(state), priority);
    
    
    /// <summary>
    /// Send action with given priority and wait for completion.
    /// </summary>
    /// <param name="action">Action.</param>
    /// <param name="priority">Priority.</param>
    public void Send(Action action, DispatcherPriority priority) =>
        this.dispatcher.Invoke(action, priority);
}