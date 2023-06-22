using System;

namespace CarinaStudio.Controls
{
	/// <summary>
	/// Base class of window of dialog.
	/// </summary>
	public class Dialog : ApplicationWindow
	{
		/// <summary>
		/// Initialize new <see cref="Dialog{TApp}"/> instance.
		/// </summary>
		public Dialog()
		{ }


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
			// use icon from owner window
			this.Icon ??= (this.Owner as Avalonia.Controls.Window)?.Icon;

			// call base
			base.OnOpened(e);
		}
	}


	/// <summary>
	/// Base class of window of dialog.
	/// </summary>
	/// <typeparam name="TApp">Type of application.</typeparam>
	public class Dialog<TApp> : Dialog where TApp : class, IApplication
	{
		/// <summary>
		/// Get application instance.
		/// </summary>
		public new TApp Application => (base.Application as TApp) ?? throw new ArgumentException($"Application doesn't implement {typeof(TApp)} interface.");
	}
}
