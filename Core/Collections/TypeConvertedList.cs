using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// <see cref="IList{T}"/> which wraps another <see cref="IList{T}"/> and convert elements to another type. Elements will be updated automatically if source <see cref="IList{T}"/> implements <see cref="INotifyCollectionChanged"/> interface.
	/// </summary>
	/// <typeparam name="TSrc">Type of element of source list.</typeparam>
	/// <typeparam name="TDest">Type of converted elements.</typeparam>
	public abstract class TypeConvertedList<TSrc, TDest> : BaseDisposable, IList, IList<TDest>, INotifyCollectionChanged, IReadOnlyList<TDest>
	{
		// Fields.
		readonly List<TDest> list = new List<TDest>();
		readonly IList<TSrc> sourceList;


		/// <summary>
		/// Initialize new <see cref="TypeConvertedList{TSrc, TDest}"/> instance.
		/// </summary>
		/// <param name="source">Source list.</param>
		protected TypeConvertedList(IList<TSrc> source)
		{
			this.sourceList = source;
			this.RebuildList();
			if (source is INotifyCollectionChanged notifyCollectionChanged)
				notifyCollectionChanged.CollectionChanged += this.OnSourceListChanged;
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
		public bool Contains(TDest element) => this.list.Contains(element);


		/// <summary>
		/// Convert source element to element in this list.
		/// </summary>
		/// <param name="srcElement">Element in source list.</param>
		/// <returns>Converted element.</returns>
		protected abstract TDest ConvertElement(TSrc srcElement);


		/// <summary>
		/// Copy elements to array.
		/// </summary>
		/// <param name="array">Array to place copied elements.</param>
		/// <param name="arrayIndex">Index of first position in <paramref name="array"/> to place copied elements.</param>
		public void CopyTo(TDest[] array, int arrayIndex) => this.list.CopyTo(array, arrayIndex);


		/// <summary>
		/// Get number of elements.
		/// </summary>
		public int Count { get => this.list.Count; }


		/// <summary>
		/// Dispose the list.
		/// </summary>
		/// <param name="disposing">True to release managed resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (this.sourceList is INotifyCollectionChanged notifyCollectionChanged)
				notifyCollectionChanged.CollectionChanged -= this.OnSourceListChanged;
		}


		/// <summary>
		/// Get <see cref="IEnumerator{T}"/> to enumerate elements.
		/// </summary>
		/// <returns><see cref="IEnumerator{T}"/>.</returns>
		public IEnumerator<TDest> GetEnumerator() => this.list.GetEnumerator();


		/// <summary>
		/// Get index of given element in list.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <returns>Index of element in list, or -1 if element is not contained in list.</returns>
		public int IndexOf(TDest element) => this.list.IndexOf(element);


		/// <summary>
		/// Release converted element.
		/// </summary>
		/// <param name="element">Converted element.</param>
		protected virtual void ReleaseElement(TDest element)
		{ }


#pragma warning disable CS8604
		// Called when source list changed.
		void OnSourceListChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					{
						var newSourceElements = e.NewItems;
						if (newSourceElements.Count == 1)
						{
							var newElement = this.ConvertElement((TSrc)newSourceElements[0]);
							this.list.Insert(e.NewStartingIndex, newElement);
							this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newElement, e.NewStartingIndex));
						}
						else
						{
							var newElements = new TDest[newSourceElements.Count];
							for (var i = newElements.Length - 1; i >= 0; --i)
								newElements[i] = this.ConvertElement((TSrc)newSourceElements[i]);
							this.list.InsertRange(e.NewStartingIndex, newElements);
							this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newElements, e.NewStartingIndex));
						}
					}
					break;
				case NotifyCollectionChangedAction.Move:
					{
						var oldIndex = e.OldStartingIndex;
						var newIndex = e.NewStartingIndex;
						var count = e.OldItems.Count;
						var list = this.list;
						var movedElements = list.ToArray(oldIndex, count);
						list.RemoveRange(oldIndex, count);
						list.InsertRange(newIndex, movedElements);
						this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, movedElements, newIndex, oldIndex));
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					{
						var index = e.OldStartingIndex;
						var count = e.OldItems.Count;
						var list = this.list;
						if (this.CollectionChanged == null)
						{
							for (var i = count - 1; i >= 0; --i)
								this.ReleaseElement(list[index + i]);
							list.RemoveRange(index, count);
						}
						else
						{
							var removedElements = list.ToArray(index, count);
							for (var i = count - 1; i >= 0; --i)
								this.ReleaseElement(removedElements[i]);
							list.RemoveRange(index, count);
							this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedElements, index));
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					{
						var index = e.OldStartingIndex;
						var count = e.OldItems.Count;
						var sourceList = this.sourceList;
						var list = this.list;
						if (this.CollectionChanged == null)
						{
							for (var i = count - 1; i >= 0; --i)
							{
								this.ReleaseElement(list[index + i]);
								list[index + i] = this.ConvertElement(sourceList[index + i]);
							}
						}
						else
						{
							var replacedElements = list.ToArray(index, count);
							var newElements = new TDest[count];
							for (var i = count - 1; i >= 0; --i)
							{
								this.ReleaseElement(replacedElements[i]);
								newElements[i] = this.ConvertElement(sourceList[index + i]);
								list[index + i] = newElements[i];
							}
							this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newElements, replacedElements, index));
						}
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					this.RebuildList();
					break;
				default:
					throw new NotSupportedException($"Unsupported type of collection change: {e.Action}.");
			}
		}
#pragma warning restore CS8604


		// Rebuild list.
		void RebuildList()
		{
			// clear elements
			var list = this.list;
			foreach (var element in list)
				this.ReleaseElement(element);
			list.Clear();

			// convert elements
			var sourceList = this.sourceList;
			for (int i = 0, count = sourceList.Count; i < count; ++i)
				list.Add(this.ConvertElement(sourceList[i]));
		}


		/// <summary>
		/// Get element.
		/// </summary>
		/// <param name="index">Index of element.</param>
		/// <returns>Element.</returns>
		public TDest this[int index] { get => this.list[index]; }


		/// <summary>
		/// Set the capacity to the actual number of elements in list.
		/// </summary>
		public void TrimExcess() => this.list.TrimExcess();


		// Interface implementations.
		void ICollection<TDest>.Add(TDest value) => throw new InvalidOperationException();
		void ICollection<TDest>.Clear() => throw new InvalidOperationException();
		void ICollection.CopyTo(Array array, int index) => this.CopyTo((TDest[])array, index);
		bool ICollection<TDest>.IsReadOnly => true;
		bool ICollection.IsSynchronized => false;
		bool ICollection<TDest>.Remove(TDest item) => throw new InvalidOperationException();
		object ICollection.SyncRoot => this;
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		int IList.Add(object? value) => throw new InvalidOperationException();
		void IList.Clear() => throw new InvalidOperationException();
		bool IList.Contains(object? value)
		{
			if (value is TDest element)
				return this.Contains(element);
			return false;
		}
		int IList.IndexOf(object? value)
		{
			if (value is TDest element)
				return this.IndexOf(element);
			return -1;
		}
		void IList.Insert(int index, object? value) => throw new InvalidOperationException();
		bool IList.IsFixedSize => false;
		bool IList.IsReadOnly => false;
		object? IList.this[int index] { get => this.list[index]; set => throw new InvalidOperationException(); }
		TDest IList<TDest>.this[int index] { get => this.list[index]; set => throw new InvalidOperationException(); }
		void IList<TDest>.Insert(int index, TDest item) => throw new InvalidOperationException();
		void IList.Remove(object? value) => throw new InvalidOperationException();
		void IList.RemoveAt(int index) => throw new InvalidOperationException();
		void IList<TDest>.RemoveAt(int index) => throw new InvalidOperationException();
	}
}
