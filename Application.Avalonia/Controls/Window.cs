using Avalonia;
using CarinaStudio.Collections;
using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// <see cref="Avalonia.Controls.Window"/> which implements <see cref="IApplicationObject"/>.
	/// </summary>
	public abstract class Window : Avalonia.Controls.Window, IApplicationObject
	{
		/// <summary>
		/// Property of <see cref="HasDialogs"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> HasDialogsProperty = AvaloniaProperty.Register<Window, bool>(nameof(HasDialogs), false);
		/// <summary>
		/// Property of <see cref="IsClosed"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsClosedProperty = AvaloniaProperty.Register<Window, bool>(nameof(IsClosed), false);
		/// <summary>
		/// Property of <see cref="IsOpened"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsOpenedProperty = AvaloniaProperty.Register<Window, bool>(nameof(IsOpened), false);


		// Fields.
		readonly List<Dialog> dialogs = new List<Dialog>();


		/// <summary>
		/// Initialize new <see cref="Window"/> instance.
		/// </summary>
		protected Window()
		{
			this.Application.VerifyAccess();
			this.Logger = this.Application.LoggerFactory.CreateLogger(this.GetType().Name);
			this.AddHandler(PointerWheelChangedEvent, (_, e) =>
			{
				if (this.HasDialogs)
					e.Handled = true;
			}, Avalonia.Interactivity.RoutingStrategies.Tunnel);
		}


		/// <summary>
		/// Get application instance.
		/// </summary>
		public IApplication Application { get; } = CarinaStudio.Application.Current;


		/// <summary>
		/// Get whether at least one <see cref="Dialog{TApp}"/> owned by this window is shown or not.
		/// </summary>
		public bool HasDialogs { get => this.GetValue<bool>(HasDialogsProperty); }


		/// <summary>
		/// Check whether window is closed or not.
		/// </summary>
		public bool IsClosed { get => this.GetValue<bool>(IsClosedProperty); }


		/// <summary>
		/// Check whether window is opened or not.
		/// </summary>
		public bool IsOpened { get => this.GetValue<bool>(IsOpenedProperty); }


		/// <summary>
		/// Get logger.
		/// </summary>
		protected ILogger Logger { get; }


		/// <summary>
		/// Called when window closed.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnClosed(EventArgs e)
		{
			this.SetValue<bool>(IsOpenedProperty, false);
			this.SetValue<bool>(IsClosedProperty, true);
			base.OnClosed(e);
		}


		/// <summary>
		/// Called when dialog closed.
		/// </summary>
		/// <param name="dialog">Closed dialog.</param>
		internal protected virtual void OnDialogClosed(Dialog dialog)
		{
			if (this.dialogs.Remove(dialog) && this.dialogs.IsEmpty())
				this.SetValue<bool>(HasDialogsProperty, false);
		}


		/// <summary>
		/// Called when dialog opened.
		/// </summary>
		/// <param name="dialog">Opened dialog.</param>
		internal protected virtual void OnDialogOpened(Dialog dialog)
		{
			this.dialogs.Add(dialog);
			if (this.dialogs.Count == 1)
				this.SetValue<bool>(HasDialogsProperty, true);
		}


		/// <summary>
		/// Called when window opened.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnOpened(EventArgs e)
		{
			this.SetValue<bool>(IsOpenedProperty, true);
			base.OnOpened(e);
		}


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
	/// <see cref="Avalonia.Controls.Window"/> which implements <see cref="IApplicationObject{TApplication}"/>.
	/// </summary>
	/// <typeparam name="TApp">Type of application.</typeparam>
	public abstract class Window<TApp> : Window, IApplicationObject<TApp> where TApp : class, IApplication
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
