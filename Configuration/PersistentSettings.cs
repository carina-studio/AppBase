using CarinaStudio.Collections;
using CarinaStudio.IO;
using CarinaStudio.Threading;
using CarinaStudio.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CarinaStudio.Configuration;

/// <summary>
/// Base implementation of <see cref="ISettings"/> which can be loaded/saved from/to file. This is thread-safe class.
/// </summary>
[ThreadSafe]
public abstract class PersistentSettings : ISettings
{
    // Control of file lock.
    class FileLock : ReaderWriterLockSlim
    {
        public int ReferenceCount;
    }
    
    
    // Static fields.
    readonly IDictionary<string, FileLock> fileLockMap = new Dictionary<string, FileLock>(PathEqualityComparer.Default);
    readonly TaskFactory ioTaskFactory = new FixedThreadsTaskFactory(1);
    
    
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


    // Copy all values.
    IDictionary<SettingKey, object> CopyValues()
    {
        using var _ = this.valuesLock.EnterScope();
        return new Dictionary<SettingKey, object>(this.values);
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
    [CalledOnBackgroundThread]
    public void Load(string fileName)
    {
        var retryCount = 0;
        var fileLock = this.fileLockMap.Lock(map =>
        {
            if (!map.TryGetValue(fileName, out var fileLock))
            {
                fileLock = new();
                map[fileName] = fileLock;
            }
            ++fileLock.ReferenceCount;
            return fileLock;
        });
        try
        {
            while (true)
            {
                using var _ = fileLock.EnterReadScope();
                try
                {
                    if (!System.IO.File.Exists(fileName))
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
        finally
        {
            this.fileLockMap.Lock(map =>
            {
                --fileLock.ReferenceCount;
                if (fileLock.ReferenceCount <= 0)
                {
                    fileLock.Dispose();
                    map.Remove(fileName);
                }
            });
        }
    }


    /// <summary>
    /// Load settings from given stream synchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to load settings from.</param>
    [ThreadSafe]
    [CalledOnBackgroundThread]
    public void Load(Stream stream)
    {
        // load to memory first
        var memoryStream = stream switch
        {
            MemoryStream _ => stream,
            UnmanagedMemoryStream _ => stream,
            _ => new MemoryStream().Also(memoryStream =>
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
    public Task LoadAsync(string fileName) => 
        ioTaskFactory.StartNew(() => this.Load(fileName));


    /// <summary>
    /// Load settings from given stream asynchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to load settings from.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public Task LoadAsync(Stream stream) => 
        Task.Run(() => this.Load(stream));


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
    [CalledOnBackgroundThread]
    public void Save(string fileName) =>
        this.Save(fileName, this.CopyValues());
    
    
    // Save settings to file synchronously.
    [ThreadSafe]
    [CalledOnBackgroundThread]
    void Save(string fileName, IDictionary<SettingKey, object> values)
    {
        // save to memory first
        using var memoryStream = new MemoryStream();
        this.serializer.Serialize(memoryStream, values, new SettingsMetadata(this.Version, this.lastModifiedTime));

        // write to file
        var fileLock = this.fileLockMap.Lock(map =>
        {
            if (!map.TryGetValue(fileName, out var fileLock))
            {
                fileLock = new();
                map[fileName] = fileLock;
            }
            ++fileLock.ReferenceCount;
            return fileLock;
        });
        try
        {
            // backup file
            try
            {
                if (System.IO.File.Exists(fileName))
                    System.IO.File.Copy(fileName, fileName + ".backup", true);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            { }
            
            // write to file
            var retryCount = 0;
            var data = memoryStream.ToArray();
            while (true)
            {
                using var _ = fileLock.EnterWriteScope();
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
        finally
        {
            this.fileLockMap.Lock(map =>
            {
                --fileLock.ReferenceCount;
                if (fileLock.ReferenceCount <= 0)
                {
                    fileLock.Dispose();
                    map.Remove(fileName);
                }
            });
        }
    }


    /// <summary>
    /// Save settings to given stream synchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to save settings.</param>
    [ThreadSafe]
    [CalledOnBackgroundThread]
    public void Save(Stream stream) =>
        this.Save(stream, this.CopyValues());
    
    
    // Save settings to given stream synchronously.
    [ThreadSafe]
    [CalledOnBackgroundThread]
    void Save(Stream stream, IDictionary<SettingKey, object> values) =>
        this.serializer.Serialize(stream, values, new SettingsMetadata(this.Version, this.lastModifiedTime));


    /// <summary>
    /// Save settings to file asynchronously.
    /// </summary>
    /// <param name="fileName">Name of file to save settings.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel saving.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public Task SaveAsync(string fileName, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        var values = this.CopyValues();
        return ioTaskFactory.StartNew(() => this.Save(fileName, values), cancellationToken);
    }


    /// <summary>
    /// Save settings to given stream asynchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to save settings.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel saving.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        var values = this.CopyValues();
        return Task.Run(() => this.Save(stream, values), cancellationToken);
    }


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