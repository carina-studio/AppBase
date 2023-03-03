using Avalonia;
using Avalonia.Controls;
using CarinaStudio.Collections;
using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Extended <see cref="Avalonia.Controls.Window"/>.
	/// </summary>
	public class Window : Avalonia.Controls.Window
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
		/// <summary>
		/// Property of <see cref="IsShownAsDialog"/>.
		/// </summary>
		public static readonly AvaloniaProperty<bool> IsShownAsDialogProperty = AvaloniaProperty.RegisterDirect<Window, bool>(nameof(IsShownAsDialog), w => w.isShownAsDialog);


		// Constants.
		const int InitSizeCorrectionTimeout = 100;


		// Fields.
		readonly ScheduledAction checkDialogsAction;
		readonly IList<(Avalonia.Controls.Window, bool)> children;
		readonly ScheduledAction clearInitSizeObserversAction;
		PixelPoint? expectedInitPosition;
		Size? expectedInitSize;
		bool hasDialogs;
		readonly IDisposable initHeightObserverToken;
		readonly IDisposable initWidthObserverToken;
		bool isActiveBeforeClosing;
		bool isClosed;
		bool isOpened;
		bool isShownAsDialog;
		Window? owner;


		/// <summary>
		/// Initialize new <see cref="Window"/> instance.
		/// </summary>
		public Window()
		{
			// setup actions
			this.checkDialogsAction = new ScheduledAction(() =>
			{
				var hasDialogs = false;
				static void RefreshChildWindowPositions(Avalonia.Controls.Window parent)
				{
					var childWindows = parent is Window window
						? window.children
						: GetInternalChildWindows(parent);
					if (childWindows == null || childWindows.Count == 0)
						return;
					foreach (var (childWindow, isDialog) in childWindows)
					{
						if (!isDialog)
							childWindow.Activate();
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
					this.SetAndRaise(HasDialogsProperty, ref this.hasDialogs, hasDialogs);
			});
			this.clearInitSizeObserversAction = new(() =>
			{
				this.initHeightObserverToken!.Dispose();
				this.initWidthObserverToken!.Dispose();
			});

			// get internal list of child windows
			this.children = GetInternalChildWindows(this) ?? Global.Run(() =>
			{
				System.Diagnostics.Debug.WriteLine("Unable to get list of child window");
				return Array.Empty<(Avalonia.Controls.Window, bool)>();
			});
			
			// attach to self
			this.initHeightObserverToken = this.GetObservable(HeightProperty).Subscribe(this.OnInitialHeightChanged);
			this.initWidthObserverToken = this.GetObservable(WidthProperty).Subscribe(this.OnInitialWidthChanged);
			this.GetObservable(IsActiveProperty).Subscribe(isActive => 
			{
				if (isActive)
					this.checkDialogsAction.Schedule();
				if (!this.isClosed)
					this.isActiveBeforeClosing = isActive;
			});
			this.AddHandler(PointerWheelChangedEvent, (_, e) =>
			{
				if (this.HasDialogs)
					e.Handled = true;
			}, Avalonia.Interactivity.RoutingStrategies.Tunnel);
			this.PositionChanged += this.OnInitialPositionChanged;
		}


		// Get internal list of child windows.
		static IList<(Avalonia.Controls.Window, bool)>? GetInternalChildWindows(Avalonia.Controls.Window window) =>
			typeof(Avalonia.Controls.Window).GetField("_children", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window) as IList<ValueTuple<Avalonia.Controls.Window, bool>>;
		

		// Check whether the window or one of its child window is active or not.
		static bool HasActiveWindow(Avalonia.Controls.Window window)
		{
			if (window.IsActive)
				return true;
			var childWindows = window is Window csWindow
				? csWindow.children
				: GetInternalChildWindows(window);
			if (childWindows.IsNotEmpty())
			{
				foreach (var (childWindow, _) in childWindows)
				{
					if (HasActiveWindow(childWindow))
						return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Get whether at least one dialog owned by this window is shown or not.
		/// </summary>
		public bool HasDialogs { get => this.hasDialogs; }


		/// <summary>
		/// Check whether window is closed or not.
		/// </summary>
		public bool IsClosed { get => this.isClosed; }


		// Check whether given window is shown as dialog or not.
		internal static bool IsDialogWindow(Avalonia.Controls.Window? parent, Avalonia.Controls.Window window)
		{
			if (parent == null)
				return WindowExtensions.IsDialogWindow(window);
			var childWindows = parent is Window csWindow
				? csWindow.children
				: GetInternalChildWindows(parent);
			if (childWindows == null || childWindows.Count == 0)
				return false;
			foreach (var (candidateWindow, isDialog) in childWindows)
			{
				if (candidateWindow == window)
					return isDialog;
			}
			return false;
		}


		/// <summary>
		/// Check whether window is opened or not.
		/// </summary>
		public bool IsOpened { get => this.isOpened; }


		/// <summary>
		/// Check whether window is shown as dialog or not.
		/// </summary>
		public bool IsShownAsDialog { get => this.isShownAsDialog; }


		// Called when child window opened or closed.
		internal void OnChildWindowOpenedOrClosed(bool isActiveBefore)
		{
			if (isActiveBefore || HasActiveWindow(this))
				this.checkDialogsAction.Schedule();
		}


		/// <summary>
		/// Called when window closed.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnClosed(EventArgs e)
		{
			this.owner?.OnChildWindowOpenedOrClosed(this.isActiveBeforeClosing);
			this.SetAndRaise(IsOpenedProperty, ref this.isOpened, false);
			this.SetAndRaise(IsClosedProperty, ref this.isClosed, true);
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
			this.owner?.OnChildWindowOpenedOrClosed(false);

			// update state
			this.SetAndRaise(IsShownAsDialogProperty, ref this.isShownAsDialog, IsDialogWindow(this.Owner as Avalonia.Controls.Window, this));
			this.SetAndRaise(IsOpenedProperty, ref this.isOpened, true);

			// call base
			base.OnOpened(e);

			// [Workaround] move to actual center of owner/screen on Linux.
			if (Platform.IsLinux)
			{
				var titleBarHeightInPixels = 75; // Not an accurate value
				switch (this.WindowStartupLocation)
				{
					case WindowStartupLocation.CenterOwner:
					{
						this.owner?.Let(owner =>
						{
							var screenScale = owner.Screens.ScreenFromVisual(owner)?.Scaling ?? 1.0;
							var titleBarHeight = titleBarHeightInPixels / screenScale;
							var width = this.Width;
							var height = this.Height;
							if (double.IsFinite(width) && double.IsFinite(height))
							{
								PixelPoint ownerPosition;
								Size ownerSize;
								var heightWithTitleBar = height + titleBarHeight;
								if (owner is CarinaStudio.Controls.Window csWindow)
								{
									ownerPosition = csWindow.expectedInitPosition?.Let(it =>
									{
										return new PixelPoint(it.X, (int)(it.Y + titleBarHeight * screenScale + 0.5));
									}) ?? csWindow.Position;
									ownerSize = csWindow.expectedInitSize ?? new(csWindow.Width, csWindow.Height);
								}
								else
								{
									ownerPosition = owner.Position;
									ownerSize = new(owner.Width, owner.Height);
								}
								var offsetX = (int)((ownerSize.Width - width) / 2 * screenScale + 0.5);
								var offsetY = (int)((ownerSize.Height + titleBarHeight - heightWithTitleBar) / 2 * screenScale + 0.5);
								var position = new PixelPoint(ownerPosition.X + offsetX, ownerPosition.Y + offsetY - (int)(titleBarHeight * screenScale + 0.5));
								this.expectedInitPosition = position;
								this.expectedInitSize = new(width, height);
								this.WindowStartupLocation = WindowStartupLocation.Manual;
							}
						});
						break;
					}

					case WindowStartupLocation.CenterScreen:
					{
						(this.Screens.ScreenFromWindow(this.PlatformImpl!) ?? this.Screens.Primary)?.Let(screen =>
						{
							var screenScale = screen.Scaling;
							var workingArea = screen.WorkingArea;
							var titleBarHeight = titleBarHeightInPixels / screenScale;
							var width = this.Width;
							var height = this.Height;
							if (double.IsFinite(width) && double.IsFinite(height))
							{
								var heightWithTitleBar = height + titleBarHeight;
								var offsetX = (int)((workingArea.Width - (width * screenScale)) / 2 + 0.5);
								var offsetY = (int)((workingArea.Height - (heightWithTitleBar * screenScale)) / 2 + 0.5);
								var position = new PixelPoint(workingArea.TopLeft.X + offsetX, workingArea.TopLeft.Y + offsetY);
								this.expectedInitPosition = position;
								this.expectedInitSize = new(width, height);
								this.WindowStartupLocation = WindowStartupLocation.Manual;
							}
						});
						break;
					}
				}
			}
			else
				this.clearInitSizeObserversAction.Execute();
		}
	}
}
