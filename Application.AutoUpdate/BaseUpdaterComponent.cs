using CarinaStudio.Threading;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Base implementation of <see cref="IUpdaterComponent"/>.
	/// </summary>
	public abstract class BaseUpdaterComponent : BaseDisposable, IUpdaterComponent
	{
		// Fields.
		readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		double progress = double.NaN;


		/// <summary>
		/// Initialize new <see cref="BaseUpdaterComponent"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		protected BaseUpdaterComponent(IApplication app)
		{
			app.VerifyAccess();
			this.Application = app;
		}


		/// <summary>
		/// Get application instance.
		/// </summary>
		public IApplication Application { get; }


		/// <summary>
		/// Cancel the operation performed by this component.
		/// </summary>
		/// <returns>True if operation cancelled successfully.</returns>
		public bool Cancel()
		{
			// check state
			this.VerifyAccess();
			this.VerifyDisposed();
			if (!this.IsStarted || this.IsCompletedOrCancelled)
				return false;
			if (this.cancellationTokenSource.IsCancellationRequested)
				return true;
			if (!this.IsCancellable)
				return false;

			// update state
			this.IsCancellable = false;
			this.OnPropertyChanged(nameof(IsCancellable));

			// cancel
			this.cancellationTokenSource.Cancel();
			return true;
		}


		/// <summary>
		/// Check whether current thread is the thread which object depends on or not.
		/// </summary>
		/// <returns>True if current thread is the thread which object depends on.</returns>
		public bool CheckAccess() => this.Application.CheckAccess();


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
		}


		/// <summary>
		/// Get <see cref="Exception"/> occurred when performing operation.
		/// </summary>
		public Exception? Exception { get; private set; }


		/// <summary>
		/// Check whether operation performed by this component is cancellable or not.
		/// </summary>
		public bool IsCancellable { get; private set; } = false;


		/// <summary>
		/// Check whether operation performed by this component is completed/cancelled or not.
		/// </summary>
		public bool IsCompletedOrCancelled { get; private set; }


		/// <summary>
		/// Check whether operation performed by this component has been started or not.
		/// </summary>
		public bool IsStarted { get; private set; }


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
			if (this.IsCancellable)
			{
				this.IsCancellable = false;
				this.OnPropertyChanged(nameof(IsCancellable));
			}
			this.IsCompletedOrCancelled = true;
			this.OnPropertyChanged(nameof(IsCompletedOrCancelled));
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
				exception = ex;
			}
			finally
			{
				if (this.IsStarted && !this.IsCompletedOrCancelled && !this.IsDisposed)
					this.OnCompletedOrCancelled(exception);
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
			if (!this.IsStarted || this.IsCompletedOrCancelled || this.IsDisposed)
				return;
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
			if (this.IsStarted)
				return !this.IsCompletedOrCancelled && !this.cancellationTokenSource.IsCancellationRequested;

			// update state
			this.IsStarted = true;
			this.OnPropertyChanged(nameof(IsStarted));
			this.IsCancellable = true;
			this.OnPropertyChanged(nameof(IsCancellable));

			// perform operation
			this.PerformOperation();
			return true;
		}


		/// <summary>
		/// Get <see cref="SynchronizationContext"/>.
		/// </summary>
		public SynchronizationContext SynchronizationContext => this.Application.SynchronizationContext;
	}
}
