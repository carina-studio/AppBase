using CarinaStudio.Collections;
using CarinaStudio.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarinaStudio.ViewModels
{
	/// <summary>
	/// Test implementation of <see cref="ViewModel"/>.
	/// </summary>
	class TestViewModel : ViewModel
	{
		// Properties.
		public static readonly ObservableProperty<int> TestInt32Property = ObservableProperty.Register<TestViewModel, int>(nameof(TestInt32));
		public static readonly ObservableProperty<int> TestRangeInt32Property = ObservableProperty.Register<TestViewModel, int>(nameof(TestRangeInt32), 1, (o, value) => Math.Max(Math.Min(MaxTestRangeInt32, value), MinTestRangeInt32), (value) => value != InvalidTestRangeInt32);


		// Constants.
		public const int InvalidTestRangeInt32 = 0;
		public const int MaxTestRangeInt32 = 100;
		public const int MinTestRangeInt32 = -100;


		// Fields.
		public SettingChangedEventArgs? LatestSettingChangedEventArgs;
		public SettingChangingEventArgs? LatestSettingChangingEventArgs;
		readonly Random random = new Random();


		// Constructor.
		public TestViewModel(TestApplication app) : base(app)
		{ }


		// Setting changed.
		protected override void OnSettingChanged(SettingChangedEventArgs e)
		{
			base.OnSettingChanged(e);
			this.LatestSettingChangedEventArgs = e;
		}


		// Setting changing.
		protected override void OnSettingChanging(SettingChangingEventArgs e)
		{
			base.OnSettingChanging(e);
			this.LatestSettingChangingEventArgs = e;
		}


		// Perform necessary task.
		public async Task PerformNecessaryTaskAsync() => await this.WaitForNecessaryTaskAsync(Task.Delay(this.random.Next(50, 3000)));


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
