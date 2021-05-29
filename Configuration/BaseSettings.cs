using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Base implementation of application settings. This is thread-safe class.
	/// </summary>
	public abstract class BaseSettings
	{
		// Fields.
		readonly object eventLock = new object();
		DateTime lastModifiedTime = DateTime.Now;
		readonly ISettingsSerializer serializer;
		volatile EventHandler<SettingChangedEventArgs>? settingChanged;
		volatile EventHandler<SettingChangingEventArgs>? settingChanging;
		readonly Dictionary<SettingKey, object> values;


		/// <summary>
		/// Initialize new <see cref="BaseSettings"/> instance.
		/// </summary>
		/// <param name="serializer">Settings serializer.</param>
		protected BaseSettings(ISettingsSerializer serializer)
		{
			this.serializer = serializer;
			this.values = new Dictionary<SettingKey, object>();
		}


		/// <summary>
		/// Initialize new <see cref="BaseSettings"/> instance.
		/// </summary>
		/// <param name="template">Template settings to initialize values.</param>
		/// <param name="serializer">Settings serializer.</param>
		protected BaseSettings(BaseSettings template, ISettingsSerializer serializer)
		{
			this.serializer = serializer;
			this.values = new Dictionary<SettingKey, object>(template.values);
		}


		/// <summary>
		/// Get raw value stored in settings no matter what type of value specified by key.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <returns>Raw setting value.</returns>
		public object? GetRawValue(SettingKey key) => this.values.Lock((values) =>
		{
			values.TryGetValue(key, out var rawValue);
			return rawValue;
		});


		/// <summary>
		/// Get setting value as type specified by key, or get default value.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <returns>Setting value, or default value.</returns>
		[Obsolete("Try using generic GetValueOrDefault() instead, unless you don't know the type of value.")]
		public object GetValueOrDefault(SettingKey key)
		{
			var rawValue = this.GetRawValue(key);
			if (rawValue != null && key.ValueType.IsAssignableFrom(rawValue.GetType()))
				return rawValue;
			return key.DefaultValue;
		}


#pragma warning disable CS0618
		/// <summary>
		/// Get setting value as type specified by key, or get default value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="key">Key of setting.</param>
		/// <returns>Setting value, or default value.</returns>
		public T GetValueOrDefault<T>(SettingKey<T> key) => (T)this.GetValueOrDefault((SettingKey)key);
#pragma warning restore CS0618


		/// <summary>
		/// Get all setting keys.
		/// </summary>
		public IEnumerable<SettingKey> Keys { get => this.values.Lock((it) => it.Keys.ToArray()); }


		/// <summary>
		/// Load settings from file synchronously.
		/// </summary>
		/// <param name="fileName">Name of file to load settings from.</param>
		public void Load(string fileName)
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					if (!File.Exists(fileName))
					{
						this.ResetValues();
						break;
					}
					using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
					this.Load(fileStream);
					break;
				}
				catch
				{
					++retryCount;
					if (retryCount > 10)
						throw;
					Thread.Sleep(500);
				}
			}
		}


		/// <summary>
		/// Load settings from given stream synchronously.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/> to load settings from.</param>
		public void Load(Stream stream)
		{
			// load to memory first
			if (!(stream is MemoryStream memoryStream) || memoryStream.Position != 0)
			{
				memoryStream = new MemoryStream().Also((memoryStream) =>
				{
					stream.CopyTo(memoryStream);
					memoryStream.Position = 0;
				});
			}

			// load
			this.serializer.Deserialize(memoryStream, out var values, out var metadata);

			// override current values
			var keysNeedToReset = this.values.Keys.Except(values.Keys);
#pragma warning disable CS0618
			foreach (var keyValue in values)
				this.SetValue(keyValue.Key, keyValue.Value);
#pragma warning restore CS0618
			foreach (var key in keysNeedToReset)
				this.ResetValue(key);

			// upgrade if needed
			if (metadata.Version < this.Version)
				this.OnUpgrade(metadata.Version);

			// keep timestamp
			this.lastModifiedTime = metadata.LastModifiedTime;
		}


		/// <summary>
		/// Load settings from file asynchronously.
		/// </summary>
		/// <param name="fileName">Name of file to load settings from.</param>
		/// <returns><see cref="Task"/> of asynchronous operation.</returns>
		public async Task LoadAsync(string fileName) => await Task.Run(() => this.Load(fileName));


		/// <summary>
		/// Load settings from given stream asynchronously.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/> to load settings from.</param>
		/// <returns><see cref="Task"/> of asynchronous operation.</returns>
		public async Task LoadAsync(Stream stream) => await Task.Run(() => this.Load(stream));


		/// <summary>
		/// Called to upgrade settings.
		/// </summary>
		/// <param name="oldVersion">Old version to upgrade from.</param>
		protected abstract void OnUpgrade(int oldVersion);


		/// <summary>
		/// Reset all values to default.
		/// </summary>
		public void ResetValues() => this.ResetValues(this.Keys);


		/// <summary>
		/// Reset specific set of values to default.
		/// </summary>
		public void ResetValues(IEnumerable<SettingKey> keys)
		{
			foreach (var key in this.Keys)
				this.ResetValue(key);
		}


#pragma warning disable CS0618
		/// <summary>
		/// Reset setting to default value.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		public void ResetValue(SettingKey key) => this.SetValue(key, key.DefaultValue);
#pragma warning restore CS0618


		/// <summary>
		/// Raised after changing setting.
		/// </summary>
		public event EventHandler<SettingChangedEventArgs>? SettingChanged
		{
			add
			{
				lock (this.eventLock)
					this.settingChanged += value;
			}
			remove
			{
				lock (this.eventLock)
					this.settingChanged -= value;
			}
		}


		/// <summary>
		/// Raised before changing setting.
		/// </summary>
		public event EventHandler<SettingChangingEventArgs>? SettingChanging
		{
			add
			{
				lock (this.eventLock)
					this.settingChanging += value;
			}
			remove
			{
				lock (this.eventLock)
					this.settingChanging -= value;
			}
		}


		/// <summary>
		/// Save settings to file synchronously.
		/// </summary>
		/// <param name="fileName">Name of file to save settings.</param>
		public void Save(string fileName)
		{
			// backup file
			try
			{
				if (File.Exists(fileName))
					File.Copy(fileName, fileName + ".backup", true);
			}
			catch
			{ }

			// save to memory first
			var values = this.values.Lock((values) => new Dictionary<SettingKey, object>(values));
			using var memoryStream = new MemoryStream();
			this.serializer.Serialize(memoryStream, values, new SettingsMetadata(this.Version, this.lastModifiedTime));

			// write to file
			var retryCount = 0;
			var data = memoryStream.ToArray();
			while (true)
			{
				try
				{
					using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
					fileStream.Write(data);
					break;
				}
				catch
				{
					++retryCount;
					if (retryCount > 10)
						throw;
					Thread.Sleep(500);
				}
			}
		}


		/// <summary>
		/// Save settings to given stream synchronously.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/> to save settings.</param>
		public void Save(Stream stream)
		{
			var values = this.values.Lock((values) => new Dictionary<SettingKey, object>(values));
			this.serializer.Serialize(stream, values, new SettingsMetadata(this.Version, this.lastModifiedTime));
		}


		/// <summary>
		/// Save settings to file asynchronously.
		/// </summary>
		/// <param name="fileName">Name of file to save settings.</param>
		/// <returns><see cref="Task"/> of asynchronous operation.</returns>
		public async Task SaveAsync(string fileName) => await Task.Run(() => this.Save(fileName));


		/// <summary>
		/// Save settings to given stream asynchronously.
		/// </summary>
		/// <param name="stream"><see cref="Stream"/> to save settings.</param>
		/// <returns><see cref="Task"/> of asynchronous operation.</returns>
		public async Task SaveAsync(Stream stream) => await Task.Run(() => this.Save(stream));


		/// <summary>
		/// Set value of setting.
		/// </summary>
		/// <param name="key">Key og setting.</param>
		/// <param name="value">New value.</param>
		[Obsolete("Try using generic SetValue() instead, unless you don't know the type of value.")]
		public void SetValue(SettingKey key, object value)
		{
			// check value
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (!key.ValueType.IsAssignableFrom(value.GetType()))
				throw new ArgumentException($"Value {value} is not {key.ValueType.Name}.");

			// check previous value
			var prevValue = this.GetRawValue(key) ?? key.DefaultValue;
			if (prevValue.Equals(value))
				return;

			// raise event
			lock (eventLock)
			{
				this.settingChanging?.Invoke(this, new SettingChangingEventArgs(key, prevValue, value));
			}

			// update value
			lock (this.values)
			{
				if (!prevValue.Equals(this.GetRawValue(key) ?? key.DefaultValue))
					return;
				if (value.Equals(key.DefaultValue))
					this.values.Remove(key);
				else
					this.values[key] = value;
				this.lastModifiedTime = DateTime.Now;
			}

			// raise event
			lock (eventLock)
			{
				this.settingChanged?.Invoke(this, new SettingChangedEventArgs(key, prevValue, value));
			}
		}


#pragma warning disable CS8604, CS0618
		/// <summary>
		/// Set value of setting.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="key">Key og setting.</param>
		/// <param name="value">New value.</param>
		public void SetValue<T>(SettingKey<T> key, T value) => this.SetValue((SettingKey)key, value);
#pragma warning restore CS8604, CS0618


		/// <summary>
		/// Check whether type of value of given setting is correct or not. You can get raw value by calling <see cref="GetRawValue(SettingKey)"/> if type is incorrect.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <returns>True if type of value of given setting is correct.</returns>
		public bool VerifyValue(SettingKey key)
		{
			var rawValue = this.GetRawValue(key);
			return rawValue == null || key.ValueType.IsAssignableFrom(rawValue.GetType());
		}


		/// <summary>
		/// Get version of settings.
		/// </summary>
		protected abstract int Version { get; }
	}


	/// <summary>
	/// Data for setting changed event.
	/// </summary>
	public class SettingChangedEventArgs : SettingEventArgs
	{
		/// <summary>
		/// Initialize new <see cref="SettingChangedEventArgs"/> instance.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <param name="prevValue">Previous value of setting.</param>
		/// <param name="value">Current value of setting.</param>
		public SettingChangedEventArgs(SettingKey key, object prevValue, object value) : base(key, value)
		{
			this.PreviousValue = prevValue;
		}


		/// <summary>
		/// Previous value of setting.
		/// </summary>
		public object PreviousValue { get; }
	}


	/// <summary>
	/// Data for setting changing event.
	/// </summary>
	public class SettingChangingEventArgs : SettingEventArgs
	{
		/// <summary>
		/// Initialize new <see cref="SettingChangingEventArgs"/> instance.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <param name="value">Current value of setting.</param>
		/// <param name="newValue">New value of setting.</param>
		public SettingChangingEventArgs(SettingKey key, object value, object newValue) : base(key, value)
		{
			this.NewValue = newValue;
		}


		/// <summary>
		/// New value of setting.
		/// </summary>
		public object NewValue { get; }
	}


	/// <summary>
	/// Data for setting related event.
	/// </summary>
	public class SettingEventArgs : EventArgs
	{
		/// <summary>
		/// Initialize new <see cref="SettingEventArgs"/> instance.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <param name="value">Current value of setting.</param>
		public SettingEventArgs(SettingKey key, object value)
		{
			this.Key = key;
			this.Value = value;
		}


		/// <summary>
		/// Key of related setting.
		/// </summary>
		public SettingKey Key { get; }


		/// <summary>
		/// Current value of related setting.
		/// </summary>
		public object Value { get; }
	}


	/// <summary>
	/// Key of setting.
	/// </summary>
	public class SettingKey
	{
		/// <summary>
		/// Initialize new <see cref="SettingKey"/> instance.
		/// </summary>
		/// <param name="name">Name of key.</param>
		/// <param name="valueType">Type of value of setting.</param>
		/// <param name="defaultValue">Default value.</param>
		public SettingKey(string name, Type valueType, object defaultValue)
		{
			if (defaultValue == null)
				throw new ArgumentNullException(nameof(defaultValue));
			if (!valueType.IsAssignableFrom(defaultValue.GetType()))
				throw new ArgumentException($"Default value {defaultValue} is not {valueType.Name}.");
			this.DefaultValue = defaultValue;
			this.Name = name;
			this.ValueType = valueType;
		}


		/// <summary>
		/// Default value.
		/// </summary>
		public object DefaultValue { get; }


#pragma warning disable CS1591
		// Check key equality.
		public override bool Equals(object? obj)
		{
			if (obj is SettingKey anotherKey)
				return this.Name == anotherKey.Name;
			return false;
		}
#pragma warning restore CS1591


#pragma warning disable CS1591
		// Calculate hash-code.
		public override int GetHashCode() => this.Name.GetHashCode();
#pragma warning restore CS1591


		/// <summary>
		/// Name of key.
		/// </summary>
		public string Name { get; }


#pragma warning disable CS1591
		// To readable string.
		public override string ToString() => this.Name;
#pragma warning restore CS1591


		/// <summary>
		/// Type of value of setting.
		/// </summary>
		public Type ValueType { get; }
	}


	/// <summary>
	/// Key of setting.
	/// </summary>
	public class SettingKey<T> : SettingKey
	{
#pragma warning disable CS8601
#pragma warning disable CS8604
		/// <summary>
		/// Initialize new <see cref="SettingKey"/> instance.
		/// </summary>
		/// <param name="name">Name of key.</param>
		/// <param name="defaultValue">Default value.</param>
		public SettingKey(string name, T defaultValue = default) : base(name, typeof(T), defaultValue)
#pragma warning restore CS8604
#pragma warning restore CS8601
		{
			this.DefaultValue = defaultValue;
		}


		/// <summary>
		/// Default value.
		/// </summary>
		public new T DefaultValue { get; }
	}


	/// <summary>
	/// Metadata of <see cref="BaseSettings"/>.
	/// </summary>
	public class SettingsMetadata
	{
		/// <summary>
		/// Initialize new <see cref="SettingsMetadata"/> instance.
		/// </summary>
		/// <param name="version">Version of settings.</param>
		/// <param name="lastModifiedTime">Timestamp of last change to settings.</param>
		public SettingsMetadata(int version, DateTime lastModifiedTime)
		{
			this.LastModifiedTime = lastModifiedTime;
			this.Version = version;
		}


		/// <summary>
		/// Timestamp of last change to settings.
		/// </summary>
		public DateTime LastModifiedTime { get; }


		/// <summary>
		/// Version of settings.
		/// </summary>
		public int Version { get; }
	}
}
