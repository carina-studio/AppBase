using NUnit.Framework;
using System;
using System.ComponentModel;

namespace CarinaStudio.Tests
{
	/// <summary>
	/// Tests of <see cref="EventMonitor{TArgs}"/>.
	/// </summary>
	[TestFixture]
	class EventMonitorTests
	{
		// Test class.
		class TestObject
		{
			public PropertyChangedEventArgs? LatestEventArgs;
			public void RaiseTestEvents(int count = 1)
			{
				for (var i = 0; i < count; ++i)
				{
					this.LatestEventArgs = new PropertyChangedEventArgs("");
					this.TestEvent?.Invoke(this, this.LatestEventArgs);
				}
			}
			public event EventHandler<PropertyChangedEventArgs>? TestEvent;
		}


		// Static fields.
		static PropertyChangedEventArgs? LatestStaticEventArgs;


		/// <summary>
		/// Test for monitoring instance event.
		/// </summary>
		[Test]
		public void MonitoringInstanceEventTest()
		{
			// prepare
			var obj = new TestObject();
			using var monitor = new EventMonitor<PropertyChangedEventArgs>(obj, nameof(TestObject.TestEvent));
			Assert.That(0 == monitor.EventCount);
			Assert.That(monitor.EventArgs is null);

			// raise event
			obj.RaiseTestEvents();
			Assert.That(1 == monitor.EventCount);
			Assert.That(ReferenceEquals(obj.LatestEventArgs, monitor.EventArgs));

			// raise event multiple times
			obj.RaiseTestEvents(128);
			Assert.That(129 == monitor.EventCount);
			Assert.That(ReferenceEquals(obj.LatestEventArgs, monitor.EventArgs));

			// dispose monitor
			// ReSharper disable once DisposeOnUsingVariable
			monitor.Dispose();

			// raise event after disposing
			obj.RaiseTestEvents(128);
			Assert.That(129 == monitor.EventCount);
			Assert.That(!ReferenceEquals(obj.LatestEventArgs, monitor.EventArgs));
		}


		/// <summary>
		/// Test for monitoring static event.
		/// </summary>
		[Test]
		public void MonitoringStaticEventTest()
		{
			// prepare
			using var monitor = new EventMonitor<PropertyChangedEventArgs>(typeof(EventMonitorTests), nameof(TestStaticEvent));
			Assert.That(0 == monitor.EventCount);
			Assert.That(monitor.EventArgs is null);

			// raise event
			RaiseTestStaticEvents();
			Assert.That(1 == monitor.EventCount);
			Assert.That(ReferenceEquals(LatestStaticEventArgs, monitor.EventArgs));

			// raise event multiple times
			RaiseTestStaticEvents(128);
			Assert.That(129 == monitor.EventCount);
			Assert.That(ReferenceEquals(LatestStaticEventArgs, monitor.EventArgs));

			// dispose monitor
			// ReSharper disable once DisposeOnUsingVariable
			monitor.Dispose();

			// raise event after disposing
			RaiseTestStaticEvents(128);
			Assert.That(129 == monitor.EventCount);
			Assert.That(!ReferenceEquals(LatestStaticEventArgs, monitor.EventArgs));
		}


		// Raise test static events.
		static void RaiseTestStaticEvents(int count = 1)
		{
			for (var i = 0; i < count; ++i)
			{
				LatestStaticEventArgs = new PropertyChangedEventArgs("");
				TestStaticEvent?.Invoke(null, LatestStaticEventArgs);
			}
		}


		// Static test event.
		public static event EventHandler<PropertyChangedEventArgs>? TestStaticEvent;
	}
}
