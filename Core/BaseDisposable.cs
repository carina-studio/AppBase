﻿using System;

namespace CarinaStudio;

/// <summary>
/// Base implementation of <see cref="IDisposable"/>.
/// </summary>
public abstract class BaseDisposable : IDisposable
{
	// Fields.
	bool isDisposed;


	/// <summary>
	/// Finalizer.
	/// </summary>
	~BaseDisposable() => this.Dispose(false);


	/// <summary>
	/// Dispose instance.
	/// </summary>
	public void Dispose()
	{
		if (this.isDisposed)
			return;
		this.isDisposed = true;
		GC.SuppressFinalize(this);
		this.Dispose(true);
	}


	/// <summary>
	/// Called to dispose instance.
	/// </summary>
	/// <param name="disposing">True to release managed resources also.</param>
	protected abstract void Dispose(bool disposing);


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
