using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace CarinaStudio.Collections;

/// <summary>
/// Extensions for <see cref="IDictionary{TKey, TValue}"/>.
/// </summary>
public static class DictionaryExtensions
{
	/// <summary>
	/// Add all <see cref="KeyValuePair{TKey, TValue}"/>s to dictionary.
	/// </summary>
	/// <typeparam name="TKey">Type of key.</typeparam>
	/// <typeparam name="TValue">Type of value.</typeparam>
	/// <param name="dictionary"><see cref="IDictionary{TKey, TValue}"/>.</param>
	/// <param name="keyValues"><see cref="KeyValuePair{TKey, TValue}"/>s to add.</param>
	public static void AddAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> keyValues) where TKey : notnull
	{
		foreach (var pair in keyValues)
			dictionary.Add(pair.Key, pair.Value);
	}


	/// <summary>
	/// Make dictionary as read-only.
	/// </summary>
	/// <typeparam name="TKey">Type of key.</typeparam>
	/// <typeparam name="TValue">Type of value.</typeparam>
	/// <param name="dictionary"><see cref="IDictionary{TKey, TValue}"/>.</param>
	/// <returns>Read-only dictionary.</returns>
	[Pure]
	public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull
	{
		if (dictionary.IsReadOnly)
			return dictionary;
		return new ReadOnlyDictionary<TKey, TValue>(dictionary);
	}


	/// <summary>
	/// Get value with given key. The default value will be returned if value cannot be found.
	/// </summary>
	/// <param name="dictionary"><see cref="IDictionary{TKey, TValue}"/>.</param>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	/// <typeparam name="TKey">Type of key in dictionary.</typeparam>
	/// <typeparam name="TValue">Type of value in dictionary.</typeparam>
	/// <returns>Value of given default value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[return: NotNullIfNotNull(nameof(defaultValue))]
	public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = null) where TKey : notnull where TValue : struct
	{
		if (dictionary.TryGetValue(key, out var value))
			return value;
		return defaultValue;
	}


	/// <summary>
	/// Get value with given key. The default value will be returned if value cannot be found.
	/// </summary>
	/// <param name="dictionary"><see cref="IDictionary{TKey, TValue}"/>.</param>
	/// <param name="key">Key.</param>
	/// <param name="defaultValue">Default value.</param>
	/// <typeparam name="TKey">Type of key in dictionary.</typeparam>
	/// <typeparam name="TValue">Type of value in dictionary.</typeparam>
	/// <returns>Value of given default value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[return: NotNullIfNotNull(nameof(defaultValue))]
	public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = null) where TKey : notnull where TValue : class
	{
		if (dictionary.TryGetValue(key, out var value))
			return value;
		return defaultValue;
	}
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given dictionary is empty or not.
	/// </summary>
	/// <typeparam name="TKey">Type of key in dictionary.</typeparam>
	/// <typeparam name="TValue">Type of value in dictionary.</typeparam>
	/// <param name="dictionary">Dictionary to check.</param>
	/// <returns>True if the dictionary is empty.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) => dictionary.Count <= 0;
#endif


#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether the given dictionary is not empty or not.
	/// </summary>
	/// <typeparam name="TKey">Type of key in dictionary.</typeparam>
	/// <typeparam name="TValue">Type of value in dictionary.</typeparam>
	/// <param name="dictionary">Dictionary to check.</param>
	/// <returns>True if the dictionary is not empty.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNotEmpty<TKey, TValue>([NotNullWhen(true)] IDictionary<TKey, TValue>? dictionary) => dictionary is not null && dictionary.Count > 0;
#endif
	
	
#if !NET9_0_OR_GREATER
	/// <summary>
	/// Check whether given dictionary is null/empty or not.
	/// </summary>
	/// <typeparam name="TKey">Type of key in dictionary.</typeparam>
	/// <typeparam name="TValue">Type of value in dictionary.</typeparam>
	/// <param name="dictionary">Dictionary to check.</param>
	/// <returns>True if the dictionary is null or empty.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNullOrEmpty<TKey, TValue>([NotNullWhen(false)] IDictionary<TKey, TValue>? dictionary) => dictionary is null || dictionary.Count <= 0;
#endif
	

	/// <summary>
	/// Try getting value from dictionary as given type.
	/// </summary>
	/// <param name="dictionary"><see cref="IDictionary{TKey, TValue}"/>.</param>
	/// <param name="key">Key.</param>
	/// <param name="value">Got value.</param>
	/// <typeparam name="TKey">Type of key in dictionary.</typeparam>
	/// <typeparam name="TValue">Type of value in dictionary.</typeparam>
	/// <typeparam name="TOut">Desired type of value.</typeparam>
	/// <returns>True if value has been got as given type successfully.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetValue<TKey, TValue, TOut>(this IDictionary<TKey, TValue> dictionary, TKey key, [NotNullWhen(true)] out TOut value) where TKey : notnull where TOut : TValue
	{
		if (dictionary.TryGetValue(key, out var rawValue) && rawValue is TOut outValue)
		{
			value = outValue;
			return true;
		}
#pragma warning disable CS8601
		value = default;
#pragma warning restore CS8601
		return false;
	}
}