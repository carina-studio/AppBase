using System;
using System.Threading;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Object which relates to specific <see cref="SynchronizationContext"/>.
	/// </summary>
	public interface ISynchronizable
	{
		/// <summary>
		/// Get <see cref="SynchronizationContext"/> which the instance relates to.
		/// </summary>
		SynchronizationContext SynchronizationContext { get; }
	}
}
