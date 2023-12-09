using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Implementation of <see cref="IList{T}"/> which also implements <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	/// <typeparam name="T">Type of element.</typeparam>
	public class ObservableList<T> : IList, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyList<T>
	{
		// Fields.
		readonly List<T> list;


		/// <summary>
		/// Initialize new <see cref="ObservableList{T}"/> instance.
		/// </summary>
		/// <param name="elements">Initial elements.</param>
		public ObservableList(IEnumerable<T>? elements = null)
		{
			if (elements == null)
				this.list = new List<T>();
			else
				this.list = new List<T>(elements);
		}


		/// <summary>
		/// Initialize new <see cref="ObservableList{T}"/> instance.
		/// </summary>
		/// <param name="capacity">Initial capacity.</param>
		public ObservableList(int capacity)
		{
			this.list = new List<T>(capacity);
		}


		/// <summary>
		/// Add element to list.
		/// </summary>
		/// <param name="element">Element to add.</param>
		/// <returns>Index of added element in list.</returns>
		public int Add(T element)
		{
			var index = this.list.Count;
			this.list.Add(element);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, index));
			return index;
		}


		/// <summary>
		/// Add elements to list.
		/// </summary>
		/// <param name="elements">Elements to add.</param>
		public void AddRange(IEnumerable<T> elements)
		{
			var index = this.list.Count;
			this.list.AddRange(elements);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ToIList(elements), index));
		}


		/// <summary>
		/// Get capacity of the list.
		/// </summary>
		public int Capacity => this.list.Capacity;


		/// <summary>
		/// Clear all elements.
		/// </summary>
		public void Clear()
		{
			if (this.list.IsEmpty())
				return;
			this.list.Clear();
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
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
		public bool Contains(T element) => this.list.Contains(element);


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
		public int Count => this.list.Count;


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
		public int IndexOf(T element) => this.list.IndexOf(element);


		/// <summary>
		/// Insert element at given position.
		/// </summary>
		/// <param name="index">Index of position to insert element.</param>
		/// <param name="element">Element to insert.</param>
		public void Insert(int index, T element)
		{
			this.list.Insert(index, element);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, index));
		}


		/// <summary>
		/// Insert elements at given position.
		/// </summary>
		/// <param name="index">Index of position to insert elements.</param>
		/// <param name="elements">Elements to insert.</param>
		public void InsertRange(int index, IEnumerable<T> elements)
		{
			this.list.InsertRange(index, elements);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ToIList(elements), index));
		}


		/// <summary>
		/// Move element in the list.
		/// </summary>
		/// <param name="index">Index of element to move.</param>
		/// <param name="newIndex">New index of element.</param>
		public void Move(int index, int newIndex)
		{
			if (newIndex < 0 || newIndex >= this.list.Count)
				throw new ArgumentOutOfRangeException(nameof(newIndex));
			if (index == newIndex)
				return;
			T element = this.list[index];
			this.list.RemoveAt(index);
			this.list.Insert(newIndex, element);
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, element, newIndex, index));
		}


		/// <summary>
		/// Move elements in the list.
		/// </summary>
		/// <param name="index">Index of first element to move.</param>
		/// <param name="newIndex">New index of first moved element.</param>
		/// <param name="count">Number of elements to move.</param>
		public void MoveRange(int index, int newIndex, int count)
		{
			if (count < 0 || index + count > this.list.Count)
				throw new ArgumentOutOfRangeException(nameof(count));
			if (newIndex < 0 || newIndex + count > this.list.Count)
				throw new ArgumentOutOfRangeException(nameof(newIndex));
			if (count == 0)
				return;
			if (index == newIndex)
				return;
			var elements = this.list.ToArray(index, count);
			this.list.RemoveRange(index, count);
			this.list.InsertRange(newIndex, elements);
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, elements, newIndex, index));
		}


		/// <summary>
		/// Raised when property changed.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;


		/// <summary>
		/// Remove first found of given element.
		/// </summary>
		/// <param name="element">Element to remove.</param>
		/// <returns>True if element has been removed.</returns>
		public bool Remove(T element)
		{
			var index = this.list.IndexOf(element);
			if (index < 0)
				return false;
			this.list.RemoveAt(index);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, element, index));
			return true;
		}


		/// <summary>
		/// Remove elements which match given condition from list.
		/// </summary>
		/// <param name="predicate">Function to check whether log matches given condition or not.</param>
		/// <returns>Number of removed elements.</returns>
		public int RemoveAll(Predicate<T> predicate)
		{
			var list = this.list;
			var result = 0;
			var removingEndIndex = -1;
			var removingStartIndex = -1;
			var collectionChangedHandlers = this.CollectionChanged;
			var propertyChangedHandlers = this.PropertyChanged;
			for (var i = list.Count - 1; i >= 0; --i)
			{
				if (predicate(list[i]))
				{
					removingStartIndex = i;
					if (removingEndIndex < 0)
						removingEndIndex = (i + 1);
				}
				else if (removingStartIndex >= 0)
				{
					var count = (removingEndIndex - removingStartIndex);
					var removedElements = collectionChangedHandlers != null
						? list.ToArray(removingStartIndex, count)
						: null;
					list.RemoveRange(removingStartIndex, count);
					propertyChangedHandlers?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
					collectionChangedHandlers?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, removingStartIndex));
					result += count;
					removingStartIndex = -1;
					removingEndIndex = -1;
				}
			}
			if (removingStartIndex >= 0)
			{
				var count = (removingEndIndex - removingStartIndex);
				var removedElements = collectionChangedHandlers != null
					? list.ToArray(removingStartIndex, count)
					: null;
				list.RemoveRange(removingStartIndex, count);
				propertyChangedHandlers?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
				collectionChangedHandlers?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, removingStartIndex));
				result += count;
			}
			return result;
		}


		/// <summary>
		/// Remove element at given position.
		/// </summary>
		/// <param name="index">Index of element to remove.</param>
		public void RemoveAt(int index)
		{
			var collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				var removedElement = this.list[index];
				this.list.RemoveAt(index);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
				collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElement, index));
			}
			else
			{
				this.list.RemoveAt(index);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			}
		}


		/// <summary>
		/// Remove elements.
		/// </summary>
		/// <param name="index">Index of first element to remove.</param>
		/// <param name="count">Number of elements to remove.</param>
		public void RemoveRange(int index, int count)
		{
			var collectionChanged = this.CollectionChanged;
			if (collectionChanged != null)
			{
				var removedElements = this.list.ToArray(index, count);
				this.list.RemoveRange(index, count);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
				collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, index));
			}
			else
			{
				this.list.RemoveRange(index, count);
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
			}
		}


		/// <summary>
		/// Get or set element.
		/// </summary>
		/// <param name="index">Index of element.</param>
		/// <returns>Element.</returns>
		public T this[int index] 
		{
			get => this.list[index];
			set
			{
				var collectionChangedHandlers = this.CollectionChanged;
				if (collectionChangedHandlers != null)
				{
					var oldValue = this.list[index];
					this.list[index] = value;
					if (!(oldValue?.Equals(value) ?? ReferenceEquals(value, null)))
						collectionChangedHandlers(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, index));
				}
				else
					this.list[index] = value;
			}
		}


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


		// Convert elements to IList.
		static IList ToIList(IEnumerable<T> elements)
		{
			if (elements is IList list)
				return list;
			return elements.ToArray();
		}


		/// <summary>
		/// Set the capacity to the actual number of elements in list.
		/// </summary>
		public void TrimExcess() => this.list.TrimExcess();


		// Interface implementations.
		void ICollection<T>.Add(T value) => this.Add(value);
		void ICollection.CopyTo(Array array, int index)
		{
			if (array is T[] targetArray)
				this.list.CopyTo(targetArray, index);
			else
				((ICollection)this.list).CopyTo(array, index);
		}
		bool ICollection<T>.IsReadOnly => false;
		bool ICollection.IsSynchronized => false;
		object ICollection.SyncRoot => this;
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
#pragma warning disable CS8604
#pragma warning disable CS8600
		int IList.Add(object? value) => this.Add((T)value);
#pragma warning restore CS8604
#pragma warning restore CS8600
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
#pragma warning disable CS8604
#pragma warning disable CS8600
		void IList.Insert(int index, object? value) => this.Insert(index, (T)value);
#pragma warning restore CS8604
#pragma warning restore CS8600
		bool IList.IsFixedSize => false;
		bool IList.IsReadOnly => false;
#pragma warning disable CS8601
#pragma warning disable CS8600
		object? IList.this[int index] 
		{ 
			get => this.list[index];
			set => this.list[index] = (T)value;
		}
#pragma warning restore CS8601
#pragma warning restore CS8600
		void IList.Remove(object? value)
		{
			if (value is T element)
				this.Remove(element);
		}
	}
}