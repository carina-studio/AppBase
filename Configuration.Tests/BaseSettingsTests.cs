using NUnit.Framework;
using System;
using System.Linq;

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
				Assert.That(key.DefaultValue, Is.Not.EqualTo(value), "Test value should be different from default value.");
				Assert.That(baseSettings.GetValueOrDefault(key), Is.EqualTo(value), "Value should be same as set one.");
			}

			// copy settings
			var copiedSettings = this.CreateSettings(baseSettings);
			foreach (var key in TestSettings.AllKeys)
			{
				var baseValue = baseSettings.GetValueOrDefault(key);
				var copiedValue = copiedSettings.GetValueOrDefault(key);
				Assert.That(copiedValue, Is.EqualTo(baseValue), "Value should be same as base.");
			}

			// reset bae settings.
			baseSettings.ResetValues();

			// check copied settings
			foreach (var key in TestSettings.AllKeys)
				Assert.That(key.DefaultValue, Is.Not.EqualTo(copiedSettings.GetValueOrDefault(key)), "Copied value should be affected by base.");
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
				Assert.That(value, Is.EqualTo(key.DefaultValue), "Value should be default value.");
			}

			// modify values
			foreach (var key in TestSettings.AllKeys)
			{
				if (!TestSettings.TestValues.TryGetValue(key, out var value))
					continue;
				settings.SetValue(key, value);
				Assert.That(key.DefaultValue, Is.Not.EqualTo(value), "Test value should be different from default value.");
				Assert.That(settings.GetValueOrDefault(key), Is.EqualTo(value), "Value should be same as set one.");
			}

			// reset all values
			settings.ResetValues();

			// check all values
			foreach (var key in TestSettings.AllKeys)
			{
				var value = settings.GetValueOrDefault(key);
				Assert.That(value, Is.EqualTo(key.DefaultValue), "Value should be default value.");
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
			Assert.That(settingChangingEventArgs, Is.Null, "Should not receive SettingChanging event.");
			Assert.That(settingChangedEventArgs, Is.Null, "Should not receive SettingChanged event.");

			// modify value
			var testValue = TestSettings.TestValues[TestSettings.Int32Key];
			settings.SetValue(TestSettings.Int32Key, testValue);
			Assert.That(settingChangingEventArgs, Is.Not.Null, "Should receive SettingsChanging event.");
			Assert.That(settingChangingEventArgs?.Key, Is.EqualTo(TestSettings.Int32Key), "Key reported by SettingChanging is incorrect.");
			Assert.That(settingChangingEventArgs?.Value, Is.EqualTo(TestSettings.Int32Key.DefaultValue), "Current value reported by SettingChanging is incorrect.");
			Assert.That(settingChangingEventArgs?.NewValue, Is.EqualTo(testValue), "New value reported by SettingChanging is incorrect.");
			Assert.That(settingChangedEventArgs, Is.Not.Null, "Should receive SettingChanged event.");
			Assert.That(settingChangedEventArgs?.PreviousValue, Is.EqualTo(TestSettings.Int32Key.DefaultValue), "Previous value reported by SettingChanged is incorrect.");
			Assert.That(settingChangedEventArgs?.Value, Is.EqualTo(testValue), "Current value reported by SettingChanged is incorrect.");

			// modify by same value
			settingChangingEventArgs = null;
			settingChangedEventArgs = null;
			settings.SetValue(TestSettings.Int32Key, testValue);
			Assert.That(settingChangingEventArgs, Is.Null, "Should not receive SettingChanging event.");
			Assert.That(settingChangedEventArgs, Is.Null, "Should not receive SettingChanged event.");
		}


		/// <summary>
		/// Test for updating key.
		/// </summary>
		[Test]
		public void KeyUpdatingTest()
        {
			// prepare
			var settings = this.CreateSettings();
			var settingChangedEventArgs = (SettingChangedEventArgs?)null;
			var settingChangingEventArgs = (SettingChangingEventArgs?)null;
			var boolKey = TestSettings.BooleanKey;
			var anotherBoolKey = new SettingKey<bool>(boolKey.Name, !boolKey.DefaultValue);
			var anotherBoolKey2 = new SettingKey<bool>(boolKey.Name, boolKey.DefaultValue);
			settings.SettingChanged += (_, e) => settingChangedEventArgs = e;
			settings.SettingChanging += (_, e) => settingChangingEventArgs = e;

			// reset value
			Assert.That(settings.GetValueOrDefault(boolKey), Is.EqualTo(boolKey.DefaultValue));
			Assert.That(settings.GetValueOrDefault(anotherBoolKey), Is.EqualTo(anotherBoolKey.DefaultValue));
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, boolKey)), Is.Null);
			settings.ResetValue(anotherBoolKey);
			Assert.That(settingChangingEventArgs, Is.Null);
			Assert.That(settingChangedEventArgs, Is.Null);
			Assert.That(settings.GetValueOrDefault(boolKey), Is.EqualTo(boolKey.DefaultValue));
			Assert.That(settings.GetValueOrDefault(anotherBoolKey), Is.EqualTo(anotherBoolKey.DefaultValue));
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, anotherBoolKey)), Is.Null);

			// set value
			settings.SetValue<bool>(anotherBoolKey, anotherBoolKey.DefaultValue);
			Assert.That(settingChangingEventArgs, Is.Null);
			Assert.That(settingChangedEventArgs, Is.Null);
			Assert.That(settings.GetValueOrDefault(anotherBoolKey), Is.EqualTo(anotherBoolKey.DefaultValue));
			settings.SetValue<bool>(boolKey, anotherBoolKey.DefaultValue);
			Assert.That(settingChangingEventArgs, Is.Not.Null);
			Assert.That(settingChangedEventArgs, Is.Not.Null);
			Assert.That(settings.GetValueOrDefault(anotherBoolKey), Is.EqualTo(anotherBoolKey.DefaultValue));
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, boolKey)), Is.Not.Null);
			settingChangingEventArgs = null;
			settingChangedEventArgs = null;
			settings.SetValue<bool>(boolKey, boolKey.DefaultValue);
			Assert.That(settingChangingEventArgs, Is.Not.Null);
			Assert.That(settingChangedEventArgs, Is.Not.Null);
			Assert.That(settings.GetValueOrDefault(boolKey), Is.EqualTo(boolKey.DefaultValue));
			settingChangingEventArgs = null;
			settingChangedEventArgs = null;
			settings.SetValue<bool>(anotherBoolKey, boolKey.DefaultValue);
			Assert.That(settingChangingEventArgs, Is.Not.Null);
			Assert.That(settingChangedEventArgs, Is.Not.Null);
			Assert.That(settings.GetValueOrDefault(anotherBoolKey), Is.EqualTo(boolKey.DefaultValue));
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, anotherBoolKey)), Is.Not.Null);
			settingChangingEventArgs = null;
			settingChangedEventArgs = null;
			settings.SetValue<bool>(anotherBoolKey, anotherBoolKey.DefaultValue);
			Assert.That(settingChangingEventArgs, Is.Not.Null);
			Assert.That(settingChangedEventArgs, Is.Not.Null);
			Assert.That(settings.GetValueOrDefault(anotherBoolKey), Is.EqualTo(anotherBoolKey.DefaultValue));
			settingChangingEventArgs = null;
			settingChangedEventArgs = null;
			settings.SetValue<bool>(boolKey, !boolKey.DefaultValue);
			Assert.That(settingChangingEventArgs, Is.Not.Null);
			Assert.That(settingChangedEventArgs, Is.Not.Null);
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, boolKey)), Is.Not.Null);
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, anotherBoolKey)), Is.Null);
			settingChangingEventArgs = null;
			settingChangedEventArgs = null;
			settings.SetValue<bool>(anotherBoolKey2, !boolKey.DefaultValue);
			Assert.That(settingChangingEventArgs, Is.Null);
			Assert.That(settingChangedEventArgs, Is.Null);
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, boolKey)), Is.Null);
			Assert.That(settings.Keys.FirstOrDefault(it => object.ReferenceEquals(it, anotherBoolKey2)), Is.Not.Null);
		}
	}
#pragma warning restore CS0618
}
