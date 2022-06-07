using Avalonia;
using Avalonia.Controls;
using CarinaStudio.Threading;
using System;
using System.Diagnostics;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Base class of window of dialog.
	/// </summary>
	public abstract class Dialog : Window
	{
		// Constants.
		const int SizeCorrectionTimeout = 1000;


		// Fields.
		double? desiredWidth;
		long openedTime;
		readonly Stopwatch stopWatch = new Stopwatch().Also(it => it.Start());


		/// <summary>
		/// Initialize new <see cref="Dialog{TApp}"/> instance.
		/// </summary>
		protected Dialog()
		{ }


		/// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
			// keep desired size in first measuring
			if (!this.desiredWidth.HasValue)
				this.desiredWidth = this.Width;

			// call base
            var size = base.MeasureOverride(availableSize);

			// [Workaround] Restore to desired size on Linux to prevent incorrect window size when 'AVALONIA_SCREEN_SCALE_FACTORS' is set
			if (Platform.IsLinux && double.IsFinite(this.desiredWidth.Value))
			{
				if (this.openedTime <= 0 || (this.stopWatch.ElapsedMilliseconds - this.openedTime) <= SizeCorrectionTimeout)
				{
					if (size.Width < this.desiredWidth.Value)
						size = new Size(this.desiredWidth.Value, size.Height);
				}
			}

			// complete
			return size;
        }


		/// <inheritdoc/>
		protected override void OnClosed(EventArgs e)
		{
			this.stopWatch.Stop();
			base.OnClosed(e);
		}


		/// <inheritdoc/>
		protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyModifiers == 0 && e.Key == Avalonia.Input.Key.Escape)
				this.Close();
		}


		/// <inheritdoc/>
		protected override void OnOpened(EventArgs e)
		{
			// keep time
			this.openedTime = this.stopWatch.ElapsedMilliseconds;

			// call base
			base.OnOpened(e);

			// use icon from owner window
			var owner = this.Owner as Avalonia.Controls.Window;
			if (this.Icon == null)
				this.Icon = owner?.Icon;

			// [workaround] move to center of owner or screen
			switch (this.WindowStartupLocation)
			{
				case WindowStartupLocation.CenterScreen:
					if ((Platform.IsWindows && (owner == null || owner.WindowState == WindowState.Maximized))
						|| Platform.IsLinux)
					{
						this.Screens.ScreenFromVisual(this)?.Let(screen =>
						{
							var screenBounds = screen.WorkingArea;
							var pixelDensity = screen.PixelDensity;
							var width = this.Width * pixelDensity;
							var height = this.Height * pixelDensity;
							var position = new PixelPoint((int)(screenBounds.X + (screenBounds.Width - width) / 2), (int)(screenBounds.Y + (screenBounds.Height - height) / 2));
							this.WindowStartupLocation = WindowStartupLocation.Manual;
							this.SynchronizationContext.Post(() => this.Position = position);
							this.SynchronizationContext.PostDelayed(() => this.Position = position, 100);
						});
					}
					break;
				case WindowStartupLocation.CenterOwner:
					if (Platform.IsLinux)
					{
						owner?.Let(owner =>
						{
							var position = owner.Position.Let((position) =>
							{
								var screenScale = owner.Screens.ScreenFromVisual(owner)?.PixelDensity ?? 1.0;
								var offsetX = (int)((owner.Width - this.Width) / 2 * screenScale);
								var offsetY = (int)((owner.Height - this.Height) / 2 * screenScale);
								return new PixelPoint(position.X + offsetX, position.Y + offsetY);
							});
							this.WindowStartupLocation = WindowStartupLocation.Manual;
							this.SynchronizationContext.Post(() => this.Position = position);
							this.SynchronizationContext.PostDelayed(() => this.Position = position, 100);
						});
					}
					break;
			}
		}
	}


	/// <summary>
	/// Base class of window of dialog.
	/// </summary>
	/// <typeparam name="TApp">Type of application.</typeparam>
	public abstract class Dialog<TApp> : Dialog where TApp : class, IApplication
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
