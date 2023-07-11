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
	
	
	/// <summary>
	/// Object which relates to specific type of <see cref="SynchronizationContext"/>.
	/// </summary>
	/// <typeparam name="TSyncContext">Type of <see cref="SynchronizationContext"/>.</typeparam>
	public interface ISynchronizable<out TSyncContext> : ISynchronizable where TSyncContext : SynchronizationContext
	{
		/// <summary>
		/// Get <see cref="SynchronizationContext"/> which the instance relates to.
		/// </summary>
		new TSyncContext SynchronizationContext { get; }
	}
}
