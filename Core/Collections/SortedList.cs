using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// List which makes elements sorted automatically. Implmentations of <see cref="IList{T}"/> are also optimized for sorted case.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	public class SortedList<T> : IList, IList<T>, INotifyCollectionChanged, IReadOnlyList<T>
	{
		// Fields.
		readonly IComparer<T> comparer;
		readonly List<T> list = new List<T>();


		/// <summary>
		/// Initialize new <see cref="SortedList{T}"/> instance.
		/// </summary>
		/// <param name="comparer"><see cref="IComparer{T}"/> to sort elements.</param>
		/// <param name="elements">Initial elements.</param>
		public SortedList(IComparer<T> comparer, IEnumerable<T>? elements = null)
		{
			this.comparer = comparer;
			this.SetupInitElements(elements);
		}


		/// <summary>
		/// Initialize new <see cref="SortedList{T}"/> instance.
		/// </summary>
		/// <param name="comparison"><see cref="Comparison{T}"/> to sort elements.</param>
		/// <param name="elements">Initial elements.</param>
		public SortedList(Comparison<T> comparison, IEnumerable<T>? elements = null)
		{
			this.comparer = Comparer<T>.Create(comparison);
			this.SetupInitElements(elements);
		}


		/// <summary>
		/// Initialize new <see cref="SortedList{T}"/> instance.
		/// </summary>
		/// <param name="elements">Initial elements.</param>
		public SortedList(IEnumerable<T>? elements = null)
		{
			if (!typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
				throw new ArgumentException($"Element type '{typeof(T).Name}' doesn't implement IComparable.");
			this.comparer = Comparer<T>.Default;
			this.SetupInitElements(elements);
		}


		/// <summary>
		/// Add element to list.
		/// </summary>
		/// <param name="element">Element to add.</param>
		/// <returns>Index of added element in list.</returns>
		public int Add(T element)
		{
			// insert to tail of list
			var list = this.list;
			var count = list.Count;
			var comparer = this.comparer;
			if (count == 0 || comparer.Compare(element, list[count - 1]) >= 0)
			{
				list.Add(element);
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, count));
				return count;
			}

			// insert to head of list
			if (comparer.Compare(element, list[0]) <= 0)
			{
				list.Insert(0, element);
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, 0));
				return 0;
			}

			// find position and insert
			var insertionIndex = list.BinarySearch(element, comparer);
			if (insertionIndex < 0)
				insertionIndex = ~insertionIndex;
			list.Insert(insertionIndex, element);
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, insertionIndex));
			return insertionIndex;
		}


		/// <summary>
		/// Add elements to list.
		/// </summary>
		/// <param name="elements">Elements to add.</param>
		/// <param name="isSorted">Whether elements in <paramref name="elements"/> are already sorted or not.</param>
		public void AddAll(IEnumerable<T> elements, bool isSorted = false)
		{
			// sort elements
			var sortedElements = this.Sort(elements, isSorted);
			var elementCount = sortedElements.Count;
			if (elementCount <= 0)
				return;
			if (elementCount == 1)
			{
				this.Add(sortedElements[0]);
				return;
			}

			// insert to tail of list
			var list = this.list;
			var count = list.Count;
			var comparer = this.comparer;
			if (count == 0 || comparer.Compare(sortedElements[0], list[count - 1]) >= 0)
			{
				list.AddRange(sortedElements);
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)sortedElements.AsReadOnly(), count));
				return;
			}

			// insert to head of list
			if (comparer.Compare(sortedElements[elementCount - 1], list[0]) <= 0)
			{
				list.InsertRange(0, sortedElements);
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)sortedElements.AsReadOnly(), 0));
				return;
			}

			// find insertion position
			var insertStartIndex = list.BinarySearch(sortedElements[0], comparer);
			var insertEndIndex = list.BinarySearch(sortedElements[elementCount - 1], comparer);
			if (insertStartIndex < 0)
				insertStartIndex = ~insertStartIndex;
			if (insertEndIndex < 0)
				insertEndIndex = ~insertEndIndex;

			// insert to single position
			var insertionRegionSize = (insertEndIndex - insertStartIndex);
			if (insertionRegionSize < 0)
			{
				if (!this.IsSorted(comparer))
					throw new InternalStateCorruptedException();
				throw new ArgumentException("Elements are not sorted properly.");
			}
			if (insertionRegionSize == 0)
			{
				list.InsertRange(insertStartIndex, sortedElements);
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)sortedElements.AsReadOnly(), insertStartIndex));
				return;
			}

			// reserve space first
			if (list.Capacity < count + elementCount)
			{
				if (list.Capacity < int.MaxValue >> 1)
					list.Capacity = Math.Max(count << 1, count + (elementCount << 1));
				else
					list.Capacity = count + (elementCount << 1);
			}

			// insert by blocks
			var seStartIndex = 0;
			var seIndex = 0;
			var insertionCount = 0;
			while (seIndex < elementCount && insertStartIndex < count)
			{
				var seElement = sortedElements[seIndex];
				var comparisonResult = comparer.Compare(seElement, list[insertStartIndex]);
				if (comparisonResult <= 0)
					++seIndex;
				else
				{
					insertionCount = (seIndex - seStartIndex);
					if (insertionCount == 1)
					{
						list.Insert(insertStartIndex, sortedElements[seStartIndex]);
						this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sortedElements[seStartIndex], insertStartIndex));
						insertStartIndex += 2;
						++count;
					}
					else if (insertionCount > 0)
					{
						var insertionElements = new ListRangeView<T>(sortedElements, seStartIndex, insertionCount);
						list.InsertRange(insertStartIndex, insertionElements);
						this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, insertionElements, insertStartIndex));
						insertStartIndex += insertionCount + 1;
						count += insertionCount;
					}
					else
						++insertStartIndex;
					seStartIndex = seIndex;
				}
			}
			insertionCount = (elementCount - seStartIndex);
			if (insertionCount == 1)
			{
				list.Insert(insertStartIndex, sortedElements[seStartIndex]);
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sortedElements[seStartIndex], insertStartIndex));
			}
			else if (insertionCount > 0)
			{
				var insertionElements = new ListRangeView<T>(sortedElements, seStartIndex, insertionCount);
				list.InsertRange(insertStartIndex, insertionElements);
				this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, insertionElements, insertStartIndex));
			}
		}


		/// <summary>
		/// Clear all elements.
		/// </summary>
		public void Clear()
		{
			this.list.Clear();
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
		public bool Contains(T element) => this.list.BinarySearch(element, this.comparer) >= 0;


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <param name="array">Array to place copied elements.</param>
		/// <param name="arrayIndex">Index of first position in <paramref name="array"/> to place copied elements.</param>
		public void CopyTo(T[] array, int arrayIndex) => this.list.CopyTo(array, arrayIndex);


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <param name="index">Index of first element in list to copy.</param>
		/// <param name="array">Array to place copied elements.</param>
		/// <param name="arrayIndex">Index of first position in <paramref name="array"/> to place copied elements.</param>
		/// <param name="count">Number of elements to copy.</param>
		public void CopyTo(int index, T[] array, int arrayIndex, int count) => this.list.CopyTo(index, array, arrayIndex, count);


		/// <summary>
		/// Get number of elements.
		/// </summary>
		public int Count { get => this.list.Count; }


		/// <summary>
		/// Get <see cref="IEnumerator{T}"/> to enumerate elements.
		/// </summary>
		/// <returns><see cref="IEnumerator{T}"/>.</returns>
		public IEnumerator<T> GetEnumerator() => this.list.GetEnumerator();


		/// <summary>
		/// Get index of given element in list.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <returns>Index of element in list, or -1 if element is not contained in list.</returns>
		public int IndexOf(T element) => this.list.BinarySearch(element, this.comparer).Let((it) => it >= 0 ? it : -1);


		/// <summary>
		/// Remove first found of given element.
		/// </summary>
		/// <param name="element">Element to remove.</param>
		/// <returns>True if element has been removed.</returns>
		public bool Remove(T element)
		{
			var index = this.list.BinarySearch(element, this.comparer);
			if (index < 0)
				return false;
			this.list.RemoveAt(index);
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element, index));
			return true;
		}


		/// <summary>
		/// Remove elements from list.
		/// </summary>
		/// <param name="elements">Elements to remove.</param>
		/// <param name="isSorted">Whether elements in <paramref name="elements"/> are already sorted or not.</param>
		/// <returns>Number of removed elements.</returns>
		public int RemoveAll(IEnumerable<T> elements, bool isSorted = false)
		{
			// check state
			var list = this.list;
			var count = list.Count;
			if (count <= 0)
				return 0;

			// sort elements
			var sortedElements = this.Sort(elements, isSorted);
			var elementCount = sortedElements.Count;
			if (elementCount <= 0)
				return 0;

			// check whether elements exceed the range or not
			var comparer = this.comparer;
			if (comparer.Compare(sortedElements[0], list[count - 1]) > 0 || comparer.Compare(sortedElements[elementCount - 1], list[0]) < 0)
				return 0;

			// remove elements backward
			var result = 0;
			var seIndex = elementCount - 1;
			var listIndex = count - 1;
			var removingStartIndex = -1;
			var removingEndIndex = -1;
			var collectionChangedHandlers = this.CollectionChanged;
			while (seIndex >= 0 && listIndex >= 0)
			{
				var seElement = sortedElements[seIndex];
				var listElement = list[listIndex];
				var comparisonResult = comparer.Compare(seElement, listElement);
				if (comparisonResult == 0)
				{
					if (removingStartIndex > listIndex + 1)
					{
						var removingCount = removingEndIndex - removingStartIndex;
						var removedElements = collectionChangedHandlers != null
							? new ListRangeView<T>(this.list, removingStartIndex, removingCount)
							: null;
						list.RemoveRange(removingStartIndex, removingCount);
						collectionChangedHandlers?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, removingStartIndex));
						removingEndIndex = listIndex + 1;
						result += removingCount;
					}
					removingStartIndex = listIndex;
					if (removingEndIndex < 0)
						removingEndIndex = listIndex + 1;
					--listIndex;
				}
				else if (comparisonResult < 0)
					--listIndex;
				else
					--seIndex;
			}
			if (removingEndIndex > removingStartIndex)
			{
				var removingCount = removingEndIndex - removingStartIndex;
				var removedElements = collectionChangedHandlers != null
					? new ListRangeView<T>(this.list, removingStartIndex, removingCount)
					: null;
				list.RemoveRange(removingStartIndex, removingCount);
				collectionChangedHandlers?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, removingStartIndex));
				result += removingCount;
			}

			// complete
			return result;
		}


		/// <summary>
		/// Remove elements.
		/// </summary>
		/// <param name="index">Index of first element to remove.</param>
		/// <param name="count">Number of elements to remove.</param>
		public void RemoveRange(int index, int count)
		{
			if (count <= 0)
				return;
			if (index < 0 || index >= this.list.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (index + count > this.list.Count)
				throw new ArgumentOutOfRangeException(nameof(count));
			var collectionChangedHandlers = this.CollectionChanged;
			var removedElements = collectionChangedHandlers != null
					? new ListRangeView<T>(this.list, index, count)
					: null;
			this.list.RemoveRange(index, count);
			collectionChangedHandlers?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, index));
		}


		// Setup initial elements.
		void SetupInitElements(IEnumerable<T>? elements)
		{
			if (elements == null)
				return;
			this.list.AddRange(elements);
			if (!(elements is SortedList<T>))
				this.list.Sort(this.comparer);
		}


		// Sort given elements and make it as list.
		IList<T> Sort(IEnumerable<T> elements, bool isSorted) => elements.Let((it) =>
		{
			if (it is SortedList<T> sortedList)
				return sortedList;
			if (!isSorted)
				return it.ToArray().Also((array) => Array.Sort(array, this.comparer));
			if (elements is IList<T> list && elements is IList)
				return list;
			return it.ToArray();
		});


		/// <summary>
		/// Get element.
		/// </summary>
		/// <param name="index">Index of element.</param>
		/// <returns>Element.</returns>
		public T this[int index] { get => this.list[index]; }


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <returns>Array of copied elements</returns>
		public T[] ToArray() => this.list.ToArray();


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <param name="index">Index of first element in list to copy.</param>
		/// <param name="count">Number of elements to copy.</param>
		/// <returns>Array of copied elements</returns>
		public T[] ToArray(int index, int count) => new T[count].Also((it) => this.CopyTo(index, it, 0, count));


		/// <summary>
		/// Set the capacity to the actual number of elements in list.
		/// </summary>
		public void TrimExcess() => this.list.TrimExcess();


		// Interface implementations.
		void ICollection<T>.Add(T value) => this.Add(value);
		void ICollection.CopyTo(Array array, int index) => this.CopyTo((T[])array, index);
		bool ICollection<T>.IsReadOnly => false;
		bool ICollection.IsSynchronized => false;
		object ICollection.SyncRoot => this;
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		int IList.Add(object? value) => this.Add((T)value.AsNonNull());
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
		void IList.Insert(int index, object? value) => throw new NotSupportedException();
		bool IList.IsFixedSize => false;
		bool IList.IsReadOnly => false;
		object? IList.this[int index] { get => this.list[index]; set => throw new NotSupportedException(); }
		T IList<T>.this[int index] { get => this.list[index]; set => throw new NotSupportedException(); }
		void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
		void IList.Remove(object? value)
		{
			if (value is T element)
				this.Remove(element);
		}
		void IList.RemoveAt(int index) => throw new NotSupportedException();
		void IList<T>.RemoveAt(int index) => throw new NotSupportedException();
	}
}
