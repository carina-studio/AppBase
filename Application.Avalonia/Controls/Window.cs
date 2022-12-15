using Avalonia;
using Avalonia.Controls;
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
		public static readonly AvaloniaProperty<bool> HasDialogsProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(HasDialogs), w => w.hasDialogs);
		/// <summary>
		/// Property of <see cref="IsClosed"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsClosedProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(IsClosed), w => w.isClosed);
		/// <summary>
		/// Property of <see cref="IsOpened"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsOpenedProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(IsOpened), w => w.isOpened);


		// Constants.
		const int InitSizeCorrectionTimeout = 100;


		// Static fields.
		static readonly Version AvaloniaVersion = typeof(AvaloniaObject).Assembly.GetName()?.Version ?? new Version();


		// Fields.
		readonly ScheduledAction checkDialogsAction;
		readonly IList<(Avalonia.Controls.Window, bool)> children;
		readonly ScheduledAction clearInitSizeObserversAction;
		PixelPoint? expectedInitPosition;
		Size? expectedInitSize;
		bool hasDialogs;
		IDisposable initHeightObserverToken = EmptyDisposable.Default;
		IDisposable initWidthObserverToken = EmptyDisposable.Default;
		bool isClosed;
		bool isOpened;
		Window? owner;


		/// <summary>
		/// Initialize new <see cref="Window"/> instance.
		/// </summary>
		protected Window()
		{
			// check state
			this.Application.VerifyAccess();

			// create logger
			this.Logger = this.Application.LoggerFactory.CreateLogger(this.GetType().Name);

			// setup actions
			this.checkDialogsAction = new ScheduledAction(() =>
			{
				var hasDialogs = false;
				var isAvalonia_0_10_15_OrAbove = AvaloniaVersion.Major == 0 
					&& (AvaloniaVersion.Minor > 10 || AvaloniaVersion.Build >= 15);
				static void RefreshChildWindowPositions(Avalonia.Controls.Window parent)
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
			this.clearInitSizeObserversAction = new(() =>
			{
				this.initHeightObserverToken.Dispose();
				this.initWidthObserverToken.Dispose();
			});

			// get internal list of child windows
			this.children = GetInternalChildWindows(this) ?? Global.Run(() =>
			{
				this.Logger.LogError("Unable to get list of child window");
				return Array.Empty<(Avalonia.Controls.Window, bool)>();
			});
			
			// attach to self
			this.initHeightObserverToken = this.GetObservable(HeightProperty).Subscribe(this.OnInitialHeightChanged);
			this.initWidthObserverToken = this.GetObservable(WidthProperty).Subscribe(this.OnInitialWidthChanged);
			this.GetObservable(IsActiveProperty).Subscribe(_ => this.checkDialogsAction.Schedule());
			this.AddHandler(PointerWheelChangedEvent, (_, e) =>
			{
				if (this.HasDialogs)
					e.Handled = true;
			}, Avalonia.Interactivity.RoutingStrategies.Tunnel);
			this.PositionChanged += this.OnInitialPositionChanged;
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


		// Called when initial height of window changed.
		void OnInitialHeightChanged(double height)
		{
			if (this.expectedInitSize.HasValue)
			{
				if (this.IsOpened)
				{
					var expectedSize = this.expectedInitSize.Value;
					if (Math.Abs(height - expectedSize.Height) <= 0.1)
						this.clearInitSizeObserversAction.Schedule(InitSizeCorrectionTimeout);
					else
					{
						this.Height = expectedSize.Height;
						this.clearInitSizeObserversAction.Reschedule(InitSizeCorrectionTimeout);
					}
				}
			}
			else if (this.IsOpened)
				this.clearInitSizeObserversAction.Execute();
		}


		// Called when initial position of window changed.
		void OnInitialPositionChanged(object? sender, PixelPointEventArgs e)
		{
			if (this.expectedInitPosition.HasValue)
			{
				if (this.IsOpened)
				{
					this.Position = this.expectedInitPosition.Value;
					this.PositionChanged -= this.OnInitialPositionChanged;
					this.expectedInitPosition = null;
				}
			}
			else if (this.IsOpened)
				this.PositionChanged -= this.OnInitialPositionChanged;
		}


		// Called when initial width of window changed.
		void OnInitialWidthChanged(double width)
		{
			if (this.expectedInitSize.HasValue)
			{
				if (this.IsOpened)
				{
					var expectedSize = this.expectedInitSize.Value;
					if (Math.Abs(width - expectedSize.Width) <= 0.1)
						this.clearInitSizeObserversAction.Schedule(InitSizeCorrectionTimeout);
					else
					{
						this.Width = expectedSize.Width;
						this.clearInitSizeObserversAction.Reschedule(InitSizeCorrectionTimeout);
					}
				}
			}
			else if (this.IsOpened)
				this.clearInitSizeObserversAction.Execute();
		}


		/// <summary>
		/// Called when window opened.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnOpened(EventArgs e)
		{
			// notify owner
			this.owner = this.Owner as Window;
			this.owner?.OnChildWindowOpenedOrClosed();

			// update state
			this.SetAndRaise<bool>(IsOpenedProperty, ref this.isOpened, true);

			// call base
			base.OnOpened(e);

			// [Workaround] move to actual center of owner on Linux.
			if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner && Platform.IsLinux)
			{
				this.owner?.Let(owner =>
				{
					var screenScale = owner.Screens.ScreenFromVisual(owner)?.Scaling ?? 1.0;
					var titleBarHeight = (Platform.IsGnome ? 75 : 0) / screenScale;
					var width = this.Width;
					var height = this.Height;
					if (double.IsFinite(width) && double.IsFinite(height))
					{
						var heightWithTitleBar = height + titleBarHeight;
						var position = owner.Position.Let((position) =>
						{
							var offsetX = (int)((owner.Width - width) / 2 * screenScale + 0.5);
							var offsetY = (int)((owner.Height + titleBarHeight - heightWithTitleBar) / 2 * screenScale + 0.5);
							return new PixelPoint(position.X + offsetX, position.Y + offsetY - (int)(titleBarHeight * screenScale + 0.5));
						});
						this.expectedInitPosition = position;
						this.expectedInitSize = new(width, height);
						this.WindowStartupLocation = WindowStartupLocation.Manual;
					}
				});
			}
			else
				this.clearInitSizeObserversAction.Execute();
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
