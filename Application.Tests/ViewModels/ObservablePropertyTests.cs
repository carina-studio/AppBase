using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CarinaStudio.ViewModels
{
	/// <summary>
	/// Tests of <see cref="ObservableProperty{T}"/>.
	/// </summary>
	[TestFixture]
	class ObservablePropertyTests
	{
		// Fields.
		readonly Random random = new();


		/// <summary>
		/// Test for registering property.
		/// </summary>
		[Test]
		public void RegistrationTest()
		{
			// prepare
			var valueTypes = new Type[128].Also(it =>
			{
				for (var i = it.Length - 1; i >= 0; --i)
				{
					it[i] = this.random.Next(6) switch
					{
						0 => typeof(byte),
						1 => typeof(short),
						2 => typeof(int),
						3 => typeof(long),
						4 => typeof(double),
						_ => typeof(string),
					};
				}
			});

			// register
			var properties = new ObservableProperty[valueTypes.Length].Also(it =>
			{
				for (var i = it.Length - 1; i >= 0; --i)
				{
					if (valueTypes[i] == typeof(byte))
						it[i] = ObservableProperty.Register<ObservablePropertyTests, byte>($"{i}");
					else if (valueTypes[i] == typeof(short))
						it[i] = ObservableProperty.Register<ObservablePropertyTests, short>($"{i}");
					else if (valueTypes[i] == typeof(int))
						it[i] = ObservableProperty.Register<ObservablePropertyTests, int>($"{i}");
					else if (valueTypes[i] == typeof(long))
						it[i] = ObservableProperty.Register<ObservablePropertyTests, long>($"{i}");
					else if (valueTypes[i] == typeof(double))
						it[i] = ObservableProperty.Register<ObservablePropertyTests, double>($"{i}");
					else if (valueTypes[i] == typeof(string))
						it[i] = ObservableProperty.Register<ObservablePropertyTests, string>($"{i}");
				}
			});

			// verify
			var propertyIds = new HashSet<int>();
			for (var i = valueTypes.Length - 1; i >= 0; --i)
			{
				Assert.That(propertyIds.Add(properties[i].Id));
				Assert.That(typeof(ObservablePropertyTests) == properties[i].OwnerType);
				Assert.That(valueTypes[i] == properties[i].ValueType);
				Assert.That(i == int.Parse(properties[i].Name));
			}
		}
	}
}
