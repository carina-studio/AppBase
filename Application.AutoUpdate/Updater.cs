using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Core object to perform auto/self update.
	/// </summary>
	public class Updater : BaseDisposable, IApplicationObject, INotifyPropertyChanged
	{
		// Fields.
		UpdaterState state = UpdaterState.Initializing;


		/// <summary>
		/// Initialize new <see cref="Updater"/> instance.
		/// </summary>
		/// <param name="app">Application.</param>
		public Updater(IApplication app)
		{
			this.Application = app;
		}


		/// <summary>
		/// Get application instance.
		/// </summary>
		public IApplication Application { get; }


		// Change state.
		bool ChangeState(UpdaterState state)
		{
			if (this.state == state)
				return true;
			this.state = state;
			this.OnPropertyChanged(nameof(State));
			return (this.state == state);
		}


		/// <summary>
		/// Check whether current thread is the thread which object depends on or not.
		/// </summary>
		/// <returns>True if current thread is the thread which object depends on.</returns>
		public bool CheckAccess() => this.Application.CheckAccess();


		/// <summary>
		/// Dispose instance.
		/// </summary>
		/// <param name="disposing">True to dispose managed resources.</param>
		protected override void Dispose(bool disposing)
		{
			// check thread
			if (disposing)
				this.VerifyAccess();

			// change state
			if (disposing)
				this.ChangeState(UpdaterState.Disposed);
		}


		/// <summary>
		/// Raise <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">Name of changed property.</param>
		protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


		/// <summary>
		/// Raise when property changed.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;


		/// <summary>
		/// Get current state.
		/// </summary>
		public UpdaterState State { get => this.state; }


		/// <summary>
		/// Get <see cref="SynchronizationContext"/>.
		/// </summary>
		public SynchronizationContext SynchronizationContext => this.Application.SynchronizationContext;


		// Throw exception if current state is not initializing.
		void VerifyInitializing()
		{
			if (this.state != UpdaterState.Initializing)
				throw new InvalidOperationException($"Cannot perform oprtation when state is {this.state}.");
		}
	}


	/// <summary>
	/// State of <see cref="Updater"/>.
	/// </summary>
	public enum UpdaterState
	{
		/// <summary>
		/// Initializing.
		/// </summary>
		Initializing,
		/// <summary>
		/// Disposed.
		/// </summary>
		Disposed,
	}
}
