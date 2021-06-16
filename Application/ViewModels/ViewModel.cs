using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace CarinaStudio.ViewModels
{
	/// <summary>
	/// Base class for view-model.
	/// </summary>
	public abstract class ViewModel : BaseDisposable, IApplicationObject, INotifyPropertyChanged
	{
		// Static fields.
		static volatile int nextId = 0;


		// Fields.
		readonly Thread dependencyThread;
		readonly SortedList<ObservableProperty, object?> propertyValues = new SortedList<ObservableProperty, object?>(ObservableProperty.Comparer);


		/// <summary>
		/// Initialize new <see cref="ViewModel"/> instance.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/> which view-model belongs to.</param>
		protected ViewModel(IApplication app)
		{
			app.VerifyAccess();
			this.Application = app;
			this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
			this.dependencyThread = Thread.CurrentThread;
			this.Id = Interlocked.Increment(ref nextId);
			this.Logger = app.LoggerFactory.CreateLogger($"{this.GetType().Name}-{this.Id}");
		}


		/// <summary>
		/// Get <see cref="IApplication"/> which view-model belongs to.
		/// </summary>
		public IApplication Application { get; }


		/// <summary>
		/// Check whether current thread is the thread which view-model depends on or not.
		/// </summary>
		/// <returns>True if current thread is the thread which view-model depends on.</returns>
		public bool CheckAccess() => Thread.CurrentThread == this.dependencyThread;


		// Check equality of two values.
		bool CheckValueEuqality(object? x, object? y)
		{
			if (x != null)
				return x.Equals(y);
			return y == null;
		}


		/// <summary>
		/// Dispose the view-model.
		/// </summary>
		/// <param name="disposing">True to release managed resources.</param>
		protected override void Dispose(bool disposing)
		{
			this.PropertyChanged = null;
		}


#pragma warning disable CS8603
#pragma warning disable CS8600
		/// <summary>
		/// Get property value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="property">Property to get.</param>
		/// <returns>Value of property.</returns>
		protected T GetValue<T>(ObservableProperty<T> property)
		{
			this.VerifyAccess();
			if (this.propertyValues.TryGetValue(property, out var value))
				return (T)value;
			return property.DefaultValue;
		}
#pragma warning restore CS8600
#pragma warning restore CS8603


		/// <summary>
		/// Get unique ID of view-model instance.
		/// </summary>
		public int Id { get; }


		/// <summary>
		/// Logger of this instance.
		/// </summary>
		protected ILogger Logger { get; }


		/// <summary>
		/// Called when property changed.
		/// </summary>
		/// <param name="property">Changed property.</param>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		protected virtual void OnPropertyChanged(ObservableProperty property, object? oldValue, object? newValue)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.Name));
		}


		/// <summary>
		/// Raised when property changed.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;


		/// <summary>
		/// Set property value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="property">Property to set.</param>
		/// <param name="value">Value.</param>
		protected void SetValue<T>(ObservableProperty<T> property, T value)
		{
			// check state
			this.VerifyAccess();
			this.VerifyDisposed();

			// check owner
			if (!property.OwnerType.IsAssignableFrom(this.GetType()))
				throw new ArgumentException($"{this.GetType().Name} is not owner of property '{property.Name}'.");

			// coerce value
			if (property.CoercionFunction != null)
				value = property.CoercionFunction(value);

			// validate value
			if (property.ValidationFunction!=null && !property.ValidationFunction(value))
				throw new ArgumentException($"Invalid value for property '{property.Name}': {value}.");

			// get old value
			if (!this.propertyValues.TryGetValue(property, out var oldValue))
				oldValue = property.DefaultValue;

			// check equality
			if (this.CheckValueEuqality(oldValue, value))
				return;

			// update value
			if (this.CheckValueEuqality(property.DefaultValue, value))
				this.propertyValues.Remove(property);
			else
				this.propertyValues[property] = value;
			this.OnPropertyChanged(property, oldValue, value);
		}


		/// <summary>
		/// Get <see cref="SynchronizationContext"/> on thread which view-model depends on.
		/// </summary>
		public SynchronizationContext SynchronizationContext { get; }


		/// <summary>
		/// Get readable string represents this view-model.
		/// </summary>
		/// <returns>String represents this view-model.</returns>
		public override string ToString() => $"{this.GetType().Name}-{this.Id}";
	}
}
