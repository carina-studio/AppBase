using System;
using System.Threading;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Base implementation of <see cref="IThreadDependent"/>.
	/// </summary>
	public abstract class BaseThreadDependent : IThreadDependent
	{
		// Fields.
		readonly Thread dependencyThread;


		/// <summary>
		/// Initialize new <see cref="BaseThreadDependent"/> instance.
		/// </summary>
		protected BaseThreadDependent()
		{
			this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("There is no SynchronizationContext on current thread.");
			this.dependencyThread = Thread.CurrentThread;
		}


		/// <summary>
		/// Check whether current thread is the thread which object depends on or not.
		/// </summary>
		/// <returns>True if current thread is the thread which object depends on.</returns>
		public bool CheckAccess() => Thread.CurrentThread == this.dependencyThread;


		/// <summary>
		/// Get <see cref="SynchronizationContext"/> relates to the thread which instance depends on.
		/// </summary>
		public SynchronizationContext SynchronizationContext { get; }
	}
}
