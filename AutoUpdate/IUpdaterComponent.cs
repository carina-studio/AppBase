using CarinaStudio.Threading;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.AutoUpdate;

/// <summary>
/// Interface of component of <see cref="Updater"/>.
/// </summary>
public interface IUpdaterComponent : IDisposable, INotifyPropertyChanged, IThreadDependent
{
	/// <summary>
	/// Cancel the operation performed by this component.
	/// </summary>
	/// <returns>True if operation cancelled successfully.</returns>
	bool Cancel();


	/// <summary>
	/// Get <see cref="Exception"/> occurred when performing operation.
	/// </summary>
	Exception? Exception { get; }


	/// <summary>
	/// Get progress of operation performed by this component. The range is [0.0, 1.0] or <see cref="double.NaN"/> if progress is unknown.
	/// </summary>
	double Progress { get; }


	/// <summary>
	/// Start performing operation.
	/// </summary>
	/// <returns>True if operation started successfully.</returns>
	bool Start();


	/// <summary>
	/// Get current state.
	/// </summary>
	UpdaterComponentState State { get; }
}


/// <summary>
/// Extensions for <see cref="IUpdaterComponent"/>.
/// </summary>
public static class UpdaterComponentExtensions
{
	/// <summary>
	/// Check whether state of <see cref="IUpdaterComponent"/> is <see cref="UpdaterComponentState.Initializing"/> or not.
	/// </summary>
	/// <param name="updaterComponent"><see cref="IUpdaterComponent"/>.</param>
	/// <returns>True if state of <see cref="IUpdaterComponent"/> is <see cref="UpdaterComponentState.Initializing"/>.</returns>
	public static bool IsInitializing(this IUpdaterComponent updaterComponent) => updaterComponent.State == UpdaterComponentState.Initializing;


	/// <summary>
	/// Check whether state of <see cref="IUpdaterComponent"/> represents completed/cancelled or not.
	/// </summary>
	/// <param name="updaterComponent"><see cref="IUpdaterComponent"/>.</param>
	/// <returns>True if state of <see cref="IUpdaterComponent"/> is one of states represent completed or cancelled.</returns>
	public static bool IsCompletedOrCancelled(this IUpdaterComponent updaterComponent) => updaterComponent.State switch
	{
		UpdaterComponentState.Cancelled => true,
		UpdaterComponentState.Failed => true,
		UpdaterComponentState.Succeeded => true,
		_ => false,
	};


	/// <summary>
	/// Check whether state of <see cref="IUpdaterComponent"/> represents started/cancelling or not.
	/// </summary>
	/// <param name="updaterComponent"><see cref="IUpdaterComponent"/>.</param>
	/// <returns>True if state of <see cref="IUpdaterComponent"/> is one of states represent started or cancelling.</returns>
	public static bool IsStartedOrCancelling(this IUpdaterComponent updaterComponent) => updaterComponent.State switch
	{
		UpdaterComponentState.Started => true,
		UpdaterComponentState.Cancelling => true,
		_ => false,
	};


	/// <summary>
	/// Start <see cref="IUpdaterComponent"/> and wait for completed/cancelled asynchronously.
	/// </summary>
	/// <param name="updaterComponent"><see cref="IUpdaterComponent"/>.</param>
	/// <returns>Task of waiting.</returns>
	public static Task StartAndWaitAsync(this IUpdaterComponent updaterComponent) =>
		StartAndWaitAsync(updaterComponent, CancellationToken.None);


	/// <summary>
	/// Start <see cref="IUpdaterComponent"/> and wait for completed/cancelled asynchronously.
	/// </summary>
	/// <param name="updaterComponent"><see cref="IUpdaterComponent"/>.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>Task of waiting.</returns>
	public static async Task StartAndWaitAsync(this IUpdaterComponent updaterComponent, CancellationToken cancellationToken)
	{
		// check state
		updaterComponent.VerifyAccess();
		if (updaterComponent.State == UpdaterComponentState.Disposed)
			throw new ObjectDisposedException($"{updaterComponent.GetType().Name} has been disposed.");
		if (updaterComponent.IsCompletedOrCancelled())
			return;

		// attach to component
		var taskCompletionSource = new TaskCompletionSource();
		var propertyChangedHandler = new PropertyChangedEventHandler((_, e) =>
		{
			if (e.PropertyName == nameof(IUpdaterComponent.State))
			{
				if (updaterComponent.IsCompletedOrCancelled() || updaterComponent.State == UpdaterComponentState.Disposed)
					taskCompletionSource.TrySetResult();
			}
		});
		updaterComponent.PropertyChanged += propertyChangedHandler;

		// start and wait
		try
		{
			if (updaterComponent.IsInitializing() && !updaterComponent.Start())
				throw new InvalidOperationException("Unable to start component.");
			await using var _ = cancellationToken != CancellationToken.None
				? cancellationToken.Register(() => taskCompletionSource.TrySetCanceled())
				: new();
			await taskCompletionSource.Task;
			if (updaterComponent.State == UpdaterComponentState.Disposed)
				throw new ObjectDisposedException($"{updaterComponent.GetType().Name} has been disposed.");
		}
		finally
		{
			updaterComponent.PropertyChanged -= propertyChangedHandler;
		}
	}
}


/// <summary>
/// State of <see cref="IUpdaterComponent"/>.
/// </summary>
public enum UpdaterComponentState
{
	/// <summary>
	/// Initializing.
	/// </summary>
	Initializing,
	/// <summary>
	/// Started.
	/// </summary>
	Started,
	/// <summary>
	/// Cancelling.
	/// </summary>
	Cancelling,
	/// <summary>
	/// Cancelled.
	/// </summary>
	Cancelled,
	/// <summary>
	/// Failed.
	/// </summary>
	Failed,
	/// <summary>
	/// Succeeded.
	/// </summary>
	Succeeded,
	/// <summary>
	/// Disposed.
	/// </summary>
	Disposed,
}