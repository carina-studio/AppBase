using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Extensions for <see cref="IEnumerable"/> and <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Generate readable string represents content in <see cref="IEnumerable"/>.
		/// </summary>
		/// <param name="enumerable"><see cref="IEnumerable"/>.</param>
		/// <returns>Readable string represents content.</returns>
		public static string ContentToString(this IEnumerable enumerable)
		{
			if (enumerable is string str)
				return str;
			var stringBuilder = new StringBuilder("[");
			foreach (var element in enumerable)
			{
				if (stringBuilder.Length > 1)
					stringBuilder.Append(", ");
				if (element is string || element is StringBuilder)
				{
					stringBuilder.Append('"');
					stringBuilder.Append(element.ToString());
					stringBuilder.Append('"');
				}
				else if (element is IEnumerable innerEnumerable)
					stringBuilder.Append(innerEnumerable.ContentToString());
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
