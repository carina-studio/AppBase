using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;

namespace CarinaStudio.Controls;

/// <summary>
/// <see cref="CarinaStudio.Controls.Window"/> which implements <see cref="IApplicationObject"/>.
/// </summary>
public class ApplicationWindow : Window, IApplicationObject
{
	/// <summary>
	/// Initialize new <see cref="ApplicationWindow"/> instance.
	/// </summary>
	public ApplicationWindow()
	{
		// create logger
#if !NET10_0_OR_GREATER
		this.Application = CarinaStudio.Application.Current;
		this.Logger = this.Application.LoggerFactory.CreateLogger(this.GetType().Name);
#endif
	}


	/// <summary>
	/// Get application instance.
	/// </summary>
	[ThreadSafe]
	public IApplication Application
	{
#if NET10_0_OR_GREATER
		get
		{
			field ??= CarinaStudio.Application.Current;
			return field;
		}
#else
		get;
#endif
	}


	/// <summary>
	/// Get logger.
	/// </summary>
	[ThreadSafe]
	protected ILogger Logger
	{
#if NET10_0_OR_GREATER
		get
		{
			field ??= this.Application.LoggerFactory.CreateLogger(this.GetType().Name);
			return field;
		}
#else
		get;
#endif
	}


	/// <summary>
	/// Get persistent state.
	/// </summary>
	[ThreadSafe]
	protected ISettings PersistentState => this.Application.PersistentState;


	/// <summary>
	/// Get application settings.
	/// </summary>
	[ThreadSafe]
	protected ISettings Settings => this.Application.Settings;
}


/// <summary>
/// <see cref="CarinaStudio.Controls.Window"/> which implements <see cref="IApplicationObject{TApplication}"/>.
/// </summary>
/// <typeparam name="TApp">Type of application.</typeparam>
public abstract class ApplicationWindow<TApp> : ApplicationWindow, IApplicationObject<TApp> where TApp : class, IAvaloniaApplication
{
	/// <summary>
	/// Get application instance.
	/// </summary>
	[ThreadSafe]
	public new TApp Application => base.Application as TApp ?? throw new ArgumentException($"Application doesn't implement {typeof(TApp)} interface.");
}