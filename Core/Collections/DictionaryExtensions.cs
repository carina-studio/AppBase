using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CarinaStudio.Collections
{
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
		public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull
		{
			if (dictionary.IsReadOnly)
				return dictionary;
			return new ReadOnlyDictionary<TKey, TValue>(dictionary);
		}


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
		public static bool TryGetValue<TKey, TValue, TOut>(this IDictionary<TKey, TValue> dictionary, TKey key, out TOut value) where TKey : notnull where TOut : TValue
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
}
