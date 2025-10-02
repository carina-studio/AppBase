using System.Threading.Tasks;

namespace CarinaStudio.Threading.Tasks;

/// <summary>
/// <see cref="TaskFactory"/> which uses dedicated and fixed execution threads to run tasks.
/// </summary>
[ThreadSafe]
public class FixedThreadsTaskFactory : TaskFactory
{
    /// <summary>
    /// Initialize new <see cref="FixedThreadsTaskFactory"/> instance.
    /// </summary>
    /// <param name="maxConcurrencyLevel">Maximum concurrency level.</param>
    /// <param name="useBackgroundThreads">True to set execution threads as background thread.</param>
    public FixedThreadsTaskFactory(int maxConcurrencyLevel, bool useBackgroundThreads = true) : this(null, maxConcurrencyLevel, useBackgroundThreads)
    { }


    /// <summary>
    /// Initialize new <see cref="FixedThreadsTaskFactory"/> instance.
    /// </summary>
    /// <param name="name">Name of task factory.</param>
    /// <param name="maxConcurrencyLevel">Maximum concurrency level.</param>
    /// <param name="useBackgroundThreads">True to set execution threads as background thread.</param>
    public FixedThreadsTaskFactory(string? name, int maxConcurrencyLevel, bool useBackgroundThreads = true) : base(new FixedThreadsTaskScheduler(name, maxConcurrencyLevel, useBackgroundThreads))
    {
	    this.Name = name;
    }


    /// <summary>
	/// Get number of threads which are executing tasks.
	/// </summary>
	[ThreadSafe]
    public int BusyThreadCount => ((FixedThreadsTaskScheduler)this.Scheduler!).BusyThreadCount;


    /// <summary>
	/// Get maximum concurrency level supported by its <see cref="TaskFactory.Scheduler"/>.
	/// </summary>
	[ThreadSafe]
    public int MaximumConcurrencyLevel => ((FixedThreadsTaskScheduler)this.Scheduler!).MaximumConcurrencyLevel;
    
    
    /// <summary>
    /// Name of task factory.
    /// </summary>
    [ThreadSafe]
    public string? Name { get; }


    /// <inheritdoc/>
    [ThreadSafe]
    public override string ToString() =>
        $"{this.Name ?? nameof(FixedThreadsTaskFactory)} [{this.Scheduler!.Id}]";
}