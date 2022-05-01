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
		public static readonly AvaloniaProperty<bool> HasDialogsProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(HasDialogs), w => w.hasDialogs);
		/// <summary>
		/// Property of <see cref="IsClosed"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsClosedProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(IsClosed), w => w.isClosed);
		/// <summary>
		/// Property of <see cref="IsOpened"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsOpenedProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(IsOpened), w => w.isOpened);


		// Fields.
		readonly List<Dialog> dialogs = new List<Dialog>();
		bool hasDialogs;
		bool isClosed;
		bool isOpened;


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
		public bool HasDialogs { get => this.hasDialogs; }


		/// <summary>
		/// Check whether window is closed or not.
		/// </summary>
		public bool IsClosed { get => this.isClosed; }


		/// <summary>
		/// Check whether window is opened or not.
		/// </summary>
		public bool IsOpened { get => this.isOpened; }


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
			this.SetAndRaise<bool>(IsOpenedProperty, ref this.isOpened, false);
			this.SetAndRaise<bool>(IsClosedProperty, ref this.isClosed, true);
			base.OnClosed(e);
		}


		/// <summary>
		/// Called when dialog closed.
		/// </summary>
		/// <param name="dialog">Closed dialog.</param>
		internal protected virtual void OnDialogClosed(Dialog dialog)
		{
			if (this.dialogs.Remove(dialog) && this.dialogs.IsEmpty())
				this.SetAndRaise<bool>(HasDialogsProperty, ref this.hasDialogs, false);
		}


		/// <summary>
		/// Called when dialog opened.
		/// </summary>
		/// <param name="dialog">Opened dialog.</param>
		internal protected virtual void OnDialogOpened(Dialog dialog)
		{
			this.dialogs.Add(dialog);
			if (this.dialogs.Count == 1)
				this.SetAndRaise<bool>(HasDialogsProperty, ref this.hasDialogs, true);
		}


		/// <summary>
		/// Called when window opened.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnOpened(EventArgs e)
		{
			this.SetAndRaise<bool>(IsOpenedProperty, ref this.isOpened, true);
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
