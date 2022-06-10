using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CarinaStudio.Collections
{
    /// <summary>
    /// Tests of <see cref="FilteredObservableList{T}"/>
    /// </summary>
    [TestFixture]
    public class FilteredObservableListTests
    {
        /// <summary>
        /// Test for adding items.
        /// </summary>
        [Test]
        public void AddingTest()
        {
            // setup
            var eventArgs = (NotifyCollectionChangedEventArgs?)null;
            var source = new ObservableList<int>(new int[] { 0, 1, 2, 3, 100, 101, 102, 103 });
            var filteredList = new FilteredObservableList<int>(source, Filter);
            filteredList.CollectionChanged += (s, e) => 
            {
                if (s == filteredList)
                    eventArgs = e;
            };
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102 }));

            // add items to middle of list
            source.InsertRange(4, new int[] { 10, 11, 12, 13 });
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 10, 12, 100, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 10, 12 }));
            eventArgs = null;

            // add items to head of list
            source.InsertRange(0, new int[] { -13, -12, -11, -10 });
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { -12, -10, 0, 2, 10, 12, 100, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { -12, -10 }));
            eventArgs = null;

            // add items to tail of list
            source.InsertRange(source.Count, new int[] { 200, 201, 202, 203 });
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { -12, -10, 0, 2, 10, 12, 100, 102, 200, 202 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(8, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 200, 202 }));
            eventArgs = null;

            // add items which cannot match the condition
            source.AddRange(new int[] { 21, 23, 25, 27 });
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { -12, -10, 0, 2, 10, 12, 100, 102, 200, 202 }));
            Assert.IsNull(eventArgs);

            // setup
            source.Clear();
            source.AddRange(new int[] { 0, 1, 100, 101 });
            filteredList = new FilteredObservableList<int>(source);
            filteredList.CollectionChanged += (s, e) => 
            {
                if (s == filteredList)
                    eventArgs = e;
            };
            Assert.IsTrue(filteredList.SequenceEqual(source));

            // add items to middle of list
            source.InsertRange(2, new int[] { 10, 11 });
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 10, 11 }));
            eventArgs = null;

            // add items to head of list
            source.InsertRange(0, new int[] { -11, -10 });
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { -11, -10 }));
            eventArgs = null;

            // add items to tail of list
            source.AddRange(new int[] { 200, 201 });
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(8, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 200, 201 }));
            eventArgs = null;
        }


        /// <summary>
        /// Test for creating list.
        /// </summary>
        [Test]
        public void CreationTest()
        {
            var source = new int[] { 0, 1, 2, 3, 4, 5 };
            var list1 = new FilteredObservableList<int>(source);
            var list2 = new FilteredObservableList<int>(source, Filter);
            var list3 = new FilteredObservableList<int>(source, it => false);
            Assert.IsTrue(list1.SequenceEqual(source));
            Assert.IsTrue(list2.SequenceEqual(new int[] { 0, 2, 4 }));
            Assert.IsTrue(list3.IsEmpty());
        }


        // Filter function.
        static bool Filter(int n) =>
            (n & 1) == 0;
        

        /// <summary>
        /// Test for moving items.
        /// </summary>
        [Test]
        public void MovingTest()
        {
            // setup
            var eventArgs = (NotifyCollectionChangedEventArgs?)null;
            var source = new ObservableList<int>(new int[] { 0, 1, 2, 3 });
            var filteredList = new FilteredObservableList<int>(source, Filter);
            filteredList.CollectionChanged += (s, e) => 
            {
                if (s == filteredList)
                    eventArgs = e;
            };
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2 }));

            // move items
            source.Move(0, 3); // { 1, 2, 3, 0 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 2, 0 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.OldStartingIndex);
            Assert.AreEqual(1, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;
            source.Move(3, 0); // { 0, 1, 2, 3 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(1, eventArgs!.OldStartingIndex);
            Assert.AreEqual(0, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;

            source.Move(0, 2); // { 1, 2, 0, 3 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 2, 0 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.OldStartingIndex);
            Assert.AreEqual(1, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;
            source.Move(2, 0); // { 0, 1, 2, 3 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(1, eventArgs!.OldStartingIndex);
            Assert.AreEqual(0, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;

            source.Move(1, 3); // { 0, 2, 3, 1 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2 }));
            Assert.IsNull(eventArgs);
            source.Move(3, 1); // { 0, 1, 2, 3 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2 }));
            Assert.IsNull(eventArgs);

            // setup
            filteredList = new FilteredObservableList<int>(source);
            filteredList.CollectionChanged += (s, e) => 
            {
                if (s == filteredList)
                    eventArgs = e;
            };
            Assert.IsTrue(filteredList.SequenceEqual(source));

            // move items
            source.Move(0, 3); // { 1, 2, 3, 0 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.OldStartingIndex);
            Assert.AreEqual(3, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;
            source.Move(3, 0); // { 0, 1, 2, 3 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(3, eventArgs!.OldStartingIndex);
            Assert.AreEqual(0, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;

            source.Move(1, 2); // { 0, 2, 1, 3 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(1, eventArgs!.OldStartingIndex);
            Assert.AreEqual(2, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 1 }));
            eventArgs = null;
            source.Move(2, 1); // { 0, 1, 2, 3 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Move, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.OldStartingIndex);
            Assert.AreEqual(1, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 1 }));
            eventArgs = null;

            // move range of items
            source.MoveRange(0, 2, 2); // { 2, 3, 0, 1 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            eventArgs = null;
        }
        

        /// <summary>
        /// Test for removing items.
        /// </summary>
        [Test]
        public void RemovingTest()
        {
            // setup
            var eventArgs = (NotifyCollectionChangedEventArgs?)null;
            var source = new ObservableList<int>(new int[] { 0, 1, 2, 3, 100, 101, 102, 103 });
            var filteredList = new FilteredObservableList<int>(source, Filter);
            filteredList.CollectionChanged += (s, e) => 
            {
                if (s == filteredList)
                    eventArgs = e;
            };
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102 }));

            // remove items from middle of list
            source.RemoveRange(3, 2); // { 0, 1, 2, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 100 }));
            eventArgs = null;

            // remove items from head of list
            source.RemoveRange(0, 2); // { 2, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 2, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;

            // remove items from tail of list
            source.RemoveRange(2, 2); // { 2, 101 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 2 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(1, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 102 }));
            eventArgs = null;

            // remove items which cannot match the condition
            source.RemoveAt(1);
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 2 }));
            Assert.IsNull(eventArgs);

            // clear
            source.Clear();
            Assert.IsTrue(filteredList.IsEmpty());
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, eventArgs!.Action);
            eventArgs = null;

            // setup
            source.AddRange(new int[] { 0, 1, 2, 3, 100, 101, 102, 103 });
            filteredList = new FilteredObservableList<int>(source);
            filteredList.CollectionChanged += (s, e) => 
            {
                if (s == filteredList)
                    eventArgs = e;
            };
            Assert.IsTrue(filteredList.SequenceEqual(source));

            // remove items from middle of list
            source.RemoveRange(3, 2); // { 0, 1, 2, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(3, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 3, 100 }));
            eventArgs = null;

            // remove items from head of list
            source.RemoveRange(0, 2); // { 2, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0, 1 }));
            eventArgs = null;

            // remove items from tail of list
            source.RemoveRange(2, 2); // { 2, 101 }
            Assert.IsTrue(filteredList.SequenceEqual(source));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 102, 103 }));
            eventArgs = null;

            // clear
            source.Clear();
            Assert.IsTrue(filteredList.IsEmpty());
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, eventArgs!.Action);
            eventArgs = null;
        }


        /// <summary>
        /// Test for replacing items.
        /// </summary>
        [Test]
        public void ReplacingTest()
        {
            // setup
            var eventArgs = (NotifyCollectionChangedEventArgs?)null;
            var source = new ObservableList<int>(new int[] { 0, 1, 2, 3, 100, 101, 102, 103 });
            var filteredList = new FilteredObservableList<int>(source, Filter);
            filteredList.CollectionChanged += (s, e) => 
            {
                if (s == filteredList)
                    eventArgs = e;
            };
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102 }));

            // replace item at middle of list
            source[4] = 98; // { 0, 1, 2, 3, 98, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 98, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 100 }));
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 98 }));
            eventArgs = null;
            source[4] = 99; // { 0, 1, 2, 3, 99, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 98 }));
            eventArgs = null;
            source[4] = 101; // { 0, 1, 2, 3, 101, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 102 }));
            Assert.IsNull(eventArgs);
            eventArgs = null;
            source[4] = 100; // { 0, 1, 2, 3, 100, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(2, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 100 }));
            eventArgs = null;

            // replace item at head of list
            source[0] = -2; // { -2, 1, 2, 3, 100, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { -2, 2, 100, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { -2 }));
            eventArgs = null;
            source[0] = -1; // { -1, 1, 2, 3, 100, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 2, 100, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { -2 }));
            eventArgs = null;
            source[0] = -3; // { -3, 1, 2, 3, 100, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 2, 100, 102 }));
            Assert.IsNull(eventArgs);
            source[0] = 0; // { 0, 1, 2, 3, 100, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(0, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 0 }));
            eventArgs = null;

            // replace item at tail of list
            source[7] = 105; // { 0, 1, 2, 3, 100, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102 }));
            Assert.IsNull(eventArgs);
            source[7] = 104; // { 0, 1, 2, 3, 100, 101, 102, 104 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102, 104 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Add, eventArgs!.Action);
            Assert.AreEqual(4, eventArgs!.NewStartingIndex);
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 104 }));
            eventArgs = null;
            source[7] = 106; // { 0, 1, 2, 3, 100, 101, 102, 106 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102, 106 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Replace, eventArgs!.Action);
            Assert.AreEqual(4, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 104 }));
            Assert.IsTrue(eventArgs.NewItems!.Cast<int>().SequenceEqual(new int[] { 106 }));
            eventArgs = null;
            source[7] = 103; // { 0, 1, 2, 3, 100, 101, 102, 103 }
            Assert.IsTrue(filteredList.SequenceEqual(new int[] { 0, 2, 100, 102 }));
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, eventArgs!.Action);
            Assert.AreEqual(4, eventArgs!.OldStartingIndex);
            Assert.IsTrue(eventArgs.OldItems!.Cast<int>().SequenceEqual(new int[] { 106 }));
            eventArgs = null;
        }
    }
}