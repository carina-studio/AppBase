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
    class DelayedCallbackNode
    {
        // Fields.
        public readonly IDelayedCallbackStub CallbackStub;
        public DelayedCallbackNode? Next;
        public DelayedCallbackNode? Previous;
        public readonly long ReadyTime;

        // Constructor.
        public DelayedCallbackNode(IDelayedCallbackStub callbackStub, long readyTime)
        {
            this.CallbackStub = callbackStub;
            this.ReadyTime = readyTime;
        }
    }
    
    
    // Fields.
    static volatile DelayedCallbackNode? DelayedCallbackListHead;
    static readonly object DelayedCallbackSyncLock = new();
    static readonly Thread DelayedCallbackThread;
    static readonly Stopwatch DelayedCallbackWatch = new();
    
    
    // Initializer.
    static DelayedCallbacks()
    {
        DelayedCallbackThread = new Thread(DelayedCallbackThreadProc)
        {
            IsBackground = true
        };
    }
    
    
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
			        var currentTime = DelayedCallbackWatch.ElapsedMilliseconds;
			        var timeDiff = DelayedCallbackListHead.ReadyTime - currentTime;
			        if (timeDiff <= 0)
			        {
				        delayedCallbackNode = DelayedCallbackListHead;
				        if (delayedCallbackNode.Next is not null)
					        delayedCallbackNode.Next.Previous = null;
				        DelayedCallbackListHead = delayedCallbackNode.Next;
				        delayedCallbackNode.Next = null;
			        }
			        else if (timeDiff <= int.MaxValue)
				        waitingTime = (int)timeDiff;
			        else
				        waitingTime = int.MaxValue;
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
    }


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
        var readyTime = DelayedCallbackWatch.ElapsedMilliseconds + delayMillis;
        var delayedCallbackNode = new DelayedCallbackNode(callbackStub, readyTime);

        // enqueue to list or post directly
        if (delayMillis > 0)
        {
	        lock (DelayedCallbackSyncLock)
	        {
		        var prevDelayedCallbackNode = default(DelayedCallbackNode);
		        var nextDelayedCallback = DelayedCallbackListHead;
		        while (nextDelayedCallback is not null)
		        {
			        if (nextDelayedCallback.ReadyTime > readyTime)
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