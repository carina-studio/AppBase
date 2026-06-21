using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Tests of <see cref="PersistentSettings"/>.
	/// </summary>
	[TestFixture]
	class PersistentSettingsTests : BaseSettingsTests
	{
		// MemoryStream that reports CanRead=false to test stream guards.
		class NonReadableStream : MemoryStream
		{
			public override bool CanRead => false;
		}


		// MemoryStream that reports CanSeek=false to test stream guards.
		class NonSeekableStream : MemoryStream
		{
			public override bool CanSeek => false;
		}


		// Create a fresh PersistentSettings instance.
		TestSettings CreatePersistentSettings() => new TestSettings(1, JsonSettingsSerializer.Default);


		// Create settings.
		protected override ISettings CreateSettings(ISettings? template = null)
		{
			if (template == null)
				return new TestSettings(1, JsonSettingsSerializer.Default);
			return new TestSettings(template, JsonSettingsSerializer.Default);
		}


		/// <summary>
		/// <c>Save(Stream, keepUnknownKeys: true)</c> preserves stream entries the saving instance never touched.
		/// </summary>
		[Test]
		public void KeepUnknownKeysOnStreamPreservesEntries()
		{
			using var stream = new MemoryStream();

			// instance A writes two keys to stream
			var settingsA = this.CreatePersistentSettings();
			settingsA.SetValue<bool>(TestSettings.BooleanKey, false);
			settingsA.SetValue<int>(TestSettings.Int32Key, 42);
			settingsA.Save(stream);

			// rewind, B adds a third key with keep
			stream.Position = 0;
			var settingsB = this.CreatePersistentSettings();
			settingsB.SetValue<string>(TestSettings.StringKey, "added");
			settingsB.Save(stream, true);

			// rewind and verify
			stream.Position = 0;
			var verify = this.CreatePersistentSettings();
			verify.Load(stream);
			Assert.AreEqual(false, verify.GetValueOrDefault(TestSettings.BooleanKey), "BooleanKey from A should survive.");
			Assert.AreEqual(42, verify.GetValueOrDefault(TestSettings.Int32Key), "Int32Key from A should survive.");
			Assert.AreEqual("added", verify.GetValueOrDefault(TestSettings.StringKey), "StringKey from B should be present.");
		}


		/// <summary>
		/// Saving with <c>keepUnknownKeys: true</c> overrides the file's value for keys the saving instance explicitly set.
		/// </summary>
		[Test]
		public void KeepUnknownKeysOverridesAppSetKeys()
		{
			var filePath = NewTempFilePath();
			try
			{
				// file has Int32Key=10, StringKey="old"
				var settingsA = this.CreatePersistentSettings();
				settingsA.SetValue<int>(TestSettings.Int32Key, 10);
				settingsA.SetValue<string>(TestSettings.StringKey, "old");
				settingsA.Save(filePath);

				// B sets Int32Key=999 and saves with keep
				var settingsB = this.CreatePersistentSettings();
				settingsB.SetValue<int>(TestSettings.Int32Key, 999);
				settingsB.Save(filePath, true);

				// verify: Int32Key overridden, StringKey preserved
				var verify = this.CreatePersistentSettings();
				verify.Load(filePath);
				Assert.AreEqual(999, verify.GetValueOrDefault(TestSettings.Int32Key), "Int32Key should be overridden by B.");
				Assert.AreEqual("old", verify.GetValueOrDefault(TestSettings.StringKey), "StringKey should be preserved from file.");
			}
			finally
			{
				TryDeleteWithBackup(filePath);
			}
		}


		/// <summary>
		/// When another writer adds a key to the file between our load and save, <c>keepUnknownKeys: true</c> preserves it.
		/// </summary>
		[Test]
		public void KeepUnknownKeysPreservesExternallyAddedKey()
		{
			var filePath = NewTempFilePath();
			try
			{
				// initial: file has Int32Key=10
				var setup = this.CreatePersistentSettings();
				setup.SetValue<int>(TestSettings.Int32Key, 10);
				setup.Save(filePath);

				// our instance loads
				var ours = this.CreatePersistentSettings();
				ours.Load(filePath);

				// another writer overwrites file with Int32Key=10 + StringKey="external"
				var external = this.CreatePersistentSettings();
				external.SetValue<int>(TestSettings.Int32Key, 10);
				external.SetValue<string>(TestSettings.StringKey, "external");
				external.Save(filePath);

				// we mutate and save with keep — must not clobber the external key
				ours.SetValue<int>(TestSettings.Int32Key, 20);
				ours.Save(filePath, true);

				// verify both changes are present
				var verify = this.CreatePersistentSettings();
				verify.Load(filePath);
				Assert.AreEqual(20, verify.GetValueOrDefault(TestSettings.Int32Key), "Our update to Int32Key should win.");
				Assert.AreEqual("external", verify.GetValueOrDefault(TestSettings.StringKey), "External writer's StringKey should be preserved.");
			}
			finally
			{
				TryDeleteWithBackup(filePath);
			}
		}


		/// <summary>
		/// Saving with <c>keepUnknownKeys: true</c> preserves keys the saving instance never touched.
		/// </summary>
		[Test]
		public void KeepUnknownKeysPreservesFileEntries()
		{
			var filePath = NewTempFilePath();
			try
			{
				// instance A writes two keys
				var settingsA = this.CreatePersistentSettings();
				settingsA.SetValue<bool>(TestSettings.BooleanKey, false);
				settingsA.SetValue<int>(TestSettings.Int32Key, 42);
				settingsA.Save(filePath);

				// instance B (no Load) writes a third key with keep
				var settingsB = this.CreatePersistentSettings();
				settingsB.SetValue<string>(TestSettings.StringKey, "hi");
				settingsB.Save(filePath, true);

				// verify all three are present
				var verify = this.CreatePersistentSettings();
				verify.Load(filePath);
				Assert.AreEqual(false, verify.GetValueOrDefault(TestSettings.BooleanKey), "BooleanKey from A should survive.");
				Assert.AreEqual(42, verify.GetValueOrDefault(TestSettings.Int32Key), "Int32Key from A should survive.");
				Assert.AreEqual("hi", verify.GetValueOrDefault(TestSettings.StringKey), "StringKey from B should be present.");
			}
			finally
			{
				TryDeleteWithBackup(filePath);
			}
		}


		/// <summary>
		/// Explicitly setting a key to its default value behaves like a reset under <c>keepUnknownKeys: true</c>: entry removed from file.
		/// </summary>
		[Test]
		public void KeepUnknownKeysRemovesExplicitDefaultSet()
		{
			var filePath = NewTempFilePath();
			try
			{
				// file has Int32Key=42, StringKey="alive"
				var settingsA = this.CreatePersistentSettings();
				settingsA.SetValue<int>(TestSettings.Int32Key, 42);
				settingsA.SetValue<string>(TestSettings.StringKey, "alive");
				settingsA.Save(filePath);

				// B loads, sets Int32Key to default, saves with keep
				var settingsB = this.CreatePersistentSettings();
				settingsB.Load(filePath);
				settingsB.SetValue<int>(TestSettings.Int32Key, (int)TestSettings.Int32Key.DefaultValue);
				settingsB.Save(filePath, true);

				// verify: Int32Key gone from file, StringKey preserved
				var verify = this.CreatePersistentSettings();
				verify.Load(filePath);
				Assert.IsNull(verify.Keys.FirstOrDefault(k => k.Name == TestSettings.Int32Key.Name), "Int32Key entry should be absent from the file.");
				Assert.AreEqual("alive", verify.GetValueOrDefault(TestSettings.StringKey), "StringKey should be preserved from file.");
			}
			finally
			{
				TryDeleteWithBackup(filePath);
			}
		}


		/// <summary>
		/// Saving with <c>keepUnknownKeys: true</c> removes a key the saving instance reset via <see cref="ISettings.ResetValue"/>.
		/// </summary>
		[Test]
		public void KeepUnknownKeysRemovesResetKey()
		{
			var filePath = NewTempFilePath();
			try
			{
				// file has Int32Key=42, StringKey="alive"
				var settingsA = this.CreatePersistentSettings();
				settingsA.SetValue<int>(TestSettings.Int32Key, 42);
				settingsA.SetValue<string>(TestSettings.StringKey, "alive");
				settingsA.Save(filePath);

				// B loads, resets Int32Key, saves with keep
				var settingsB = this.CreatePersistentSettings();
				settingsB.Load(filePath);
				settingsB.ResetValue(TestSettings.Int32Key);
				settingsB.Save(filePath, true);

				// verify: Int32Key gone from file, StringKey preserved
				var verify = this.CreatePersistentSettings();
				verify.Load(filePath);
				Assert.AreEqual(TestSettings.Int32Key.DefaultValue, verify.GetValueOrDefault(TestSettings.Int32Key), "Int32Key should fall back to default after reset.");
				Assert.IsNull(verify.Keys.FirstOrDefault(k => k.Name == TestSettings.Int32Key.Name), "Int32Key entry should be absent from the file.");
				Assert.AreEqual("alive", verify.GetValueOrDefault(TestSettings.StringKey), "StringKey should be preserved from file.");
			}
			finally
			{
				TryDeleteWithBackup(filePath);
			}
		}


		/// <summary>
		/// Saving with <c>keepUnknownKeys: true</c> when the file does not exist creates the file like a normal save.
		/// </summary>
		[Test]
		public void KeepUnknownKeysWithNoExistingFile()
		{
			var filePath = NewTempFilePath();
			try
			{
				Assert.IsFalse(File.Exists(filePath), "Precondition: file should not exist.");

				var settings = this.CreatePersistentSettings();
				settings.SetValue<int>(TestSettings.Int32Key, 7);
				Assert.DoesNotThrow(() => settings.Save(filePath, true), "Save with keep should not throw when file is absent.");
				Assert.IsTrue(File.Exists(filePath), "File should be created.");

				var verify = this.CreatePersistentSettings();
				verify.Load(filePath);
				Assert.AreEqual(7, verify.GetValueOrDefault(TestSettings.Int32Key), "Saved value should round-trip.");
			}
			finally
			{
				TryDeleteWithBackup(filePath);
			}
		}


		// Generate a unique temp file path without creating the file.
		static string NewTempFilePath() =>
			Path.Combine(Path.GetTempPath(), $"persistent-settings-test-{Guid.NewGuid():N}.json");


		/// <summary>
		/// <c>Save(Stream, keepUnknownKeys: true)</c> rejects a non-readable stream.
		/// </summary>
		[Test]
		public void SaveStreamKeepUnknownKeysRejectsNonReadableStream()
		{
			var settings = this.CreatePersistentSettings();
			settings.SetValue<int>(TestSettings.Int32Key, 5);
			using var stream = new NonReadableStream();
			Assert.Throws<ArgumentException>(() => settings.Save(stream, true), "Non-readable stream should be rejected with keep.");
		}


		/// <summary>
		/// <c>Save(Stream, keepUnknownKeys: true)</c> rejects a non-seekable stream.
		/// </summary>
		[Test]
		public void SaveStreamKeepUnknownKeysRejectsNonSeekableStream()
		{
			var settings = this.CreatePersistentSettings();
			settings.SetValue<int>(TestSettings.Int32Key, 5);
			using var stream = new NonSeekableStream();
			Assert.Throws<ArgumentException>(() => settings.Save(stream, true), "Non-seekable stream should be rejected with keep.");
		}


		/// <summary>
		/// <c>Save(Stream, keepUnknownKeys: true)</c> truncates trailing bytes when the new combined content is shorter than the original.
		/// </summary>
		[Test]
		public void SaveStreamTruncatesWhenNewContentShorter()
		{
			using var stream = new MemoryStream();

			// pre-fill stream with all test values (large content)
			var settingsA = this.CreatePersistentSettings();
			foreach (var kv in TestSettings.TestValues)
#pragma warning disable CS0618
				settingsA.SetValue(kv.Key, kv.Value);
#pragma warning restore CS0618
			settingsA.Save(stream);
			var initialLength = stream.Length;

			// B loads, resets everything, saves with keep — combined result should be near-empty
			stream.Position = 0;
			var settingsB = this.CreatePersistentSettings();
			settingsB.Load(stream);
			foreach (var kv in TestSettings.TestValues)
				settingsB.ResetValue(kv.Key);
			stream.Position = 0;
			settingsB.Save(stream, true);

			Assert.Less(stream.Length, initialLength, "New content should be shorter than original.");
			Assert.AreEqual(stream.Position, stream.Length, "Stream should be truncated to new content length (no trailing garbage).");

			// stream should still be a valid serialized payload
			stream.Position = 0;
			var verify = this.CreatePersistentSettings();
			Assert.DoesNotThrow(() => verify.Load(stream), "Truncated stream should still be a valid serialization.");
#pragma warning disable CS0618
			foreach (var key in TestSettings.AllKeys)
				Assert.AreEqual(key.DefaultValue, verify.GetValueOrDefault(key), $"{key.Name} should be at default after reset round-trip.");
#pragma warning restore CS0618
		}


		/// <summary>
		/// Tombstones (keys marked reset via <see cref="ISettings.ResetValue"/>) are carried over by the template-copy constructor.
		/// </summary>
		[Test]
		public void TemplateConstructorCopiesTombstones()
		{
			var filePath = NewTempFilePath();
			try
			{
				// file has Int32Key=99
				var setup = this.CreatePersistentSettings();
				setup.SetValue<int>(TestSettings.Int32Key, 99);
				setup.Save(filePath);

				// A loads + resets Int32Key (now has tombstone)
				var settingsA = this.CreatePersistentSettings();
				settingsA.Load(filePath);
				settingsA.ResetValue(TestSettings.Int32Key);

				// B constructed from A — should inherit the tombstone
				var settingsB = new TestSettings(settingsA, JsonSettingsSerializer.Default);
				settingsB.Save(filePath, true);

				// verify: file no longer contains Int32Key
				var verify = this.CreatePersistentSettings();
				verify.Load(filePath);
				Assert.IsNull(verify.Keys.FirstOrDefault(k => k.Name == TestSettings.Int32Key.Name), "Tombstone from template should have removed Int32Key from the file.");
			}
			finally
			{
				TryDeleteWithBackup(filePath);
			}
		}


		// Delete file and best-effort delete its .backup sibling.
		static void TryDeleteWithBackup(string filePath)
		{
			try { if (File.Exists(filePath)) File.Delete(filePath); } catch { /* best effort */ }
			try { if (File.Exists(filePath + ".backup")) File.Delete(filePath + ".backup"); } catch { /* best effort */ }
		}
	}
}
