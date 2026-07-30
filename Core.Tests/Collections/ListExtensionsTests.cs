using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Tests of <see cref="ListExtensions"/>.
	/// </summary>
	[TestFixture]
	class ListExtensionsTests
	{
		// Fields.
		readonly Random random = new Random();


		/// <summary>
		/// Test binary search by key.
		/// </summary>
		[Test]
		public void BinarySearchByKeyTest()
		{
			// prepare
			var keyGetter = new Func<int[], int>(it => it[0]);
			var comparison = new Comparison<int>((x, y) => x - y);
			var list = new List<int[]>();
			for (var i = 0; i < 1024; ++i)
				list.Add(new[] { i * 2 });

			// find existing elements
			for (var t = 0; t < 100; ++t)
			{
				var index = this.random.Next(list.Count);
				var key = list[index][0];
				Assert.That(index == list.BinarySearch(key, keyGetter, comparison));
			}

			// find non-existing elements
			Assert.That(~0 == list.BinarySearch(-1, keyGetter, comparison));
			Assert.That(~list.Count == list.BinarySearch(list[^1][0] + 1, keyGetter, comparison));
			for (var t = 0; t < 100; ++t)
			{
				var index = this.random.Next(list.Count);
				var key = list[index][0] - 1;
				Assert.That(~index == list.BinarySearch(key, keyGetter, comparison));
			}
		}


		/// <summary>
		/// Test binary search on <see cref="ImmutableList{T}"/> and <see cref="ImmutableList{T}.Builder"/>.
		/// </summary>
		[Test]
		public void BinarySearchOnImmutableListTest()
		{
			// prepare comparison functions and comparers
			var ascendingComparison = new Comparison<int>((x, y) => x - y);
			var ascendingComparer = Comparer<int>.Create(ascendingComparison);
			var descendingComparison = new Comparison<int>((x, y) => y - x);
			var descendingComparer = Comparer<int>.Create(descendingComparison);

			// check whether all overloads on immutable lists report the same result as the generic implementation or not
			void VerifyResults(IList<int> refList, ImmutableList<int> immutableList, ImmutableList<int>.Builder builder, int element, Comparison<int> comparison, IComparer<int> comparer, bool isDefaultOrder, bool hasEquivalentElements)
			{
				var expected = refList.BinarySearch(element, comparison);
				var results = new List<int>
				{
					((IList<int>)immutableList).BinarySearch(element, comparison),
					((IList<int>)immutableList).BinarySearch(element, comparer),
					((IReadOnlyList<int>)immutableList).BinarySearch(element, comparison),
					((IReadOnlyList<int>)immutableList).BinarySearch(element, comparer),
					((IList<int>)builder).BinarySearch(element, comparison),
					((IList<int>)builder).BinarySearch(element, comparer),
					((IReadOnlyList<int>)builder).BinarySearch(element, comparison),
					((IReadOnlyList<int>)builder).BinarySearch(element, comparer),
				};
				if (isDefaultOrder)
				{
					results.Add(((IList<int>)immutableList).BinarySearch(element));
					results.Add(((IReadOnlyList<int>)immutableList).BinarySearch(element));
					results.Add(((IList<int>)builder).BinarySearch(element));
					results.Add(((IReadOnlyList<int>)builder).BinarySearch(element));
				}
				foreach (var result in results)
				{
					// the index of an element which is equivalent to another element in list is unspecified, only the position to insert element is
					if (expected < 0 || !hasEquivalentElements)
						Assert.That(expected == result, $"Result of searching {element} should be {expected} but is {result}.");
					else
					{
						Assert.That(result >= 0, $"Element {element} should be found in list.");
						Assert.That(0 == comparison(element, immutableList[result]), $"Element at position {result} should be equivalent to {element}.");
					}
				}
			}

			// build immutable lists with given elements, the builder is filled element by element to build a tree with different shape
			void VerifyResultsOfSearchingElements(List<int> refList, int firstElement, int lastElement, Comparison<int> comparison, IComparer<int> comparer, bool isDefaultOrder, bool hasEquivalentElements)
			{
				var immutableList = ImmutableList.CreateRange(refList);
				var builder = ImmutableList.CreateBuilder<int>();
				foreach (var element in refList)
					builder.Add(element);
				var step = firstElement <= lastElement ? 1 : -1;
				for (var element = firstElement; ; element += step)
				{
					VerifyResults(refList, immutableList, builder, element, comparison, comparer, isDefaultOrder, hasEquivalentElements);
					if (element == lastElement)
						break;
				}
			}

			// search in empty lists
			VerifyResultsOfSearchingElements(new List<int>(), -1, 1, ascendingComparison, ascendingComparer, true, false);
			VerifyResultsOfSearchingElements(new List<int>(), 1, -1, descendingComparison, descendingComparer, false, false);

			// search in lists with distinct elements sorted by default order
			for (var t = 0; t < 10; ++t)
			{
				var count = this.random.Next(1, 101);
				var refList = new List<int>(count).Also(it =>
				{
					for (var i = 0; i < count; ++i)
						it.Add(i * 2);
				});
				VerifyResultsOfSearchingElements(refList, refList[0] - 1, refList[count - 1] + 1, ascendingComparison, ascendingComparer, true, false);
			}

			// search in lists with distinct elements sorted by descending order
			for (var t = 0; t < 10; ++t)
			{
				var count = this.random.Next(1, 101);
				var refList = new List<int>(count).Also(it =>
				{
					for (var i = count - 1; i >= 0; --i)
						it.Add(i * 2);
				});
				VerifyResultsOfSearchingElements(refList, refList[0] + 1, refList[count - 1] - 1, descendingComparison, descendingComparer, false, false);
			}

			// search in lists with equivalent elements
			for (var t = 0; t < 10; ++t)
			{
				var count = this.random.Next(2, 101);
				var refList = new List<int>(count).Also(it =>
				{
					for (var i = 0; i < count; ++i)
						it.Add(this.random.Next(0, 10) * 2);
					it.Sort();
				});
				VerifyResultsOfSearchingElements(refList, -1, 19, ascendingComparison, ascendingComparer, true, true);
			}
		}


		/// <summary>
		/// Test binary search.
		/// </summary>
		[Test]
		public void BinarySearchTest()
		{
			// prepare comparison function
			var comparison = new Comparison<int>((x, y) => x - y);

			// search in empty list
			var testList = (IList<int>)Array.Empty<int>();
			Assert.That(~0 == testList.BinarySearch(1));
			Assert.That(~0 == testList.BinarySearch(1, comparison));

			// search in list with same elements
			for (var i = 0; i < 10; ++i)
			{
				var count = this.random.Next(1, 101);
				testList = new List<int>(count).Also((it) =>
				{
					for (var j = count; j > 0; --j)
						it.Add(1);
				});
				Assert.That(~0 == testList.BinarySearch(0));
				Assert.That(~0 == testList.BinarySearch(0, comparison));
				Assert.That(~count == testList.BinarySearch(2));
				Assert.That(~count == testList.BinarySearch(2, comparison));
			}

			// search in sorted list
			for (var i = 0; i < 100; ++i)
			{
				// build list
				var count = this.random.Next(10, 101);
				testList = new List<int>(count).Also((it) =>
				{
					for (var j = 1; j <= count; ++j)
						it.Add(j * 2);
				});

				// search value which should be placed before head
				Assert.That(~0 == testList.BinarySearch(testList[0] - 1));
				Assert.That(~0 == testList.BinarySearch(testList[0] - 1, comparison));

				// search first value
				Assert.That(0 == testList.BinarySearch(testList[0]));
				Assert.That(0 == testList.BinarySearch(testList[0], comparison));

				// search last value
				Assert.That(count - 1 == testList.BinarySearch(testList[count - 1]));
				Assert.That(count - 1 == testList.BinarySearch(testList[count - 1], comparison));

				// search value which should be placed after tail
				Assert.That(~count == testList.BinarySearch(testList[count - 1] + 1));
				Assert.That(~count == testList.BinarySearch(testList[count - 1] + 1, comparison));

				// search existent value
				for (var j = 0; j < 10; ++j)
				{
					var index = this.random.Next(0, count);
					Assert.That(index == testList.BinarySearch(testList[index]));
					Assert.That(index == testList.BinarySearch(testList[index], comparison));
				}

				// search non-existent value
				for (var j = 0; j < 10; ++j)
				{
					var index = this.random.Next(0, count);
					Assert.That(~index == testList.BinarySearch(testList[index] - 1));
					Assert.That(~index == testList.BinarySearch(testList[index] - 1, comparison));
				}
			}
		}


		/// <summary>
		/// Test for <see cref="ListExtensions.Cast{TOut}(System.Collections.IList)"/>.
		/// </summary>
		[Test]
		public void CastingTest()
		{
			// prepare
			var sourceList = (IList)new int[1024].Also(it =>
			{
				for (var i = it.Length - 1; i >= 0; --i)
					it[i] = this.random.Next(int.MinValue, int.MaxValue);
			});

			// cast to IList<int>
			var intList = sourceList.Cast<int>();
			Assert.That(sourceList.Count == intList.Count);
			for (var i = intList.Count - 1; i >= 0; --i)
				Assert.That((int)sourceList[i]! == intList[i]);
			
			// copy from IList<int> to array
			var intArray = new int[intList.Count];
			intList.CopyTo(intArray, 0);
			for (var i = intArray.Length - 1; i >= 0; --i)
				Assert.That((int)sourceList[i]! == intArray[i]);
			
			// cast to IList<object>
			var objList = sourceList.Cast<object>();
			Assert.That(sourceList.Count == objList.Count);
			for (var i = objList.Count - 1; i >= 0; --i)
				Assert.That((int)sourceList[i]! == (int)objList[i]);
			
			// copy from IList<object> to array
			var objArray = new object[objList.Count];
			objList.CopyTo(objArray, 0);
			for (var i = objArray.Length - 1; i >= 0; --i)
				Assert.That((int)sourceList[i]! == (int)objArray[i]);
			
			// cast to IList<IConvertible>
			var convertibleList = sourceList.Cast<IConvertible>();
			Assert.That(sourceList.Count == convertibleList.Count);
			for (var i = convertibleList.Count - 1; i >= 0; --i)
				Assert.That((int)sourceList[i]! == (int)convertibleList[i]);
			
			// copy from IList<IConvertible> to array
			var convertibleArray = new IConvertible[convertibleList.Count];
			convertibleList.CopyTo(convertibleArray, 0);
			for (var i = convertibleArray.Length - 1; i >= 0; --i)
				Assert.That((int)sourceList[i]! == (int)convertibleArray[i]);
			
			// cast to IList<string>
			try
			{
				sourceList.Cast<string>();
				throw new AssertionException("Should not support type casting.");
			}
			catch (Exception ex)
			{
				if (ex is AssertionException)
					throw;
			}

			// cast empty list.
			var emptyList = Array.Empty<int>().Cast<string>();
			Assert.That(0 == emptyList.Count);
		}


		/// <summary>
		/// Test for copying elements of list to array.
		/// </summary>
		[Test]
		public void CopyingToArrayTest()
		{
			// prepare
			var testList = (IList<int>)new int[1024].Also((it) =>
			{
				for (var i = it.Length - 1; i >= 0; --i)
					it[i] = this.random.Next(int.MinValue, int.MaxValue);
			});
			var refList = new List<int>(testList);

			// copy head of list
			var array = new int[testList.Count / 10];
			var refArray = new int[array.Length];
			testList.CopyTo(0, array, 0, array.Length);
			refList.CopyTo(0, refArray, 0, refArray.Length);
			Assert.That(array.SequenceEqual(refArray), "Copied elements are incorrect.");

			// copy tail of list
			testList.CopyTo(testList.Count - array.Length, array, 0, array.Length);
			refList.CopyTo(refList.Count - refArray.Length, refArray, 0, refArray.Length);
			Assert.That(array.SequenceEqual(refArray), "Copied elements are incorrect.");

			// copy middle of list
			var index = this.random.Next(1, testList.Count - array.Length);
			testList.CopyTo(index, array, 0, array.Length);
			refList.CopyTo(index, refArray, 0, refArray.Length);
			Assert.That(array.SequenceEqual(refArray), "Copied elements are incorrect.");
		}


		/// <summary>
		/// Test for <see cref="ListExtensions.GetRangeView{T}(IList{T}, int, int)"/>.
		/// </summary>
		[Test]
		public void GettingRangeViewTest()
		{
			// empty list
			var sourceList = (IList<int>)Array.Empty<int>();
			var view = sourceList.GetRangeView(0, 0);
			Assert.That(0 == view.Count);
			view = sourceList.GetRangeView(0, 1);
			Assert.That(0 == view.Count);
			view = sourceList.GetRangeView(1, 0);
			Assert.That(0 == view.Count);

			// normal list
			sourceList = new[] { 0, 1, 2, 3, 4, 5 };
			view = sourceList.GetRangeView(0, 0);
			Assert.That(0 == view.Count);
			view = sourceList.GetRangeView(0, 3);
			Assert.That(3 == view.Count);
			for (var i = view.Count - 1; i >= 0; --i)
				Assert.That(sourceList[i] == view[i]);
			Assert.That(view.SequenceEqual(new[] { 0, 1, 2 }));
			view = sourceList.GetRangeView(3, 3);
			Assert.That(3 == view.Count);
			for (var i = view.Count - 1; i >= 0; --i)
				Assert.That(sourceList[i + 3] == view[i]);
			Assert.That(view.SequenceEqual(new[] { 3, 4, 5 }));
			view = sourceList.GetRangeView(4, 3);
			Assert.That(2 == view.Count);
			for (var i = view.Count - 1; i >= 0; --i)
				Assert.That(sourceList[i + 4] == view[i]);
			Assert.That(view.SequenceEqual(new[] { 4, 5 }));

			// modify source list
			Assert.That(4 == view[0]);
			sourceList[4] = 0;
			Assert.That(0 == view[0]);
		}


		/// <summary>
		/// Test checking whether elements in <see cref="ImmutableList{T}"/> and <see cref="ImmutableList{T}.Builder"/> are sorted or not.
		/// </summary>
		[Test]
		public void IsSortedOnImmutableListTest()
		{
			// prepare comparison function and comparer
			var comparison = new Comparison<int>((x, y) => x - y);
			var comparer = Comparer<int>.Create(comparison);

			// check whether all overloads on immutable lists report the same result as the generic implementation or not
			void VerifyResults(List<int> refList, bool expected)
			{
				var immutableList = ImmutableList.CreateRange(refList);
				var builder = ImmutableList.CreateBuilder<int>();
				foreach (var element in refList)
					builder.Add(element);
				var results = new[]
				{
					((IList<int>)refList).IsSorted(comparison),
					((IList<int>)refList).IsSorted(comparer),
					((IList<int>)refList).IsSorted(),
					((IList<int>)immutableList).IsSorted(comparison),
					((IList<int>)immutableList).IsSorted(comparer),
					((IList<int>)immutableList).IsSorted(),
					((IReadOnlyList<int>)immutableList).IsSorted(comparison),
					((IReadOnlyList<int>)immutableList).IsSorted(comparer),
					((IReadOnlyList<int>)immutableList).IsSorted(),
					((IList<int>)builder).IsSorted(comparison),
					((IList<int>)builder).IsSorted(comparer),
					((IList<int>)builder).IsSorted(),
					((IReadOnlyList<int>)builder).IsSorted(comparison),
					((IReadOnlyList<int>)builder).IsSorted(comparer),
					((IReadOnlyList<int>)builder).IsSorted(),
				};
				foreach (var result in results)
					Assert.That(expected == result, $"Result of checking whether [{string.Join(", ", refList)}] is sorted or not should be {expected}.");
			}

			// check empty list and list with single element
			VerifyResults(new List<int>(), true);
			VerifyResults(new List<int> { 1 }, true);

			// check sorted lists
			VerifyResults(new List<int> { 1, 1, 1 }, true);
			VerifyResults(new List<int> { 1, 2 }, true);
			VerifyResults(new List<int> { 1, 2, 2, 3, 4 }, true);
			for (var t = 0; t < 10; ++t)
			{
				var count = this.random.Next(2, 101);
				VerifyResults(new List<int>(count).Also(it =>
				{
					for (var i = 0; i < count; ++i)
						it.Add(this.random.Next(0, 100));
					it.Sort();
				}), true);
			}

			// check unsorted lists with element which is out of order at head, middle and tail
			VerifyResults(new List<int> { 2, 1 }, false);
			VerifyResults(new List<int> { 5, 1, 2, 3, 4 }, false);
			VerifyResults(new List<int> { 1, 2, 0, 3, 4 }, false);
			VerifyResults(new List<int> { 1, 2, 3, 4, 0 }, false);
			for (var t = 0; t < 10; ++t)
			{
				var count = this.random.Next(2, 101);
				var refList = new List<int>(count).Also(it =>
				{
					for (var i = 0; i < count; ++i)
						it.Add(i * 2);
				});
				var index = this.random.Next(0, count);
				refList[index] = index == count - 1 ? refList[0] - 1 : refList[count - 1] + 1;
				VerifyResults(refList, false);
			}
		}


		/// <summary>
		/// Test for <see cref="ListExtensions.Reverse{T}(IList{T})"/>.
		/// </summary>
		[Test]
		public void ReversingTest()
		{
			// empty list
			var sourceList = new List<int>();
			var reversedList = ListExtensions.Reverse(sourceList);
			Assert.That(0 == reversedList.Count);
			Assert.That(!reversedList.IsReadOnly);
			foreach (var _ in reversedList)
				throw new AssertionException("Should not allow enumerating items in empty list.");
			
			// read from list
			sourceList.AddRange(new[]{ 0, 1, 2, 3, 4 });
			Assert.That(sourceList.Count == reversedList.Count);
			Assert.That(0 == reversedList[^1]);
			Assert.That(4 == reversedList[0]);
			Assert.That(2 == reversedList[2]);
			Assert.That(reversedList.SequenceEqual(new[]{ 4, 3, 2, 1, 0 }));
			
			// write to list
			sourceList.Add(5);
			Assert.That(sourceList.Count == reversedList.Count);
			Assert.That(5 == reversedList[0]);
			sourceList.Insert(0, -1);
			Assert.That(sourceList.Count == reversedList.Count);
			Assert.That(-1 == reversedList[^1]);
			reversedList.Add(-2);
			Assert.That(sourceList.Count == reversedList.Count);
			Assert.That(-2 == sourceList[0]);
			reversedList.Insert(0, 6);
			Assert.That(sourceList.Count == reversedList.Count);
			Assert.That(6 == sourceList[^1]);
			reversedList.Insert(6, 0);
			Assert.That(0 == sourceList[2]);
			Assert.That(0 == sourceList[3]);
			sourceList.Clear();
			Assert.That(0 == reversedList.Count);
			
			// setup observable list
			var collectionChangedEventArgs = default(NotifyCollectionChangedEventArgs);
			var observableSourceList = new ObservableList<int>();
			reversedList = ((IList<int>)observableSourceList).Reverse();
			((INotifyCollectionChanged)reversedList).CollectionChanged += (_, e) =>
			{
				collectionChangedEventArgs = e;
			};
			
			// add items
			observableSourceList.Add(0); // 0
			Assert.That(observableSourceList.Count == reversedList.Count);
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Add == collectionChangedEventArgs!.Action);
			Assert.That(0 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.NewItems!.Cast<int>().SequenceEqual(new[]{ 0 }));
			collectionChangedEventArgs = null;
			
			observableSourceList.Add(2); // 0, 2
			Assert.That(observableSourceList.Count == reversedList.Count);
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Add == collectionChangedEventArgs!.Action);
			Assert.That(0 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.NewItems!.Cast<int>().SequenceEqual(new[]{ 2 }));
			collectionChangedEventArgs = null;
			
			observableSourceList.Insert(0, -2); // -2, 0, 2
			Assert.That(observableSourceList.Count == reversedList.Count);
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Add == collectionChangedEventArgs!.Action);
			Assert.That(2 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.NewItems!.Cast<int>().SequenceEqual(new[]{ -2 }));
			collectionChangedEventArgs = null;
			
			observableSourceList.InsertRange(2, new[]{ 1, 1 }); // -2, 0, 1, 1, 2
			Assert.That(observableSourceList.Count == reversedList.Count);
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Add == collectionChangedEventArgs!.Action);
			Assert.That(1 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.NewItems!.Cast<int>().SequenceEqual(new[]{ 1, 1 }));
			collectionChangedEventArgs = null;
			
			observableSourceList.AddRange(new[]{ 4, 6 }); // -2, 0, 1, 1, 2, 4, 6
			Assert.That(observableSourceList.Count == reversedList.Count);
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Add == collectionChangedEventArgs!.Action);
			Assert.That(0 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.NewItems!.Cast<int>().SequenceEqual(new[]{ 6, 4 }));
			collectionChangedEventArgs = null;
			
			// replace items
			observableSourceList[0] = -4; // -4, 0, 1, 1, 2, 4, 6
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Replace == collectionChangedEventArgs!.Action);
			Assert.That(6 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.OldItems!.Cast<int>().SequenceEqual(new[]{ -2 }));
			Assert.That(collectionChangedEventArgs.NewItems!.Cast<int>().SequenceEqual(new[]{ -4 }));
			collectionChangedEventArgs = null;
			
			observableSourceList[4] = 3; // -4, 0, 1, 1, 3, 4, 6
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Replace == collectionChangedEventArgs!.Action);
			Assert.That(2 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.OldItems!.Cast<int>().SequenceEqual(new[]{ 2 }));
			Assert.That(collectionChangedEventArgs.NewItems!.Cast<int>().SequenceEqual(new[]{ 3 }));
			collectionChangedEventArgs = null;
			
			// move items
			observableSourceList.Move(1, 3); // -4, 1, 1, 0, 3, 4, 6
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Move == collectionChangedEventArgs!.Action);
			Assert.That(5 == collectionChangedEventArgs.OldStartingIndex);
			Assert.That(3 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.OldItems!.Cast<int>().SequenceEqual(new[]{ 0 }));
			collectionChangedEventArgs = null;
			
			observableSourceList.MoveRange(4, 0, 3); // 3, 4, 6, -4, 1, 1, 0
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Move == collectionChangedEventArgs!.Action);
			Assert.That(2 == collectionChangedEventArgs.OldStartingIndex);
			Assert.That(6 == collectionChangedEventArgs.NewStartingIndex);
			Assert.That(collectionChangedEventArgs.OldItems!.Cast<int>().SequenceEqual(new[]{ 6, 4, 3 }));
			collectionChangedEventArgs = null;
			Assert.That(reversedList.SequenceEqual(new[] { 0, 1, 1, -4, 6, 4, 3 }));
			
			// remove items
			observableSourceList.RemoveAt(1); // 3, 6, -4, 1, 1, 0
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Remove == collectionChangedEventArgs!.Action);
			Assert.That(5 == collectionChangedEventArgs.OldStartingIndex);
			Assert.That(collectionChangedEventArgs.OldItems!.Cast<int>().SequenceEqual(new[]{ 4 }));
			collectionChangedEventArgs = null;
			
			observableSourceList.RemoveRange(2, 3); // 3, 6, 0
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Remove == collectionChangedEventArgs!.Action);
			Assert.That(3 == collectionChangedEventArgs.OldStartingIndex);
			Assert.That(collectionChangedEventArgs.OldItems!.Cast<int>().SequenceEqual(new[]{ 1, 1, -4 }));
			collectionChangedEventArgs = null;
			Assert.That(reversedList.SequenceEqual(new[] { 0, 6, 3 }));
			
			// clear items
			observableSourceList.Clear();
			Assert.That(collectionChangedEventArgs is not null);
			Assert.That(NotifyCollectionChangedAction.Reset == collectionChangedEventArgs!.Action);
			collectionChangedEventArgs = null;
		}
	}
}
