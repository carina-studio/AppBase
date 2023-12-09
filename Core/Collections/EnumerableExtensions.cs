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
		/// <param name="maxLength">Maximum length of generated string.</param>
		/// <returns>Readable string represents content.</returns>
		public static string ContentToString(this IEnumerable enumerable, int maxLength = 4096)
		{
			if (maxLength < 0)
				throw new ArgumentOutOfRangeException(nameof(maxLength));
			if (maxLength == 0)
				return "";
			if (enumerable is string str)
			{
				if (str.Length <= maxLength)
					return str;
				if (maxLength <= 3)
					return str.Substring(0, maxLength);
				return str.Substring(0, maxLength - 3) + "...";
			}
			var stringBuilder = new StringBuilder("[");
			foreach (var element in enumerable)
			{
				if (stringBuilder.Length > 1)
					stringBuilder.Append(", ");
				if (element is string || element is StringBuilder)
				{
					stringBuilder.Append('"');
					stringBuilder.Append(element);
					stringBuilder.Append('"');
				}
				else if (element is IEnumerable innerEnumerable)
					stringBuilder.Append(innerEnumerable.ContentToString());
				else if (element != null)
					stringBuilder.Append(element);
				else
					stringBuilder.Append("Null");
				if (stringBuilder.Length >= maxLength)
				{
					if (maxLength <= 3)
						stringBuilder.Remove(maxLength, stringBuilder.Length - maxLength);
					else
					{
						stringBuilder.Remove(maxLength - 3, stringBuilder.Length - maxLength + 3);
						stringBuilder.Append("...");
					}
					return stringBuilder.ToString();
				}
			}
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}
	}
}
