using System;
using System.Collections.Generic;

namespace CarinaStudio.Configuration
{
	/// <summary>
	/// Application settings.
	/// </summary>
	public interface ISettings
	{
		/// <summary>
		/// Get raw value stored in settings no matter what type of value specified by key.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <returns>Raw setting value.</returns>
		object? GetRawValue(SettingKey key);


		/// <summary>
		/// Get all setting keys.
		/// </summary>
		IEnumerable<SettingKey> Keys { get; }


		/// <summary>
		/// Reset setting to default value.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		void ResetValue(SettingKey key);


		/// <summary>
		/// Raised after changing setting.
		/// </summary>
		event EventHandler<SettingChangedEventArgs>? SettingChanged;


		/// <summary>
		/// Raised before changing setting.
		/// </summary>
		event EventHandler<SettingChangingEventArgs>? SettingChanging;


		/// <summary>
		/// Set value of setting.
		/// </summary>
		/// <param name="key">Key of setting.</param>
		/// <param name="value">New value.</param>
		[Obsolete("Try using generic SetValue() instead, unless you don't know the type of value.")]
		void SetValue(SettingKey key, object value);


		/// <summary>
		/// Get version of settings.
		/// </summary>
		int Version { get; }
	}


	/// <summary>
	/// Extensions for <see cref="ISettings"/>.
	/// </summary>
	public static class SettingsExtensions
	{
		/// <summary>
		/// Get setting value as type specified by key, or get default value.
		/// </summary>
		/// <param name="settings"><see cref="ISettings"/>. The reference can be Null.</param>
		/// <param name="key">Key of setting.</param>
		/// <returns>Setting value, or default value.</returns>
		[Obsolete("Try using generic GetValueOrDefault() instead, unless you don't know the type of value.")]
		public static object GetValueOrDefault(this ISettings? settings, SettingKey key)
		{
			if (settings == null)
				return key.DefaultValue;
			var rawValue = settings.GetRawValue(key);
			if (rawValue != null)
			{
				var targetType = key.ValueType;
				if (targetType.IsAssignableFrom(rawValue.GetType()))
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
					catch
					{ }
				}
			}
			return key.DefaultValue;
		}


#pragma warning disable CS0618
		/// <summary>
		/// Get setting value as type specified by key, or get default value.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="settings"><see cref="ISettings"/>. The reference can be Null.</param>
		/// <param name="key">Key of setting.</param>
		/// <returns>Setting value, or default value.</returns>
		public static T GetValueOrDefault<T>(this ISettings? settings, SettingKey<T> key) => 
			(T)settings.GetValueOrDefault((SettingKey)key);
#pragma warning restore CS0618


		/// <summary>
		/// Check whether the value is False or not.
		/// </summary>
		/// <param name="settings"><see cref="ISettings"/>.</param>
		/// <param name="key">Key of setting.</param>
		/// <returns>True if the value is False.</returns>
		public static bool IsFalse(this ISettings settings, SettingKey<bool> key) =>
			!settings.GetValueOrDefault(key);
		

		/// <summary>
		/// Check whether the value is True or not.
		/// </summary>
		/// <param name="settings"><see cref="ISettings"/>.</param>
		/// <param name="key">Key of setting.</param>
		/// <returns>True if the value is True.</returns>
		public static bool IsTrue(this ISettings settings, SettingKey<bool> key) =>
			settings.GetValueOrDefault(key);


		/// <summary>
		/// Reset all values to default.
		/// </summary>
		/// <param name="settings"><see cref="ISettings"/>.</param>
		public static void ResetValues(this ISettings settings) => 
			settings.ResetValues(settings.Keys);


		/// <summary>
		/// Reset specific set of values to default.
		/// </summary>
		/// <param name="settings"><see cref="ISettings"/>.</param>
		/// <param name="keys">Key of settings to reset.</param>
		public static void ResetValues(this ISettings settings, IEnumerable<SettingKey> keys)
		{
			foreach (var key in keys)
				settings.ResetValue(key);
		}


#pragma warning disable CS8604, CS0618
		/// <summary>
		/// Set value of setting.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="settings"><see cref="ISettings"/>.</param>
		/// <param name="key">Key of setting.</param>
		/// <param name="value">New value.</param>
		public static void SetValue<T>(this ISettings settings, SettingKey<T> key, T value) => 
			settings.SetValue((SettingKey)key, value);
#pragma warning restore CS8604, CS0618


		/// <summary>
		/// Toggle the boolean value.
		/// </summary>
		/// <param name="settings"><see cref="ISettings"/>.</param>
		/// <param name="key">Key of setting.</param>
		/// <returns>Toggled value.</returns>
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
		public static bool VerifyValue(this ISettings settings, SettingKey key)
		{
			var rawValue = settings.GetRawValue(key);
			return rawValue == null || key.ValueType.IsAssignableFrom(rawValue.GetType());
		}
	}
}
