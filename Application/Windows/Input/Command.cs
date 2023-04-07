using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CarinaStudio.Windows.Input
{
	/// <summary>
	/// Implementation of <see cref="ICommand"/>.
	/// </summary>
	public class Command : BaseCommand
	{
		// Fields.
		readonly Action? action;
		readonly Func<Task>? asyncAction;
		bool isExecutingAsyncAction;


		/// <summary>
		/// Initialize new <see cref="Command"/> instance.
		/// </summary>
		/// <param name="execute">Action to execute command.</param>
		/// <param name="canExecute"><see cref="IObservable{T}"/> to indicate whether command can be executed or not.</param>
		public Command(Action execute, IObservable<bool>? canExecute = null) : base(canExecute)
		{
			this.action = execute;
			this.asyncAction = null;
		}
		
		
		/// <summary>
		/// Initialize new <see cref="Command"/> instance.
		/// </summary>
		/// <param name="asyncExecute">Asynchronous action to execute command.</param>
		/// <param name="canExecute"><see cref="IObservable{T}"/> to indicate whether command can be executed or not.</param>
		public Command(Func<Task> asyncExecute, IObservable<bool>? canExecute = null) : base(canExecute)
		{
			this.action = null;
			this.asyncAction = asyncExecute;
		}


		/// <inheritdoc/>
		public override bool CanExecute(object? parameter) =>
			base.CanExecute(parameter) && !this.isExecutingAsyncAction;


		/// <summary>
		/// Execute command.
		/// </summary>
		/// <param name="parameter">Parameter.</param>
		public override async void Execute(object? parameter)
		{
			if (!this.CanExecute(parameter))
				return;
			if (this.action is not null)
				this.action();
			else if (this.asyncAction is not null)
			{
				this.isExecutingAsyncAction = true;
				this.InvalidateCanExecute();
				try
				{
					await this.asyncAction();
				}
				finally
				{
					this.isExecutingAsyncAction = false;
					this.InvalidateCanExecute();
				}
			}
		}
	}


	/// <summary>
	/// Implementation of <see cref="ICommand"/>.
	/// </summary>
	public class Command<TParam> : BaseCommand
	{
		// Fields.
		readonly Action<TParam>? action;
		readonly Func<TParam, Task>? asyncAction;
		bool isExecutingAsyncAction;


		/// <summary>
		/// Initialize new <see cref="Command"/> instance.
		/// </summary>
		/// <param name="execute">Action to execute command.</param>
		/// <param name="canExecute"><see cref="IObservable{T}"/> to indicate whether command can be executed or not.</param>
		public Command(Action<TParam> execute, IObservable<bool>? canExecute = null) : base(canExecute)
		{
			this.action = execute;
			this.asyncAction = null;
		}
		
		
		/// <summary>
		/// Initialize new <see cref="Command"/> instance.
		/// </summary>
		/// <param name="asyncExecute">Asynchronous action to execute command.</param>
		/// <param name="canExecute"><see cref="IObservable{T}"/> to indicate whether command can be executed or not.</param>
		public Command(Func<TParam, Task> asyncExecute, IObservable<bool>? canExecute = null) : base(canExecute)
		{
			this.action = null;
			this.asyncAction = asyncExecute;
		}


		/// <inheritdoc/>
		public override bool CanExecute(object? parameter)
		{
			if (!base.CanExecute(parameter) || this.isExecutingAsyncAction)
				return false;
			var expectedParamType = typeof(TParam);
			if (expectedParamType.IsValueType)
			{
				if (expectedParamType.IsGenericType && expectedParamType.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					if (parameter is null)
						return true;
					return expectedParamType.GenericTypeArguments[0].IsInstanceOfType(parameter);
				}
				return parameter is TParam;
			}
			return parameter is null or TParam;
		}


#pragma warning disable CS8600
#pragma warning disable CS8604
		/// <summary>
		/// Execute command.
		/// </summary>
		/// <param name="parameter">Parameter.</param>
		public override async void Execute(object? parameter)
		{
			if (!this.CanExecute(parameter))
				return;
			if (this.action is not null)
				this.action((TParam)parameter);
			else if (this.asyncAction is not null)
			{
				this.isExecutingAsyncAction = true;
				this.InvalidateCanExecute();
				try
				{
					await this.asyncAction((TParam)parameter);
				}
				finally
				{
					this.isExecutingAsyncAction = false;
					this.InvalidateCanExecute();
				}
			}
		}
#pragma warning restore CS8600
#pragma warning restore CS8604
	}
}
