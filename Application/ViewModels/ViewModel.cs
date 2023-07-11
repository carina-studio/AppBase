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
		/// <summary>
		/// Property of <see cref="HasNecessaryTasks"/>.
		/// </summary>
		public static readonly ObservableProperty<bool> HasNecessaryTasksProperty = ObservableProperty.Register<ViewModel, bool>(nameof(HasNecessaryTasks));
		/// <summary>
		/// Property of <see cref="Owner"/>.
		/// </summary>
		public static readonly ObservableProperty<ViewModel?> OwnerProperty = ObservableProperty.Register<ViewModel, ViewModel?>(nameof(Owner));


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
		static int nextId;


		// Fields.
		readonly Thread dependencyThread;
		readonly List<ViewModel> ownedViewModels = new();
		readonly SortedList<ObservableProperty, object?> propertyValues = new(ObservableProperty.Comparer);
		readonly List<Task> waitingNecessaryTasks = new();


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
			this.PersistentState = app.PersistentState;
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
		static bool CheckValueEquality(object? x, object? y)
		{
			if (x != null)
				return x.Equals(y);
			return y == null;
		}


		/// <summary>
		/// Coerce value for given property.
		/// </summary>
		/// <param name="property">Property.</param>
		/// <param name="value">Value.</param>
		/// <typeparam name="T">Type of property value.</typeparam>
		/// <returns>Coerced value.</returns>
		protected T CoerceValue<T>(ObservableProperty<T> property, T value)
		{
			if (!property.OwnerType.IsInstanceOfType(this))
				throw new ArgumentException($"{this.GetType().Name} is not owner of property '{property.Name}'.");
			return property.CoercionFunction(this, value);
		}


		/// <summary>
		/// Dispose the view-model.
		/// </summary>
		/// <param name="disposing">True to release managed resources.</param>
		// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
		protected override void Dispose(bool disposing)
		{
			// check thread
			if (disposing)
				this.VerifyAccess();

			// clear event handlers
			this.PropertyChanged = null;

			// detach from settings
			this.Settings?.Let(settings =>
			{
				settings.SettingChanged -= this.OnSettingChanged;
				settings.SettingChanging -= this.OnSettingChanging;
			});

			// detach from application
			this.Application?.Let(app =>
			{
				app.PropertyChanged -= this.OnApplicationPropertyChanged;
				app.StringsUpdated -= this.OnApplicationStringsUpdated;
			});

			// check necessary tasks
			if (this.waitingNecessaryTasks.IsNotEmpty())
				this.Logger?.LogWarning("There are {count} necessary task(s) not completed yet", this.waitingNecessaryTasks.Count);

			// notify owner
			if (disposing)
				this.Owner?.OnOwnedViewModelRemoved(this);
		}
		// ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract


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
			if (!property.OwnerType.IsInstanceOfType(this))
				throw new ArgumentException($"{this.GetType().Name} is not owner of property '{property.Name}'.");
			return new ObservablePropertyValue<T>(property).Also((it) => this.propertyValues[property] = it);
		}


		/// <summary>
		/// Check whether at least one necessary task is not completed yet or not.
		/// </summary>
		public bool HasNecessaryTasks => this.GetValue(HasNecessaryTasksProperty);


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
		/// Called when owner of the view-model has been changed.
		/// </summary>
		/// <param name="prevOwner">Previous owner.</param>
		/// <param name="newOwner">New owner.</param>
		protected virtual void OnOwnerChanged(ViewModel? prevOwner, ViewModel? newOwner)
		{ }


		/// <summary>
		/// Called when <see cref="ViewModel"/> which is owned by this instance has been added.
		/// </summary>
		/// <param name="viewModel"><see cref="ViewModel"/>.</param>
		protected virtual void OnOwnedViewModelAdded(ViewModel viewModel)
		{
			this.ownedViewModels.Add(viewModel);
		}


		/// <summary>
		/// Called when <see cref="ViewModel"/> which is owned by this instance has been removed.
		/// </summary>
		/// <param name="viewModel"><see cref="ViewModel"/>.</param>
		protected virtual void OnOwnedViewModelRemoved(ViewModel viewModel)
		{
			this.ownedViewModels.Remove(viewModel);
		}


		/// <summary>
		/// Called when property changed.
		/// </summary>
		/// <param name="property">Changed property.</param>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		protected virtual void OnPropertyChanged(ObservableProperty property, object? oldValue, object? newValue)
        {
			if (property == OwnerProperty)
			{
				var prevOwner = (oldValue as ViewModel);
				var newOwner = (newValue as ViewModel);
				prevOwner?.OnOwnedViewModelRemoved(this);
				newOwner?.OnOwnedViewModelAdded(this);
				this.OnOwnerChanged(prevOwner, newOwner);
			}
			this.OnPropertyChanged(property.Name);
		}


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
		/// Get or set owner of this view-model.
		/// </summary>
		public ViewModel? Owner
        {
			get => this.GetValue(OwnerProperty);
			set => this.SetValue(OwnerProperty, value);
        }


		/// <summary>
		/// Get persistent application state.
		/// </summary>
		protected ISettings PersistentState { get; private set; }


		/// <summary>
		/// Raised when property changed.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;


		/// <summary>
		/// Reset value of given <see cref="ObservableProperty"/> to its default value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="property"><see cref="ObservableProperty"/> to reset.</param>
		protected void ResetValue<T>(ObservableProperty<T> property) =>
			this.SetValue(property, property.DefaultValue);


		/// <summary>
		/// Get application user settings.
		/// </summary>
		protected ISettings Settings { get; private set; }


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
			if (!property.OwnerType.IsInstanceOfType(this))
				throw new ArgumentException($"{this.GetType().Name} is not owner of property '{property.Name}'.");

			// coerce value
			value = property.CoercionFunction(this, value);

			// validate value
			if (!property.ValidationFunction(value))
				throw new ArgumentException($"Invalid value for property '{property.Name}': {value}.");

			// get old value
			T oldValue = property.DefaultValue;
			if (this.propertyValues.TryGetValue(property, out var propertyValueObj) && propertyValueObj != null)
				oldValue = ((ObservablePropertyValue<T>)propertyValueObj).Value;

			// check equality
			if (CheckValueEquality(oldValue, value))
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
			if (this.waitingNecessaryTasks.Count == 1)
				this.SetValue(HasNecessaryTasksProperty, true);
			try
			{
				await task;
			}
			finally
			{
				this.waitingNecessaryTasks.Remove(task);
				if (this.waitingNecessaryTasks.IsEmpty())
					this.SetValue(HasNecessaryTasksProperty, false);
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
			if (this.waitingNecessaryTasks.IsEmpty())
				this.SetValue(HasNecessaryTasksProperty, false);
		}
	}


	/// <summary>
	/// Base class for view-model.
	/// </summary>
	/// <typeparam name="TApplication">Type of application.</typeparam>
	public abstract class ViewModel<TApplication> : ViewModel, IApplicationObject<TApplication> where TApplication : class, IApplication
	{
		/// <summary>
		/// Initialize new <see cref="ViewModel{TApplication}"/> instance.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/> which view-model belongs to.</param>
		protected ViewModel(TApplication app) : base(app)
		{ }


		/// <summary>
		/// Get <see cref="IApplication"/> which view-model belongs to.
		/// </summary>
		public new TApplication Application => (TApplication)base.Application;
	}
	
	
	/// <summary>
	/// Base class for view-model.
	/// </summary>
	/// <typeparam name="TApplication">Type of application.</typeparam>
	/// <typeparam name="TSyncContext">Type of <see cref="SynchronizationContext"/>.</typeparam>
	public abstract class ViewModel<TApplication, TSyncContext> : ViewModel<TApplication>, IApplicationObject<TApplication, TSyncContext> where TApplication : class, IApplication<TSyncContext> where TSyncContext : SynchronizationContext
	{
		/// <summary>
		/// Initialize new <see cref="ViewModel{TApplication, TSyncContext}"/> instance.
		/// </summary>
		/// <param name="app"><see cref="IApplication"/> which view-model belongs to.</param>
		protected ViewModel(TApplication app) : base(app)
		{ }


		/// <inheritdoc/>
		public new TSyncContext SynchronizationContext => this.Application.SynchronizationContext;
	}
}
