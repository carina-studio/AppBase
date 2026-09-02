using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CarinaStudio.Threading;

/// <summary>
/// Manage delayed call-backs.
/// </summary>
internal static class DelayedCallbacks
{
    // Node of delayed call-back.
    class DelayedCallbackNode(IDelayedCallbackStub callbackStub, long readyTimeTicks)
    {
        public readonly IDelayedCallbackStub CallbackStub = callbackStub;
        public volatile DelayedCallbackNode? Next;
        public volatile DelayedCallbackNode? Previous;
        public readonly long ReadyTimeTicks = readyTimeTicks;
    }
    
    
    // Fields.
    static volatile DelayedCallbackNode? DelayedCallbackListHead;
    static readonly object DelayedCallbackSyncLock = new();
    static readonly Thread DelayedCallbackThread = new(DelayedCallbackThreadProc)
    {
	    IsBackground = true,
	    Name = "Delayed call-backs"
    };
    static readonly Stopwatch DelayedCallbackWatch = new();
    
    
    // Cancel a delayed call-back.
    public static bool Cancel(object token)
    {
        if (token is not DelayedCallbackNode delayedCallbackNode)
	        return false;
        lock (DelayedCallbackSyncLock)
        {
	        if (DelayedCallbackListHead == delayedCallbackNode)
	        {
		        DelayedCallbackListHead = delayedCallbackNode.Next;
		        if (delayedCallbackNode.Next is not null)
			        delayedCallbackNode.Next.Previous = null;
		        delayedCallbackNode.Next = null;
	        }
	        else if (delayedCallbackNode.Previous is not null || delayedCallbackNode.Next is not null)
	        {
		        if (delayedCallbackNode.Previous is not null)
			        delayedCallbackNode.Previous.Next = delayedCallbackNode.Next;
		        if (delayedCallbackNode.Next is not null)
			        delayedCallbackNode.Next.Previous = delayedCallbackNode.Previous;
		        delayedCallbackNode.Previous = null;
		        delayedCallbackNode.Next = null;
	        }
        }
        return delayedCallbackNode.CallbackStub.Cancel();
    }
    
    
    // Entry of delayed call-back thread.
    [DoesNotReturn]
    [CalledOnBackgroundThread]
    static void DelayedCallbackThreadProc()
    {
        while (true)
        {
	        // select next call-back
	        DelayedCallbackNode? delayedCallbackNode = null;
	        lock (DelayedCallbackSyncLock)
	        {
		        // check call-back
		        var waitingTime = 0;
		        if (DelayedCallbackListHead is not null)
		        {
			        var currentTimeTicks = DelayedCallbackWatch.ElapsedTicks;
			        var timeDiffTicks = DelayedCallbackListHead.ReadyTimeTicks - currentTimeTicks;
			        if (timeDiffTicks <= 0)
			        {
				        delayedCallbackNode = DelayedCallbackListHead;
				        if (delayedCallbackNode.Next is not null)
					        delayedCallbackNode.Next.Previous = null;
				        DelayedCallbackListHead = delayedCallbackNode.Next;
				        delayedCallbackNode.Next = null;
			        }
			        else
				        waitingTime = TicksToMilliseconds(timeDiffTicks);
		        }
		        else
			        waitingTime = Timeout.Infinite;

		        // wait for next call-back
		        if (waitingTime != 0)
		        {
			        Monitor.Wait(DelayedCallbackSyncLock, waitingTime);
			        continue;
		        }
	        }

	        // call-back
	        delayedCallbackNode?.CallbackStub.Callback();
        }
        // ReSharper disable once FunctionNeverReturns
    }


    // Convert from milliseconds to number of ticks of Stopwatch.
    static long MillisecondsToTicks(int millis) =>
        millis * Stopwatch.Frequency / 1000;


    // Schedule a delayed call-back.
    public static object Schedule(IDelayedCallbackStub callbackStub, int delayMillis)
    {
        // setup environment
        if (!DelayedCallbackWatch.IsRunning)
        {
	        lock (typeof(SynchronizationContextExtensions))
	        {
		        if (!DelayedCallbackWatch.IsRunning)
		        {
			        DelayedCallbackWatch.Start();
			        DelayedCallbackThread.Start();
		        }
	        }
        }

        // create delayed call-back
        if (delayMillis < 0)
	        delayMillis = 0;
        var readyTimeTicks = DelayedCallbackWatch.ElapsedTicks + MillisecondsToTicks(delayMillis);
        var delayedCallbackNode = new DelayedCallbackNode(callbackStub, readyTimeTicks);

        // enqueue to list or post directly
        if (delayMillis > 0)
        {
	        lock (DelayedCallbackSyncLock)
	        {
		        var prevDelayedCallbackNode = default(DelayedCallbackNode);
		        var nextDelayedCallback = DelayedCallbackListHead;
		        while (nextDelayedCallback is not null)
		        {
			        if (nextDelayedCallback.ReadyTimeTicks > readyTimeTicks)
				        break;
			        prevDelayedCallbackNode = nextDelayedCallback;
			        nextDelayedCallback = nextDelayedCallback.Next;
		        }
		        if (nextDelayedCallback is not null)
		        {
			        delayedCallbackNode.Next = nextDelayedCallback;
			        nextDelayedCallback.Previous = delayedCallbackNode;
		        }
		        if (prevDelayedCallbackNode is not null)
		        {
			        prevDelayedCallbackNode.Next = delayedCallbackNode;
			        delayedCallbackNode.Previous = prevDelayedCallbackNode;
		        }
		        else
		        {
			        DelayedCallbackListHead = delayedCallbackNode;
			        Monitor.Pulse(DelayedCallbackSyncLock);
		        }
	        }
        }
        else
	        callbackStub.Callback();
        return delayedCallbackNode;
    }
    
    
    // Convert from number of ticks of Stopwatch to milliseconds. The value is rounded up to make sure that
    // a delayed call-back will never be performed earlier than its requested delay time.
    static int TicksToMilliseconds(long ticks)
    {
        if (ticks <= 0)
	        return 0;
        var millis = (ticks * 1000 + Stopwatch.Frequency - 1) / Stopwatch.Frequency;
        return millis < int.MaxValue ? (int)millis : int.MaxValue;
    }
    
    
    // Try getting corresponding stub of call-back.
    public static bool TryGetCallbackStub(object token, [NotNullWhen(true)] out IDelayedCallbackStub? callback)
    {
        if (token is DelayedCallbackNode node)
        {
	        callback = node.CallbackStub;
	        return true;
        }
        callback = null;
        return false;
    }
}