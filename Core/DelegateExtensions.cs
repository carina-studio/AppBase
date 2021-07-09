using System;
using System.Runtime.CompilerServices;

namespace CarinaStudio
{
	/// <summary>
	/// Extensions for delegates.
	/// </summary>
	public static class DelegateExtensions
	{
		/// <summary>
		/// Create new <see cref="Comparison{T}"/> which generates inverse result of given <see cref="Comparison{T}"/>.
		/// </summary>
		/// <typeparam name="T">Type of value to compare.</typeparam>
		/// <param name="comparison"><see cref="Comparison{T}"/>.</param>
		/// <returns><see cref="Comparison{T}"/> which generates inverse result of given <see cref="Comparison{T}"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Comparison<T> Invert<T>(this Comparison<T> comparison) => (x, y) => comparison(y, x);
	}
}
