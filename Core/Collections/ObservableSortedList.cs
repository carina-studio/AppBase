using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// <see cref="SortedList{T}"/> which implements <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	public class ObservableSortedList<T> : SortedList<T>, INotifyCollectionChanged
	{
		/// <summary>
		/// Initialize new <see cref="ObservableSortedList{T}"/> instance.
		/// </summary>
		/// <param name="comparer"><see cref="IComparer{T}"/> to sort elements.</param>
		/// <param name="elements">Initial elements.</param>
		public ObservableSortedList(IComparer<T> comparer, IEnumerable<T>? elements = null) : base(comparer, elements)
		{ }


		/// <summary>
		/// Initialize new <see cref="ObservableSortedList{T}"/> instance.
		/// </summary>
		/// <param name="comparison"><see cref="Comparison{T}"/> to sort elements.</param>
		/// <param name="elements">Initial elements.</param>
		public ObservableSortedList(Comparison<T> comparison, IEnumerable<T>? elements = null) : base(comparison, elements)
		{ }


		/// <summary>
		/// Initialize new <see cref="ObservableSortedList{T}"/> instance.
		/// </summary>
		/// <param name="elements">Initial elements.</param>
		public ObservableSortedList(IEnumerable<T>? elements = null) : base(elements)
		{ }


		/// <summary>
		/// Raised when list changed.
		/// </summary>
		public event NotifyCollectionChangedEventHandler? CollectionChanged;


		/// <summary>
		/// Called when list changed.
		/// </summary>
		/// <param name="e">Event data.</param>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);
			this.CollectionChanged?.Invoke(this, e);
		}
	}
}
