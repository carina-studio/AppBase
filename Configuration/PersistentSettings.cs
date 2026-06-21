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
    
    
    // Constants.
    const int MaxFileReadingRetryCount = 10;
    const int MaxFileWritingRetryCount = 10;
    const int FileReadingRetryDelay = 500;
    const int FileWritingRetryDelay = 500;
    
    
    // Static fields.
    readonly IDictionary<string, FileLock> fileLockMap = new Dictionary<string, FileLock>(PathEqualityComparer.Default);
    readonly TaskFactory ioTaskFactory = new FixedThreadsTaskFactory(1);
    
    
    // Fields.
    readonly Lock eventLock = new();
    readonly Lock valuesLock = new();
    DateTime lastModifiedTime = DateTime.Now;
    readonly HashSet<SettingKey> resetKeys = new();
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


    // Combine given values with existing values in the stream.
    [ThreadSafe]
    [CalledOnBackgroundThread]
    IDictionary<SettingKey, object> CombineValues(Stream stream, IDictionary<SettingKey, object> values, ISet<SettingKey> resetKeys)
    {
        if (!stream.CanRead)
            throw new ArgumentException("Cannot keep unknown keys with stream that does not support reading.");
        if (!stream.CanSeek)
            throw new ArgumentException("Cannot keep unknown keys with stream that does not support seeking.");
        var position = stream.Position;
        this.serializer.Deserialize(stream, out var existingValues, out _);
        stream.Seek(position, SeekOrigin.Begin);
        foreach (var key in resetKeys)
            existingValues.Remove(key);
        foreach (var value in values)
        {
            existingValues.Remove(value.Key); // to make sure that the key will be overwritten
            existingValues.Add(value.Key, value.Value);
        }
        return existingValues;
    }
    
    
    // Copy all values and reset keys.
    void CopyValuesAndResetKeys(out IDictionary<SettingKey, object> values, out ISet<SettingKey> resetKeys)
    {
        using var _ = this.valuesLock.EnterScope();
        values = new Dictionary<SettingKey, object>(this.values);
        resetKeys = new HashSet<SettingKey>(this.resetKeys);
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
        {
            this.resetKeys.AddAll(persistentSettings.resetKeys);
            this.values.AddAll(persistentSettings.values);
        }
        else
        {
            foreach (var key in template.Keys)
            {
                var value = template.GetRawValue(key);
                if (value is not null)
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
                    if (retryCount > MaxFileReadingRetryCount)
                        throw;
                    Thread.Sleep(FileReadingRetryDelay);
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
        var keysNeedToReset = this.values.Keys.Except(values.Keys).ToArray();
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
        this.Save(fileName, false);


    /// <summary>
    /// Save settings to file synchronously.
    /// </summary>
    /// <param name="fileName">Name of file to save settings.</param>
    /// <param name="keepUnknownKeys">True to keep unknown keys in the file. The reset values will still override existing values in the file.</param>
    [ThreadSafe]
    [CalledOnBackgroundThread]
    public void Save(string fileName, bool keepUnknownKeys)
    {
        this.CopyValuesAndResetKeys(out var values, out var resetKeys);
        this.Save(fileName, values, resetKeys, keepUnknownKeys);
    }


    // Save settings to file synchronously.
    [ThreadSafe]
    [CalledOnBackgroundThread]
    void Save(string fileName, IDictionary<SettingKey, object> values, ISet<SettingKey> resetKeys, bool keepUnknownKeys)
    {
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
        var isFileLockHeld = false;
        try
        {
            // backup file
            var isFileExisting = System.IO.File.Exists(fileName);
            try
            {
                if (isFileExisting)
                    System.IO.File.Copy(fileName, fileName + ".backup", true);
            }
            catch
            { /* best effort */ }

            // read existing values to combine (release lock between retries, KEEP on success)
            var valuesToWrite = values;
            if (keepUnknownKeys && isFileExisting)
            {
                var readRetryCount = 0;
                while (true)
                {
                    fileLock.EnterWriteLock();
                    isFileLockHeld = true;
                    try
                    {
                        using var readStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                        valuesToWrite = this.CombineValues(readStream, values, resetKeys);
                        break; // keep file lock held and pass it to writing block
                    }
                    catch
                    {
                        fileLock.ExitWriteLock();
                        isFileLockHeld = false;
                        ++readRetryCount;
                        if (readRetryCount > MaxFileReadingRetryCount)
                            throw;
                        Thread.Sleep(FileReadingRetryDelay);
                    }
                }
            }

            // serialize to memory (file is untouched if this throws)
            byte[] dataToWrite;
            try
            {
                using var memoryStream = new MemoryStream();
                this.serializer.Serialize(memoryStream, valuesToWrite, new SettingsMetadata(this.Version, this.lastModifiedTime));
                dataToWrite = memoryStream.ToArray();
            }
            catch
            {
                if (isFileLockHeld)
                {
                    fileLock.ExitWriteLock();
                    isFileLockHeld = false;
                }
                throw;
            }

            // write to file (reuse held lock on first attempt, re-acquire on retries)
            var writeRetryCount = 0;
            while (true)
            {
                if (!isFileLockHeld)
                {
                    fileLock.EnterWriteLock();
                    isFileLockHeld = true;
                }
                try
                {
                    using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    fileStream.Write(dataToWrite);
                    break;
                }
                catch
                {
                    if (isFileLockHeld)
                    {
                        fileLock.ExitWriteLock();
                        isFileLockHeld = false;
                    }
                    ++writeRetryCount;
                    if (writeRetryCount > MaxFileWritingRetryCount)
                        throw;
                    Thread.Sleep(FileWritingRetryDelay);
                }
            }
        }
        finally
        {
            if (isFileLockHeld)
                fileLock.ExitWriteLock();
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
        this.Save(stream, false);


    /// <summary>
    /// Save settings to given stream synchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to save settings.</param>
    /// <param name="keepUnknownKeys">True to keep unknown keys in the stream. The reset values will still override existing values in the stream.</param>
    [ThreadSafe]
    [CalledOnBackgroundThread]
    public void Save(Stream stream, bool keepUnknownKeys)
    {
        this.CopyValuesAndResetKeys(out var values, out var resetKeys);
        this.Save(stream, values, resetKeys, keepUnknownKeys);
    }
    
    
    // Save settings to given stream synchronously.
    [ThreadSafe]
    [CalledOnBackgroundThread]
    void Save(Stream stream, IDictionary<SettingKey, object> values, ISet<SettingKey> resetKeys, bool keepUnknownKeys)
    {
        if (keepUnknownKeys)
        {
            values = this.CombineValues(stream, values, resetKeys);
            stream.SetLength(stream.Position);
        }
        this.serializer.Serialize(stream, values, new SettingsMetadata(this.Version, this.lastModifiedTime));   
    }


    /// <summary>
    /// Save settings to file asynchronously.
    /// </summary>
    /// <param name="fileName">Name of file to save settings.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel saving.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public Task SaveAsync(string fileName, CancellationToken cancellationToken = default) =>
        this.SaveAsync(fileName, false, cancellationToken);


    /// <summary>
    /// Save settings to file asynchronously.
    /// </summary>
    /// <param name="fileName">Name of file to save settings.</param>
    /// <param name="keepUnknownKeys">True to keep unknown keys in the file. The reset values will still override existing values in the file.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel saving.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public Task SaveAsync(string fileName, bool keepUnknownKeys, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        this.CopyValuesAndResetKeys(out var values, out var resetKeys);
        return ioTaskFactory.StartNew(() => this.Save(fileName, values, resetKeys, keepUnknownKeys), cancellationToken);
    }


    /// <summary>
    /// Save settings to given stream asynchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to save settings.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel saving.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public Task SaveAsync(Stream stream, CancellationToken cancellationToken = default) =>
        this.SaveAsync(stream, false, cancellationToken);


    /// <summary>
    /// Save settings to given stream asynchronously.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> to save settings.</param>
    /// <param name="keepUnknownKeys">True to keep unknown keys in the stream. The reset values will still override existing values in the stream.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> to cancel saving.</param>
    /// <returns><see cref="Task"/> of asynchronous operation.</returns>
    [ThreadSafe]
    public Task SaveAsync(Stream stream, bool keepUnknownKeys, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);
        this.CopyValuesAndResetKeys(out var values, out var resetKeys);
        return Task.Run(() => this.Save(stream, values, resetKeys, keepUnknownKeys), cancellationToken);
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
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        if (!key.ValueType.IsInstanceOfType(value))
            throw new ArgumentException($"Value {value} is not {key.ValueType.Name}.");

        // check previous value
        var defaultValue = key.DefaultValue;
        var prevValue = this.GetRawValue(key);
        var valueChanged = !value.Equals(prevValue ?? defaultValue);

        // raise event
        if (valueChanged)
        {
            lock (eventLock)
                this.settingChanging?.Invoke(this, new SettingChangingEventArgs(key, prevValue ?? defaultValue, value));
        }

        // update value
        lock (valuesLock)
        {
            if (valueChanged && !(prevValue ?? defaultValue).Equals(this.GetRawValue(key) ?? defaultValue))
                return;
            var isDefaultValue = value.Equals(defaultValue);
            this.resetKeys.Remove(key); // always remove first to make sure that the key will be updated
            this.values.Remove(key); // always remove first to make sure that the key will be updated
            if (isDefaultValue)
                this.resetKeys.Add(key);
            else
                this.values.Add(key, value);
            if (valueChanged)
                this.lastModifiedTime = DateTime.Now;
        }

        // raise event
        if (valueChanged)
        {
            lock (eventLock)
                this.settingChanged?.Invoke(this, new SettingChangedEventArgs(key, prevValue ?? defaultValue, value));
        }
    }


    /// <summary>
    /// Get version of settings.
    /// </summary>
    [ThreadSafe]
    public abstract int Version { get; }
}