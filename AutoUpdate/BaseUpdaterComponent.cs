using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Base implementation of <see cref="IUpdaterComponent"/>.
	/// </summary>
	public abstract class BaseUpdaterComponent : BaseDisposableApplicationObject, IUpdaterComponent
	{
		// Fields.
		readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		double progress = double.NaN;
		UpdaterComponentState state = UpdaterComponentState.Initializing;


		/// <summary>
		/// Initialize new <see cref="BaseUpdaterComponent"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		protected BaseUpdaterComponent(IApplication app) : base(app)
		{ 
			this.Logger = app.LoggerFactory.CreateLogger(this.GetType().Name);
		}


		/// <summary>
		/// Cancel the operation performed by this component.
		/// </summary>
		/// <returns>True if operation cancelled successfully.</returns>
		public bool Cancel()
		{
			// check state
			this.VerifyAccess();
			this.VerifyDisposed();
			switch (this.state)
			{
				case UpdaterComponentState.Started:
					break;
				case UpdaterComponentState.Cancelling:
				case UpdaterComponentState.Cancelled:
					return true;
				default:
					return false;
			}

			// update state
			this.ChangeState(UpdaterComponentState.Cancelling);

			// cancel
			this.cancellationTokenSource.Cancel();
			return true;
		}


		// Change state.
		bool ChangeState(UpdaterComponentState state)
		{
			if (this.state == state)
				return true;
			this.Logger.LogDebug($"Change state from {this.state} to {state}");
			this.state = state;
			this.OnPropertyChanged(nameof(State));
			return (this.state == state);
		}


		/// <summary>
		/// Dispose the instance.
		/// </summary>
		/// <param name="disposing">True to dispose managed resources.</param>
		protected override void Dispose(bool disposing)
		{
			// check thread
			if (disposing)
				this.VerifyAccess();

			// cancel operation
			this.cancellationTokenSource.Cancel();

			// change state
			if (disposing)
				this.ChangeState(UpdaterComponentState.Disposed);
		}


		/// <summary>
		/// Get <see cref="Exception"/> occurred when performing operation.
		/// </summary>
		public Exception? Exception { get; private set; }


		/// <summary>
		/// Get logger.
		/// </summary>
		protected ILogger Logger { get; }


		/// <summary>
		/// Called when operation completed or cancelled.
		/// </summary>
		/// <param name="ex">Exception occurred while performing operation.</param>
		protected virtual void OnCompletedOrCancelled(Exception? ex)
		{
			if (ex != null)
			{
				this.Exception = ex;
				this.OnPropertyChanged(nameof(Exception));
			}
			if (this.state == UpdaterComponentState.Cancelling)
				this.ChangeState(UpdaterComponentState.Cancelled);
			else if (ex == null)
				this.ChangeState(UpdaterComponentState.Succeeded);
			else
				this.ChangeState(UpdaterComponentState.Failed);
		}


		/// <summary>
		/// Raise <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">Name of changed property.</param>
		protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		// Perform operation.
		async void PerformOperation()
		{
			var exception = (Exception?)null;
			try
			{
				await this.PerformOperationAsync(this.cancellationTokenSource.Token);
			}
			catch (TaskCanceledException)
			{ }
			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Error occurred while performing operation");
				exception = ex;
			}
			finally
			{
				switch(this.state)
				{
					case UpdaterComponentState.Started:
					case UpdaterComponentState.Cancelling:
						this.OnCompletedOrCancelled(exception);
						break;
				}
			}
		}


		/// <summary>
		/// Perform operation asynchronously.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>Task of performing operation.</returns>
		protected abstract Task PerformOperationAsync(CancellationToken cancellationToken);


		/// <summary>
		/// Get progress of operation performed by this component. The range is [0.0, 1.0] or <see cref="double.NaN"/> if progress is unknown.
		/// </summary>
		public double Progress { get => this.progress; }


		/// <summary>
		/// Raise when property changed.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;


		/// <summary>
		/// Report progress of operation.
		/// </summary>
		/// <param name="progress">Progress.</param>
		/// <remarks>You can call this method from any thread.</remarks>
		protected void ReportProgress(double progress)
		{
			if (!this.CheckAccess())
			{
				this.SynchronizationContext.Post(() => this.ReportProgress(progress));
				return;
			}
			switch (this.state)
			{
				case UpdaterComponentState.Started:
				case UpdaterComponentState.Cancelling:
					break;
				default:
					return;
			}
			if (double.IsNaN(progress))
			{
				if (double.IsNaN(this.progress))
					return;
			}
			else
			{
				progress = Math.Min(Math.Max(0, progress), 1);
				if (!double.IsNaN(this.progress) && Math.Abs(this.progress - progress) <= 0.001)
					return;
			}
			this.progress = progress;
			this.OnPropertyChanged(nameof(Progress));
		}


		/// <summary>
		/// Start performing operation.
		/// </summary>
		/// <returns>True if operation started successfully.</returns>
		public bool Start()
		{
			// check state
			this.VerifyAccess();
			this.VerifyDisposed();
			if (this.state != UpdaterComponentState.Initializing)
				return false;

			// check parameters
			if (!this.ValidateParametersToStart())
				return false;

			// update state
			if (!this.ChangeState(UpdaterComponentState.Started))
				return false;

			// perform operation
			this.PerformOperation();
			return true;
		}


		/// <summary>
		/// Get current state.
		/// </summary>
		public UpdaterComponentState State { get => this.state; }


		/// <summary>
		/// Validate parameters to start performing operation.
		/// </summary>
		/// <returns>True if all parameters are valid.</returns>
		protected virtual bool ValidateParametersToStart() => true;


		/// <summary>
		/// Throw <see cref="InvalidOperationException"/> if current state is not <see cref="UpdaterComponentState.Initializing"/>.
		/// </summary>
		protected void VerifyInitializing()
		{
			if (this.state != UpdaterComponentState.Initializing)
				throw new InvalidOperationException($"Cannot perform oprtation when state is {this.state}.");
		}
	}
}
