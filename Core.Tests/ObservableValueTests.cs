using NUnit.Framework;
using System;

namespace CarinaStudio
{
	/// <summary>
	/// Tests of <see cref="ObservableValue{T}"/>.
	/// </summary>
	[TestFixture]
	class ObservableValueTests
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


		/// <summary>
		/// Test for <see cref="ObservableExtensions.Invert"/>
		/// </summary>
		[Test]
		public void InversionTest()
		{
			var notifiedValue = default(bool?);
			var source = new MutableObservableBoolean(false);
			var inverted = source.Invert();
			inverted.Subscribe(value => notifiedValue = value);
			Assert.That(notifiedValue!.Value);
			notifiedValue = default;
			source.Update(true);
			Assert.That(!notifiedValue!.Value);
		}


		/// <summary>
		/// Test for observer subscription.
		/// </summary>
		[Test]
		public void ObserverSubscriptionTest()
		{
			// prepare
			var observableValue = new MutableObservableValue<int>(1);
			var observer1 = new Observer<int>();
			var observer2 = new Observer<int>();
			var subscription1 = observableValue.Subscribe(observer1);
			var subscription2 = (IDisposable?)observableValue.Subscribe(observer2);

			// check initial notification
			Assert.That(observer1.IsOnNextCalled, "First notification should be sent.");
			Assert.That(observer2.IsOnNextCalled, "First notification should be sent.");
			Assert.That(1 == observer1.LatestValue, "Value of initial notification is unexpected.");
			Assert.That(1 == observer2.LatestValue, "Value of initial notification is unexpected.");

			// update value
			observer1.IsOnNextCalled = false;
			observer2.IsOnNextCalled = false;
			observableValue.Update(2);
			Assert.That(observer1.IsOnNextCalled, "Notification should be sent.");
			Assert.That(observer2.IsOnNextCalled, "Notification should be sent.");
			Assert.That(2 == observer1.LatestValue, "Value of notification is unexpected.");
			Assert.That(2 == observer2.LatestValue, "Value of notification is unexpected.");

			// unsubscribe
			subscription2 = subscription2.DisposeAndReturnNull();
			observer1.IsOnNextCalled = false;
			observer2.IsOnNextCalled = false;
			observableValue.Update(3);
			Assert.That(observer1.IsOnNextCalled, "Notification should be sent.");
			Assert.That(!observer2.IsOnNextCalled, "Notification should not be sent.");
			Assert.That(3 == observer1.LatestValue, "Value of notification is unexpected.");

			// subscribe when updating value
			var isFirstOnNext3 = true;
			var observer3 = new Observer<int>(() =>
			{
				if (isFirstOnNext3)
					isFirstOnNext3 = false;
				else if (subscription2 == null)
					subscription2 = observableValue.Subscribe(observer2);
				else
					subscription2 = subscription2.DisposeAndReturnNull();
			});
			var subscription3 = observableValue.Subscribe(observer3);
			observer1.IsOnNextCalled = false;
			observer3.IsOnNextCalled = false;
			Assert.That(!observer2.IsOnNextCalled, "Notification should not be sent.");
			observableValue.Update(0);
			Assert.That(observer1.IsOnNextCalled, "Notification should be sent.");
			Assert.That(observer2.IsOnNextCalled, "Notification should be sent.");
			Assert.That(observer3.IsOnNextCalled, "Notification should be sent.");

			// unsubscribe when updating value
			observer1.IsOnNextCalled = false;
			observer2.IsOnNextCalled = false;
			observer3.IsOnNextCalled = false;
			observableValue.Update(1);
			Assert.That(observer1.IsOnNextCalled, "Notification should be sent.");
			Assert.That(!observer2.IsOnNextCalled, "Notification should not be sent.");
			Assert.That(observer3.IsOnNextCalled, "Notification should be sent.");

			// subscribe
			subscription3.Dispose();
			var observer4 = new Observer<int>();
			_ = observableValue.Subscribe(observer4);
			observer1.IsOnNextCalled = false;
			observer2.IsOnNextCalled = false;
			observer3.IsOnNextCalled = false;
			observer4.IsOnNextCalled = false;
			observableValue.Update(2);
			Assert.That(observer1.IsOnNextCalled, "Notification should be sent.");
			Assert.That(!observer2.IsOnNextCalled, "Notification should not be sent.");
			Assert.That(!observer3.IsOnNextCalled, "Notification should not be sent.");
			Assert.That(observer4.IsOnNextCalled, "Notification should be sent.");

			// complete
			subscription1.Dispose();
		}


		/// <summary>
		/// Test for updating value.
		/// </summary>
		[Test]
		public void ValueUpdatingTest()
		{
			// prepare
			var observableValue = new MutableObservableValue<int>(1);
			var observer1 = new Observer<int>();
			using var subscription1 = observableValue.Subscribe(observer1);

			// check initial notification
			Assert.That(observer1.IsOnNextCalled, "First notification should be sent.");
			Assert.That( 1== observer1.LatestValue, "Value of initial notification is unexpected.");

			// update different value
			observer1.IsOnNextCalled = false;
			observableValue.Update(2);
			Assert.That(observer1.IsOnNextCalled, "Notification should be sent.");
			Assert.That(2 == observer1.LatestValue, "Value of notification is unexpected.");

			// update same value
			observer1.IsOnNextCalled = false;
			observableValue.Update(2);
			Assert.That(!observer1.IsOnNextCalled, "Notification should not be sent.");

			// update value nestedly
			var observer2 = new Observer<int>(() =>
			{
				observableValue.Update(123);
			});
			var observer3 = new Observer<int>();
			using var subscription2 = observableValue.Subscribe(observer2);
			using var subscription3 = observableValue.Subscribe(observer3);
			observer2.IsOnNextCalled = false;
			observer3.IsOnNextCalled = false;
			observableValue.Update(3);
			Assert.That(observer1.IsOnNextCalled, "Notification should be sent.");
			Assert.That(observer2.IsOnNextCalled, "Notification should be sent.");
			Assert.That(observer3.IsOnNextCalled, "Notification should be sent.");
			Assert.That(123 == observer1.LatestValue, "Value of notification is unexpected.");
			Assert.That(123 == observer2.LatestValue, "Value of notification is unexpected.");
			Assert.That(123 == observer3.LatestValue, "Value of notification is unexpected.");
		}
	}
}
