## 👉Extensions for *System.Threading.SynchronizationContext*
### *PostDelayed()* and *CancelDelayed()*
To post a call-back to given ```SynchronizationContext``` and execute it after given delay time. Actually it holds the call-back when you call ```PostDelayed()```, and calls ```Post()``` to given ```SynchronizationContext``` automatically after the given delay time. 

After calling ```PostDelayed()``` you will get a token reprensents the delayed call-back you post. You can call ```CancelDelayed()``` if you want to cancel it before call-back execution.

```c#
// Post a call-back and execute it after 500ms.
var token = SynchronizationContext.Current?.Post(() =>
{
    ...
}, 500);

// Cancel call-back.
SynchronizationContext.Current?.CancelDelayed(token);
```

## 👉*SingleThreadSynchronizationContext*
Implementation of ```System.Threading.SynchronizationContext``` which creates a dedicated ```Thread``` to execute all call-backs posted to it. Because it holds a ```Thread``` inside, you need to call ```Dispose()``` when it is no longer needed.

## 👉*FixedThreadsTaskScheduler*
Implementation of ```System.Threading.Tasks.TaskScheduler``` which creates one or more dedicated and fixed ```Thread```s to execute tasks according to its maximum concurrency level. Because it holds ```Thread```s inside, you need to call ```Dispose()``` when it is no longer needed.

```c#
// Create task scheduler with maximum concurrency level be 4.
readonly FixedThreadsTaskScheduler taskScheduler = new FixedThreadsTaskScheduler(4);
readonly TaskFactory taskFactory = new TaskFactory(taskScheduler);

// Run a task by the scheduler.
await taskFactory.StartNew(() =>
{
    // Run task here.
});

// Dispose the scheduler to make sure no threads left.
taskScheduler.Dispose();
```

## 👉*ScheduledAction*
Represents an action which can be executed by specific ```SynchronizationContext```, you can call ```Schedule()``` to schedule it with or without delay time. Once you call ```Schedule()```, no more ```Schedule()``` allowed (call without effect) before its execution unless you call:

* ```Cancel()``` to cancel scheduled execution.
* ```Execute()``` to cancel scheduled execution and execute immediately.
* ```ExecuteIfScheduled()``` to execute immediately if it has been scheduled (scheduled execution will be cancelled).
* ```Reschedule()``` to override scheduled execution.

The class is designed to help you to eliminate unnecessary action execution to improve performance.

```c#
// Create ScheduledAction which binds to the SynchronizationContext on current thread.
readonly ScheduledAction saveWindowSizeToSettingsAction = new ScheduledAction(() =>
{
    // Code to save size of window to settings.
});

protected override void OnWindowSizeChanged()
{
    // Schedule to save size 500ms later.
    // The call to Schedule() may be no effect if it has been scheduled but not executed yet.
    // The action will be executed 500ms later from the first call to Schedule().
    saveWindowSizeToSettingsAction.Schedule(500);
    ...
}

protected override void OnWindowClosing()
{
    // Save size immediately no matter it has been scheduled or not.
    saveWindowSizeToSettingsAction.Execute();
    ...
}
```
