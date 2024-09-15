using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CarinaStudio
{
	/// <summary>
	/// Pool of <see cref="string"/> to share <see cref="string"/> instances and reduce redundent <see cref="string"/> instances. This is thread-safe class.
	/// </summary>
	public class StringPool
	{
		// Fields
		readonly Dictionary<string, string> strings = new Dictionary<string, string>();


		/// <summary>
		/// Clear all string instances from pool.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Clear() => this.strings.Clear();


		/// <summary>
		/// Get <see cref="string"/> instance from pool.
		/// </summary>
		/// <param name="value">String value. The instance will be added to pool if it is not found in pool.</param>
		/// <returns><see cref="string"/> instance.</returns>
		public string this[string value]
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get
			{
				if (this.strings.TryGetValue(value, out var instance))
					return instance;
				this.strings.Add(value, value);
				return value;
			}
		}
	}
}
