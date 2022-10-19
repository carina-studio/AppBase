using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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
				list.Add(new int[] { i * 2 });

			// find existing elements
			for (var t = 0; t < 100; ++t)
			{
				var index = this.random.Next(list.Count);
				var key = list[index][0];
				Assert.AreEqual(index, list.BinarySearch(key, keyGetter, comparison));
			}

			// find non-existing elements
			Assert.AreEqual(~0, list.BinarySearch(-1, keyGetter, comparison));
			Assert.AreEqual(~list.Count, list.BinarySearch(list[list.Count - 1][0] + 1, keyGetter, comparison));
			for (var t = 0; t < 100; ++t)
			{
				var index = this.random.Next(list.Count);
				var key = list[index][0] - 1;
				Assert.AreEqual(~index, list.BinarySearch(key, keyGetter, comparison));
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
			var testList = (IList<int>)new int[0];
			Assert.AreEqual(~0, testList.BinarySearch(1));
			Assert.AreEqual(~0, testList.BinarySearch(1, comparison));

			// search in list with same elements
			for (var i = 0; i < 10; ++i)
			{
				var count = this.random.Next(1, 101);
				testList = new List<int>(count).Also((it) =>
				{
					for (var j = count; j > 0; --j)
						it.Add(1);
				});
				Assert.AreEqual(~0, testList.BinarySearch(0));
				Assert.AreEqual(~0, testList.BinarySearch(0, comparison));
				Assert.AreEqual(~count, testList.BinarySearch(2));
				Assert.AreEqual(~count, testList.BinarySearch(2, comparison));
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
				Assert.AreEqual(~0, testList.BinarySearch(testList[0] - 1));
				Assert.AreEqual(~0, testList.BinarySearch(testList[0] - 1, comparison));

				// search first value
				Assert.AreEqual(0, testList.BinarySearch(testList[0]));
				Assert.AreEqual(0, testList.BinarySearch(testList[0], comparison));

				// search last value
				Assert.AreEqual(count - 1, testList.BinarySearch(testList[count - 1]));
				Assert.AreEqual(count - 1, testList.BinarySearch(testList[count - 1], comparison));

				// search value which should be placed after tail
				Assert.AreEqual(~count, testList.BinarySearch(testList[count - 1] + 1));
				Assert.AreEqual(~count, testList.BinarySearch(testList[count - 1] + 1, comparison));

				// search existent value
				for (var j = 0; j < 10; ++j)
				{
					var index = this.random.Next(0, count);
					Assert.AreEqual(index, testList.BinarySearch(testList[index]));
					Assert.AreEqual(index, testList.BinarySearch(testList[index], comparison));
				}

				// search non-existent value
				for (var j = 0; j < 10; ++j)
				{
					var index = this.random.Next(0, count);
					Assert.AreEqual(~index, testList.BinarySearch(testList[index] - 1));
					Assert.AreEqual(~index, testList.BinarySearch(testList[index] - 1, comparison));
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
			var sourceList = (IList)new int[1024].Also((it) =>
			{
				for (var i = it.Length - 1; i >= 0; --i)
					it[i] = this.random.Next(int.MinValue, int.MaxValue);
			});

			// cast to IList<int>
			var intList = sourceList.Cast<int>();
			Assert.AreEqual(sourceList.Count, intList.Count);
			for (var i = intList.Count - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i], intList[i]);
			
			// copy from IList<int> to array
			var intArray = new int[intList.Count];
			intList.CopyTo(intArray, 0);
			for (var i = intArray.Length - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i], intArray[i]);
			
			// cast to IList<object>
			var objList = sourceList.Cast<object>();
			Assert.AreEqual(sourceList.Count, objList.Count);
			for (var i = objList.Count - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i], objList[i]);
			
			// copy from IList<object> to array
			var objArray = new object[objList.Count];
			objList.CopyTo(objArray, 0);
			for (var i = objArray.Length - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i], objArray[i]);
			
			// cast to IList<IConvertible>
			var convertibleList = sourceList.Cast<IConvertible>();
			Assert.AreEqual(sourceList.Count, convertibleList.Count);
			for (var i = convertibleList.Count - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i], convertibleList[i]);
			
			// copy from IList<IConvertible> to array
			var convertibleArray = new IConvertible[convertibleList.Count];
			convertibleList.CopyTo(convertibleArray, 0);
			for (var i = convertibleArray.Length - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i], convertibleArray[i]);
			
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
			var emptyList = ((IList)new int[0]).Cast<string>();
			Assert.AreEqual(0, emptyList.Count);
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
			Assert.IsTrue(array.SequenceEqual(refArray), "Copied elements are incorrect.");

			// copy tail of list
			testList.CopyTo(testList.Count - array.Length, array, 0, array.Length);
			refList.CopyTo(refList.Count - refArray.Length, refArray, 0, refArray.Length);
			Assert.IsTrue(array.SequenceEqual(refArray), "Copied elements are incorrect.");

			// copy middle of list
			var index = this.random.Next(1, testList.Count - array.Length);
			testList.CopyTo(index, array, 0, array.Length);
			refList.CopyTo(index, refArray, 0, refArray.Length);
			Assert.IsTrue(array.SequenceEqual(refArray), "Copied elements are incorrect.");
		}


		/// <summary>
		/// Test for <see cref="ListExtensions.GetRangeView{T}(IList{T}, int, int)"/>.
		/// </summary>
		[Test]
		public void GettingRangeViewTest()
		{
			// empty list
			var sourceList = new int[0];
			var view = sourceList.GetRangeView(0, 0);
			Assert.AreEqual(0, view.Count);
			view = sourceList.GetRangeView(0, 1);
			Assert.AreEqual(0, view.Count);
			view = sourceList.GetRangeView(1, 0);
			Assert.AreEqual(0, view.Count);

			// normal list
			sourceList = new int[] { 0, 1, 2, 3, 4, 5 };
			view = sourceList.GetRangeView(0, 0);
			Assert.AreEqual(0, view.Count);
			view = sourceList.GetRangeView(0, 3);
			Assert.AreEqual(3, view.Count);
			for (var i = view.Count - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i], view[i]);
			Assert.IsTrue(view.SequenceEqual(new int[] { 0, 1, 2 }));
			view = sourceList.GetRangeView(3, 3);
			Assert.AreEqual(3, view.Count);
			for (var i = view.Count - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i + 3], view[i]);
			Assert.IsTrue(view.SequenceEqual(new int[] { 3, 4, 5 }));
			view = sourceList.GetRangeView(4, 3);
			Assert.AreEqual(2, view.Count);
			for (var i = view.Count - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i + 4], view[i]);
			Assert.IsTrue(view.SequenceEqual(new int[] { 4, 5 }));

			// modify source list
			Assert.AreEqual(4, view[0]);
			sourceList[4] = 0;
			Assert.AreEqual(0, view[0]);
		}
	}
}
