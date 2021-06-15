using System;

namespace CarinaStudio.Threading
{
	/// <summary>
	/// Object which depends on specific thread.
	/// </summary>
	public interface IThreadDependent : ISynchronizable
	{
		/// <summary>
		/// Check whether current thread is the thread which object depends on or not.
		/// </summary>
		/// <returns>True if current thread is the thread which object depends on.</returns>
		bool CheckAccess();
	}


	/// <summary>
	/// Extensions for <see cref="IThreadDependent"/>.
	/// </summary>
	public static class ThreadDependentExtensions
	{
		/// <summary>
		/// Throw <see cref="InvalidOperationException"/> if current thread is not the thread which object depends on.
		/// </summary>
		/// <param name="threadDependent"><see cref="IThreadDependent"/>.</param>
		public static void VerifyAccess(this IThreadDependent threadDependent)
		{
			if (!threadDependent.CheckAccess())
				throw new InvalidOperationException("Access the object from the thread which is not the object depends on.");
		}
	}
}
