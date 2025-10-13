using Avalonia.Threading;
using System;
using System.Threading;

namespace CarinaStudio.Threading;

/// <summary>
/// <see cref="ScheduledAction"/> which schedules the action with specific <see cref="DispatcherPriority"/>.
/// </summary>
[ThreadSafe]
public class DispatcherScheduledAction : ScheduledAction
{
    // Fields.
    readonly DispatcherPriority priority;
    
    
    /// <summary>
    /// Initialize new <see cref="DispatcherScheduledAction"/> instance with <see cref="DispatcherPriority.Default"/> priority.
    /// </summary>
    /// <param name="synchronizationContext"><see cref="DispatcherSynchronizationContext"/> to perform action.</param>
    /// <param name="action">Action.</param>
    public DispatcherScheduledAction(DispatcherSynchronizationContext synchronizationContext, Action action) : this(synchronizationContext, action, DispatcherPriority.Default)
    { }
    
    
    /// <summary>
    /// Initialize new <see cref="DispatcherScheduledAction"/> instance.
    /// </summary>
    /// <param name="synchronizationContext"><see cref="DispatcherSynchronizationContext"/> to perform action.</param>
    /// <param name="action">Action.</param>
    /// <param name="priority">Priority.</param>
    public DispatcherScheduledAction(DispatcherSynchronizationContext synchronizationContext, Action action, DispatcherPriority priority) : base(synchronizationContext, action)
    {
        this.priority = priority;
    }
    
    
    /// <summary>
    /// Initialize new <see cref="DispatcherScheduledAction"/> instance with <see cref="DispatcherPriority.Default"/> priority.
    /// </summary>
    /// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="DispatcherSynchronizationContext"/> to perform action.</param>
    /// <param name="action">Action.</param>
    public DispatcherScheduledAction(ISynchronizable synchronizable, Action action) : this((DispatcherSynchronizationContext)synchronizable.SynchronizationContext, action, DispatcherPriority.Default)
    { }


    /// <summary>
    /// Initialize new <see cref="DispatcherScheduledAction"/> instance.
    /// </summary>
    /// <param name="synchronizable"><see cref="ISynchronizable"/> to provide <see cref="DispatcherSynchronizationContext"/> to perform action.</param>
    /// <param name="action">Action.</param>
    /// <param name="priority">Priority.</param>
    public DispatcherScheduledAction(ISynchronizable synchronizable, Action action, DispatcherPriority priority) : this((DispatcherSynchronizationContext)synchronizable.SynchronizationContext, action, priority)
    { }


    /// <inheritdoc/>
    [ThreadSafe]
    protected override bool CancelAction(object token) =>
        ((DispatcherSynchronizationContext)this.SynchronizationContext).CancelDelayed(token);


    /// <inheritdoc/>
    [ThreadSafe]
    protected override object PostAction(SendOrPostCallback action, object? state, int delayMillis) =>
        ((DispatcherSynchronizationContext)this.SynchronizationContext).PostDelayed(action, state, this.priority, delayMillis);


    /// <inheritdoc/>
    [ThreadSafe]
    protected override void SendAction(SendOrPostCallback action, object? state) =>
        ((DispatcherSynchronizationContext)this.SynchronizationContext).Send(action, state, this.priority);
}