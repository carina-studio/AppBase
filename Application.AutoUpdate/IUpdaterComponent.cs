using System;
using System.ComponentModel;

namespace CarinaStudio.AutoUpdate
{
	/// <summary>
	/// Interface of component of <see cref="Updater"/>.
	/// </summary>
	public interface IUpdaterComponent : IApplicationObject, IDisposable, INotifyPropertyChanged
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
		/// Check whether operation performed by this component is cancellable or not.
		/// </summary>
		bool IsCancellable { get; }


		/// <summary>
		/// Check whether operation performed by this component is completed/cancelled or not.
		/// </summary>
		bool IsCompletedOrCancelled { get; }


		/// <summary>
		/// Check whether operation performed by this component has been started or not.
		/// </summary>
		bool IsStarted { get; }


		/// <summary>
		/// Get progress of operation performed by this component. The range is [0.0, 1.0] or <see cref="double.NaN"/> if progress is unknown.
		/// </summary>
		double Progress { get; }


		/// <summary>
		/// Start performing operation.
		/// </summary>
		/// <returns>True if operation started successfully.</returns>
		bool Start();
	}
}
