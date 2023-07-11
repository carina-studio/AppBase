using CarinaStudio.Threading;
using System.Threading;

namespace CarinaStudio
{
	/// <summary>
	/// Object which belongs to an <see cref="IApplication"/>.
	/// </summary>
	public interface IApplicationObject : IThreadDependent
	{
		/// <summary>
		/// Get <see cref="IApplication"/> which object belongs to.
		/// </summary>
		IApplication Application { get; }
	}


	/// <summary>
	/// Object which belongs to specific type of <see cref="IApplication"/>.
	/// </summary>
	/// <typeparam name="TApplication">Type of application.</typeparam>
	public interface IApplicationObject<out TApplication> : IApplicationObject where TApplication : class, IApplication
	{
		/// <summary>
		/// Get <see cref="IApplication"/> which object belongs to.
		/// </summary>
		new TApplication Application { get; }
	}
	
	
	/// <summary>
	/// Object which belongs to specific type of <see cref="IApplication{TSyncContext}"/>.
	/// </summary>
	/// <typeparam name="TApplication">Type of application.</typeparam>
	/// <typeparam name="TSyncContext">Type of <see cref="SynchronizationContext"/>.</typeparam>
	public interface IApplicationObject<out TApplication, out TSyncContext> : IApplicationObject<TApplication>, IThreadDependent<TSyncContext> where TApplication : class, IApplication<TSyncContext> where TSyncContext : SynchronizationContext
	{ }
}
