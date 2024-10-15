using Android.OS;
using System;
using System.Reflection;
using System.Threading;

namespace CarinaStudio.Android.Threading;

/// <summary>
/// implementation of <see cref="SynchronizationContext"/> based-on <see cref="Looper"/>.
/// </summary>
public class LooperSynchronizationContext : SynchronizationContext
{
    // Static fields.
    static volatile LooperSynchronizationContext? MainInstance;
    static readonly Lock SyncLock = new();
    
    
    // Fields.
    readonly Handler handler;
    readonly Looper looper;


    /// <summary>
    /// Initialize new <see cref="LooperSynchronizationContext"/> instance by <see cref="Looper"/> of current thread.
    /// </summary>
    public LooperSynchronizationContext() : this(Looper.MyLooper().AsNonNull())
    { }


    /// <summary>
    /// Initialize new <see cref="LooperSynchronizationContext"/> instance.
    /// </summary>
    /// <param name="looper"><see cref="Looper"/>.</param>
    public LooperSynchronizationContext(Looper looper)
    {
        this.handler = new(looper);
        this.looper = looper;
    }


    /// <inheritdoc/>
    public override SynchronizationContext CreateCopy() =>
        new LooperSynchronizationContext(this.looper);


    /// <summary>
    /// Get <see cref="LooperSynchronizationContext"/> instance for main looper.
    /// </summary>
    public static LooperSynchronizationContext Main
    {
        get
        {
            if (MainInstance is not null)
                return MainInstance;
            lock (SyncLock)
            {
                // ReSharper disable ConvertIfStatementToNullCoalescingExpression
                if (MainInstance is null)
                    MainInstance = new(Looper.MainLooper.AsNonNull());
                // ReSharper restore ConvertIfStatementToNullCoalescingExpression
                return MainInstance;
            }
        }
    }


    /// <inheritdoc/>
    public override void Post(SendOrPostCallback d, object? state)
    {
        if (!this.handler.Post(() => d(state)))
            throw new InvalidOperationException("Unable to post call-back to handler.");
    }


    /// <inheritdoc/>
    public override void Send(SendOrPostCallback d, object? state)
    {
        if (this.handler.Looper.IsCurrentThread)
            d(state);
        else
        {
            var syncLock = new object();
            var exception = (Exception?)null;
            lock (syncLock)
            {
                var stub = new Action(() =>
                {
                    try
                    {
                        d(state);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }
                    finally
                    {
                        lock (syncLock)
                            Monitor.PulseAll(syncLock);
                    }
                });
                if (!this.handler.Post(stub))
                    throw new InvalidOperationException("Unable to post call-back to handler.");
                Monitor.Wait(syncLock);
                if (exception != null)
                    throw new TargetInvocationException(exception);
            }
        }
    }
}