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
}
