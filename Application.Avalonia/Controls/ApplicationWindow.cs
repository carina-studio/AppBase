using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// <see cref="CarinaStudio.Controls.Window"/> which implements <see cref="IApplicationObject"/>.
	/// </summary>
	public class ApplicationWindow : CarinaStudio.Controls.Window, IApplicationObject
	{
		/// <summary>
		/// Initialize new <see cref="ApplicationWindow"/> instance.
		/// </summary>
		public ApplicationWindow()
		{
			// create logger
			this.Logger = this.Application.LoggerFactory.CreateLogger(this.GetType().Name);
		}


		/// <summary>
		/// Get application instance.
		/// </summary>
		public IApplication Application { get; } = CarinaStudio.Application.Current;


		/// <summary>
		/// Get logger.
		/// </summary>
		protected ILogger Logger { get; }


		/// <summary>
		/// Get persistent state.
		/// </summary>
		protected ISettings PersistentState { get => this.Application.PersistentState; }


		/// <summary>
		/// Get application settings.
		/// </summary>
		protected ISettings Settings { get => this.Application.Settings; }


		/// <summary>
		/// Get <see cref="SynchronizationContext"/>.
		/// </summary>
		public SynchronizationContext SynchronizationContext { get => this.Application.SynchronizationContext; }
	}


	/// <summary>
	/// <see cref="CarinaStudio.Controls.Window"/> which implements <see cref="IApplicationObject{TApplication}"/>.
	/// </summary>
	/// <typeparam name="TApp">Type of application.</typeparam>
	public abstract class ApplicationWindow<TApp> : ApplicationWindow, IApplicationObject<TApp> where TApp : class, IApplication
    {
		/// <summary>
		/// Get application instance.
		/// </summary>
		public new TApp Application
		{
			get => (base.Application as TApp) ?? throw new ArgumentException($"Application doesn't implement {typeof(TApp)} interface.");
		}
	}
}
