using System.Threading.Tasks;

namespace CarinaStudio.Threading.Tasks
{
    /// <summary>
    /// <see cref="TaskFactory"/> which uses dedicated and fixed execution threads to run tasks.
    /// </summary>
    public class FixedThreadsTaskFactory : TaskFactory
    {
        /// <summary>
        /// Initialize new <see cref="FixedThreadsTaskFactory"/> instance.
        /// </summary>
        /// <param name="maxConcurrencyLevel">Maximum concurrency level.</param>
		/// <param name="useBackgroundThreads">True to set execution threads as background thread.</param>
        public FixedThreadsTaskFactory(int maxConcurrencyLevel, bool useBackgroundThreads = true) : base(new FixedThreadsTaskScheduler(maxConcurrencyLevel, useBackgroundThreads))
        { }


        /// <summary>
		/// Get number of threads which are executing tasks.
		/// </summary>
        public int BusyThreadCount => ((FixedThreadsTaskScheduler)this.Scheduler!).BusyThreadCount;


        /// <summary>
		/// Get maximum concurrency level supported by its <see cref="TaskFactory.Scheduler"/>.
		/// </summary>
        public int MaximumConcurrencyLevel => ((FixedThreadsTaskScheduler)this.Scheduler!).MaximumConcurrencyLevel;
    }
}