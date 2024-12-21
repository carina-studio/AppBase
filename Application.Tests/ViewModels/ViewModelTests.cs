using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.ViewModels;

/// <summary>
/// Tests of <see cref="ViewModel"/>.
/// </summary>
[TestFixture]
class ViewModelTests
{
	// Implementation of IAsyncDisposable.
	class AsyncDisposable: IAsyncDisposable
	{
		// Fields.
		public bool IsDisposed;
		
		// Implementations.
		public ValueTask DisposeAsync()
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException(nameof(AsyncDisposable));
			this.IsDisposed = true;
			return ValueTask.CompletedTask;
		}
	}
	
	
	// Implementation of IDisposable.
	class Disposable: IDisposable
	{
		// Fields.
		public bool IsDisposed;
		
		// Implementations.
		public void Dispose()
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException(nameof(AsyncDisposable));
			this.IsDisposed = true;
		}
	}
	
	
	// Implementation of observer.
	class Observer<T> : IObserver<T>
	{
		// Fields.
		public bool IsOnNextCalled;
#pragma warning disable CS8601
		public T LatestValue = default;
#pragma warning restore CS8601
		readonly Action? onNextAction;

		// Constructor.
		public Observer(Action? onNextAction = null)
		{
			this.onNextAction = onNextAction;
		}

		// Implementations.
		public void OnCompleted()
		{ }
		public void OnError(Exception error)
		{ }
		public void OnNext(T value)
		{
			this.IsOnNextCalled = true;
			this.LatestValue = value;
			this.onNextAction?.Invoke();
		}
	}


	// Fields.
	TestApplication? application;
	SingleThreadSynchronizationContext? testSyncContext;


	// Complete tests.
	[OneTimeTearDown]
	public void Complete()
	{
		this.testSyncContext?.Dispose();
	}


	/// <summary>
	/// Test for instance creation.
	/// </summary>
	[Test]
	public void CreationTest()
	{
		// create from correct thread
		this.testSyncContext?.Send(() =>
		{
			using var viewModel = new TestViewModel(this.application.AsNonNull());
		});

		// create from wrong thread
		try
		{
			using var viewModel = new TestViewModel(this.application.AsNonNull());
			throw new AssertionException("Should not create from another thread.");
		}
		catch (Exception ex)
		{
			if (ex is AssertionException)
				throw;
		}

		// check ID
		this.testSyncContext?.Send(() =>
		{
			var usingId = new HashSet<int>();
			for (var i = 0; i < 100; ++i)
			{
				using var viewModel = new TestViewModel(this.application.AsNonNull());
				Assert.That(usingId.Add(viewModel.Id), "Duplicate ID of view-model.");
			}
		});
	}



	// Test for printing log.
	[Test]
	public void LogPrintingTest()
	{
		this.testSyncContext?.Send(() =>
		{
			// prepare
			using var viewModel = new TestViewModel(this.application.AsNonNull());
			var logItems = TestLoggerProvider.Default.LogItems;
			var originalLogItemCount = logItems.Count;

			// print log
			var categoryName = viewModel.ToString();
			var eventId1 = new EventId(1, "Event1");
			var eventId2 = new EventId(2, "Event2");
			viewModel.PrintLog(LogLevel.Debug, eventId1, "Message 1");
			viewModel.PrintLog(LogLevel.Error, eventId2, "Message 2");

			// check log
			var logItem1 = logItems[originalLogItemCount];
			Assert.That(categoryName == logItem1.CategoryName);
			Assert.That(LogLevel.Debug == logItem1.Level);
			Assert.That(eventId1 == logItem1.EventId);
			Assert.That("Message 1" == logItem1.Message);
			var logItem2 = logItems[originalLogItemCount + 1];
			Assert.That(categoryName == logItem2.CategoryName);
			Assert.That(LogLevel.Error == logItem2.Level);
			Assert.That(eventId2 == logItem2.EventId);
			Assert.That("Message 2" == logItem2.Message);
		});
	}


	// Prepare for all tests.
	[OneTimeSetUp]
	public void Prepare()
	{
		this.testSyncContext = new SingleThreadSynchronizationContext();
		this.testSyncContext.Post(() =>
		{
			this.application = new TestApplication();
		});
	}


	/// <summary>
	/// Test for properties.
	/// </summary>
	[Test]
	public void PropertyTest()
	{
		this.testSyncContext?.Send(() =>
		{
			// prepare
			using var viewModel = new TestViewModel(this.application.AsNonNull());
			var latestChangedPropertyName = "";
			viewModel.PropertyChanged += (_, e) =>
			{
				latestChangedPropertyName = e.PropertyName;
			};

			// check default property value
			Assert.That(TestViewModel.TestInt32Property.DefaultValue == viewModel.TestInt32, "Default property value is incorrect.");
			Assert.That(TestViewModel.TestRangeInt32Property.DefaultValue == viewModel.TestRangeInt32, "Default property value is incorrect.");

			// change property
			viewModel.TestInt32 = 123;
			Assert.That(123 == viewModel.TestInt32, "Property value is different from set one.");
			Assert.That(nameof(TestViewModel.TestInt32) == latestChangedPropertyName, "PropertyChanged not received.");
			latestChangedPropertyName = "";
			viewModel.TestInt32 = 123;
			Assert.That("" == latestChangedPropertyName, "PropertyChanged should not be raised.");
			viewModel.TestInt32 = TestViewModel.TestInt32Property.DefaultValue;
			Assert.That(TestViewModel.TestInt32Property.DefaultValue == viewModel.TestInt32, "Property value is different from set one.");
			Assert.That(nameof(TestViewModel.TestInt32) == latestChangedPropertyName, "PropertyChanged not received.");
			latestChangedPropertyName = "";

			// change property with coercion function
			viewModel.TestRangeInt32 = TestViewModel.MaxTestRangeInt32 - 1;
			Assert.That(TestViewModel.MaxTestRangeInt32 - 1 == viewModel.TestRangeInt32, "Property value is different from set one.");
			Assert.That(nameof(TestViewModel.TestRangeInt32) == latestChangedPropertyName, "PropertyChanged not received.");
			latestChangedPropertyName = "";
			viewModel.TestRangeInt32 = TestViewModel.MinTestRangeInt32 + 1;
			Assert.That(TestViewModel.MinTestRangeInt32 + 1 == viewModel.TestRangeInt32, "Property value is different from set one.");
			Assert.That(nameof(TestViewModel.TestRangeInt32) == latestChangedPropertyName, "PropertyChanged not received.");
			latestChangedPropertyName = "";
			viewModel.TestRangeInt32 = TestViewModel.MaxTestRangeInt32 + 1;
			Assert.That(TestViewModel.MaxTestRangeInt32 == viewModel.TestRangeInt32, "Property value is not coerced.");
			Assert.That(nameof(TestViewModel.TestRangeInt32) == latestChangedPropertyName, "PropertyChanged not received.");
			latestChangedPropertyName = "";
			viewModel.TestRangeInt32 = TestViewModel.MaxTestRangeInt32 + 2;
			Assert.That(TestViewModel.MaxTestRangeInt32 == viewModel.TestRangeInt32, "Property value is not coerced.");
			Assert.That("" == latestChangedPropertyName, "PropertyChanged should not be raised.");
			viewModel.TestRangeInt32 = TestViewModel.MinTestRangeInt32 - 1;
			Assert.That(TestViewModel.MinTestRangeInt32 == viewModel.TestRangeInt32, "Property value is not coerced.");
			Assert.That(nameof(TestViewModel.TestRangeInt32) == latestChangedPropertyName, "PropertyChanged not received.");
			latestChangedPropertyName = "";
			viewModel.TestRangeInt32 = TestViewModel.MinTestRangeInt32 - 2;
			Assert.That(TestViewModel.MinTestRangeInt32 == viewModel.TestRangeInt32, "Property value is not coerced.");
			Assert.That("" == latestChangedPropertyName, "PropertyChanged should not be raised.");

			// get property as IObservable
			var observable = viewModel.GetValueAsObservable(TestViewModel.TestInt32Property);
			var observer = new Observer<int>();
			using var subscribedObserver = observable.Subscribe(observer);
			viewModel.TestInt32 = -123;
			Assert.That(-123 == observer.LatestValue, "Observer for property was not notified.");
			viewModel.TestInt32 = 123;
			Assert.That(123 == observer.LatestValue, "Observer for property was not notified.");
			latestChangedPropertyName = "";

			// change property with validation function
			try
			{
				viewModel.TestRangeInt32 = TestViewModel.InvalidTestRangeInt32;
				throw new AssertionException("Property value is not validated.");
			}
			catch (Exception ex)
			{
				if (ex is AssertionException)
					throw;
			}
			Assert.That("" == latestChangedPropertyName, "PropertyChanged should not be raised.");

			// get property from another thread
			var exception = (Exception?)null;
			var syncLock = new object();
			lock (syncLock)
			{
				// ReSharper disable once AllUnderscoreLocalParameterName
				// ReSharper disable once RedundantAssignment
				ThreadPool.QueueUserWorkItem(_ =>
				{
					try
					{
						// ReSharper disable once RedundantAssignment
						// ReSharper disable once AssignmentInsteadOfDiscard
						_ = viewModel.TestInt32;
					}
					catch (Exception ex)
					{
						exception = ex;
					}
					finally
					{
						lock (syncLock)
							Monitor.Pulse(syncLock);
					}
				});
				Assert.That(Monitor.Wait(syncLock, 5000), "Unable to complete waiting for test.");
			}
			if (exception == null)
				throw new AssertionException("Should not allow getting property from another thread.");

			// set property from another thread
			lock (syncLock)
			{
				ThreadPool.QueueUserWorkItem(_ =>
				{
					try
					{
						viewModel.TestInt32 = -1;
					}
					catch (Exception ex)
					{
						exception = ex;
					}
					finally
					{
						lock (syncLock)
							Monitor.Pulse(syncLock);
					}
				});
				Assert.That(Monitor.Wait(syncLock, 5000), "Unable to complete waiting for test.");
			}
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (exception is null)
				throw new AssertionException("Should not allow setting property from another thread.");
		});
	}


	/// <summary>
	/// Test for adding and disposing resources.
	/// </summary>
	[Test]
	public Task ResourcesTest() =>
		this.testSyncContext?.SendAsync(async Task () =>
		{
			var disposableResources = new List<Disposable>();
			var asyncDisposableResources = new List<AsyncDisposable>();
			var isCustomResourceReady = false;
			var isCustomResourceDisposed = false;
			var isAsyncCustomResourceReady = false;
			var isAsyncCustomResourceDisposed = false;
			var isSlowAsyncCustomResourceReady = false;
			var isSlowAsyncCustomResourceDisposed = false;
			using (var viewModel = new TestViewModel(this.application.AsNonNull()))
			{
				new Disposable().Let(it =>
				{
					viewModel.AddResource(it);
					disposableResources.Add(it);
				});
				new AsyncDisposable().Let(it =>
				{
					viewModel.AddResource(it);
					asyncDisposableResources.Add(it);
				});
				viewModel.AddResource(
					() => isCustomResourceReady = true,
					() => isCustomResourceDisposed = true
				);
				viewModel.AddResource(
					async Task () =>
					{
						await Task.Delay(500, CancellationToken.None);
						isAsyncCustomResourceReady = true;
					},
					async Task () =>
					{
						await Task.Delay(500, CancellationToken.None);
						isAsyncCustomResourceDisposed = true;
					}
				);
				viewModel.AddResource(
					async Task () =>
					{
						await Task.Delay(1500, CancellationToken.None);
						isSlowAsyncCustomResourceReady = true;
					},
					Task () =>
					{
						isSlowAsyncCustomResourceDisposed = true;
						return Task.CompletedTask;
					}
				);
				viewModel.AddResources(
					new Disposable().Also(it => disposableResources.Add(it)),
					new Disposable().Also(it => disposableResources.Add(it))
				);
				viewModel.AddResources(
					new AsyncDisposable().Also(it => asyncDisposableResources.Add(it)),
					new AsyncDisposable().Also(it => asyncDisposableResources.Add(it))
				);
				await Task.Delay(1000, CancellationToken.None);
				Assert.That(isCustomResourceReady);
				Assert.That(isAsyncCustomResourceReady);
				Assert.That(!isSlowAsyncCustomResourceReady);
			}
			await Task.Delay(1000, CancellationToken.None);
			foreach (var resource in disposableResources)
				Assert.That(resource.IsDisposed);
			foreach (var resource in asyncDisposableResources)
				Assert.That(resource.IsDisposed);
			Assert.That(isCustomResourceDisposed);
			Assert.That(isAsyncCustomResourceDisposed);
			Assert.That(isSlowAsyncCustomResourceReady);
			Assert.That(isSlowAsyncCustomResourceDisposed);
		}) ?? Task.FromException(new InvalidOperationException());


	/// <summary>
	/// Test for monitoring settings change.
	/// </summary>
	[Test]
	public void SettingsChangeTest()
	{
		this.testSyncContext?.Send(() =>
		{
			// prepare
			using var viewModel = new TestViewModel(this.application.AsNonNull());
			var settings = this.application.AsNonNull().Settings;

			// check initial state
			Assert.That(viewModel.LatestSettingChangedEventArgs is null, "OnSettingChanged should not be called.");
			Assert.That(viewModel.LatestSettingChangingEventArgs is null, "OnSettingChanging should not be called.");

			// change setting
#pragma warning disable CS0618
			settings.SetValue(TestSettings.Int32, 123);
			Assert.That(viewModel.LatestSettingChangedEventArgs is not null, "OnSettingChanged should be called.");
			Assert.That(viewModel.LatestSettingChangingEventArgs is not null, "OnSettingChanging should be called.");
			Assert.That(TestSettings.Int32 == viewModel.LatestSettingChangedEventArgs?.Key!, "Key received in OnSettingChanged is incorrect.");
			Assert.That(TestSettings.Int32 == viewModel.LatestSettingChangingEventArgs?.Key!, "Key received in OnSettingChanging is incorrect.");
			viewModel.LatestSettingChangedEventArgs = null;
			viewModel.LatestSettingChangingEventArgs = null;
			settings.SetValue(TestSettings.Int32, 123);
			Assert.That(viewModel.LatestSettingChangedEventArgs is null, "OnSettingChanged should not be called.");
			Assert.That(viewModel.LatestSettingChangingEventArgs is null, "OnSettingChanging should not be called.");

			// change setting again
			settings.SetValue(TestSettings.Int32, 456);
			Assert.That(viewModel.LatestSettingChangedEventArgs is not null, "OnSettingChanged should be called.");
			Assert.That(viewModel.LatestSettingChangingEventArgs is not null, "OnSettingChanging should be called.");
			Assert.That(TestSettings.Int32 == viewModel.LatestSettingChangedEventArgs?.Key!, "Key received in OnSettingChanged is incorrect.");
			Assert.That(TestSettings.Int32 == viewModel.LatestSettingChangingEventArgs?.Key!, "Key received in OnSettingChanging is incorrect.");
			viewModel.LatestSettingChangedEventArgs = null;
			viewModel.LatestSettingChangingEventArgs = null;

			// dispose view-model
			// ReSharper disable once DisposeOnUsingVariable
			viewModel.Dispose();

			// change setting after disposing
			settings.SetValue(TestSettings.Int32, 1234);
			Assert.That(viewModel.LatestSettingChangedEventArgs is null, "OnSettingChanged should not be called after disposing.");
			Assert.That(viewModel.LatestSettingChangingEventArgs is null, "OnSettingChanging should not be called after disposing.");
#pragma warning restore CS0618
		});
	}


	/// <summary>
	/// Test for calling <see cref="ViewModel.WaitForNecessaryTasksAsync"/>.
	/// </summary>
	[Test]
	public void WaitForNecessaryTasksTest()
	{
		object syncLock = new object();
		lock (syncLock)
		{
			// ReSharper disable once AsyncVoidLambda
			this.testSyncContext?.Post(async () =>
			{
				// prepare
				using var viewModel = new TestViewModel(this.application.AsNonNull());

				// perform necessary tasks
				Assert.That(!viewModel.HasNecessaryTasks);
				for (var t = 0; t < 10; ++t)
					_ = viewModel.PerformNecessaryTaskAsync();
				Assert.That(viewModel.HasNecessaryTasks);

				// wait for completion of tasks
				await viewModel.WaitForNecessaryTasksAsync();
				Assert.That(!viewModel.HasNecessaryTasks);

				// complete
				lock (syncLock)
					Monitor.Pulse(syncLock);
			});
			Assert.That(Monitor.Wait(syncLock, 10000));
		}
	}
}