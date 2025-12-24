using CarinaStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#if NET9_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace CarinaStudio.Configuration;

/// <summary>
/// Application settings.
/// </summary>
[ThreadSafe]
public interface ISettings
{
#if NET9_0_OR_GREATER
	/// <summary>
	/// Get setting value as type specified by key, or get default value.
	/// </summary>
	/// <param name="key">Key of setting.</param>
	/// <returns>Setting value, or default value.</returns>
	[ThreadSafe]
	[OverloadResolutionPriority(0)]
	public object GetValueOrDefault(SettingKey key)
	{
		var rawValue = this.GetRawValue(key);
		if (rawValue != null)
		{
			var targetType = key.ValueType;
			if (targetType.IsInstanceOfType(rawValue))
				return rawValue;
			if (rawValue is IConvertible convertible)
			{
				try
				{
					if (targetType == typeof(bool))
						return convertible.ToBoolean(null);
					if (targetType == typeof(int))
						return convertible.ToInt32(null);
					if (targetType == typeof(long))
						return convertible.ToInt64(null);
					if (targetType == typeof(double))
						return convertible.ToDouble(null);
					if (targetType.IsEnum)
					{
						if (rawValue is string strValue)
						{
							Enum.TryParse(targetType, strValue, false, out var enumValue);
							return enumValue ?? key.DefaultValue;
						}
						return key.DefaultValue;
					}
					if (targetType == typeof(string))
						return convertible.ToString(null);
					if (targetType == typeof(sbyte))
						return convertible.ToSByte(null);
					if (targetType == typeof(short))
						return convertible.ToInt16(null);
					if (targetType == typeof(ushort))
						return convertible.ToUInt16(null);
					if (targetType == typeof(uint))
						return convertible.ToUInt16(null);
					if (targetType == typeof(ulong))
						return convertible.ToUInt64(null);
					if (targetType == typeof(float))
						return convertible.ToSingle(null);
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{ }
			}
		}
		return key.DefaultValue;
	}
#endif
	
	
#if NET9_0_OR_GREATER
	/// <summary>
	/// Get setting value as type specified by key, or get default value.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	/// <param name="key">Key of setting.</param>
	/// <returns>Setting value, or default value.</returns>
	[ThreadSafe]
	[OverloadResolutionPriority(1)]
	public T GetValueOrDefault<T>(SettingKey<T> key) => 
		(T)this.GetValueOrDefault((SettingKey)key);
#endif

	
	/// <summary>
	/// Get raw value stored in settings no matter what type of value specified by key.
	/// </summary>
	/// <param name="key">Key of setting.</param>
	/// <returns>Raw setting value.</returns>
	[ThreadSafe]
	object? GetRawValue(SettingKey key);


	/// <summary>
	/// Get all setting keys.
	/// </summary>
	[ThreadSafe]
	IEnumerable<SettingKey> Keys { get; }


	/// <summary>
	/// Reset setting to default value.
	/// </summary>
	/// <param name="key">Key of setting.</param>
	[ThreadSafe]
	void ResetValue(SettingKey key);


	/// <summary>
	/// Raised after changing setting.
	/// </summary>
	[ThreadSafe]
	event EventHandler<SettingChangedEventArgs>? SettingChanged;


	/// <summary>
	/// Raised before changing setting.
	/// </summary>
	[ThreadSafe]
	event EventHandler<SettingChangingEventArgs>? SettingChanging;


	/// <summary>
	/// Set value of setting.
	/// </summary>
	/// <param name="key">Key of setting.</param>
	/// <param name="value">New value.</param>
	[ThreadSafe]
#if NET9_0_OR_GREATER
	[OverloadResolutionPriority(0)]
#else
	[Obsolete("Try using generic SetValue() instead, unless you don't know the type of value.")]
#endif
	void SetValue(SettingKey key, object value);
	
	
#if NET9_0_OR_GREATER
	/// <summary>
	/// Set value of setting.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	/// <param name="key">Key of setting.</param>
	/// <param name="value">New value.</param>
	[ThreadSafe]
	[OverloadResolutionPriority(1)]
	void SetValue<T>(SettingKey<T> key, T value) where T : notnull=> 
		this.SetValue((SettingKey)key, value);
#endif


	/// <summary>
	/// Get version of settings.
	/// </summary>
	[ThreadSafe]
	int Version { get; }
}


/// <summary>
/// Extensions for <see cref="ISettings"/>.
/// </summary>
public static class SettingsExtensions
{
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Get setting value as type specified by key, or get default value.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>. The reference can be Null.</param>
	/// <param name="key">Key of setting.</param>
	/// <returns>Setting value, or default value.</returns>
	[Obsolete("Try using generic GetValueOrDefault() instead, unless you don't know the type of value.")]
	[ThreadSafe]
	public static object GetValueOrDefault(this ISettings? settings, SettingKey key)
	{
		if (settings == null)
			return key.DefaultValue;
		var rawValue = settings.GetRawValue(key);
		if (rawValue != null)
		{
			var targetType = key.ValueType;
			if (targetType.IsInstanceOfType(rawValue))
				return rawValue;
			if (rawValue is IConvertible convertible)
			{
				try
				{
					if (targetType == typeof(bool))
						return convertible.ToBoolean(null);
					if (targetType == typeof(int))
						return convertible.ToInt32(null);
					if (targetType == typeof(long))
						return convertible.ToInt64(null);
					if (targetType == typeof(double))
						return convertible.ToDouble(null);
					if (targetType.IsEnum)
					{
						if (rawValue is string strValue)
						{
							Enum.TryParse(targetType, strValue, false, out var enumValue);
							return enumValue ?? key.DefaultValue;
						}
						return key.DefaultValue;
					}
					if (targetType == typeof(string))
						return convertible.ToString(null);
					if (targetType == typeof(sbyte))
						return convertible.ToSByte(null);
					if (targetType == typeof(short))
						return convertible.ToInt16(null);
					if (targetType == typeof(ushort))
						return convertible.ToUInt16(null);
					if (targetType == typeof(uint))
						return convertible.ToUInt16(null);
					if (targetType == typeof(ulong))
						return convertible.ToUInt64(null);
					if (targetType == typeof(float))
						return convertible.ToSingle(null);
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{ }
			}
		}
		return key.DefaultValue;
	}
#endif


#if !NET9_0_OR_GREATER
#pragma warning disable CS0618
	/// <summary>
	/// Get setting value as type specified by key, or get default value.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	/// <param name="settings"><see cref="ISettings"/>. The reference can be Null.</param>
	/// <param name="key">Key of setting.</param>
	/// <returns>Setting value, or default value.</returns>
	[ThreadSafe]
	public static T GetValueOrDefault<T>(this ISettings? settings, SettingKey<T> key) => 
		(T)settings.GetValueOrDefault((SettingKey)key);
#pragma warning restore CS0618
#endif


	/// <summary>
	/// Check whether the value is False or not.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	/// <param name="key">Key of setting.</param>
	/// <returns>True if the value is False.</returns>
	[ThreadSafe]
	public static bool IsFalse(this ISettings settings, SettingKey<bool> key) =>
		!settings.GetValueOrDefault(key);
	

	/// <summary>
	/// Check whether the value is True or not.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	/// <param name="key">Key of setting.</param>
	/// <returns>True if the value is True.</returns>
	[ThreadSafe]
	public static bool IsTrue(this ISettings settings, SettingKey<bool> key) =>
		settings.GetValueOrDefault(key);


	/// <summary>
	/// Reset all values to default.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	[ThreadSafe]
	public static void ResetValues(this ISettings settings) => 
		settings.ResetValues(settings.Keys);


	/// <summary>
	/// Reset specific set of values to default.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	/// <param name="keys">Key of settings to reset.</param>
	[ThreadSafe]
	public static void ResetValues(this ISettings settings, IEnumerable<SettingKey> keys)
	{
		foreach (var key in keys)
			settings.ResetValue(key);
	}


#if !NET9_0_OR_GREATER
#pragma warning disable CS8604, CS0618
	/// <summary>
	/// Set value of setting.
	/// </summary>
	/// <typeparam name="T">Type of value.</typeparam>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	/// <param name="key">Key of setting.</param>
	/// <param name="value">New value.</param>
	[ThreadSafe]
	public static void SetValue<T>(this ISettings settings, SettingKey<T> key, T value) => 
		settings.SetValue(key, value);
#pragma warning restore CS8604, CS0618
#endif


	/// <summary>
	/// Toggle the boolean value.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	/// <param name="key">Key of setting.</param>
	/// <returns>Toggled value.</returns>
	[ThreadSafe]
	public static bool ToggleValue(this ISettings settings, SettingKey<bool> key)
	{
		var value = !settings.GetValueOrDefault(key);
		settings.SetValue<bool>(key, value);
		return value;
	}


	/// <summary>
	/// Check whether values in two <see cref="ISettings"/> are same or not.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	/// <param name="another">Another <see cref="ISettings"/>.</param>
	/// <returns></returns>
	[ThreadSafe]
	public static bool ValuesEqual(this ISettings settings, ISettings another)
	{
		if (settings == another)
			return true;
		var keys = new HashSet<SettingKey>(settings.Keys);
		if (!keys.SetEquals(another.Keys))
			return false;
		foreach (var key in keys)
		{
			var value = settings.GetRawValue(key);
			var anotherValue = another.GetRawValue(key);
			if (!(value?.Equals(anotherValue) ?? anotherValue == null))
				return false;
		}
		return true;
	}


	/// <summary>
	/// Check whether type of value of given setting is correct or not. You can get raw value by calling <see cref="ISettings.GetRawValue(SettingKey)"/> if type is incorrect.
	/// </summary>
	/// <param name="settings"><see cref="ISettings"/>.</param>
	/// <param name="key">Key of setting.</param>
	/// <returns>True if type of value of given setting is correct.</returns>
	[ThreadSafe]
	public static bool VerifyValue(this ISettings settings, SettingKey key)
	{
		var rawValue = settings.GetRawValue(key);
		return rawValue == null || key.ValueType.IsInstanceOfType(rawValue);
	}
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