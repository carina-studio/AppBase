using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace CarinaStudio.Collections
{
    /// <summary>
    /// Implementation of <see cref="IList{T}"/> which filters elements from source <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    public class FilteredObservableList<T> : IList, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyList<T>
    {
        // Fields.
        Predicate<T>? filter;
        readonly List<(T, int)> items;
        readonly IList<T> source;


        /// <summary>
        /// Initialize new <see cref="FilteredObservableList{T}"/> instance.
        /// </summary>
        /// <param name="source">Source list.</param>
        /// <param name="filter">Filter function.</param>
        public FilteredObservableList(IList<T> source, Predicate<T>? filter = null)
        {
            var sourceCount = source.Count;
            this.filter = filter;
            this.items = (filter == null || sourceCount <= 1024) ? new List<(T, int)>(sourceCount) : new List<(T, int)>();
            this.source = source;
            this.Rebuild();
            if (source is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.AddWeakCollectionChangedEventHandler(this.OnSourceChanged);
        }


        /// <summary>
        /// Raised when collection changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;


        /// <inheritdoc/>
        public bool Contains(T item) =>
            this.filter?.Invoke(item) != false && this.source.Contains(item);

        
        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            var items = this.items;
            var count = items.Count;
            if (count == 0)
                return;
            if (arrayIndex + count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            for (var i = 0; i < count ; ++i, ++arrayIndex)
                array[arrayIndex] = items[i].Item1;
        }


        /// <inheritdoc cref="ICollection{T}.Count"/>
        public int Count => this.items.Count;


        /// <summary>
        /// Get or set filter function.
        /// </summary>
        public Predicate<T>? Filter
        {
            get => this.filter;
            set
            {
                if (this.filter == value)
                    return;
                this.filter = value;
                this.Rebuild();
            }
        }


        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() =>
            this.items.Select(it => it.Item1).GetEnumerator();


        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            var index = this.source.IndexOf(item);
            if (index >= 0)
                return this.items.BinarySearch(index, it => it.Item2);
            return -1;
        }


        /// <inheritdoc cref="ICollection{T}.IsReadOnly"/>
        public bool IsReadOnly => true;


        // Called when source collection changed.
        void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var filter = this.filter;
            var items = this.items;
            var source = this.source;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var startIndex = e.NewStartingIndex;
                        var count = e.NewItems!.Count;
                        if (filter != null)
                        {
                            var insertionIndex = items.BinarySearch(startIndex, it => it.Item2);
                            if (insertionIndex < 0)
                                insertionIndex = ~insertionIndex;
                            var collectionChangedHandler = this.CollectionChanged;
                            var newItems = new List<(T, int)>(Math.Min(1024, count));
                            var filteredNewSourceItems = collectionChangedHandler != null ? new List<T>(newItems.Capacity) : null;
                            for (var i = 0; i < count; ++i)
                            {
                                var item = source[startIndex + i];
                                if (filter(item))
                                {
                                    filteredNewSourceItems?.Add(item);
                                    newItems.Add((item, startIndex + i));
                                }
                            }
                            if (newItems.IsEmpty())
                                break;
                            items.InsertRange(insertionIndex, newItems);
                            var itemCount = items.Count;
                            for (var i = insertionIndex + newItems.Count; i < itemCount; ++i)
                            {
                                var (item, index) = items[i];
                                items[i] = (item, index + count);
                            }
                            collectionChangedHandler?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, filteredNewSourceItems, insertionIndex));
                        }
                        else
                        {
                            var newItems = new (T, int)[count];
                            for (var i = 0; i < count; ++i)
                                newItems[i] = (source[startIndex + i], startIndex + i);
                            items.InsertRange(startIndex, newItems);
                            var itemCount = items.Count;
                            for (var i = startIndex + count; i < itemCount; ++i)
                                items[i] = (items[i].Item1, i);
                            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, startIndex));
                        }
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        var startIndex = e.OldStartingIndex;
                        var count = e.OldItems!.Count;
                        if (filter != null)
                        {
                            var removingStartIndex = items.BinarySearch(startIndex, it => it.Item2);
                            var removingEndIndex = items.BinarySearch(startIndex + count, it => it.Item2);
                            if (removingStartIndex < 0)
                                removingStartIndex = ~removingStartIndex;
                            if (removingEndIndex < 0)
                                removingEndIndex = ~removingEndIndex;
                            var removingCount = removingEndIndex - removingStartIndex;
                            if (removingCount <= 0)
                                break;
                            var itemCount = items.Count;
                            for (var i = removingEndIndex; i < itemCount; ++i)
                            {
                                var (item, index) = items[i];
                                items[i] = (item, index - count);
                            }
                            if (this.CollectionChanged != null)
                            {
                                var removedItems = this.items.GetRangeView(removingStartIndex, removingCount).Select(it => it.Item1).ToList();
                                items.RemoveRange(removingStartIndex, removingCount);
                                this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, removingStartIndex));
                            }
                            else
                                items.RemoveRange(removingStartIndex, removingCount);
                        }
                        else
                        {
                            items.RemoveRange(startIndex, count);
                            var itemCount = items.Count;
                            for (var i = startIndex; i < itemCount; ++i)
                                items[i] = (items[i].Item1, i);
                            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, startIndex));
                        }
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems!.Count == 1)
                    {
                        var sourceIndex = e.NewStartingIndex;
                        var sourceItem = source[sourceIndex];
                        var itemIndex = items.BinarySearch(sourceIndex, it => it.Item2);
                        if (filter?.Invoke(sourceItem) != false)
                        {
                            if (itemIndex >= 0)
                            {
                                var oldItem = this.items[itemIndex].Item1;
                                items[itemIndex] = (sourceItem, sourceIndex);
                                this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sourceItem, oldItem, itemIndex));
                            }
                            else
                            {
                                itemIndex = ~itemIndex;
                                items.Insert(itemIndex, (sourceItem, sourceIndex));
                                this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, sourceItem, itemIndex));
                            }
                        }
                        else if (itemIndex >= 0)
                        {
                            var oldItem = this.items[itemIndex].Item1;
                            items.RemoveAt(itemIndex);
                            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, itemIndex));
                        }
                    }
                    else
                        this.Rebuild();
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.NewItems!.Count == 1)
                    {
                        var oldSourceIndex = e.OldStartingIndex;
                        var newSourceIndex = e.NewStartingIndex;
                        var sourceItem = source[newSourceIndex];
                        if (filter == null)
                        {
                            items.RemoveAt(oldSourceIndex);
                            items.Insert(newSourceIndex, (sourceItem, newSourceIndex));
                            if (oldSourceIndex < newSourceIndex)
                            {
                                for (var i = oldSourceIndex; i < newSourceIndex; ++i)
                                {
                                    var (item, index) = items[i];
                                    items[i] = (item, index - 1);
                                }
                            }
                            else
                            {
                                for (var i = newSourceIndex + 1; i <= oldSourceIndex; ++i)
                                {
                                    var (item, index) = items[i];
                                    items[i] = (item, index + 1);
                                }
                            }
                            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, sourceItem, newSourceIndex, oldSourceIndex));
                        }
                        else if (filter(sourceItem))
                        {
                            var oldItemIndex = items.BinarySearch(oldSourceIndex, it => it.Item2);
                            if (oldItemIndex < 0)
                                break;
                            items.RemoveAt(oldItemIndex);
                            for (var i = items.Count - 1; i >= oldItemIndex; --i)
                            {
                                var (item, index) = items[i];
                                items[i] = (item, index - 1);
                            }
                            var newItemIndex = items.BinarySearch(newSourceIndex, it => it.Item2);
                            if (newItemIndex < 0)
                                newItemIndex = ~newItemIndex;
                            items.Insert(newItemIndex, (sourceItem, newSourceIndex));
                            for (var i = items.Count - 1; i > newItemIndex; --i)
                            {
                                var (item, index) = items[i];
                                items[i] = (item, index + 1);
                            }
                            this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, sourceItem, newItemIndex, oldItemIndex));
                        }
                    }
                    else
                        this.Rebuild();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Rebuild();
                    break;
            }
        }


        /// <summary>
        /// Raised when property changed.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;


        // Rebuild list.
        void Rebuild()
        {
            var prevCount = this.items.Count;
            var source = this.source;
            var sourceCount = source.Count;
            var filter = this.filter;
            var items = this.items;
            if (items.IsNotEmpty())
            {
                items.Clear();
                this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            if (filter == null)
            {
                for (var i = 0; i < sourceCount; ++i)
                    items.Add((source[i], i));
            }
            else
            {
                for (var i = 0; i < sourceCount; ++i)
                {
                    var item = source[i];
                    if (filter(item))
                        items.Add((item, i));
                }
            }
            if (items.IsNotEmpty())
                this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.items, 0));
            if (prevCount != items.Count)
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }


        /// <inheritdoc/>
        public T this[int index] => this.items[index].Item1;


        // Interface implementations.
        int IList.Add(object? value) => throw new InvalidOperationException();
        void ICollection<T>.Add(T item) => throw new InvalidOperationException();
        void IList.Clear() => throw new InvalidOperationException();
        void ICollection<T>.Clear() => throw new InvalidOperationException();
        bool IList.Contains(object? value) => value is T item && this.Contains(item);
        void ICollection.CopyTo(Array array, int index)
        {
            if (array is T[] targetArray)
				this.CopyTo(targetArray, index);
            else if (array.GetType().GetElementType()!.IsAssignableFrom(typeof(T)))
			{
				var count = this.items.Count;
				if (count == 0)
					return;
				if (index < 0 || (index + count) > array.Length)
					throw new ArgumentOutOfRangeException(nameof(index));
				for (var i = 0; i < count; ++i)
					array.SetValue(this.items[i].Item1, index++);
			}
			else
				throw new ArgumentException($"Type of array element {array.GetType().GetElementType()!.Name} is not parent type of {typeof(T).Name}.");
        }
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        int IList.IndexOf(object? value) => value is T item ? this.IndexOf(item) : -1;
        void IList.Insert(int index, object? value) => throw new InvalidOperationException();
        void IList<T>.Insert(int index, T item) => throw new InvalidOperationException();
        bool IList.IsFixedSize => false;
        bool ICollection.IsSynchronized => false;
        bool ICollection<T>.Remove(T item) => throw new InvalidOperationException();
        void IList.Remove(object? value) => throw new InvalidOperationException();
        void IList.RemoveAt(int index) => throw new InvalidOperationException();
        void IList<T>.RemoveAt(int index) => throw new InvalidOperationException();
        object ICollection.SyncRoot => this;
        object? IList.this[int index] 
        { 
            get => this[index]; 
            set => throw new InvalidOperationException();
        }
        T IList<T>.this[int index] 
        { 
            get => this[index]; 
            set => throw new InvalidOperationException();
        }
    }
}