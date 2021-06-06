using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Test of <see cref="SortedList{T}"/>.
	/// </summary>
	[TestFixture]
	class SortedListTests
	{
		// Fields.
		readonly Random random = new Random();


		/// <summary>
		/// Test for adding elements.
		/// </summary>
		[Test]
		public void AddingTest()
		{
			// prepare
			var randomElements = this.GenerateRandomArray(10240);
			var sortedElements = ((int[])randomElements.Clone()).Also((it) => Array.Sort(it));
			var planeElements = new int[randomElements.Length];
			var sortedList = new ObservableSortedList<int>();
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


		// Generate array with random value.
		int[] GenerateRandomArray(int count) => new int[count].Also((it) =>
		{
			for (var i = count - 1; i >= 0; --i)
				it[i] = this.random.Next(0, count / 2);
		});


		/// <summary>
		/// Test for removing elements.
		/// </summary>
		[Test]
		public void RemovingTest()
		{
			// prepare
			var randomElements = this.GenerateRandomArray(10240);
			var sortedElements = ((int[])randomElements.Clone()).Also((it) => Array.Sort(it));
			var sortedList = new ObservableSortedList<int>(randomElements);
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
		}


		// Verify sorted list.
		void VerifySortedList(SortedList<int> sortedList, IList<int> refList)
		{
			Assert.AreEqual(refList.Count, sortedList.Count, "Number of elements is incorrect.");
			for (var i = sortedList.Count - 1; i >= 0; --i)
				Assert.AreEqual(refList[i], sortedList[i], $"Element[{i}] is incorrect.");
		}
	}
}
