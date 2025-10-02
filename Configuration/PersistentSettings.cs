using CarinaStudio.Collections;
using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Configuration;

/// <summary>
/// Base implementation of <see cref="ISettings"/> which can be loaded/saved from/to file. This is thread-safe class.
/// </summary>
[ThreadSafe]
public abstract class PersistentSettings : ISettings
{
    // Fields.
    readonly Lock eventLock = new();
    readonly Lock valuesLock = new();
    DateTime lastModifiedTime = DateTime.Now;
    readonly ISettingsSerializer serializer;
    EventHandler<SettingChangedEventArgs>? settingChanged;
    EventHandler<SettingChangingEventArgs>? settingChanging;
    readonly Dictionary<SettingKey, object> values = new();


    /// <summary>
    /// Initialize new <see cref="PersistentSettings"/> instance.
    /// </summary>
    /// <param name="serializer">Settings serializer.</param>
    protected PersistentSettings(ISettingsSerializer serializer)
    {
        this.serializer = serializer;
    }


    /// <summary>
    /// Initialize new <see cref="PersistentSettings"/> instance.
    /// </summary>
    /// <param name="template">Template settings to initialize values.</param>
    /// <param name="serializer">Settings serializer.</param>
    protected PersistentSettings(ISettings template, ISettingsSerializer serializer)
    {
        this.serializer = serializer;
        if (template is PersistentSettings persistentSettings)
            this.values.AddAll(persistentSettings.values);
        else
        {
            foreach (var key in template.Keys)
            {
                var value = template.GetRawValue(key);
                if (value != null)
                    this.values[key] = value;
            }
        }
    }


    /// <summary>
    /// Get raw value stored in settings no matter what type of value specified by key.
    /// </summary>
    /// <param name="key">Key of setting.</param>
    /// <returns>Raw setting value.</returns>
    [ThreadSafe]
    public object? GetRawValue(SettingKey key)
    {
        lock (valuesLock)
        {
            values.TryGetValue(key, out var rawValue);
            return rawValue;
        }
    }


    /// <summary>
    /// Get all setting keys.
    /// </summary>
    [ThreadSafe]
    public IEnumerable<SettingKey> Keys
    {
        get
        {
            lock (valuesLock)
            {
                return values.Keys.ToArray();
            }
        }
    }


    /// <summary>
    /// Load settings from file synchronously.
    /// </summary>
    /// <param name="fileName">Name of file to load settings from.</param>
    [ThreadSafe]
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
    [ThreadSafe]
    public void Load(Stream stream)
    {
        // load to memory first
        var memoryStream = stream switch
        {
            MemoryStream _ => stream,
            UnmanagedMemoryStream _ => stream,
            _ => new MemoryStream().Also((memoryStream) =>
            {
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;
            }),
        };

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
    [ThreadSafe]
    public async Task LoadAsync(string fileName) => await Task.Run(() => this.Load(fileName));


    /// <summary>
    /// Load settings from given stream asynchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to load settings from.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public async Task LoadAsync(Stream stream) => await Task.Run(() => this.Load(stream));


    /// <summary>
    /// Called to upgrade settings.
    /// </summary>
    /// <param name="oldVersion">Old version to upgrade from.</param>
    [ThreadSafe]
    protected abstract void OnUpgrade(int oldVersion);


#pragma warning disable CS0618
    /// <summary>
    /// Reset setting to default value.
    /// </summary>
    /// <param name="key">Key of setting.</param>
    [ThreadSafe]
    public void ResetValue(SettingKey key) => this.SetValue(key, key.DefaultValue);
#pragma warning restore CS0618


    /// <summary>
    /// Raised after changing setting.
    /// </summary>
    [ThreadSafe]
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
    [ThreadSafe]
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
    [ThreadSafe]
    public void Save(string fileName)
    {
        // backup file
        try
        {
            if (File.Exists(fileName))
                File.Copy(fileName, fileName + ".backup", true);
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch
        { }

        // save to memory first
        Dictionary<SettingKey, object> values;
        lock (valuesLock)
        {
            values = new Dictionary<SettingKey, object>(this.values);
        }
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
    [ThreadSafe]
    public void Save(Stream stream)
    {
        Dictionary<SettingKey, object> values;
        lock (valuesLock)
        {
            values = new Dictionary<SettingKey, object>(this.values);
        }
        this.serializer.Serialize(stream, values, new SettingsMetadata(this.Version, this.lastModifiedTime));
    }


    /// <summary>
    /// Save settings to file asynchronously.
    /// </summary>
    /// <param name="fileName">Name of file to save settings.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public async Task SaveAsync(string fileName) => await Task.Run(() => this.Save(fileName));


    /// <summary>
    /// Save settings to given stream asynchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to save settings.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public async Task SaveAsync(Stream stream) => await Task.Run(() => this.Save(stream));


    /// <summary>
    /// Set value of setting.
    /// </summary>
    /// <param name="key">Key og setting.</param>
    /// <param name="value">New value.</param>
    [Obsolete("Try using generic SetValue() instead, unless you don't know the type of value.")]
    [ThreadSafe]
    public void SetValue(SettingKey key, object value)
    {
        // check value
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (!key.ValueType.IsInstanceOfType(value))
            throw new ArgumentException($"Value {value} is not {key.ValueType.Name}.");

        // check previous value
        var prevValue = this.GetRawValue(key) ?? key.DefaultValue;
        var valueChanged = !prevValue.Equals(value);

        // raise event
        if (valueChanged)
        {
            lock (eventLock)
                this.settingChanging?.Invoke(this, new SettingChangingEventArgs(key, prevValue, value));
        }

        // update value
        lock (valuesLock)
        {
            if (valueChanged && !prevValue.Equals(this.GetRawValue(key) ?? key.DefaultValue))
                return;
            this.values.Remove(key); // need to remove current key first to ensure that key will be updated
            if (!value.Equals(key.DefaultValue))
                this.values[key] = value;
            if (valueChanged)
                this.lastModifiedTime = DateTime.Now;
        }

        // raise event
        if (valueChanged)
        {
            lock (eventLock)
                this.settingChanged?.Invoke(this, new SettingChangedEventArgs(key, prevValue, value));
        }
    }


    /// <summary>
    /// Get version of settings.
    /// </summary>
    [ThreadSafe]
    public abstract int Version { get; }
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
        if (!valueType.IsInstanceOfType(defaultValue))
            throw new ArgumentException($"Default value {defaultValue} is not {valueType.Name}.");
        this.DefaultValue = defaultValue;
        this.Name = name;
        this.ValueType = valueType;
    }


    /// <summary>
    /// Default value.
    /// </summary>
    public object DefaultValue { get; }


    /// <summary>
    /// Get all public <see cref="SettingKey"/> defined by given type.
    /// </summary>
    /// <returns>List of public <see cref="SettingKey"/>.</returns>
    public static IList<SettingKey> GetDefinedKeys<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] T>()
    {
        var keys = new List<SettingKey>();
        foreach (var fieldInfo in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (fieldInfo.GetValue(null) is SettingKey key)
                keys.Add(key);
        }
        return keys.AsReadOnly();
    }


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


    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="x">Operand 1.</param>
    /// <param name="y">Operand 2.</param>
    /// <returns>True if operands are equivalent.</returns>
    public static bool operator ==(SettingKey x, SettingKey y) => x.Equals(y);


    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="x">Operand 1.</param>
    /// <param name="y">Operand 2.</param>
    /// <returns>True if operands are not equivalent.</returns>
    public static bool operator !=(SettingKey x, SettingKey y) => !x.Equals(y);


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
/// Metadata of <see cref="PersistentSettings"/>.
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