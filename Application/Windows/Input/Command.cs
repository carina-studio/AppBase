using System;
using System.Windows.Input;

namespace CarinaStudio.Windows.Input
{
	/// <summary>
	/// Implementation of <see cref="ICommand"/>.
	/// </summary>
	public class Command : BaseCommand
	{
		// Fields.
		readonly Action action;


		/// <summary>
		/// Initialize new <see cref="Command"/> instance.
		/// </summary>
		/// <param name="execute">Action to execute command.</param>
		/// <param name="canExecute"><see cref="IObservable{T}"/> to indicate whether command can be executed or not.</param>
		public Command(Action execute, IObservable<bool>? canExecute = null) : base(canExecute)
		{
			this.action = execute;
		}


		/// <summary>
		/// Execute command.
		/// </summary>
		/// <param name="parameter">Parameter.</param>
		public override void Execute(object? parameter) => this.action();
	}


	/// <summary>
	/// Implementation of <see cref="ICommand"/>.
	/// </summary>
	public class Command<TParam> : BaseCommand
	{
		// Fields.
		readonly Action<TParam> action;


		/// <summary>
		/// Initialize new <see cref="Command"/> instance.
		/// </summary>
		/// <param name="execute">Action to execute command.</param>
		/// <param name="canExecute"><see cref="IObservable{T}"/> to indicate whether command can be executed or not.</param>
		public Command(Action<TParam> execute, IObservable<bool>? canExecute = null) : base(canExecute)
		{
			this.action = execute;
		}


#pragma warning disable CS8604
		/// <summary>
		/// Execute command.
		/// </summary>
		/// <param name="parameter">Parameter.</param>
		public override void Execute(object? parameter) => this.action((TParam)parameter);
#pragma warning restore CS8604
	}
}
