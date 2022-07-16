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
    }
}