﻿using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CarinaStudio.Configuration;

/// <summary>
/// Implementation of <see cref="ISettings"/> which keeps values in memory only. This is thread-safe class.
/// </summary>
[ThreadSafe]
public class MemorySettings : ISettings
{
    // Fields.
    readonly Lock eventLock = new();
    readonly Lock valuesLock = new();
    EventHandler<SettingChangedEventArgs>? settingChanged;
    EventHandler<SettingChangingEventArgs>? settingChanging;
    readonly Dictionary<SettingKey, object> values = new Dictionary<SettingKey, object>();


    /// <summary>
    /// Initialize new <see cref="MemorySettings"/> instance.
    /// </summary>
    public MemorySettings()
    {
        this.Version = 0;
    }


    /// <summary>
    /// Initialize new <see cref="MemorySettings"/> instance.
    /// </summary>
    /// <param name="template">Template <see cref="ISettings"/> to copy initial values from.</param>
    public MemorySettings(ISettings template)
    {
        foreach (var key in template.Keys)
        {
            var value = template.GetRawValue(key);
            if (value != null)
                this.values[key] = value;
        }
        this.Version = template.Version;
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
    public int Version { get; }
}