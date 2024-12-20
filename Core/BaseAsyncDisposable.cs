using System;
using System.Threading.Tasks;

namespace CarinaStudio;

/// <summary>
/// Base implementation of <see cref="IAsyncDisposable"/>.
/// </summary>
public abstract class BaseAsyncDisposable : IAsyncDisposable
{
	// Fields.
	bool isDisposed;


	/// <summary>
	/// Finalizer.
	/// </summary>
	~BaseAsyncDisposable() => _ = this.DisposeAsync(false);


	/// <summary>
	/// Dispose the instance asynchronously.
	/// </summary>
	/// <returns>Task of disposing the instance.</returns>
	public ValueTask DisposeAsync()
	{
		if (this.isDisposed)
			return ValueTask.CompletedTask;
		this.isDisposed = true;
		GC.SuppressFinalize(this);
		return this.DisposeAsync(true);
	}


	/// <summary>
	/// Called to dispose the instance asynchronously.
	/// </summary>
	/// <param name="disposing">True to release managed resources also.</param>
	/// <returns>Task of disposing the instance.</returns>
	protected abstract ValueTask DisposeAsync(bool disposing);


	/// <summary>
	/// Check whether instance has disposed or not.
	/// </summary>
	protected bool IsDisposed => this.isDisposed;


	/// <summary>
	/// Throw <see cref="ObjectDisposedException"/> if instance has been disposed.
	/// </summary>
	protected void VerifyDisposed()
	{
		if (this.isDisposed)
			throw new ObjectDisposedException(this.GetType().Name);
	}
}
