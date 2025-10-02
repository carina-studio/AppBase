using System;
using System.Windows.Input;

namespace CarinaStudio.Windows.Input;

/// <summary>
/// <see cref="IObservable{T}"/> which reflects <see cref="ICommand.CanExecute(object)"/>.
/// </summary>
/// <typeparam name="TParameter">Type of parameter of command.</typeparam>
/// <remarks><see cref="ObservableCommandState{TParameter}"/> uses weak event handler to monitor state of <see cref="ICommand"/> so it is not necessary to unbind from <see cref="ICommand"/> explicitly.</remarks>
public class ObservableCommandState<TParameter> : ObservableValue<bool>
{
	// Handler for CanExecuteChanged.
	class CanExecuteChangedHandler
	{
		// Fields.
		readonly WeakReference<ObservableCommandState<TParameter>> ownerReference;

		// Constructor.
		public CanExecuteChangedHandler(ObservableCommandState<TParameter> owner)
		{
			this.ownerReference = new WeakReference<ObservableCommandState<TParameter>>(owner);
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
	TParameter? parameter;


	/// <summary>
	/// Initialize new <see cref="ObservableCommandState{TParameter}"/> instance.
	/// </summary>
	/// <param name="command"><see cref="ICommand"/> to bind to.</param>
	/// <param name="parameter">Parameter of command.</param>
	/// <param name="defaultValue">Default value used when no <see cref="ICommand"/> bound.</param>
	public ObservableCommandState(ICommand? command = null, TParameter? parameter = default, bool defaultValue = false)
	{
		this.canExecuteChangedHandler = new CanExecuteChangedHandler(this);
		this.defaultValue = defaultValue;
		if (command != null)
			this.Bind(command, parameter);
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
	/// <param name="parameter">Parameter of command.</param>
	/// <exception cref="InvalidOperationException">Instance is already bound to another command.</exception>
	public void Bind(ICommand command, TParameter? parameter = default)
	{
		if (this.Command is not null)
		{
			if (this.Command == command)
			{
				if (!(this.parameter?.Equals(parameter) ?? parameter is null))
				{
					this.parameter = parameter;
					this.Value = command.CanExecute(parameter);
				}
				return;
			}
			throw new InvalidOperationException("Already bound to another command.");
		}
		this.Command = command;
		this.parameter = parameter;
		command.CanExecuteChanged += this.canExecuteChangedHandler.OnCanExecuteChanged;
		this.Value = command.CanExecute(parameter);
	}


	/// <summary>
	/// Get <see cref="ICommand"/> currently bound by this instance.
	/// </summary>
	public ICommand? Command { get; private set; }


	// Called when CanExecuteChanged.
	void OnCanExecuteChanged()
	{
		this.Value = this.Command?.CanExecute(this.parameter) ?? this.defaultValue;
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
		this.parameter = default;
		this.Value = this.defaultValue;
	}
}