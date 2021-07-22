using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.ViewModels
{
	/// <summary>
	/// Base class for view-model.
	/// </summary>
	public abstract class ViewModel : BaseDisposable, IApplicationObject, INotifyPropertyChanged
	{
		// Value holder of observable property.
		class ObservablePropertyValue<T> : ObservableValue<T>
		{
			// Constructor.
			public ObservablePropertyValue(ObservableProperty<T> property) : base(property.DefaultValue)
			{ }

			// Update value.
			public void Update(T value) => this.Value = value;
		}


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
			// check thread
			app.VerifyAccess();

			// setup fields
			this.Application = app;
			this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
			this.dependencyThread = Thread.CurrentThread;
			this.Id = Interlocked.Increment(ref nextId);
			this.Logger = app.LoggerFactory.CreateLogger($"{this.GetType().Name}-{this.Id}");
			this.Settings = app.Settings;

			// attach to application
			app.PropertyChanged += this.OnApplicationPropertyChanged;
			app.StringsUpdated += this.OnApplicationStringsUpdated;

			// attach to settings
			this.Settings.SettingChanged += this.OnSettingChanged;
			this.Settings.SettingChanging += this.OnSettingChanging;
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
			// check thread
			if (disposing)
				this.VerifyAccess();

			// clear event handlers
			this.PropertyChanged = null;

			// detach from settings
			this.Settings.SettingChanged -= this.OnSettingChanged;
			this.Settings.SettingChanging -= this.OnSettingChanging;

			// detach from application
			this.Application.PropertyChanged -= this.OnApplicationPropertyChanged;
			this.Application.StringsUpdated -= this.OnApplicationStringsUpdated;
		}


		/// <summary>
		/// Get property value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="property">Property to get.</param>
		/// <returns>Value of property.</returns>
		public T GetValue<T>(ObservableProperty<T> property)
		{
			this.VerifyAccess();
			if (this.propertyValues.TryGetValue(property, out var propertyValue) && propertyValue != null)
				return ((ObservablePropertyValue<T>)propertyValue).Value;
			return property.DefaultValue;
		}


		/// <summary>
		/// Get property value as <see cref="IObservable{T}"/>.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="property">Property to get.</param>
		/// <returns><see cref="IObservable{T}"/> represents value of property.</returns>
		public IObservable<T> GetValueAsObservable<T>(ObservableProperty<T> property)
		{
			this.VerifyAccess();
			if (this.propertyValues.TryGetValue(property, out var propertyValue) && propertyValue != null)
				return (ObservablePropertyValue<T>)propertyValue;
			if (!property.OwnerType.IsAssignableFrom(this.GetType()))
				throw new ArgumentException($"{this.GetType().Name} is not owner of property '{property.Name}'.");
			return new ObservablePropertyValue<T>(property).Also((it) => this.propertyValues[property] = it);
		}


		/// <summary>
		/// Get unique ID of view-model instance.
		/// </summary>
		public int Id { get; }


		/// <summary>
		/// Logger of this instance.
		/// </summary>
		protected ILogger Logger { get; }


		// Called when application property changed.
		void OnApplicationPropertyChanged(object? sender, PropertyChangedEventArgs e) => this.OnApplicationPropertyChanged(e);


		/// <summary>
		/// Called when property of <see cref="Application"/> has been changed.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected virtual void OnApplicationPropertyChanged(PropertyChangedEventArgs e)
		{ }


		// Called when application string resources updated.
		void OnApplicationStringsUpdated(object? sender, EventArgs e) => this.OnApplicationStringsUpdated();


		/// <summary>
		/// Called when application string resources updated.
		/// </summary>
		protected virtual void OnApplicationStringsUpdated()
		{ }


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


		// Called when application setting changed.
		void OnSettingChanged(object? sender, SettingChangedEventArgs e) => this.OnSettingChanged(e);


		/// <summary>
		/// Called when application setting has been changed.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected virtual void OnSettingChanged(SettingChangedEventArgs e)
		{ }


		// Called when application setting is changing.
		void OnSettingChanging(object? sender, SettingChangingEventArgs e) => this.OnSettingChanging(e);


		/// <summary>
		/// Called when application setting is changing.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected virtual void OnSettingChanging(SettingChangingEventArgs e)
		{ }


		/// <summary>
		/// Raised when property changed.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;


		/// <summary>
		/// Get application settings.
		/// </summary>
		protected BaseSettings Settings { get; private set; }


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
			if (property.ValidationFunction != null && !property.ValidationFunction(value))
				throw new ArgumentException($"Invalid value for property '{property.Name}': {value}.");

			// get old value
			T oldValue = property.DefaultValue;
			if (this.propertyValues.TryGetValue(property, out var propertyValueObj) && propertyValueObj != null)
				oldValue = ((ObservablePropertyValue<T>)propertyValueObj).Value;

			// check equality
			if (this.CheckValueEuqality(oldValue, value))
				return;

			// update value
			if (propertyValueObj != null)
				((ObservablePropertyValue<T>)propertyValueObj).Update(value);
			else
				this.propertyValues[property] = new ObservablePropertyValue<T>(property).Also((it) => it.Update(value));
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


		/// <summary>
		/// Wait for completion of all necessary asynchronous tasks.
		/// </summary>
		/// <returns>Task of waiting.</returns>
		public virtual Task WaitForNecessaryTasksCompletionAsync() => Task.CompletedTask;
	}
}
