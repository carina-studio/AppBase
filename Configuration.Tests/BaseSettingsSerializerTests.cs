using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Base class for tests of <see cref="ISettingsSerializer"/>.
	/// </summary>
	abstract class BaseSettingsSerializerTests<TSerializer> where TSerializer : ISettingsSerializer
	{
		/// <summary>
		/// Create instance of <see cref="TSerializer"/>.
		/// </summary>
		/// <returns><see cref="TSerializer"/> instance.</returns>
		protected abstract TSerializer CreateSettingsSerializer();


		/// <summary>
		/// Test for serializing/deserializing to/from file.
		/// </summary>
		[Test]
		public void SerializingToFileTest()
		{
			// prepare empty settings
			var settings = new TestSettings(1, this.CreateSettingsSerializer());

			// save to file
			var filePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName).AsNonNull(), "test_settings.json");
			settings.Save(filePath);

			// modify values
			foreach (var keyValue in TestSettings.TestValues)
			{
				var key = keyValue.Key;
				var value = keyValue.Value;
				settings.SetValue(key, value);
				Assert.AreNotEqual(value, key.DefaultValue, "Test value should be different from default value.");
				Assert.AreEqual(value, settings.GetValueOrDefault(key), "Value should be same as set one.");
			}

			// load from file
			settings.Load(filePath);

			// check values
			Assert.IsFalse(settings.IsOnUpgradeCalled, "Setting upgrading should not be called.");
			foreach (var key in TestSettings.AllKeys)
				Assert.AreEqual(key.DefaultValue, settings.GetValueOrDefault(key), "Value should be same as default value.");

			// modify values
			foreach (var keyValue in TestSettings.TestValues)
				settings.SetValue(keyValue.Key, keyValue.Value);

			// save to file
			settings.Save(filePath);

			// reset and load from file
			settings.ResetAllValues();
			settings.Load(filePath);

			// check values
			Assert.IsFalse(settings.IsOnUpgradeCalled, "Setting upgrading should not be called.");
			foreach (var keyValue in TestSettings.TestValues)
				Assert.AreEqual(keyValue.Value, settings.GetValueOrDefault(keyValue.Key), "Value should be same as value before saving.");
		}


		/// <summary>
		/// Test for serializing/deserializing to/from memory.
		/// </summary>
		[Test]
		public void SerializingToMemoryTest()
		{
			// prepare empty settings
			var settings = new TestSettings(1, this.CreateSettingsSerializer());

			// save to memory
			var data = new MemoryStream().Use((stream) =>
			{
				settings.Save(stream);
				return stream.ToArray();
			});

			// modify values
			foreach (var keyValue in TestSettings.TestValues)
			{
				var key = keyValue.Key;
				var value = keyValue.Value;
				settings.SetValue(key, value);
				Assert.AreNotEqual(value, key.DefaultValue, "Test value should be different from default value.");
				Assert.AreEqual(value, settings.GetValueOrDefault(key), "Value should be same as set one.");
			}

			// load from memory
			using (var stream = new MemoryStream(data))
				settings.Load(stream);

			// check values
			Assert.IsFalse(settings.IsOnUpgradeCalled, "Setting upgrading should not be called.");
			foreach (var key in TestSettings.AllKeys)
				Assert.AreEqual(key.DefaultValue, settings.GetValueOrDefault(key), "Value should be same as default value.");

			// modify values
			foreach (var keyValue in TestSettings.TestValues)
				settings.SetValue(keyValue.Key, keyValue.Value);

			// save to memory
			data = new MemoryStream().Use((stream) =>
			{
				settings.Save(stream);
				return stream.ToArray();
			});

			// reset and load from memory
			settings.ResetAllValues();
			using (var stream = new MemoryStream(data))
				settings.Load(stream);

			// check values
			Assert.IsFalse(settings.IsOnUpgradeCalled, "Setting upgrading should not be called.");
			foreach (var keyValue in TestSettings.TestValues)
				Assert.AreEqual(keyValue.Value, settings.GetValueOrDefault(keyValue.Key), "Value should be same as value before saving.");
		}


		/// <summary>
		/// Test for upgrading.
		/// </summary>
		[Test]
		public void UpgradingTest()
		{
			// prepare settings with older version
			var settings = new TestSettings(1, this.CreateSettingsSerializer());
			foreach (var keyValue in TestSettings.TestValues)
				settings.SetValue(keyValue.Key, keyValue.Value);

			// save to memory
			var data = new MemoryStream().Use((stream) =>
			{
				settings.Save(stream);
				return stream.ToArray();
			});

			// create settings with newer version
			var newSettings = new TestSettings(2, this.CreateSettingsSerializer());

			// load from memory
			using (var stream = new MemoryStream(data))
				newSettings.Load(stream);

			// check upgrading state
			Assert.IsTrue(newSettings.IsOnUpgradeCalled, "Upgrading should happens after loading.");
			Assert.AreEqual(1, newSettings.OldVersion, "Incorrect old version when upgrading.");
			newSettings.IsOnUpgradeCalled = false;

			// save and load new settings
			data = new MemoryStream().Use((stream) =>
			{
				newSettings.Save(stream);
				return stream.ToArray();
			});
			using (var stream = new MemoryStream(data))
				newSettings.Load(stream);

			// check upgrading state
			Assert.IsFalse(newSettings.IsOnUpgradeCalled, "Upgrading should not happens after loading.");
		}
	}
}
