using System.Threading.Tasks;
using Avalonia;
using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
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
		public static readonly DirectProperty<Window, bool> HasDialogsProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(HasDialogs), w => w.hasDialogs);
		/// <summary>
		/// Property of <see cref="IsClosed"/>.
		/// </summary>
		public static readonly DirectProperty<Window, bool> IsClosedProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(IsClosed), w => w.isClosed);
		/// <summary>
		/// Property of <see cref="IsOpened"/>.
		/// </summary>
		public static readonly DirectProperty<Window, bool> IsOpenedProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(IsOpened), w => w.isOpened);


		// Static fields.
		static readonly Version AvaloniaVersion = typeof(AvaloniaObject).Assembly.GetName()?.Version ?? new Version();


		// Fields.
		readonly ScheduledAction checkDialogsAction;
		readonly IList<(Avalonia.Controls.Window, bool)> children;
		bool hasDialogs;
		bool isClosed;
		bool isOpened;
		Window? owner;


		/// <summary>
		/// Initialize new <see cref="Window"/> instance.
		/// </summary>
		protected Window()
		{
			this.Application.VerifyAccess();
			this.Logger = this.Application.LoggerFactory.CreateLogger(this.GetType().Name);
			this.checkDialogsAction = new ScheduledAction(() =>
			{
				var hasDialogs = false;
				var isAvalonia_0_10_15_OrAbove = AvaloniaVersion.Major == 0 
					&& (AvaloniaVersion.Minor > 10 || AvaloniaVersion.Build >= 15);
				void RefreshChildWindowPositions(Avalonia.Controls.Window parent)
				{
					var childWindows = parent is Window window
						? window.children
						: GetInternalChildWindows(parent);
					if (childWindows == null || childWindows.Count == 0)
						return;
					foreach (var (childWindow, isDialog) in childWindows)
					{
						if (!childWindow.Topmost)
						{
							childWindow.Topmost = true;
							childWindow.Topmost = false;
						}
						RefreshChildWindowPositions(childWindow);
					}
				}
				foreach (var (childWindow, isDialog) in this.children!)
				{
					if (isDialog)
					{
						hasDialogs = true;
						break;
					}
				}
				RefreshChildWindowPositions(this);
				if (this.hasDialogs != hasDialogs)
					this.SetAndRaise<bool>(HasDialogsProperty, ref this.hasDialogs, hasDialogs);
			});
			this.children = GetInternalChildWindows(this) ?? Global.Run(() =>
			{
				this.Logger.LogError("Unable to get list of child window");
				return new (Avalonia.Controls.Window, bool)[0];
			});
			this.GetObservable(IsActiveProperty).Subscribe(_ => this.checkDialogsAction.Schedule());
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


		// Get internal list of child windows.
		static IList<(Avalonia.Controls.Window, bool)>? GetInternalChildWindows(Avalonia.Controls.Window window) =>
			typeof(Avalonia.Controls.Window).GetField("_children", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window) as IList<ValueTuple<Avalonia.Controls.Window, bool>>;


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


		// Called when child window opened or closed.
		internal void OnChildWindowOpenedOrClosed() =>
			this.checkDialogsAction.Schedule();


		/// <summary>
		/// Called when window closed.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnClosed(EventArgs e)
		{
			this.owner?.OnChildWindowOpenedOrClosed();
			this.SetAndRaise<bool>(IsOpenedProperty, ref this.isOpened, false);
			this.SetAndRaise<bool>(IsClosedProperty, ref this.isClosed, true);
			base.OnClosed(e);
			this.owner = null;
		}


		/// <summary>
		/// Called when window opened.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnOpened(EventArgs e)
		{
			this.owner = this.Owner as Window;
			this.owner?.OnChildWindowOpenedOrClosed();
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
