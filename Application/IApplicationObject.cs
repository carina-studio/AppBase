using CarinaStudio.Threading;
using System;

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
	public interface IApplicationObject<TApplication> : IApplicationObject where TApplication : IApplication
	{
		/// <summary>
		/// Get <see cref="IApplication"/> which object belongs to.
		/// </summary>
		new TApplication Application { get; }
	}
}
