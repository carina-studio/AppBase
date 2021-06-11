using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Read-only <see cref="IList{T}"/> which implements <see cref="INotifyCollectionChanged"/> and wraps another <see cref="IList{T}"/> which also implements <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	/// <typeparam name="T">Type of elements.</typeparam>
	public class ReadOnlyObservableList<T> : IList, IList<T>, INotifyCollectionChanged, IReadOnlyList<T>
	{
		// Fields.
		readonly IList<T> sourceList;


		/// <summary>
		/// Initialize new <see cref="ReadOnlyObservableList{T}"/> instance.
		/// </summary>
		/// <param name="source"></param>
		public ReadOnlyObservableList(IList<T> source)
		{
			if (!(source is INotifyCollectionChanged notifyCollectionChanged))
				throw new ArgumentException("Source list doesn't implement INotifyCollectionChanged interface.");
			this.sourceList = source;
			notifyCollectionChanged.CollectionChanged += (_, e) => this.CollectionChanged?.Invoke(this, e);
		}


		/// <summary>
		/// Raised when list changed.
		/// </summary>
		public event NotifyCollectionChangedEventHandler? CollectionChanged;


		/// <summary>
		/// Check whether given element is contained in list or not.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <returns>True if element is contained in list.</returns>
		public bool Contains(T element) => this.sourceList.Contains(element);


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <param name="array">Array to place copied elements.</param>
		/// <param name="arrayIndex">Index of first position in <paramref name="array"/> to place copied elements.</param>
		public void CopyTo(T[] array, int arrayIndex) => this.sourceList.CopyTo(array, arrayIndex);


		/// <summary>
		/// Get number of elements.
		/// </summary>
		public int Count { get => this.sourceList.Count; }


		/// <summary>
		/// Get <see cref="IEnumerator{T}"/> to enumerate elements.
		/// </summary>
		/// <returns><see cref="IEnumerator{T}"/>.</returns>
		public IEnumerator<T> GetEnumerator() => this.sourceList.GetEnumerator();


		/// <summary>
		/// Get index of given element in list.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <returns>Index of element in list, or -1 if element is not contained in list.</returns>
		public int IndexOf(T element) => this.sourceList.IndexOf(element);


		/// <summary>
		/// Get element.
		/// </summary>
		/// <param name="index">Index of element.</param>
		/// <returns>Element.</returns>
		public T this[int index] { get => this.sourceList[index]; }


		// Interface implementations.
		void ICollection<T>.Add(T value) => throw new InvalidOperationException();
		void ICollection<T>.Clear() => throw new InvalidOperationException();
		void ICollection.CopyTo(Array array, int index) => this.CopyTo((T[])array, index);
		bool ICollection<T>.IsReadOnly => true;
		bool ICollection.IsSynchronized => (this.sourceList as ICollection)?.IsSynchronized ?? false;
		bool ICollection<T>.Remove(T item) => throw new InvalidOperationException();
		object ICollection.SyncRoot => this;
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		int IList.Add(object? value) => throw new InvalidOperationException();
		void IList.Clear() => throw new InvalidOperationException();
		bool IList.Contains(object? value)
		{
			if (value is T element)
				return this.Contains(element);
			return false;
		}
		int IList.IndexOf(object? value)
		{
			if (value is T element)
				return this.IndexOf(element);
			return -1;
		}
		void IList.Insert(int index, object? value) => throw new InvalidOperationException();
		bool IList.IsFixedSize => (this.sourceList as IList)?.IsFixedSize ?? false;
		bool IList.IsReadOnly => true;
		object? IList.this[int index] { get => this.sourceList[index]; set => throw new InvalidOperationException(); }
		T IList<T>.this[int index] { get => this.sourceList[index]; set => throw new InvalidOperationException(); }
		void IList<T>.Insert(int index, T item) => throw new InvalidOperationException();
		void IList.Remove(object? value) => throw new InvalidOperationException();
		void IList.RemoveAt(int index) => throw new InvalidOperationException();
		void IList<T>.RemoveAt(int index) => throw new InvalidOperationException();
	}
}
