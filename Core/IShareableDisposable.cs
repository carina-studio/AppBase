using System;

namespace CarinaStudio
{
	/// <summary>
	/// <see cref="IDisposable"/> which allow sharing internal resources. Internal resources will be disposed only when all shared instances are disposed.
	/// </summary>
	/// <typeparam name="TSelf">Self type.</typeparam>
	public interface IShareableDisposable<out TSelf> : IDisposable where TSelf : IShareableDisposable<TSelf>
	{
		/// <summary>
		/// Create a new instance which shares internal resources.
		/// </summary>
		/// <returns>New instance which shares internal resources.</returns>
		TSelf Share();
	}
}
