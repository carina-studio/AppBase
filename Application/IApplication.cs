using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;

namespace CarinaStudio
{
	/// <summary>
	/// Application interface. This is the top-level object for application infrastructure.
	/// </summary>
	public interface IApplication : INotifyPropertyChanged, IThreadDependent
	{
		/// <summary>
		/// <see cref="ILoggerFactory"/> to create logger.
		/// </summary>
		ILoggerFactory LoggerFactory { get; }


		/// <summary>
		/// Get default application level settings.
		/// </summary>
		BaseSettings Settings { get; }
	}
}
