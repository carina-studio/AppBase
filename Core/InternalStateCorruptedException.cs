using System;

namespace CarinaStudio
{
	/// <summary>
	/// <see cref="Exception"/> indicate that internal state is corrupted.
	/// </summary>
	public class InternalStateCorruptedException : Exception
	{
		/// <summary>
		/// Initialize new <see cref="InternalStateCorruptedException"/> instance.
		/// </summary>
		public InternalStateCorruptedException()
		{ }


		/// <summary>
		/// Initialize new <see cref="InternalStateCorruptedException"/> instance.
		/// </summary>
		/// <param name="message">Message.</param>
		public InternalStateCorruptedException(string? message) : base(message)
		{ }


		/// <summary>
		/// Initialize new <see cref="InternalStateCorruptedException"/> instance.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="innerException">Inner exception.</param>
		public InternalStateCorruptedException(string? message, Exception innerException) : base(message, innerException)
		{ }
	}
}
