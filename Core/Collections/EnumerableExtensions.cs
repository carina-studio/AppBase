using System;
using System.Collections.Generic;
using System.Text;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Extensions for <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Generate readable string represents content in <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">Type of element.</typeparam>
		/// <param name="enumerable"><see cref="IEnumerable{T}"/>.</param>
		/// <returns>Readable string represents content.</returns>
		public static string ContentToString<T>(this IEnumerable<T> enumerable)
		{
			var stringBuilder = new StringBuilder("[");
			foreach (var element in enumerable)
			{
				if (stringBuilder.Length > 1)
					stringBuilder.Append(", ");
				if (element is string)
				{
					stringBuilder.Append('"');
					stringBuilder.Append(element.ToString());
					stringBuilder.Append('"');
				}
				else if (element != null)
					stringBuilder.Append(element.ToString());
				else
					stringBuilder.Append("Null");
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}
	}
}
