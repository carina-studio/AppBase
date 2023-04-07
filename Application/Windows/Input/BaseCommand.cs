using System;
using System.Windows.Input;

namespace CarinaStudio.Windows.Input
{
	/// <summary>
	/// Base implementation of <see cref="ICommand"/>.
	/// </summary>
	public abstract class BaseCommand : ICommand
	{
		// Fields.
		bool canExecute = true;


		/// <summary>
		/// Initialize new <see cref="BaseCommand"/> instance.
		/// </summary>
		/// <param name="canExecute"><see cref="IObservable{T}"/> to indicate whether command can be executed or not.</param>
		protected BaseCommand(IObservable<bool>? canExecute)
		{
			if (canExecute != null)
			{
				canExecute.Subscribe(new Observer<bool>(value =>
				{
					if (this.canExecute != value)
					{
						this.canExecute = value;
						this.InvalidateCanExecute();
					}
				}));
				if (canExecute is ObservableValue<bool> observableValue)
					this.canExecute = observableValue.Value;
			}
		}


		/// <summary>
		/// Check whether command can be executed or not.
		/// </summary>
		/// <param name="parameter">Parameter.</param>
		/// <returns></returns>
		public virtual bool CanExecute(object? parameter) => this.canExecute;


		/// <summary>
		/// Raised when result of <see cref="CanExecute(object?)"/> changed.
		/// </summary>
		public event EventHandler? CanExecuteChanged;


		/// <summary>
		/// Execute command.
		/// </summary>
		/// <param name="parameter">Parameter.</param>
		public abstract void Execute(object? parameter);
		
		
		/// <summary>
		/// Raise <see cref="CanExecuteChanged"/> event.
		/// </summary>
		protected void InvalidateCanExecute() => 
			this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}
}
