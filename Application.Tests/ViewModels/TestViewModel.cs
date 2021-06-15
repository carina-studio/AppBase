using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CarinaStudio.ViewModels
{
	/// <summary>
	/// Test implementation of <see cref="ViewModel"/>.
	/// </summary>
	class TestViewModel : ViewModel
	{
		// Properties.
		public static readonly ObservableProperty<int> TestInt32Property = ObservableProperty.Register<TestViewModel, int>(nameof(TestInt32));
		public static readonly ObservableProperty<int> TestRangeInt32Property = ObservableProperty.Register<TestViewModel, int>(nameof(TestRangeInt32), 1, (value) => Math.Max(Math.Min(MaxTestRangeInt32, value), MinTestRangeInt32), (value) => value != InvalidTestRangeInt32);


		// Constants.
		public const int InvalidTestRangeInt32 = 0;
		public const int MaxTestRangeInt32 = 100;
		public const int MinTestRangeInt32 = -100;


		// Constructor.
		public TestViewModel(TestApplication app) : base(app)
		{ }


		// Print log.
		public void PrintLog(LogLevel level, EventId eventId, string message) => this.Logger.Log(level, eventId, message);


		// Test property (Int32).
		public int TestInt32
		{
			get => this.GetValue(TestInt32Property);
			set => this.SetValue(TestInt32Property, value);
		}


		// Test property (Int32).
		public int TestRangeInt32
		{
			get => this.GetValue(TestRangeInt32Property);
			set => this.SetValue(TestRangeInt32Property, value);
		}
	}
}
