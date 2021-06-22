using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Tests
{
	/// <summary>
	/// Tests of <see cref="NotifyPropertyChangedExtensions"/>.
	/// </summary>
	[TestFixture]
	class NotifyPropertyChangedExtensionsTests
	{
		// Text implementation of INotifyPropertyChanged.
		class TestObject : INotifyPropertyChanged
		{
			// Fields.
			volatile int testProperty;

			// Test property.
			public int TestProperty
			{
				get => this.testProperty;
				set
				{
					lock (this)
					{
						if (this.testProperty == value)
							return;
						this.testProperty = value;
						this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestProperty)));
					}
				}
			}

			// Implementations.
			public event PropertyChangedEventHandler? PropertyChanged;
		}


		/// <summary>
		/// Test for waiting for property values.
		/// </summary>
		[Test]
		public async Task WaitingForPropertyTest()
		{
			// prepare
			var obj = new TestObject();

			// wait for current property
			var waitingResult = await obj.WaitForPropertyAsync(nameof(TestObject.TestProperty), obj.TestProperty);
			Assert.IsTrue(waitingResult);

			// change property before timeout
			_ = Task.Delay(300).ContinueWith(_ => obj.TestProperty = 123);
			waitingResult = await obj.WaitForPropertyAsync(nameof(TestObject.TestProperty), 123, 500);
			Assert.IsTrue(waitingResult);
			Assert.AreEqual(123, obj.TestProperty);

			// change property after timeout
			_ = Task.Delay(500).ContinueWith(_ => obj.TestProperty = 321);
			waitingResult = await obj.WaitForPropertyAsync(nameof(TestObject.TestProperty), 321, 300);
			Assert.IsFalse(waitingResult);
			Assert.AreNotEqual(321, obj.TestProperty);
			Thread.Sleep(400);
			Assert.AreEqual(321, obj.TestProperty);

			// cancel waiting before changing property
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				_ = Task.Delay(500).ContinueWith(_ => obj.TestProperty = 456);
				_ = Task.Delay(300).ContinueWith(_ => cancellationTokenSource.Cancel());
				waitingResult = await obj.WaitForPropertyAsync(nameof(TestObject.TestProperty), 456, cancellationToken: cancellationTokenSource.Token);
				Assert.IsFalse(waitingResult);
				Assert.AreNotEqual(456, obj.TestProperty);
				Thread.Sleep(400);
				Assert.AreEqual(456, obj.TestProperty);
			}

			// cancel waiting after changing property
			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				_ = Task.Delay(300).ContinueWith(_ => obj.TestProperty = 789);
				_ = Task.Delay(500).ContinueWith(_ => cancellationTokenSource.Cancel());
				waitingResult = await obj.WaitForPropertyAsync(nameof(TestObject.TestProperty), 789, cancellationToken: cancellationTokenSource.Token);
				Assert.IsTrue(waitingResult);
				Assert.AreEqual(789, obj.TestProperty);
			}

			// wait for invalid property
			try
			{
				await obj.WaitForPropertyAsync(nameof(TestObject.TestProperty) + "_", 0);
				throw new AssertionException("Should not be able to wait for invalid property.");
			}
			catch (Exception ex)
			{
				if (ex is AssertionException)
					throw;
			}
		}
	}
}
