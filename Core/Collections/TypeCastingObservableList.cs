using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace CarinaStudio.Collections
{
    // Class to wrap IList as generic IList<T>.
    class TypeCastingObservableList<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged, IReadOnlyList<T>
    {
        // Fields.
        readonly IList list;

        
        // Constructor.
        public TypeCastingObservableList(IList list)
        {
            this.list = list;
            var notifyPropertyChanged = (list as INotifyPropertyChanged);
            if (list is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += (_, e) => 
                {
                    this.CollectionChanged?.Invoke(this, e);
                    if (notifyPropertyChanged == null)
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                };
            }
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += (_, e) => 
                {
                    if (e.PropertyName == nameof(Count))
                        this.PropertyChanged?.Invoke(this, e);
                };
            }
        }
        

        /// <inheritdoc/>
        public void Add(T element) => 
            this.list.Add(element);

        
        /// <inheritdoc/>
        public void Clear() => 
            this.list.Clear();
        

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;


        /// <inheritdoc/>
        public bool Contains(T element) =>
            this.list.Contains(element);
        

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) =>
            this.list.CopyTo(array, arrayIndex);
        

        /// <inheritdoc/>
        public int Count { get => this.list.Count; }


        /// <inheritdoc/>
        public int IndexOf(T element) =>
            this.list.IndexOf(element);


        /// <inheritdoc/>
        public void Insert(int index, T element) =>
            this.list.Insert(index, element);
        

        /// <inheritdoc/>
        public bool IsReadOnly { get => this.list.IsReadOnly; }


        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() =>
            ((IEnumerable)this.list).Cast<T>().GetEnumerator();
        

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;


        /// <inheritdoc/>
        public bool Remove(T element)
        {
            var index = this.list.IndexOf(element);
            if (index >= 0)
            {
                this.list.RemoveAt(index);
                return true;
            }
            return false;
        }


        /// <inheritdoc/>
        public void RemoveAt(int index) =>
            this.list.RemoveAt(index);


#pragma warning disable CS8600
#pragma warning disable CS8603
        /// <inheritdoc/>
        public T this[int index]
        {
            get => (T)this.list[index];
            set => this.list[index] = value;
        }
#pragma warning restore CS8600
#pragma warning restore CS8603


        // Interface implementations.
        IEnumerator IEnumerable.GetEnumerator() =>
            this.list.GetEnumerator();
    }
}