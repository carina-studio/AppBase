using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CarinaStudio.Collections;

class ReadOnlyReversedObservableList<T> : ReadOnlyReversedList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    // Constructor.
    public ReadOnlyReversedObservableList(IReadOnlyList<T> list) : base(list)
    {
        ((INotifyCollectionChanged)list).CollectionChanged += (_, e) =>
        {
            // raise CollectionChanged
            var collectionChanged = this.CollectionChanged;
            if (collectionChanged is not null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        e.NewItems?.Let(newItems =>
                        {
                            var itemCount = newItems.Count;
                            if (itemCount == 1)
                                collectionChanged(this, new(NotifyCollectionChangedAction.Add, newItems[0], this.Count - 1 - e.NewStartingIndex));
                            else
                            {
                                var reversedNewItems = new T[itemCount];
                                for (var i = itemCount - 1; i >= 0; --i)
                                    reversedNewItems[itemCount - i - 1] = (T)newItems[i]!;
                                collectionChanged(this, new(NotifyCollectionChangedAction.Add, reversedNewItems, this.Count - itemCount - e.NewStartingIndex));
                            }
                        });
                        break;
                    case NotifyCollectionChangedAction.Move:
                        e.OldItems?.Let(movedItems =>
                        {
                            var itemCount = movedItems.Count;
                            if (itemCount == 1)
                                collectionChanged(this, new(NotifyCollectionChangedAction.Move, movedItems[0], this.Count - e.NewStartingIndex - 1, this.Count - e.OldStartingIndex - 1));
                            else
                            {
                                var reversedMovedItems = new T[itemCount];
                                for (var i = itemCount - 1; i >= 0; --i)
                                    reversedMovedItems[itemCount - i - 1] = (T)movedItems[i]!;
                                collectionChanged(this, new(NotifyCollectionChangedAction.Move, reversedMovedItems, this.Count - e.NewStartingIndex - 1, this.Count - e.OldStartingIndex - 1));
                            }
                        });
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        e.OldItems?.Let(oldItems =>
                        {
                            var itemCount = oldItems.Count;
                            if (itemCount == 1)
                                collectionChanged(this, new(NotifyCollectionChangedAction.Remove, oldItems[0], this.Count - e.OldStartingIndex));
                            else
                            {
                                var reversedOldItems = new T[itemCount];
                                for (var i = itemCount - 1; i >= 0; --i)
                                    reversedOldItems[itemCount - i - 1] = (T)oldItems[i]!;
                                collectionChanged(this, new(NotifyCollectionChangedAction.Remove, reversedOldItems, this.Count + itemCount - e.OldStartingIndex - 1));
                            }
                        });
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        e.OldItems?.Let(oldItems =>
                        {
                            e.NewItems?.Let(newItems =>
                            {
                                var itemCount = oldItems.Count;
                                if (itemCount == 1)
                                    collectionChanged(this, new(NotifyCollectionChangedAction.Replace, newItems[0], oldItems[0], this.Count - e.OldStartingIndex - 1));
                                else
                                {
                                    var reversedNewItems = new T[itemCount];
                                    for (var i = itemCount - 1; i >= 0; --i)
                                        reversedNewItems[itemCount - i - 1] = (T)newItems[i]!;
                                    var reversedOldItems = new T[itemCount];
                                    for (var i = itemCount - 1; i >= 0; --i)
                                        reversedOldItems[itemCount - i - 1] = (T)oldItems[i]!;
                                    collectionChanged(this, new(NotifyCollectionChangedAction.Replace, reversedNewItems, reversedOldItems, this.Count - e.OldStartingIndex - 1));
                                }
                            });
                        });
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        collectionChanged(this, e);
                        break;
                }
            }
            
            // raise Count property changed
            this.PropertyChanged?.Invoke(this, new(nameof(Count)));
        };
    }

    
    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;


    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
}

class ReversedObservableList<T> : ReversedList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    // Constructor.
    public ReversedObservableList(IList<T> list, bool isReadOnly) : base(list, isReadOnly)
    {
        ((INotifyCollectionChanged)list).CollectionChanged += (_, e) =>
        {
            // raise CollectionChanged
            var collectionChanged = this.CollectionChanged;
            if (collectionChanged is not null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        e.NewItems?.Let(newItems =>
                        {
                            var itemCount = newItems.Count;
                            if (itemCount == 1)
                                collectionChanged(this, new(NotifyCollectionChangedAction.Add, newItems[0], this.Count - 1 - e.NewStartingIndex));
                            else
                            {
                                var reversedNewItems = new T[itemCount];
                                for (var i = itemCount - 1; i >= 0; --i)
                                    reversedNewItems[itemCount - i - 1] = (T)newItems[i]!;
                                collectionChanged(this, new(NotifyCollectionChangedAction.Add, reversedNewItems, this.Count - itemCount - e.NewStartingIndex));
                            }
                        });
                        break;
                    case NotifyCollectionChangedAction.Move:
                        e.OldItems?.Let(movedItems =>
                        {
                            var itemCount = movedItems.Count;
                            if (itemCount == 1)
                                collectionChanged(this, new(NotifyCollectionChangedAction.Move, movedItems[0], this.Count - e.NewStartingIndex - 1, this.Count - e.OldStartingIndex - 1));
                            else
                            {
                                var reversedMovedItems = new T[itemCount];
                                for (var i = itemCount - 1; i >= 0; --i)
                                    reversedMovedItems[itemCount - i - 1] = (T)movedItems[i]!;
                                collectionChanged(this, new(NotifyCollectionChangedAction.Move, reversedMovedItems, this.Count - e.NewStartingIndex - 1, this.Count - e.OldStartingIndex - 1));
                            }
                        });
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        e.OldItems?.Let(oldItems =>
                        {
                            var itemCount = oldItems.Count;
                            if (itemCount == 1)
                                collectionChanged(this, new(NotifyCollectionChangedAction.Remove, oldItems[0], this.Count - e.OldStartingIndex));
                            else
                            {
                                var reversedOldItems = new T[itemCount];
                                for (var i = itemCount - 1; i >= 0; --i)
                                    reversedOldItems[itemCount - i - 1] = (T)oldItems[i]!;
                                collectionChanged(this, new(NotifyCollectionChangedAction.Remove, reversedOldItems, this.Count + itemCount - e.OldStartingIndex - 1));
                            }
                        });
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        e.OldItems?.Let(oldItems =>
                        {
                            e.NewItems?.Let(newItems =>
                            {
                                var itemCount = oldItems.Count;
                                if (itemCount == 1)
                                    collectionChanged(this, new(NotifyCollectionChangedAction.Replace, newItems[0], oldItems[0], this.Count - e.OldStartingIndex - 1));
                                else
                                {
                                    var reversedNewItems = new T[itemCount];
                                    for (var i = itemCount - 1; i >= 0; --i)
                                        reversedNewItems[itemCount - i - 1] = (T)newItems[i]!;
                                    var reversedOldItems = new T[itemCount];
                                    for (var i = itemCount - 1; i >= 0; --i)
                                        reversedOldItems[itemCount - i - 1] = (T)oldItems[i]!;
                                    collectionChanged(this, new(NotifyCollectionChangedAction.Replace, reversedNewItems, reversedOldItems, this.Count - e.OldStartingIndex - 1));
                                }
                            });
                        });
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        collectionChanged(this, e);
                        break;
                }
            }
            
            // raise Count property changed
            this.PropertyChanged?.Invoke(this, new(nameof(Count)));
        };
    }

    
    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;


    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;
}