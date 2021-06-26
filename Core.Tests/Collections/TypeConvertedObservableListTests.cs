using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CarinaStudio.Collections
{
	/// <summary>
	/// Test of <see cref="TypeConvertedObservableList{TSrc, TDest}"/>.
	/// </summary>
	[TestFixture]
	class TypeConvertedObservableListTests
	{
		// Test class of TypeConvertedList.
		class TestList : TypeConvertedObservableList<int, long>
		{
			// Constructor.
			public TestList(IList<int> source) : base(source)
			{ }

			// Convert.
			protected override long ConvertElement(int srcElement) => srcElement * 2L;
		}


		// Fields.
		readonly Random random = new Random();


		/// <summary>
		/// Test for element conversion.
		/// </summary>
		[Test]
		public void ElementConversionTest()
		{
			// Prepare
			var srcList = new SortedObservableList<int>();
			var testList = new TestList(srcList);
			this.VerifyTypeConvertedList(srcList, testList);

			// add source elements
			for (var i = 0; i < 100; ++i)
				srcList.Add(this.random.Next(int.MinValue, int.MaxValue));
			this.VerifyTypeConvertedList(srcList, testList);
			var randomElements = new int[100].Also((it) =>
			{
				for (var i = it.Length - 1; i >= 0; --i)
					it[i] = this.random.Next(int.MinValue, int.MaxValue);
			});
			srcList.AddAll(randomElements);
			this.VerifyTypeConvertedList(srcList, testList);

			// remove source elements
			for (var i = 0; i < 50; ++i)
				srcList.Remove(srcList[this.random.Next(0, srcList.Count)]);
			this.VerifyTypeConvertedList(srcList, testList);
			srcList.RemoveAll(randomElements);
			this.VerifyTypeConvertedList(srcList, testList);

			// clear source elements
			srcList.Clear();
			this.VerifyTypeConvertedList(srcList, testList);
		}


		// Verify elements in type converted list.
		void VerifyTypeConvertedList(IList<int> sourceList, TestList testList)
		{
			Assert.AreEqual(sourceList.Count, testList.Count, "Number of elements is incorrect.");
			for (var i = sourceList.Count - 1; i >= 0; --i)
				Assert.AreEqual(sourceList[i] * 2L, testList[i], $"Element[{i}] is incorrect.");
		}
	}
}
