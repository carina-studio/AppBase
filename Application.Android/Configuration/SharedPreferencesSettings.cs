using Android.Content;
using Android.Preferences;
using Android.Util;
using CarinaStudio.Collections;
using CarinaStudio.Configuration;
using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CarinaStudio.Android.Configuration;

/// <summary>
/// Implementation of <see cref="ISettings"/> backed by <see cref="ISharedPreferences"/>.
/// </summary>
public class SharedPreferencesSettings : ISettings
{
    // Listener of change of shared preferences.
    class SharedPreferencesChangedListener : Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        // Fields.
        readonly SharedPreferencesSettings settings;

        // Constructor.
        public SharedPreferencesChangedListener(SharedPreferencesSettings settings) =>
            this.settings = settings;
        
        /// <inheritdoc/>
        public void OnSharedPreferenceChanged(ISharedPreferences? sharedPreferences, string? key)
        {
            if (key != null)
                this.settings.OnSharedPreferenceChanged(key);
        }
    }


    // Constants.
    const int SharedPrefsEditingDelay = 100;
    const string TAG = nameof(SharedPreferencesSettings);
    const string VersionKey = "__cs_settings_version__";


    // Static fields.
    static volatile SharedPreferencesSettings? DefaultSettings;
    static readonly ISet<string> EmptyStringSet = new HashSet<string>().AsReadOnly();
    static readonly SynchronizationContext SharedPrefsEditingSyncContext = new SingleThreadSynchronizationContext();


    // Fields.
    readonly HashSet<string> changedKeys = new();
    readonly ScheduledAction editSharedPrefsAction;
    readonly Dictionary<string, SettingKey> keys = new();
    readonly HashSet<string> removedKeys = new();
    EventHandler<SettingChangingEventArgs>? settingChangingHandlers;
    readonly ISharedPreferences sharedPrefs;
    IDictionary<string, object> sharedPrefsValues;
    readonly object syncLock = new();
    readonly Dictionary<string, object> values;
    int version;


    /// <summary>
    /// Initialize new <see cref="SharedPreferencesSettings"/> instance.
    /// </summary>
    /// <param name="sharedPreferences"><see cref="ISharedPreferences"/>.</param>
    public SharedPreferencesSettings(ISharedPreferences sharedPreferences)
    {
        this.editSharedPrefsAction = new(SharedPrefsEditingSyncContext, () =>
        {
            // get values to update
            (string, object)[] changedValues;
            string[] removedKeys;
            lock (this.syncLock)
            {
                changedValues = new ValueTuple<string, object>[this.changedKeys.Count].Also(it =>
                {
                    var i = 0;
                    foreach (var key in this.changedKeys)
                        it[i++] = (key, this.values![key]);
                });
                removedKeys = this.removedKeys.ToArray();
                this.changedKeys.Clear();
                this.removedKeys.Clear();
            }
            if (changedValues.IsEmpty() && removedKeys.IsEmpty())
            {
#if DEBUG
                Log.Info(TAG, "No changed or removed value to apply");
#endif
                return;
            }
            
            // apply changes
            var editor = this.sharedPrefs!.Edit()!;
            foreach (var (key, value) in changedValues)
            {
                if (value is bool boolValue)
                    editor.PutBoolean(key, boolValue);
                else if (value is string str)
                    editor.PutString(key, str);
                else if (value is Enum)
                    editor.PutString(key, value.ToString());
                else if (value is int intValue)
                    editor.PutInt(key, intValue);
                else if (value is uint uintValue)
                    editor.PutLong(key, uintValue);
                else if (value is long longValue)
                    editor.PutLong(key, longValue);
                else if (value is float floatValue)
                    editor.PutFloat(key, floatValue);
                else if (value is double doubleValue)
                    editor.PutFloat(key, (float)doubleValue);
                else if (value is ISet<string> stringSet)
                    editor.PutStringSet(key, stringSet);
                else
                    Log.Warn(TAG, $"Ignore applying '{key}' with type {value.GetType().Name}");
            }
            foreach (var key in removedKeys)
                editor.Remove(key);
#if DEBUG
            Log.Info(TAG, $"Apply {changedValues.Length} changed values");
            Log.Info(TAG, $"Apply {removedKeys.Length} removed values");
#endif
            editor.Apply();
        });
        this.sharedPrefs = sharedPreferences;
        this.sharedPrefsValues = sharedPreferences.All.AsNonNull();
        this.values = new(this.sharedPrefsValues);
        foreach (var (name, value) in this.sharedPrefsValues)
        {
            if (name != VersionKey)
            {
                var key = CreateKey(name, value);
                if (key is not null)
                    this.keys[name] = key;
                else
                {
                    Log.Warn(TAG, $"Ignore '{name}' with type of value {value.GetType().Name}");
                    this.values.Remove(name);
                }
            }
        }
        this.version = Global.RunOrDefault(() => sharedPreferences.GetInt(VersionKey, 0));
        sharedPreferences.RegisterOnSharedPreferenceChangeListener(new SharedPreferencesChangedListener(this));
#if DEBUG
        Log.Info(TAG, $"Initialize with {this.values.Count} value(s)");
#endif
    }


    // Create key from value.
    static SettingKey? CreateKey(string name, object value) => value switch
    {
        bool => new SettingKey<bool>(name),
        int => new SettingKey<int>(name),
        long => new SettingKey<long>(name),
        float => new SettingKey<float>(name),
        string => new SettingKey<string>(name, ""),
        ISet<string> => new SettingKey<ISet<string>>(name, EmptyStringSet),
        _ => Global.Run(() =>
        {
            Log.Warn(TAG, $"Unsupported type of setting: {value.GetType().Name}.");
            return (SettingKey?)null;
        }),
    };


    /// <summary>
    /// Get default settings of application.
    /// </summary>
    /// <param name="context">Context.</param>
    /// <returns>Default settings.</returns>
    public static SharedPreferencesSettings GetDefault(Context context)
    {
        if (DefaultSettings != null)
            return DefaultSettings;
        lock (typeof(SharedPreferencesSettings))
        {
            if (DefaultSettings != null)
                return DefaultSettings;
            Log.Debug(TAG, "Create default settings");
            DefaultSettings = new(PreferenceManager.GetDefaultSharedPreferences(context.ApplicationContext));
        }
        return DefaultSettings;
    }


    /// <inheritdoc/>
    public object? GetRawValue(SettingKey key) => this.syncLock.Lock(() =>
    {
        this.values.TryGetValue(key.Name, out var value);
        return value;
    });


    /// <inheritdoc/>
    public IEnumerable<SettingKey> Keys { get => this.keys.Values; }


    // Called when shared preference changed.
    void OnSharedPreferenceChanged(string name)
    {
#if DEBUG
        Log.Info(TAG, $"Shared preferences changed: {name}");
#endif
        var key = (SettingKey?)null;
        var prevValue = (object?)null;
        var newValue = (object?)null;
        var needToResetValue = false;
        this.sharedPrefsValues = this.sharedPrefs.All;
        this.sharedPrefsValues.TryGetValue(name, out var valueInSharedPrefs);
        lock (this.syncLock)
        {
            this.keys.TryGetValue(name, out key);
            if (valueInSharedPrefs == null)
            {
                if (key is not null)
                {
#if DEBUG
                    Log.Info(TAG, $"{name} was removed from shared preferences");
#endif
                    needToResetValue = true;
                }
            }
            else if (key is null)
            {
                key = CreateKey(name, valueInSharedPrefs);
                if (key is not null)
                {
#if DEBUG
                    Log.Info(TAG, $"{name} was added shared preferences");
#endif
                    prevValue = key.DefaultValue;
                    newValue = valueInSharedPrefs;
                }
                else
                    Log.Warn(TAG, $"Ignore new key '{name}' with type {valueInSharedPrefs.GetType().Name} which is put to shared preferences");
            }
            else if (this.values.TryGetValue(name, out prevValue))
            {
                if (!object.Equals(prevValue, valueInSharedPrefs))
                {
                    if (prevValue is uint uintValue)
                    {
                        if (valueInSharedPrefs is not long longValue
                            || uintValue != longValue)
                        {
                            newValue = valueInSharedPrefs;
                        }
                    }
                    else if (prevValue is Enum enumValue)
                    {
                        if (valueInSharedPrefs is not string strValue
                            || enumValue.ToString() != strValue)
                        {
                            newValue = valueInSharedPrefs;
                        }
                    }
                    else if (prevValue is double doubleValue)
                    {
                        if (valueInSharedPrefs is not float floatValue
                            || Math.Abs(doubleValue - floatValue) > 0.0001)
                        {
                            newValue = valueInSharedPrefs;
                        }
                    }
                    else
                        newValue = valueInSharedPrefs;
                }
            }
        }
        if (needToResetValue)
            this.ResetValue(key!);
        else if (newValue != null)
        {
#if DEBUG
            Log.Info(TAG, $"{name} was changed in shared preferences");
#endif
            prevValue ??= key!.DefaultValue;
            lock (this.syncLock)
            {
                this.settingChangingHandlers?.Invoke(this, new(key!, prevValue, newValue));
                this.values[name] = newValue;
                this.keys[name] = key!;
                this.changedKeys.Remove(name);
                this.removedKeys.Remove(name);
            }
            this.SettingChanged?.Invoke(this, new(key!, prevValue, newValue));
        }
    }

    
    /// <inheritdoc/>
    public void ResetValue(SettingKey key)
    {
        // check type
        VerifyValueType(key);

        // check key
        var name = key.Name;
        if (name == VersionKey)
            throw new InvalidOperationException("Cannot reset value of version.");

        // reset value
        var prevValue = (object?)null;
        lock (this.syncLock)
        {
            if (!this.values.TryGetValue(name, out prevValue))
                return;
            this.settingChangingHandlers?.Invoke(this, new(key, prevValue, key.DefaultValue));
            this.values.Remove(name);
            this.changedKeys.Remove(name);
            this.keys.Remove(name);
            this.removedKeys.Add(name);
        }
        this.SettingChanged?.Invoke(this, new(key, prevValue, key.DefaultValue));
        this.editSharedPrefsAction.Schedule(SharedPrefsEditingDelay);
    }


    /// <inheritdoc/>
    public event EventHandler<SettingChangedEventArgs>? SettingChanged;


    /// <inheritdoc/>
    event EventHandler<SettingChangingEventArgs>? ISettings.SettingChanging
    {
        add => this.settingChangingHandlers += value;
        remove => this.settingChangingHandlers -= value;
    }


    /// <inheritdoc/>
    void ISettings.SetValue(SettingKey key, object value)
    {
        // check value type
        var expectedType = key.ValueType;
        VerifyValueType(expectedType);
        if (!expectedType.IsAssignableFrom(value.GetType()))
            throw new ArgumentException($"Value with type {value.GetType().Name} is not suitable for key '{key.Name}', {expectedType.Name} expected.");

        // check key
        var name = key.Name;
        if (name == VersionKey)
            throw new InvalidOperationException("Cannot reset value of version.");

        // update value
        var prevValue = (object?)null;
        var isDefaultValue = object.Equals(value, key.DefaultValue);
        lock (this.syncLock)
        {
            // ignore update if value doesn't change
            if (this.values.TryGetValue(name, out prevValue))
            {
                if (object.Equals(value, prevValue))
                    return;
            }
            else if (isDefaultValue)
                return;
            
            // raise event
            prevValue ??= key.DefaultValue;
            this.settingChangingHandlers?.Invoke(this, new(key, prevValue, value));

            // update value
            if (isDefaultValue)
            {
                this.values.Remove(name);
                this.changedKeys.Remove(name);
                this.keys.Remove(name);
                this.removedKeys.Add(name);
            }
            else
            {
                this.values[name] = value;
                this.changedKeys.Add(name);
                this.keys[name] = key;
                this.removedKeys.Remove(name);
            }
        }
        this.editSharedPrefsAction.Schedule(SharedPrefsEditingDelay);

        // raise event
        this.SettingChanged?.Invoke(this, new(key, prevValue, value));
    }


    // Throw exception if type of value is unsupported.
    static void VerifyValueType(SettingKey key) =>
        VerifyValueType(key.ValueType);
    static void VerifyValueType(Type valueType)
    {
        if (valueType != typeof(bool)
            && valueType != typeof(string)
            && !valueType.IsEnum
            && valueType != typeof(int)
            && valueType != typeof(uint)
            && valueType != typeof(long)
            && valueType != typeof(float)
            && valueType != typeof(double)
            && !typeof(ISet<string>).IsAssignableFrom(valueType))
        {
            throw new NotSupportedException($"Type {valueType.Name} is unsupported.");
        }
    }


    /// <inheritdoc/>
    public int Version
    {
        get => this.version;
        set
        {
            lock (this.syncLock)
            {
                if (this.version == value)
                    return;
                this.version = value;
            }
            SharedPrefsEditingSyncContext.Post(() =>
                this.sharedPrefs.Edit()!.PutInt(VersionKey, this.version)!.Apply());
        }
    }
}