using Avalonia;
using Avalonia.Controls;
using System;
using System.Runtime.InteropServices;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Base class of window of dialog.
	/// </summary>
	/// <typeparam name="TApp">Type of application.</typeparam>
	public abstract class Dialog<TApp> : Window<TApp> where TApp : class, IApplication
	{
		// Fields.
		Window<TApp>? ownerWindow;


		/// <summary>
		/// Initialize new <see cref="Dialog{TApp}"/> instance.
		/// </summary>
		protected Dialog()
		{ }


		/// <summary>
		/// Called when window closed.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnClosed(EventArgs e)
		{
			this.ownerWindow?.OnDialogClosed(this);
			this.ownerWindow = null;
			base.OnClosed(e);
		}


		/// <summary>
		/// Called when key up.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyModifiers == 0 && e.Key == Avalonia.Input.Key.Escape)
				this.Close();
		}


		/// <summary>
		/// Called when window opened.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnOpened(EventArgs e)
		{
			// call base
			base.OnOpened(e);

			// notify owner window
			this.ownerWindow = (this.Owner as Window<TApp>)?.Also(it => it.OnDialogOpened(this));

			// use icon from owner window
			if (this.Icon == null)
				this.Icon = this.ownerWindow?.Icon;

			// [workaround] move to center of owner for Linux
			if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				(this.Owner as Window)?.Let((owner) =>
				{
					this.WindowStartupLocation = WindowStartupLocation.Manual;
					this.Position = owner.Position.Let((position) =>
					{
						var screenScale = owner.Screens.ScreenFromVisual(owner).PixelDensity;
						var offsetX = (int)((owner.Width - this.Width) / 2 * screenScale);
						var offsetY = (int)((owner.Height - this.Height) / 2 * screenScale);
						return new PixelPoint(position.X + offsetX, position.Y + offsetY);
					});
				});
			}
		}
	}
}
