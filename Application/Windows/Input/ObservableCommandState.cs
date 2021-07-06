using System;
using System.Windows.Input;

namespace CarinaStudio.Windows.Input
{
	/// <summary>
	/// <see cref="IObservable{T}"/> which reflects <see cref="ICommand.CanExecute(object)"/>.
	/// </summary>
	/// <remarks><see cref="ObservableCommandState"/> uses weak event handler to monitor state of <see cref="ICommand"/> so it is not necessary to unbind from <see cref="ICommand"/> explicitly.</remarks>
	public class ObservableCommandState : ObservableValue<bool>
	{
		// Handler for CanExecuteChanged.
		class CanExecuteChangedHandler
		{
			// Fields.
			readonly WeakReference<ObservableCommandState> ownerReference;

			// Constructor.
			public CanExecuteChangedHandler(ObservableCommandState owner)
			{
				this.ownerReference = new WeakReference<ObservableCommandState>(owner);
			}

			// Called when CanExecuteChanged.
			public void OnCanExecuteChanged(object? sender, EventArgs e)
			{
				if (this.ownerReference.TryGetTarget(out var owner))
					owner.OnCanExecuteChanged();
			}
		}


		// Fields.
		readonly CanExecuteChangedHandler canExecuteChangedHandler;
		readonly bool defaultValue;


		/// <summary>
		/// Initialize new <see cref="ObservableCommandState"/> instance.
		/// </summary>
		/// <param name="command"><see cref="ICommand"/> to bind to.</param>
		/// <param name="defaultValue">Default value used when no <see cref="ICommand"/> bound.</param>
		public ObservableCommandState(ICommand? command = null, bool defaultValue = default)
		{
			this.canExecuteChangedHandler = new CanExecuteChangedHandler(this);
			this.defaultValue = defaultValue;
			if (command != null)
				this.Bind(command);
			else
				this.Value = this.defaultValue;
		}


		/// <summary>
		/// Finalizer.
		/// </summary>
		~ObservableCommandState()
		{
			this.Command?.Let(it => it.CanExecuteChanged -= this.canExecuteChangedHandler.OnCanExecuteChanged);
		}


		/// <summary>
		/// Bind to <see cref="ICommand"/>.
		/// </summary>
		/// <param name="command"><see cref="ICommand"/> to bind to.</param>
		/// <exception cref="InvalidOperationException">Instance is already bound to another command.</exception>
		public void Bind(ICommand command)
		{
			if (this.Command != null)
			{
				if (this.Command == command)
					return;
				throw new InvalidOperationException("Already bound to another command.");
			}
			this.Command = command;
			command.CanExecuteChanged += this.canExecuteChangedHandler.OnCanExecuteChanged;
			this.Value = command.CanExecute(null);
		}


		/// <summary>
		/// Get <see cref="ICommand"/> currently bound by this instance.
		/// </summary>
		public ICommand? Command { get; private set; }


		// Called when CanExecuteChanged.
		void OnCanExecuteChanged()
		{
			this.Value = this.Command?.CanExecute(null) ?? this.defaultValue;
		}


		/// <summary>
		/// Unbind from current <see cref="ICommand"/>.
		/// </summary>
		public void Unbind()
		{
			if (this.Command == null)
				return;
			this.Command.CanExecuteChanged -= this.canExecuteChangedHandler.OnCanExecuteChanged;
			this.Command = null;
			this.Value = this.defaultValue;
		}
	}
}
