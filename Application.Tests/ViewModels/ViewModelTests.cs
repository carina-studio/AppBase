using CarinaStudio.Threading;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CarinaStudio.ViewModels
{
	/// <summary>
	/// Tests of <see cref="ViewModel"/>.
	/// </summary>
	[TestFixture]
	class ViewModelTests
	{
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
					Assert.IsTrue(usingId.Add(viewModel.Id), "Duplicate ID of view-model.");
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
				Assert.AreEqual(categoryName, logItem1.CategoryName);
				Assert.AreEqual(LogLevel.Debug, logItem1.Level);
				Assert.AreEqual(eventId1, logItem1.EventId);
				Assert.AreEqual("Message 1", logItem1.Message);
				var logItem2 = logItems[originalLogItemCount + 1];
				Assert.AreEqual(categoryName, logItem2.CategoryName);
				Assert.AreEqual(LogLevel.Error, logItem2.Level);
				Assert.AreEqual(eventId2, logItem2.EventId);
				Assert.AreEqual("Message 2", logItem2.Message);
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
				Assert.AreEqual(TestViewModel.TestInt32Property.DefaultValue, viewModel.TestInt32, "Default property value is incorrect.");
				Assert.AreEqual(TestViewModel.TestRangeInt32Property.DefaultValue, viewModel.TestRangeInt32, "Default property value is incorrect.");

				// change property
				viewModel.TestInt32 = 123;
				Assert.AreEqual(123, viewModel.TestInt32, "Property value is different from set one.");
				Assert.AreEqual(nameof(TestViewModel.TestInt32), latestChangedPropertyName, "PropertyChanged not received.");
				latestChangedPropertyName = "";
				viewModel.TestInt32 = 123;
				Assert.AreEqual("", latestChangedPropertyName, "PropertyChanged should not be raised.");
				viewModel.TestInt32 = TestViewModel.TestInt32Property.DefaultValue;
				Assert.AreEqual(TestViewModel.TestInt32Property.DefaultValue, viewModel.TestInt32, "Property value is different from set one.");
				Assert.AreEqual(nameof(TestViewModel.TestInt32), latestChangedPropertyName, "PropertyChanged not received.");
				latestChangedPropertyName = "";

				// change property with coercion function
				viewModel.TestRangeInt32 = TestViewModel.MaxTestRangeInt32 - 1;
				Assert.AreEqual(TestViewModel.MaxTestRangeInt32 - 1, viewModel.TestRangeInt32, "Property value is different from set one.");
				Assert.AreEqual(nameof(TestViewModel.TestRangeInt32), latestChangedPropertyName, "PropertyChanged not received.");
				latestChangedPropertyName = "";
				viewModel.TestRangeInt32 = TestViewModel.MinTestRangeInt32 + 1;
				Assert.AreEqual(TestViewModel.MinTestRangeInt32 + 1, viewModel.TestRangeInt32, "Property value is different from set one.");
				Assert.AreEqual(nameof(TestViewModel.TestRangeInt32), latestChangedPropertyName, "PropertyChanged not received.");
				latestChangedPropertyName = "";
				viewModel.TestRangeInt32 = TestViewModel.MaxTestRangeInt32 + 1;
				Assert.AreEqual(TestViewModel.MaxTestRangeInt32, viewModel.TestRangeInt32, "Property value is not coerced.");
				Assert.AreEqual(nameof(TestViewModel.TestRangeInt32), latestChangedPropertyName, "PropertyChanged not received.");
				latestChangedPropertyName = "";
				viewModel.TestRangeInt32 = TestViewModel.MaxTestRangeInt32 + 2;
				Assert.AreEqual(TestViewModel.MaxTestRangeInt32, viewModel.TestRangeInt32, "Property value is not coerced.");
				Assert.AreEqual("", latestChangedPropertyName, "PropertyChanged should not be raised.");
				viewModel.TestRangeInt32 = TestViewModel.MinTestRangeInt32 - 1;
				Assert.AreEqual(TestViewModel.MinTestRangeInt32, viewModel.TestRangeInt32, "Property value is not coerced.");
				Assert.AreEqual(nameof(TestViewModel.TestRangeInt32), latestChangedPropertyName, "PropertyChanged not received.");
				latestChangedPropertyName = "";
				viewModel.TestRangeInt32 = TestViewModel.MinTestRangeInt32 - 2;
				Assert.AreEqual(TestViewModel.MinTestRangeInt32, viewModel.TestRangeInt32, "Property value is not coerced.");
				Assert.AreEqual("", latestChangedPropertyName, "PropertyChanged should not be raised.");

				// get property as IObservable
				var observable = viewModel.GetValueAsObservable(TestViewModel.TestInt32Property);
				var observer = new Observer<int>(); ;
				using var subscribedObserver = observable.Subscribe(observer);
				viewModel.TestInt32 = -123;
				Assert.AreEqual(-123, observer.LatestValue, "Observer for property was not notified.");
				viewModel.TestInt32 = 123;
				Assert.AreEqual(123, observer.LatestValue, "Observer for property was not notified.");
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
				Assert.AreEqual("", latestChangedPropertyName, "PropertyChanged should not be raised.");

				// get property from another thread
				var exception = (Exception?)null;
				var syncLock = new object();
				lock (syncLock)
				{
					ThreadPool.QueueUserWorkItem((_) =>
					{
						try
						{
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
					Assert.IsTrue(Monitor.Wait(syncLock, 5000), "Unable to complete waiting for test.");
				}
				if (exception == null)
					throw new AssertionException("Should not allow getting property from another thread.");

				// set property from another thread
				lock (syncLock)
				{
					ThreadPool.QueueUserWorkItem((_) =>
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
					Assert.IsTrue(Monitor.Wait(syncLock, 5000), "Unable to complete waiting for test.");
				}
				if (exception == null)
					throw new AssertionException("Should not allow setting property from another thread.");
			});
		}


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
				Assert.IsNull(viewModel.LatestSettingChangedEventArgs, "OnSettingChanged should not be called.");
				Assert.IsNull(viewModel.LatestSettingChangingEventArgs, "OnSettingChanging should not be called.");

				// change setting
				settings.SetValue(TestSettings.Int32, 123);
				Assert.IsNotNull(viewModel.LatestSettingChangedEventArgs, "OnSettingChanged should be called.");
				Assert.IsNotNull(viewModel.LatestSettingChangingEventArgs, "OnSettingChanging should be called.");
				Assert.AreEqual(TestSettings.Int32, viewModel.LatestSettingChangedEventArgs?.Key, "Key received in OnSettingChanged is incorrect.");
				Assert.AreEqual(TestSettings.Int32, viewModel.LatestSettingChangingEventArgs?.Key, "Key received in OnSettingChanging is incorrect.");
				viewModel.LatestSettingChangedEventArgs = null;
				viewModel.LatestSettingChangingEventArgs = null;
				settings.SetValue(TestSettings.Int32, 123);
				Assert.IsNull(viewModel.LatestSettingChangedEventArgs, "OnSettingChanged should not be called.");
				Assert.IsNull(viewModel.LatestSettingChangingEventArgs, "OnSettingChanging should not be called.");

				// change setting again
				settings.SetValue(TestSettings.Int32, 456);
				Assert.IsNotNull(viewModel.LatestSettingChangedEventArgs, "OnSettingChanged should be called.");
				Assert.IsNotNull(viewModel.LatestSettingChangingEventArgs, "OnSettingChanging should be called.");
				Assert.AreEqual(TestSettings.Int32, viewModel.LatestSettingChangedEventArgs?.Key, "Key received in OnSettingChanged is incorrect.");
				Assert.AreEqual(TestSettings.Int32, viewModel.LatestSettingChangingEventArgs?.Key, "Key received in OnSettingChanging is incorrect.");
				viewModel.LatestSettingChangedEventArgs = null;
				viewModel.LatestSettingChangingEventArgs = null;

				// dispose view-model
				viewModel.Dispose();

				// change setting after disposing
				settings.SetValue(TestSettings.Int32, 1234);
				Assert.IsNull(viewModel.LatestSettingChangedEventArgs, "OnSettingChanged should not be called after disposing.");
				Assert.IsNull(viewModel.LatestSettingChangingEventArgs, "OnSettingChanging should not be called after disposing.");
			});
		}


		/// <summary>
		/// Test for calling <see cref="ViewModel.WaitForNecessaryTasksCompletionAsync"/>.
		/// </summary>
		[Test]
		public void WaitForNecessaryTasksCompletionTest()
		{
			object syncLock = new object();
			lock (syncLock)
			{
				this.testSyncContext?.Post(async () =>
				{
					// prepare
					using var viewModel = new TestViewModel(this.application.AsNonNull());

					// perform necessary tasks
					Assert.IsTrue(viewModel.AreNecessaryTasksCompleted);
					for (var t = 0; t < 10; ++t)
						_ = viewModel.PerformNecessaryTaskAsync();
					Assert.IsFalse(viewModel.AreNecessaryTasksCompleted);

					// wait for completion of tasks
					await viewModel.WaitForNecessaryTasksCompletionAsync();
					Assert.IsTrue(viewModel.AreNecessaryTasksCompleted);

					// complete
					lock (syncLock)
						Monitor.Pulse(syncLock);
				});
				Assert.IsTrue(Monitor.Wait(syncLock, 10000));
			}
		}
	}
}
