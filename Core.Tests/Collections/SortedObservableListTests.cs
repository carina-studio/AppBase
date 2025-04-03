using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Test of <see cref="SortedObservableList{T}"/>.
	/// </summary>
	[TestFixture]
	class SortedObservableListTests
	{
		// Fields.
		readonly Random random = new();


		/// <summary>
		/// Test for adding elements.
		/// </summary>
		[Test]
		public void AddingTest()
		{
			// prepare
			var randomElements = this.GenerateRandomArray(10240);
			var sortedElements = ((int[])randomElements.Clone()).Also(Array.Sort);
			var planeElements = new int[randomElements.Length];
			var sortedList = new SortedObservableList<int>();
			var refList = new List<int>();

			// add element one-by-one
			foreach (var element in randomElements)
				sortedList.Add(element);
			refList.AddRange(randomElements);
			refList.Sort();
			this.VerifySortedList(sortedList, refList);

			// add elements by range
			sortedList.Clear();
			var startIndex = 0;
			var remaining = randomElements.Length;
			while (remaining > 0)
			{
				var count = Math.Min(remaining, this.random.Next(1, 100));
				var elements = new int[count].Also((it) => Array.Copy(randomElements, startIndex, it, 0, count));
				startIndex += count;
				remaining -= count;
				sortedList.AddAll(elements);
			}
			this.VerifySortedList(sortedList, refList);

			// add all elements
			sortedList.Clear();
			sortedList.AddAll(randomElements);
			this.VerifySortedList(sortedList, refList);

			// add element one-by-one (sorted)
			sortedList.Clear();
			refList.Clear();
			foreach (var element in sortedElements)
				sortedList.Add(element);
			refList.AddRange(sortedElements);
			this.VerifySortedList(sortedList, refList);

			// add elements by range (sorted)
			sortedList.Clear();
			startIndex = 0;
			remaining = sortedElements.Length;
			while (remaining > 0)
			{
				var count = Math.Min(remaining, this.random.Next(1, 100));
				var elements = new int[count].Also((it) => Array.Copy(sortedElements, startIndex, it, 0, count));
				startIndex += count;
				remaining -= count;
				sortedList.AddAll(elements);
			}
			this.VerifySortedList(sortedList, refList);

			// add all elements (sorted)
			sortedList.Clear();
			sortedList.AddAll(sortedElements);
			this.VerifySortedList(sortedList, refList);

			// add element one-by-one (plane)
			sortedList.Clear();
			refList.Clear();
			foreach (var element in planeElements)
				sortedList.Add(element);
			refList.AddRange(planeElements);
			this.VerifySortedList(sortedList, refList);

			// add elements by range (plane)
			sortedList.Clear();
			startIndex = 0;
			remaining = planeElements.Length;
			while (remaining > 0)
			{
				var count = Math.Min(remaining, this.random.Next(1, 100));
				var elements = new int[count].Also((it) => Array.Copy(planeElements, startIndex, it, 0, count));
				startIndex += count;
				remaining -= count;
				sortedList.AddAll(elements);
			}
			this.VerifySortedList(sortedList, refList);

			// add all elements (plane)
			sortedList.Clear();
			sortedList.AddAll(planeElements);
			this.VerifySortedList(sortedList, refList);
		}


		/// <summary>
		/// Test for collection changed event.
		/// </summary>
		[Test]
		public void CollectionChangedEventTest()
		{
			// prepare
			var randomElements = this.GenerateRandomArray(10240);
			var sortedList = new SortedObservableList<int>();
			var reflectedList = new List<int>();
			sortedList.CollectionChanged += (_, e) =>
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Add:
						{
							var newItems = e.NewItems;
							var elements = new int[newItems!.Count].Also((it) => newItems.CopyTo(it, 0));
							reflectedList.InsertRange(e.NewStartingIndex, elements);
						}
						break;
					case NotifyCollectionChangedAction.Remove:
						reflectedList.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
						break;
					case NotifyCollectionChangedAction.Reset:
						reflectedList.Clear();
						reflectedList.AddRange(sortedList);
						break;
					default:
						throw new AssertionException($"Uncovered collection change action: {e.Action}.");
				}
			};

			// add elements by range
			var startIndex = 0;
			var remaining = randomElements.Length;
			while (remaining > 0)
			{
				var count = Math.Min(remaining, this.random.Next(1, 100));
				var elements = new int[count].Also((it) => Array.Copy(randomElements, startIndex, it, 0, count));
				startIndex += count;
				remaining -= count;
				sortedList.AddAll(elements);
			}
			Assert.That(sortedList.SequenceEqual(reflectedList), "List built by collection change event is incorrect after adding elements.");

			// remove random range elements
			remaining = sortedList.Count;
			for (int i = remaining / 100; i > 0 && remaining > 0; --i)
			{
				var count = Math.Min(remaining, this.random.Next(1, 100));
				var removingElements = new int[count].Also((it) =>
				{
					for (var j = count - 1; j >= 0; --j)
						it[j] = randomElements[this.random.Next(0, randomElements.Length)];
				});
				sortedList.RemoveAll(removingElements);
			}
			Assert.That(sortedList.SequenceEqual(reflectedList), "List built by collection change event is incorrect after removing elements.");

			// Clear
			sortedList.Clear();
			Assert.That(sortedList.SequenceEqual(reflectedList), "List built by collection change event is incorrect after clearing.");
		}


		// Generate array with random value.
		int[] GenerateRandomArray(int count) => new int[count].Also((it) =>
		{
			for (var i = count - 1; i >= 0; --i)
				it[i] = this.random.Next(0, count / 2);
		});


		/// <summary>
		/// Test for performance of adding elements randomly comparing to <see cref="ObservableCollection{T}"/> and <see cref="List{T}"/>.
		/// </summary>
		/// <param name="elementCount">Number of final number of elements.</param>
		/// <param name="addingBlockSize">Number of elements for each adding.</param>
		public void RandomAddingPerformanceTest(int elementCount, int addingBlockSize)
		{
			var testCount = 3;
			var stopWatch = new Stopwatch().Also(it => it.Start());
			var sortedList = new SortedObservableList<int>();
			var observableCollection = new ObservableCollection<int>();
			// ReSharper disable once CollectionNeverQueried.Local
			var list = new List<int>();
			var sortedListDuration = 0L;
			var observableCollectionDuration = 0L;
			var listDuration = 0L;
			if (addingBlockSize == 1)
			{
				// prepare
				var addingElements = new int[elementCount].Also((it) =>
				{
					for (var i = elementCount - 1; i >= 0; --i)
						it[i] = this.random.Next(int.MinValue, int.MaxValue);
				});

				// test performance of sorted list
				for (var i = 0; i < testCount; ++i)
				{
					sortedList.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = elementCount - 1; j >= 0; --j)
						sortedList.Add(addingElements[j]);
					sortedListDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				sortedListDuration /= testCount;

				// test performance of observable collection
				for (var i = 0; i < testCount; ++i)
				{
					observableCollection.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = elementCount - 1; j >= 0; --j)
					{
						var element = addingElements[j];
						var index = ((IList<int>)observableCollection).BinarySearch(element);
						if (index < 0)
							index = ~index;
						observableCollection.Insert(index, element);
					}
					observableCollectionDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				sortedListDuration /= testCount;

				// test performance of list
				for (var i = 0; i < testCount; ++i)
				{
					list.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = elementCount - 1; j >= 0; --j)
					{
						list.Add(addingElements[j]);
						list.Sort();
					}
					listDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				listDuration /= testCount;
			}
			else
			{
				// prepare
				var remaining = elementCount;
				var addingBlocks = new List<int[]>().Also((it) =>
				{
					while (remaining > 0)
					{
						var blockSize = Math.Min(remaining, addingBlockSize);
						var block = new int[blockSize].Also((block) =>
						{
							for (var i = blockSize - 1; i >= 0; --i)
								block[i] = this.random.Next(int.MinValue, int.MaxValue);
						});
						it.Add(block);
						remaining -= blockSize;
					}
				});

				// test performance of sorted list
				for (var i = 0; i < testCount; ++i)
				{
					sortedList.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = addingBlocks.Count - 1; j >= 0; --j)
						sortedList.AddAll(addingBlocks[j]);
					sortedListDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				sortedListDuration /= testCount;

				// test performance of observable collection
				for (var i = 0; i < testCount; ++i)
				{
					observableCollection.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = addingBlocks.Count - 1; j >= 0; --j)
					{
						var block = addingBlocks[j];
						foreach (var element in block)
						{
							var index = ((IList<int>)observableCollection).BinarySearch(element);
							if (index < 0)
								index = ~index;
							observableCollection.Insert(index, element);
						}
					}
					observableCollectionDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				observableCollectionDuration /= testCount;

				// test performance of list
				for (var i = 0; i < testCount; ++i)
				{
					list.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = addingBlocks.Count - 1; j >= 0; --j)
					{
						list.AddRange(addingBlocks[j]);
						list.Sort();
					}
					listDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				listDuration /= testCount;
			}
			Console.WriteLine($"[N={elementCount}, B={addingBlockSize}]");
			Console.WriteLine($"SortedList: {sortedListDuration}");
			Console.WriteLine($"ObservableCollection: {observableCollectionDuration}");
			Console.WriteLine($"List: {listDuration}");
			Console.WriteLine();
		}


		/// <summary>
		/// Test for performance of adding non-overlapped elements randomly comparing to <see cref="ObservableCollection{T}"/> and <see cref="List{T}"/>.
		/// </summary>
		/// <param name="elementCount">Number of final number of elements.</param>
		/// <param name="addingBlockSize">Number of elements for each adding.</param>
		public void RandomNonOverlappedAddingPerformanceTest(int elementCount, int addingBlockSize)
		{
			var testCount = 3;
			var stopWatch = new Stopwatch().Also((it) => it.Start());
			var sortedList = new SortedObservableList<int>();
			var observableCollection = new ObservableCollection<int>();
			// ReSharper disable once CollectionNeverQueried.Local
			var list = new List<int>();
			var sortedListDuration = 0L;
			var observableCollectionDuration = 0L;
			var listDuration = 0L;
			if (addingBlockSize == 1)
			{
				// prepare
				var addingElements = new int[elementCount].Also((it) =>
				{
					for (var i = elementCount - 1; i >= 0; --i)
						it[i] = i;
					it.Shuffle();
				});

				// test performance of sorted list
				for (var i = 0; i < testCount; ++i)
				{
					sortedList.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = elementCount - 1; j >= 0; --j)
						sortedList.Add(addingElements[j]);
					sortedListDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				sortedListDuration /= testCount;

				// test performance of observable collection
				for (var i = 0; i < testCount; ++i)
				{
					observableCollection.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = elementCount - 1; j >= 0; --j)
					{
						var element = addingElements[j];
						var index = ((IList<int>)observableCollection).BinarySearch(element);
						if (index < 0)
							index = ~index;
						observableCollection.Insert(index, element);
					}
					observableCollectionDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				sortedListDuration /= testCount;

				// test performance of list
				for (var i = 0; i < testCount; ++i)
				{
					list.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = elementCount - 1; j >= 0; --j)
					{
						list.Add(addingElements[j]);
						list.Sort();
					}
					listDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				listDuration /= testCount;
			}
			else
			{
				// prepare
				var remaining = elementCount;
				var addingBlocks = new List<int[]>().Also((it) =>
				{
					while (remaining > 0)
					{
						var blockSize = Math.Min(remaining, addingBlockSize);
						var block = new int[blockSize].Also((block) =>
						{
							for (var i = blockSize - 1; i >= 0; --i)
								block[i] = remaining--;
							block.Shuffle();
						});
						it.Add(block);
					}
					it.Shuffle();
				});

				// test performance of sorted list
				for (var i = 0; i < testCount; ++i)
				{
					sortedList.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = addingBlocks.Count - 1; j >= 0; --j)
						sortedList.AddAll(addingBlocks[j]);
					sortedListDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				sortedListDuration /= testCount;

				// test performance of observable collection
				for (var i = 0; i < testCount; ++i)
				{
					observableCollection.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = addingBlocks.Count - 1; j >= 0; --j)
					{
						var block = addingBlocks[j];
						foreach (var element in block)
						{
							var index = ((IList<int>)observableCollection).BinarySearch(element);
							if (index < 0)
								index = ~index;
							observableCollection.Insert(index, element);
						}
					}
					observableCollectionDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				observableCollectionDuration /= testCount;

				// test performance of list
				for (var i = 0; i < testCount; ++i)
				{
					list.Clear();
					var startTime = stopWatch.ElapsedMilliseconds;
					for (var j = addingBlocks.Count - 1; j >= 0; --j)
					{
						list.AddRange(addingBlocks[j]);
						list.Sort();
					}
					listDuration += (stopWatch.ElapsedMilliseconds - startTime);
				}
				listDuration /= testCount;
			}
			Console.WriteLine($"[N={elementCount}, B={addingBlockSize}]");
			Console.WriteLine($"SortedList: {sortedListDuration}");
			Console.WriteLine($"ObservableCollection: {observableCollectionDuration}");
			Console.WriteLine($"List: {listDuration}");
			Console.WriteLine();
		}


		/// <summary>
		/// Test for removing elements.
		/// </summary>
		[Test]
		public void RemovingTest()
		{
			// prepare
			var randomElements = this.GenerateRandomArray(10240);
			var sortedElements = ((int[])randomElements.Clone()).Also(Array.Sort);
			var sortedList = new SortedObservableList<int>(randomElements);
			var refList = new List<int>(sortedList);
			this.VerifySortedList(sortedList, refList);

			// remove one-by-one
			for (var i = sortedList.Count / 2; i > 0; --i)
			{
				var index = this.random.Next(0, sortedList.Count);
				var element = refList[index];
				sortedList.Remove(element);
				refList.RemoveAt(index);
			}
			this.VerifySortedList(sortedList, refList);

			// remove random range elements
			sortedList.Clear();
			sortedList.AddAll(sortedElements, true);
			refList.Clear();
			refList.AddRange(sortedElements);
			var remaining = sortedList.Count;
			for (int i = remaining / 100; i > 0 && remaining > 0; --i)
			{
				var count = Math.Min(remaining, this.random.Next(1, 100));
				var removingElements = new int[count].Also((it) =>
				{
					for (var j = count - 1; j >= 0; --j)
						it[j] = randomElements[this.random.Next(0, randomElements.Length)];
				});
				sortedList.RemoveAll(removingElements);
				refList.RemoveAll((it) => removingElements.Contains(it));
				this.VerifySortedList(sortedList, refList);
			}

			// remove odd numbers
			var predicate = new Predicate<int>(n => (n & 0x1) == 1);
			sortedList.Clear();
			sortedList.AddAll(sortedElements, true);
			refList.Clear();
			refList.AddRange(sortedElements);
			if (refList.RemoveAll(predicate) > 0)
			{
				sortedList.RemoveAll(predicate);
				this.VerifySortedList(sortedList, refList);
			}
		}


		/// <summary>
		/// Test for sorting elements.
		/// </summary>
		[Test]
		public void SortingTest()
		{
			// prepare
			var count = 100;
			var sortedList = new SortedObservableList<int[]>((x, y) => x.AsNonNull()[0] - y.AsNonNull()[0]);
			var observableCollection = new ObservableCollection<int[]>();
			for (var i = 0; i < count; ++i)
			{
				var element = new[] { i };
				sortedList.Add(element);
				observableCollection.Add(element);
			}
			sortedList.CollectionChanged += (_, e) =>
			{
				if (e.Action == NotifyCollectionChangedAction.Move)
					observableCollection.Move(e.OldStartingIndex, e.NewStartingIndex);
			};

			// move element to tail of list
			var index = this.random.Next(count - 1);
			sortedList[index][0] = count;
			Assert.That(sortedList.Sort(sortedList[index]));
			Assert.That(((IList<int[]>)sortedList).IsSorted(sortedList.Comparer));
			Assert.That(sortedList.SequenceEqual(observableCollection));

			// move element to head of list
			index = this.random.Next(1, count - 1);
			sortedList[index][0] = -1;
			Assert.That(sortedList.SortAt(index));
			Assert.That(((IList<int[]>)sortedList).IsSorted(sortedList.Comparer));
			Assert.That(sortedList.SequenceEqual(observableCollection));

			// random moving elements
			for (var t = 0; t < 1000; ++t)
			{
				index = this.random.Next(count - 1);
				sortedList[index][0] = this.random.Next();
				sortedList.SortAt(index);
			}
			Assert.That(((IList<int[]>)sortedList).IsSorted(sortedList.Comparer));
			Assert.That(sortedList.SequenceEqual(observableCollection));
		}


		// Verify sorted list.
		void VerifySortedList(SortedObservableList<int> sortedList, IList<int> refList)
		{
			Assert.That(refList.Count == sortedList.Count, "Number of elements is incorrect.");
			for (var i = sortedList.Count - 1; i >= 0; --i)
				Assert.That(refList[i] == sortedList[i], $"Element[{i}] is incorrect.");
		}
	}
}
