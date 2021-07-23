using CarinaStudio.Collections;
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
		readonly List<ViewModel> ownedViewModels = new List<ViewModel>();
		readonly SortedList<ObservableProperty, object?> propertyValues = new SortedList<ObservableProperty, object?>(ObservableProperty.Comparer);
		readonly List<Task> waitingNecessaryTasks = new List<Task>();


		/// <summary>
		/// Initialize new <see cref="ViewModel"/> instance.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/> which view-model belongs to.</param>
		protected ViewModel(IApplication app)
		{
			// check thread
			app.VerifyAccess();

			// setup properties
			this.Application = app;
			this.SynchronizationContext = SynchronizationContext.Current ?? throw new InvalidOperationException("No SynchronizationContext on current thread.");
			this.dependencyThread = Thread.CurrentThread;
			this.Id = Interlocked.Increment(ref nextId);
			this.Logger = app.LoggerFactory.CreateLogger($"{this.GetType().Name}-{this.Id}");
			this.OwnedViewModels = this.ownedViewModels.AsReadOnly();
			this.Settings = app.Settings;

			// attach to application
			app.PropertyChanged += this.OnApplicationPropertyChanged;
			app.StringsUpdated += this.OnApplicationStringsUpdated;

			// attach to settings
			this.Settings.SettingChanged += this.OnSettingChanged;
			this.Settings.SettingChanging += this.OnSettingChanging;
		}


		/// <summary>
		/// Initialize new <see cref="ViewModel"/> instance.
		/// </summary>
		/// <param name="owner">Owner view-model</param>
		protected ViewModel(ViewModel owner) : this(owner.Application)
		{
			this.Owner = owner;
			owner.OnOwnedViewModelCreated(this);
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

			// check necessary tasks
			if (this.waitingNecessaryTasks.IsNotEmpty())
				this.Logger.LogWarning($"There are {this.waitingNecessaryTasks.Count} necessary task(s) not completed yet");

			// notify owner
			if (disposing)
				this.Owner?.OnOwnedViewModelDisposed(this);
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
		/// Check whether at least one necessary task is not completed yet or not.
		/// </summary>
		public bool HasNecessaryTasks { get => this.waitingNecessaryTasks.IsNotEmpty(); }


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
		/// Called when <see cref="ViewModel"/> which is owned by this instance has been created.
		/// </summary>
		/// <param name="viewModel"><see cref="ViewModel"/>.</param>
		protected virtual void OnOwnedViewModelCreated(ViewModel viewModel)
		{
			this.ownedViewModels.Add(viewModel);
		}


		/// <summary>
		/// Called when <see cref="ViewModel"/> which is owned by this instance has been disposed.
		/// </summary>
		/// <param name="viewModel"><see cref="ViewModel"/>.</param>
		protected virtual void OnOwnedViewModelDisposed(ViewModel viewModel)
		{
			this.ownedViewModels.Remove(viewModel);
		}


		/// <summary>
		/// Called when property changed.
		/// </summary>
		/// <param name="property">Changed property.</param>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		protected virtual void OnPropertyChanged(ObservableProperty property, object? oldValue, object? newValue) => this.OnPropertyChanged(property.Name);


		/// <summary>
		/// Raise <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">Name of changed property.</param>
		protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


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
		/// Get list of <see cref="ViewModel"/> which is owned by this instance.
		/// </summary>
		protected IList<ViewModel> OwnedViewModels { get; }


		/// <summary>
		/// Get owner of this view-model.
		/// </summary>
		public ViewModel? Owner { get; }


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
		/// Treat given task as necessary task and wait for completion.
		/// </summary>
		/// <param name="task">Task.</param>
		/// <returns>Task or waiting.</returns>
		/// <remarks>The task will also be waited by <see cref="WaitForNecessaryTasksAsync"/> after calling this method.</remarks>
		protected async Task WaitForNecessaryTaskAsync(Task task)
		{
			this.VerifyAccess();
			this.VerifyDisposed();
			this.waitingNecessaryTasks.Add(task);
			try
			{
				await task;
			}
			finally
			{
				this.waitingNecessaryTasks.Remove(task);
			}
		}


		/// <summary>
		/// Wait for completion of all necessary asynchronous tasks.
		/// </summary>
		/// <returns>Task of waiting.</returns>
		/// <remarks>Error generated by tasks will be ignored.</remarks>
		public virtual async Task WaitForNecessaryTasksAsync()
		{
			// check state
			this.VerifyAccess();

			// wait for tasks of owned view-model
			foreach (var viewModel in this.ownedViewModels)
				_ = this.WaitForNecessaryTaskAsync(viewModel.WaitForNecessaryTasksAsync());

			// wait for self tasks
			if (this.waitingNecessaryTasks.IsEmpty())
				return;
			foreach (var task in this.waitingNecessaryTasks.ToArray())
			{
				try
				{
					await task;
				}
				catch (Exception ex)
				{
					this.Logger.LogError(ex, "Exception occurred while waiting for necessary task");
				}
				finally
				{
					this.waitingNecessaryTasks.Remove(task);
				}
			}
		}
	}
}
