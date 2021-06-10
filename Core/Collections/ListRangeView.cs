using System;
using System.Collections;
using System.Collections.Generic;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// View of sub range of list.
	/// </summary>
	class ListRangeView<T>: IList, IList<T>
	{
		// Enumerator.
		class Enumerator : IEnumerator<T>
		{
			// Fields.
			int index;
			ListRangeView<T>? view;

			// Constructor.
			public Enumerator(ListRangeView<T> view)
			{
				this.view = view;
				this.index = view.startIndex - 1;
			}

			// Implementations.
			public T Current
			{
				get
				{
					if (this.view == null)
						throw new InvalidOperationException();
					return this.view.list[this.index];
				}
			}
			object? IEnumerator.Current => this.Current;
			public void Dispose()
			{
				this.view = null;
			}
			public bool MoveNext()
			{
				if (this.view == null)
					throw new InvalidOperationException();
				if (this.index >= this.view.endIndex)
					return false;
				++this.index;
				return true;
			}
			public void Reset()
			{
				if (this.view == null)
					throw new InvalidOperationException();
				this.index = this.view.startIndex - 1;
			}
		}


		// Fields.
		readonly int endIndex;
		readonly IList<T> list;
		readonly int startIndex;


		// Constructor.
		public ListRangeView(IList<T> list, int index, int count)
		{
			this.list = list;
			this.startIndex = index;
			this.endIndex = index + count;
			this.Count = count;
		}


		// Check element.
		public bool Contains(T element)
		{
			var index = this.list.IndexOf(element);
			return index >= this.startIndex && index < this.endIndex;
		}


		// Copy to.
		public void CopyTo(T[] array, int arrayIndex)
		{
			var list = this.list;
			for (var i = this.startIndex; i < this.endIndex; ++i)
				array[arrayIndex++] = list[i];
		}


		// Number of elements.
		public int Count { get; }


		// Get index of element.
		public int IndexOf(T element)
		{
			var index = this.list.IndexOf(element);
			if (index >= this.startIndex && index < this.endIndex)
				return (index - this.startIndex);
			return -1;
		}


		// Get enumerator.
		public IEnumerator<T> GetEnumerator() => new Enumerator(this);


		// Get element.
		public T this[int index]
		{
			get
			{
				index += this.startIndex;
				if (index < this.startIndex || index >= this.endIndex)
					throw new ArgumentOutOfRangeException();
				return this.list[index];
			}
		}


		// Implementations.
		void ICollection<T>.Add(T item) => throw new InvalidOperationException();
		void ICollection<T>.Clear() => throw new InvalidOperationException();
		void ICollection.CopyTo(Array array, int index) => this.CopyTo((T[])array, index);
		bool ICollection<T>.IsReadOnly => true;
		bool ICollection.IsSynchronized => false;
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
		void IList<T>.Insert(int index, T item) => throw new InvalidOperationException();
		bool IList.IsFixedSize => true;
		bool IList.IsReadOnly => true;
		void IList.Remove(object? value) => throw new InvalidOperationException();
		void IList.RemoveAt(int index) => throw new InvalidOperationException();
		void IList<T>.RemoveAt(int index) => throw new InvalidOperationException();
		T IList<T>.this[int index] { get => this[index]; set => throw new InvalidOperationException(); }
		object? IList.this[int index] { get => this[index]; set => throw new InvalidOperationException(); }
	}
}
