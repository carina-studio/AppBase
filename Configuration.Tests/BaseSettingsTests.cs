using NUnit.Framework;
using System;

namespace CarinaStudio.Configuration
{
#pragma warning disable CS0618
	/// <summary>
	/// Tests of <see cref="ISettings"/>.
	/// </summary>
	abstract class BaseSettingsTests
	{
		/// <summary>
		/// Test for copying settings.
		/// </summary>
		[Test]
		public void CopyingTest()
		{
			// prepare base settings
			var baseSettings = this.CreateSettings();
			foreach (var key in TestSettings.AllKeys)
			{
				if (!TestSettings.TestValues.TryGetValue(key, out var value))
					continue;
				baseSettings.SetValue(key, value);
				Assert.AreNotEqual(value, key.DefaultValue, "Test value should be different from default value.");
				Assert.AreEqual(value, baseSettings.GetValueOrDefault(key), "Value should be same as set one.");
			}

			// copy settings
			var copiedSettings = this.CreateSettings(baseSettings);
			foreach (var key in TestSettings.AllKeys)
			{
				var baseValue = baseSettings.GetValueOrDefault(key);
				var copiedValue = copiedSettings.GetValueOrDefault(key);
				Assert.AreEqual(baseValue, copiedValue, "Value should be same as base.");
			}

			// reset bae settings.
			baseSettings.ResetValues();

			// check copied settings
			foreach (var key in TestSettings.AllKeys)
				Assert.AreNotEqual(copiedSettings.GetValueOrDefault(key), key.DefaultValue, "Copied value should be affected by base.");
		}


		/// <summary>
		/// Create settings instance.
		/// </summary>
		/// <param name="template">Template to copy initial values from.</param>
		/// <returns><see cref=" ISettings"/>.</returns>
		protected abstract ISettings CreateSettings(ISettings? template = null);


		/// <summary>
		/// Test for default values.
		/// </summary>
		[Test]
		public void DefaultValuesTest()
		{
			// prepare
			var settings = this.CreateSettings();

			// check all values
			foreach (var key in TestSettings.AllKeys)
			{
				var value = settings.GetValueOrDefault(key);
				Assert.AreEqual(key.DefaultValue, value, "Value should be default value.");
			}

			// modify values
			foreach (var key in TestSettings.AllKeys)
			{
				if (!TestSettings.TestValues.TryGetValue(key, out var value))
					continue;
				settings.SetValue(key, value);
				Assert.AreNotEqual(value, key.DefaultValue, "Test value should be different from default value.");
				Assert.AreEqual(value, settings.GetValueOrDefault(key), "Value should be same as set one.");
			}

			// reset all values
			settings.ResetValues();

			// check all values
			foreach (var key in TestSettings.AllKeys)
			{
				var value = settings.GetValueOrDefault(key);
				Assert.AreEqual(key.DefaultValue, value, "Value should be default value.");
			}
		}


		/// <summary>
		/// Test for events.
		/// </summary>
		[Test]
		public void EventsTest()
		{
			// prepare
			var settings = this.CreateSettings();
			var settingChangedEventArgs = (SettingChangedEventArgs?)null;
			var settingChangingEventArgs = (SettingChangingEventArgs?)null;
			settings.SettingChanged += (_, e) => settingChangedEventArgs = e;
			settings.SettingChanging += (_, e) => settingChangingEventArgs = e;

			// reset value
			settings.ResetValue(TestSettings.Int32Key);
			Assert.IsNull(settingChangingEventArgs, "Should not receive SettingChanging event.");
			Assert.IsNull(settingChangedEventArgs, "Should not receive SettingChanged event.");

			// modify value
			var testValue = TestSettings.TestValues[TestSettings.Int32Key];
			settings.SetValue(TestSettings.Int32Key, testValue);
			Assert.IsNotNull(settingChangingEventArgs, "Should receive SettingsChanging event.");
			Assert.AreEqual(TestSettings.Int32Key, settingChangingEventArgs?.Key, "Key reported by SettingChanging is incorrect.");
			Assert.AreEqual(TestSettings.Int32Key.DefaultValue, settingChangingEventArgs?.Value, "Current value reported by SettingChanging is incorrect.");
			Assert.AreEqual(testValue, settingChangingEventArgs?.NewValue, "New value reported by SettingChanging is incorrect.");
			Assert.IsNotNull(settingChangedEventArgs, "Should receive SettingChanged event.");
			Assert.AreEqual(TestSettings.Int32Key.DefaultValue, settingChangedEventArgs?.PreviousValue, "Previous value reported by SettingChanged is incorrect.");
			Assert.AreEqual(testValue, settingChangedEventArgs?.Value, "Current value reported by SettingChanged is incorrect.");

			// modify by same value
			settingChangingEventArgs = null;
			settingChangedEventArgs = null;
			settings.SetValue(TestSettings.Int32Key, testValue);
			Assert.IsNull(settingChangingEventArgs, "Should not receive SettingChanging event.");
			Assert.IsNull(settingChangedEventArgs, "Should not receive SettingChanged event.");
		}
	}
#pragma warning restore CS0618
}
